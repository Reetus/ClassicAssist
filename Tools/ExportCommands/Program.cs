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

                List<Commands> commands = ( from type in types
                    from memberInfo in type.GetMembers( BindingFlags.Public | BindingFlags.Static )
                    let attr = memberInfo.GetCustomAttribute( cda )
                    where (dynamic) attr != null
                    select new Commands
                    {
                        Category = ( (dynamic) attr ).Category,
                        Description = ( (dynamic) attr ).Description,
                        Example = ( (dynamic) attr ).Example,
                        InsertText = ( (dynamic) attr ).InsertText,
                        Signature = memberInfo.ToString(),
                        Name = memberInfo.Name
                    } ).ToList();

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
                        markDown += $"{Resources.Description}:  \n  \n**{command.Description}**  \n  \n";
                        markDown += $"{Resources.Example}:  \n  \n```python  \n{example}  \n```  \n  \n";
                    }

                    markDown += "\n\n\n";
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

    public class Commands
    {
        public string Category { get; set; }
        public string Description { get; set; }
        public string Example { get; set; }
        public string InsertText { get; set; }
        public string Name { get; set; }
        public string Signature { get; set; }
    }
}