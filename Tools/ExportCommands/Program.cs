using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using ExportCommands.Properties;

// ReSharper disable LocalizableElement

namespace ExportCommands
{
    internal class Program
    {
        private static readonly string[] _locales = { "en-US", "it-IT" };

        private static void Main( string[] args )
        {
            if ( args.Length == 0 )
            {
                Console.WriteLine( "No file specified..." );
                return;
            }

            if ( !File.Exists( args[0] ) )
            {
                throw new FileNotFoundException( "File specified does not exist.", args[0] );
            }

            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            string originalDirectory = Environment.CurrentDirectory;

            Environment.CurrentDirectory = Path.GetDirectoryName( args[0] ) ?? throw new InvalidOperationException();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            foreach ( string locale in _locales )
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo( locale );
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo( locale );

                Assembly assembly;

                try
                {
                    assembly = Assembly.LoadFile( args[0] );
                }
                catch ( Exception e )
                {
                    Console.WriteLine( $"Failed to load assembly: {e}" );
                    return;
                }

                Console.WriteLine( "Generating Macro documentation..." );
                Console.WriteLine( $"Loading {args[0]}" );

                IEnumerable<Type> types = assembly.GetTypes().Where( t =>
                    t.Namespace != null && t.Namespace.StartsWith( "ClassicAssist.Data.Macros.Commands" ) );

                Type cda = assembly.GetType( "ClassicAssist.Data.Macros.CommandsDisplayAttribute" );
                Type cdsaa = assembly.GetType( "ClassicAssist.Data.Macros.CommandsDisplayStringSeeAlsoAttribute" );

                List<Commands> commands = new List<Commands>();
                List<Type> seeAlsoTypes = new List<Type>();

                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach ( Type type in types )
                {
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach ( MemberInfo memberInfo in type.GetMembers( BindingFlags.Public | BindingFlags.Static ) )
                    {
                        Attribute attrCD = memberInfo.GetCustomAttribute( cda );
                        Attribute attrCDSA = memberInfo.GetCustomAttribute( cdsaa );

                        if ( attrCDSA != null )
                        {
                        }

                        if ( attrCD == null )
                        {
                            continue;
                        }

                        List<Parameter> param = new List<Parameter>();

                        dynamic attrParams = ( (dynamic) attrCD ).Parameters;
                        dynamic attr2Params = null;

                        if ( attrCDSA != null )
                        {
                            attr2Params = ( (dynamic) attrCDSA ).Enums;
                        }

                        if ( memberInfo.MemberType == MemberTypes.Method && attrParams != null )
                        {
                            MethodInfo methodInfo = memberInfo as MethodInfo;
                            ParameterInfo[] parameters = methodInfo?.GetParameters();

                            int i = 0;

                            if ( parameters != null )
                            {
                                foreach ( ParameterInfo parameterInfo in parameters )
                                {
                                    Parameter parameter = new Parameter();

                                    object defaultValue = parameterInfo.RawDefaultValue;

                                    if ( i + 1 > attrParams.Length )
                                    {
                                        parameter.Name = parameterInfo.Name.ToLower();
                                        parameter.Optional =
                                            defaultValue == null || defaultValue.GetType() != typeof( DBNull );
                                        parameter.Description = Resources.ResourceManager.GetString(
                                            "PARAMETER_DESCRIPTION_UNKNOWN" );
                                        param.Add( parameter );
                                        i++;
                                    }
                                    else
                                    {
                                        dynamic attrParam = attrParams[i];

                                        if ( attrParam == null )
                                        {
                                            continue;
                                        }

                                        parameter.Name = parameterInfo.Name.ToLower();
                                        parameter.Optional =
                                            defaultValue == null || defaultValue.GetType() != typeof( DBNull );

                                        string resourceName = $"PARAMETER_DESCRIPTION_{attrParam.ToUpper()}";

                                        string resourceValue = Resources.ResourceManager.GetString( resourceName );

                                        if ( string.IsNullOrEmpty( resourceValue ) )
                                        {
                                            throw new InvalidOperationException( resourceName );
                                        }

                                        parameter.Description = !string.IsNullOrEmpty( resourceValue )
                                            ? resourceValue
                                            : Resources.Unknown;

                                        if ( attr2Params != null && attr2Params?.Length >= i + 1 &&
                                             attr2Params?[i] != null )
                                        {
                                            dynamic typeSA = FindEnumType( attr2Params?[i], assembly );
                                            parameter.SeeAlso = typeSA;

                                            if ( typeSA != null && !seeAlsoTypes.Contains( typeSA ) )
                                            {
                                                seeAlsoTypes.Add( typeSA );
                                            }
                                        }

                                        param.Add( parameter );
                                        i++;
                                    }
                                }
                            }
                        }

                        commands.Add( new Commands
                        {
                            Category = ( (dynamic) attrCD ).Category,
                            Description = ( (dynamic) attrCD ).Description,
                            Example = ( (dynamic) attrCD ).Example,
                            InsertText = ( (dynamic) attrCD ).InsertText,
                            Signature = memberInfo.ToString(),
                            Name = memberInfo.Name,
                            Parameters = param
                        } );
                    }
                }

                commands.Sort( new CommandComparer() );

                IEnumerable<string> categories = commands.Select( c => c.Category ).Distinct();

                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo( assembly.Location );

                string markDown =
                    $"# {Resources.ClassicAssist_Macro_Commands}  \n{Resources.Generated_on} {DateTime.UtcNow}  \n{Resources.Version}: {fvi.ProductVersion}  \n  \n";

                if ( !string.IsNullOrEmpty( Resources.TRANSLATE_CREDIT ) )
                {
                    markDown =
                        $"# {Resources.ClassicAssist_Macro_Commands}  \n{Resources.Generated_on} {DateTime.UtcNow}  \n{Resources.Version}: {fvi.ProductVersion}  \n{Resources.TRANSLATE_CREDIT}  \n  \n";
                }

                foreach ( string category in categories )
                {
                    IEnumerable<Commands> categoryCommands =
                        commands.Where( c => c.Category == category ).OrderBy( c => c.Name );

                    markDown += $"## {category}  \n";

                    foreach ( Commands command in categoryCommands )
                    {
                        string example = command.InsertText;

                        if ( !string.IsNullOrEmpty( command.Example ) )
                        {
                            example = command.Example;
                        }

                        markDown += $"### {command.Name}  \n  \n";
                        markDown += $"{Resources.Method_Signature}:  \n  \n**{command.Signature}**  \n  \n";

                        if ( command.Parameters.Any() )
                        {
                            markDown += $"#### {Resources.Parameters}  \n";

                            foreach ( Parameter parameter in command.Parameters )
                            {
                                markDown +=
                                    $"* {parameter.Name}: {parameter.Description}.{( parameter.Optional ? $" ({Resources.Optional})" : "" )}";

                                if ( parameter.SeeAlso != null )
                                {
                                    markDown += string.Format( $" {Resources.See_Also___0_}  \n",
                                        $"[{parameter.SeeAlso.Name}](#{parameter.SeeAlso.Name})" );
                                }
                                else
                                {
                                    markDown += "  \n";
                                }
                            }

                            markDown += "  \n";
                        }

                        markDown += $"{Resources.Description}:  \n  \n**{command.Description}**  \n  \n";
                        markDown += $"{Resources.Example}:  \n  \n```python  \n{example}  \n```  \n  \n";
                    }

                    markDown += "\n\n\n";
                }

                markDown += $"## {Resources.Types}  \n";

                seeAlsoTypes = seeAlsoTypes.OrderBy( t => t.Name ).ToList();

                foreach ( Type seeAlsoType in seeAlsoTypes )
                {
                    markDown += $"### {seeAlsoType.Name}  \n";

                    string[] enumNames = seeAlsoType.GetEnumNames();

                    if ( enumNames == null )
                    {
                        continue;
                    }

                    markDown = enumNames.Aggregate( markDown, ( current, enumName ) => current + $"* {enumName}  \n" );

                    markDown += "  \n";
                }

                string fileName = $"Macro-Commands ({locale}).md";

                if ( locale.Equals( "en-US" ) )
                {
                    fileName = "Macro-Commands.md";
                }

                File.WriteAllText( Path.Combine( originalDirectory, fileName ), markDown );
            }

