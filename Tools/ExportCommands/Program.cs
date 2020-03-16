using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ExportCommands
{
    internal class Program
    {
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

            Assembly assembly;

            try
            {
                assembly = Assembly.LoadFile( args[0] );
            }
            catch ( Exception e )
            {
                Console.WriteLine($"Failed to load assembly: {e}");
                return;
            }

            Console.WriteLine( "Generating Macro documentation..." );
            Console.WriteLine( $"Loading {args[0]}" );

            Stopwatch sw = new Stopwatch();
            sw.Start();

            IEnumerable<Type> types = assembly.GetTypes().Where( t =>
                t.Namespace != null && t.Namespace.StartsWith( "ClassicAssist.Data.Macros.Commands" ) );

            List<Commands> commands = new List<Commands>();

            Type cda = assembly.GetType( "ClassicAssist.Data.Macros.CommandsDisplayAttribute" );

            foreach ( Type type in types )
            {
                MemberInfo[] members = type.GetMembers( BindingFlags.Public | BindingFlags.Static );

                foreach ( MemberInfo memberInfo in members )
                {
                    dynamic attr = memberInfo.GetCustomAttribute( cda );

                    if ( attr != null )
                    {
                        Commands cmd = new Commands
                        {
                            Category = attr.Category,
                            Description = attr.Description,
                            Example = attr.Example,
                            InsertText = attr.InsertText,
                            Name = memberInfo.ToString()
                        };

                        commands.Add( cmd );
                    }
                }
            }

            commands.Sort( new CommandComparer() );

            IEnumerable<string> categories = commands.Select( c => c.Category ).Distinct();

            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo( assembly.Location );

            string markDown =
                $"# ClassicAssist Macro Commands  \nGenerated on {DateTime.UtcNow}  \nVersion: {fvi.ProductVersion}  \n  \n";

            foreach ( string category in categories )
            {
                IEnumerable<Commands> categoryCommands = commands.Where( c => c.Category == category );

                markDown += $"## {category}  \n";

                foreach ( Commands command in categoryCommands )
                {
                    var example = command.InsertText;

                    if ( !string.IsNullOrEmpty( command.Example ) )
                    {
                        example = command.Example;
                    }

                    markDown += $"Method Signature:  \n  \n**{command.Name}**  \n  \n";
                    markDown += $"Description:  \n  \n**{command.Description}**  \n  \n";
                    markDown += $"Example:  \n  \n```python  \n{example}  \n```  \n  \n";
                }

                markDown += "\n\n\n";
            }

            File.WriteAllText( Path.Combine( originalDirectory, "Macro-Commands.md" ), markDown );

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
    }
}