            sw.Stop();

            Console.WriteLine( $"Finished in {sw.Elapsed}" );
        }

        private static Type FindEnumType( string shortName, Assembly assembly )
        {
            Type matchingType = assembly.GetTypes().FirstOrDefault( t => t.Name == shortName );

            return matchingType;
        }

        private static Assembly OnAssemblyResolve( object sender, ResolveEventArgs args )
        {
            string assemblyname = new AssemblyName( args.Name ).Name;

            string[] searchPaths = { Environment.CurrentDirectory, RuntimeEnvironment.GetRuntimeDirectory() };

            if ( assemblyname.Contains( "Colletions" ) )
            {
                assemblyname = "System.Collections";
            }

            foreach ( string searchPath in searchPaths )
            {
                string fullPath = Path.Combine( searchPath, assemblyname + ".dll" );

                string culture = new AssemblyName( args.Name ).CultureName;

                if ( !File.Exists( fullPath ) )
                {
                    string culturePath = Path.Combine( searchPath, culture, assemblyname + ".dll" );

                    if ( File.Exists( culturePath ) )
                    {
                        fullPath = culturePath;
                    }
                    else
                    {
                        continue;
                    }
                }

                Assembly assembly = Assembly.LoadFrom( fullPath );
                Console.WriteLine( $"Loading {fullPath}" );

                return assembly;
            }

            Console.WriteLine( $"Couldn't locate {assemblyname}..." );

            return null;
        }
    }

    internal class CommandComparer : IComparer<Commands>
    {
        public int Compare( Commands x, Commands y )
        {
            int result = string.Compare( x?.Category, y?.Category, StringComparison.InvariantCultureIgnoreCase );

            return result;
        }
    }

    internal class Parameter
    {
        public string Description { get; set; } = "Unknown";
        public string Name { get; set; } = "Unknown";
        public bool Optional { get; set; }
        public Type SeeAlso { get; set; }
    }

    internal class Commands
    {
        public string Category { get; set; }
        public string Description { get; set; }
        public string Example { get; set; }
        public string InsertText { get; set; }
        public string Name { get; set; }
        public List<Parameter> Parameters { get; set; }
        public string Signature { get; set; }
    }
}