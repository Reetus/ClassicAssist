using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Macros;
using ClassicAssist.Data.Macros.Commands;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Mcp
{
    public static class McpTools
    {
        public static IReadOnlyList<McpTool> GetTools()
        {
            List<McpTool> tools = new List<McpTool>
            {
                new McpTool
                {
                    Name = "listMacros",
                    Description = "List all macros with their current status and metadata.",
                    InputSchema = ObjectSchema(
                        new JObject
                        {
                            ["filter"] = StringProperty( "Optional case-insensitive substring to filter macro names." )
                        } )
                },
                new McpTool
                {
                    Name = "getMacro",
                    Description = "Get the source code and metadata of a single macro by name.",
                    InputSchema = ObjectSchema( new JObject { ["name"] = StringProperty( "The macro name." ) }, "name" )
                },
                new McpTool
                {
                    Name = "createMacro",
                    Description = "Create a new macro with Python source code. Pass a filePath to store it as a file-backed .py macro; otherwise it is embedded in the profile.",
                    InputSchema = ObjectSchema(
                        new JObject
                        {
                            ["name"] = StringProperty( "The macro name (must be unique)." ),
                            ["code"] = StringProperty( "The Python macro source code." ),
                            ["background"] = new JObject { ["type"] = "boolean", ["description"] = "Whether the macro runs in the background (optional)." },
                            ["filePath"] = StringProperty( "Optional file path (absolute or relative to the Macros folder) to write the macro to a .py file." )
                        }, "name", "code" )
                },
                new McpTool
                {
                    Name = "updateMacro",
                    Description = "Update an existing macro's source code and/or rename it.",
                    InputSchema = ObjectSchema(
                        new JObject
                        {
                            ["name"] = StringProperty( "The macro name to update." ),
                            ["code"] = StringProperty( "New Python source code (optional)." ),
                            ["newName"] = StringProperty( "New name for the macro (optional)." )
                        }, "name" )
                },
                new McpTool
                {
                    Name = "deleteMacro",
                    Description = "Delete a macro by name.",
                    InputSchema = ObjectSchema( new JObject { ["name"] = StringProperty( "The macro name." ) }, "name" )
                },
                new McpTool
                {
                    Name = "playMacro",
                    Description = "Run a macro by name, optionally passing string arguments. Waits up to waitMs (default 3000) for the macro to finish or error, then reports the result including any error.",
                    InputSchema = ObjectSchema(
                        new JObject
                        {
                            ["name"] = StringProperty( "The macro name to run." ),
                            ["args"] = new JObject
                            {
                                ["type"] = "array",
                                ["items"] = new JObject { ["type"] = "string" },
                                ["description"] = "Optional arguments passed to the macro."
                            },
                            ["waitMs"] = IntegerProperty( "Optional maximum milliseconds to wait for the macro to finish or error (default 3000)." )
                        }, "name" )
                },
                new McpTool
                {
                    Name = "stopMacro",
                    Description = "Stop a running macro by name, or the currently running macro if no name is given.",
                    InputSchema = ObjectSchema( new JObject { ["name"] = StringProperty( "The macro name (optional)." ) } )
                },
                new McpTool
                {
                    Name = "stopAllMacros",
                    Description = "Stop all running macros.",
                    InputSchema = ObjectSchema()
                },
                new McpTool
                {
                    Name = "getMacroStatus",
                    Description = "Get the running, paused and error status of a macro by name, or of all macros if no name is given.",
                    InputSchema = ObjectSchema( new JObject { ["name"] = StringProperty( "The macro name (optional)." ) } )
                }
            };

            tools.AddRange( McpGameStateTools.GetTools() );
            tools.AddRange( McpCommandInvoker.GetTools() );
            tools.AddRange( McpAgentTools.GetTools() );

            return tools;
        }

        public static CallToolResult Invoke( string name, JObject args )
        {
            try
            {
                switch ( name )
                {
                    case "listMacros":
                        return Text( ListMacros( GetString( args, "filter" ) ) );
                    case "getMacro":
                        return Text( GetMacro( RequireString( args, "name" ) ) );
                    case "createMacro":
                        return Text( CreateMacro(
                            RequireString( args, "name" ),
                            RequireString( args, "code" ),
                            GetBool( args, "background" ),
                            GetString( args, "filePath" ) ) );
                    case "updateMacro":
                        return Text( UpdateMacro(
                            RequireString( args, "name" ),
                            GetString( args, "code" ),
                            GetString( args, "newName" ) ) );
                    case "deleteMacro":
                        return Text( DeleteMacro( RequireString( args, "name" ) ) );
                    case "playMacro":
                        return Text( PlayMacro( RequireString( args, "name" ), GetStringArray( args, "args" ), GetInt( args, "waitMs" ) ) );
                    case "stopMacro":
                        return Text( StopMacro( GetString( args, "name" ) ) );
                    case "stopAllMacros":
                        return Text( StopAllMacros() );
                    case "getMacroStatus":
                        return Text( GetMacroStatus( GetString( args, "name" ) ) );
                    default:
                    {
                        CallToolResult result = McpGameStateTools.Invoke( name, args ) ??
                                               McpCommandInvoker.Invoke( name, args ) ??
                                               McpAgentTools.Invoke( name, args );

                        return result ?? Error( $"Unknown tool: {name}" );
                    }
                }
            }
            catch ( Exception e )
            {
                return Error( e.Message );
            }
        }

        private static string ListMacros( string filter )
        {
            List<JObject> results = OnUi( () =>
            {
                IEnumerable<MacroEntry> items = GetItems();

                if ( !string.IsNullOrEmpty( filter ) )
                {
                    items = items.Where( m => m.Name.IndexOf( filter, StringComparison.OrdinalIgnoreCase ) >= 0 );
                }

                return items.OrderBy( m => m.Name ).Select( Summarize ).ToList();
            } );

            return JsonConvert.SerializeObject( results, Formatting.Indented );
        }

        private static string GetMacro( string name )
        {
            JObject result = OnUi( () =>
            {
                MacroEntry entry = Find( name );

                if ( entry == null )
                {
                    return null;
                }

                return new JObject
                {
                    ["name"] = entry.Name,
                    ["code"] = entry.Macro ?? string.Empty,
                    ["isFileBacked"] = entry.IsFileBacked,
                    ["filePath"] = entry.FilePath,
                    ["isBackground"] = entry.IsBackground,
                    ["loop"] = entry.Loop,
                    ["group"] = entry.Group ?? string.Empty
                };
            } );

            return result == null
                ? throw new InvalidOperationException( $"Macro '{name}' not found." )
                : JsonConvert.SerializeObject( result, Formatting.Indented );
        }

        private static string CreateMacro( string name, string code, bool background, string filePath )
        {
            return OnUi( () =>
            {
                MacroManager manager = MacroManager.GetInstance();

                EnsureItems( manager );

                if ( manager.Items.Any( m => m.Name.Equals( name, StringComparison.OrdinalIgnoreCase ) ) )
                {
                    throw new InvalidOperationException( $"Macro '{name}' already exists." );
                }

                MacroEntry entry = new MacroEntry { Name = name, Macro = code ?? string.Empty, IsBackground = background };

                if ( !string.IsNullOrEmpty( filePath ) )
                {
                    string resolved = ResolveMacrosPath( filePath );

                    string dir = Path.GetDirectoryName( resolved );

                    if ( !string.IsNullOrEmpty( dir ) )
                    {
                        Directory.CreateDirectory( dir );
                    }

                    File.WriteAllText( resolved, entry.Macro );
                    entry.FilePath = resolved;
                }

                entry.Action = ( hks, parameters ) => MacroManager.GetInstance().Execute( entry, parameters );

                manager.Items.Add( entry );

                Options.Save( Options.CurrentOptions );

                JObject summary = Summarize( entry );
                summary["code"] = entry.Macro;

                return JsonConvert.SerializeObject( summary, Formatting.Indented );
            } );
        }

        private static string UpdateMacro( string name, string code, string newName )
        {
            return OnUi( () =>
            {
                MacroEntry entry = Find( name );

                if ( entry == null )
                {
                    throw new InvalidOperationException( $"Macro '{name}' not found." );
                }

                if ( code != null )
                {
                    entry.Macro = code;
                }

                if ( !string.IsNullOrEmpty( newName ) && !newName.Equals( entry.Name, StringComparison.Ordinal ) )
                {
                    MacroManager manager = MacroManager.GetInstance();

                    if ( manager.Items.Any( m => !ReferenceEquals( m, entry ) &&
                                                 m.Name.Equals( newName, StringComparison.OrdinalIgnoreCase ) ) )
                    {
                        throw new InvalidOperationException( $"Macro '{newName}' already exists." );
                    }

                    entry.Name = newName;
                }

                Options.Save( Options.CurrentOptions );

                JObject summary = Summarize( entry );
                summary["code"] = entry.Macro;

                return JsonConvert.SerializeObject( summary, Formatting.Indented );
            } );
        }

        private static string DeleteMacro( string name )
        {
            return OnUi( () =>
            {
                MacroEntry entry = Find( name );

                if ( entry == null )
                {
                    throw new InvalidOperationException( $"Macro '{name}' not found." );
                }

                string filePath = entry.IsFileBacked ? entry.FilePath : null;

                if ( entry.IsRunning )
                {
                    entry.Stop();
                }

                MacroManager.GetInstance().Items.Remove( entry );

                if ( !string.IsNullOrEmpty( filePath ) )
                {
                    try
                    {
                        File.Delete( filePath );
                    }
                    catch
                    {
                        // best effort
                    }
                }

                Options.Save( Options.CurrentOptions );

                return $"Deleted macro '{name}'.";
            } );
        }

        private const int DEFAULT_PLAY_WAIT_MS = 3000;

        private static string PlayMacro( string name, string[] args, int? waitMs )
        {
            MacroEntry entry = OnUi( () => Find( name ) );

            if ( entry == null )
            {
                throw new InvalidOperationException( $"Macro '{name}' not found." );
            }

            object[] parameters = args?.Cast<object>().ToArray();

            MainCommands.BringClientWindowToFront();

            MacroManager.GetInstance().Execute( entry, parameters );

            JObject result = WaitForResult( entry, waitMs ?? DEFAULT_PLAY_WAIT_MS );

            return JsonConvert.SerializeObject( result, Formatting.Indented );
        }

        private static JObject WaitForResult( MacroEntry entry, int timeoutMs )
        {
            const int pollInterval = 50;
            int waited = 0;

            while ( waited < timeoutMs )
            {
                if ( entry.LastException != null || !entry.IsRunning )
                {
                    break;
                }

                Thread.Sleep( pollInterval );
                waited += pollInterval;
            }

            // Authoritative read on the UI thread - IsRunning and LastException are updated there.
            (bool running, Exception exception) state = OnUi( () => ( entry.IsRunning, entry.LastException ) );

            return new JObject
            {
                ["name"] = entry.Name,
                ["isRunning"] = state.running,
                ["success"] = !state.running && state.exception == null,
                ["error"] = state.exception != null ? GetErrorInfo( state.exception ) : null
            };
        }

        private static JObject GetErrorInfo( Exception exception )
        {
            JObject error = new JObject
            {
                ["type"] = exception.GetType().Name,
                ["message"] = exception.Message
            };

            try
            {
                if ( exception is SyntaxErrorException syntaxError )
                {
                    error["line"] = syntaxError.RawSpan.Start.Line;
                }
                else
                {
                    DynamicStackFrame frame = PythonOps.GetDynamicStackFrames( exception ).FirstOrDefault();

                    if ( frame != null )
                    {
                        string fileName = frame.GetFileName();

                        if ( fileName != "<string>" )
                        {
                            error["file"] = fileName;
                        }

                        error["line"] = frame.GetFileLineNumber();
                    }
                }
            }
            catch
            {
                // best effort - message alone is still useful
            }

            return error;
        }

        private static string StopMacro( string name )
        {
            OnUi( () =>
            {
                MacroManager manager = MacroManager.GetInstance();

                if ( string.IsNullOrEmpty( name ) )
                {
                    manager.Stop();
                    return;
                }

                MacroEntry entry = Find( name );

                if ( entry == null )
                {
                    throw new InvalidOperationException( $"Macro '{name}' not found." );
                }

                entry.Stop();
            } );

            return string.IsNullOrEmpty( name ) ? "Stopped current macro." : $"Stopped macro '{name}'.";
        }

        private static string StopAllMacros()
        {
            OnUi( () => MacroManager.GetInstance().StopAll() );

            return "Stopped all macros.";
        }

        private static string GetMacroStatus( string name )
        {
            List<JObject> results = OnUi( () =>
            {
                IEnumerable<MacroEntry> items = GetItems();

                if ( !string.IsNullOrEmpty( name ) )
                {
                    MacroEntry single = Find( name );

                    if ( single == null )
                    {
                        throw new InvalidOperationException( $"Macro '{name}' not found." );
                    }

                    items = new[] { single };
                }

                return items.OrderBy( m => m.Name ).Select( StatusOf ).ToList();
            } );

            return JsonConvert.SerializeObject( results, Formatting.Indented );
        }

        private static JObject StatusOf( MacroEntry entry )
        {
            return new JObject
            {
                ["name"] = entry.Name,
                ["isRunning"] = entry.IsRunning,
                ["isPaused"] = entry.IsPaused,
                ["pausedLine"] = entry.PausedLinedNumber,
                ["lastException"] = entry.LastException?.Message,
                ["error"] = entry.LastException != null ? GetErrorInfo( entry.LastException ) : null
            };
        }

        private static JObject Summarize( MacroEntry entry )
        {
            return new JObject
            {
                ["name"] = entry.Name,
                ["id"] = entry.Id,
                ["isRunning"] = entry.IsRunning,
                ["isPaused"] = entry.IsPaused,
                ["isBackground"] = entry.IsBackground,
                ["isFileBacked"] = entry.IsFileBacked,
                ["filePath"] = entry.FilePath,
                ["group"] = entry.Group ?? string.Empty,
                ["loop"] = entry.Loop,
                ["isAutostart"] = entry.IsAutostart
            };
        }

        private static MacroEntry Find( string name )
        {
            return GetItems().FirstOrDefault( m => m.Name.Equals( name, StringComparison.OrdinalIgnoreCase ) );
        }

        private static IEnumerable<MacroEntry> GetItems()
        {
            ObservableCollection<MacroEntry> items = MacroManager.GetInstance().Items;

            return items ?? Enumerable.Empty<MacroEntry>();
        }

        private static void EnsureItems( MacroManager manager )
        {
            if ( manager.Items == null )
            {
                throw new InvalidOperationException( "Macro collection is not initialized (Macros tab has not loaded)." );
            }
        }

        private static string ResolveMacrosPath( string filePath )
        {
            if ( Path.IsPathRooted( filePath ) )
            {
                return filePath;
            }

            string resolved = Path.Combine( AssistantOptions.GetGlobalPath(), "Macros", filePath );

            if ( !resolved.EndsWith( ".py", StringComparison.OrdinalIgnoreCase ) )
            {
                resolved += ".py";
            }

            return resolved;
        }

        private static T OnUi<T>( Func<T> func )
        {
            if ( Engine.Dispatcher == null )
            {
                return func();
            }

            return Engine.Dispatcher.Invoke( func );
        }

        private static void OnUi( Action action )
        {
            if ( Engine.Dispatcher == null )
            {
                action();
                return;
            }

            Engine.Dispatcher.Invoke( action );
        }

        internal static CallToolResult Text( string text )
        {
            return new CallToolResult { Content = new List<McpContent> { new McpContent { Text = text } } };
        }

        internal static CallToolResult Error( string message )
        {
            return new CallToolResult
            {
                IsError = true,
                Content = new List<McpContent> { new McpContent { Text = message } }
            };
        }

        internal static JObject ObjectSchema( JObject properties = null, params string[] required )
        {
            JObject schema = new JObject { ["type"] = "object" };

            if ( properties != null && properties.Count > 0 )
            {
                schema["properties"] = properties;
            }

            if ( required != null && required.Length > 0 )
            {
                schema["required"] = new JArray( required );
            }

            return schema;
        }

        internal static JObject StringProperty( string description )
        {
            return new JObject { ["type"] = "string", ["description"] = description };
        }

        internal static JObject IntegerProperty( string description )
        {
            return new JObject { ["type"] = "integer", ["description"] = description };
        }

        internal static string RequireString( JObject args, string name )
        {
            string value = GetString( args, name );

            if ( string.IsNullOrEmpty( value ) )
            {
                throw new InvalidOperationException( $"Missing required argument '{name}'." );
            }

            return value;
        }

        internal static string GetString( JObject args, string name )
        {
            JToken token = args?[name];

            if ( token == null || token.Type == JTokenType.Null )
            {
                return null;
            }

            return token.ToObject<string>();
        }

        internal static bool GetBool( JObject args, string name )
        {
            JToken token = args?[name];

            return token != null && token.Type != JTokenType.Null && token.ToObject<bool>();
        }

        internal static string[] GetStringArray( JObject args, string name )
        {
            JToken token = args?[name];

            if ( token == null || token.Type != JTokenType.Array )
            {
                return null;
            }

            return token.Select( t => t.ToObject<string>() ).ToArray();
        }

        internal static int? GetInt( JObject args, string name )
        {
            JToken token = args?[name];

            if ( token == null || token.Type == JTokenType.Null )
            {
                return null;
            }

            if ( token.Type == JTokenType.Integer )
            {
                return token.ToObject<int>();
            }

            string text = token.ToObject<string>();

            return TryParseInt( text, out int value ) ? value : (int?) null;
        }

        internal static int RequireInt( JObject args, string name )
        {
            int? value = GetInt( args, name );

            if ( value == null )
            {
                throw new InvalidOperationException( $"Missing or invalid required argument '{name}'." );
            }

            return value.Value;
        }

        internal static bool TryParseInt( string text, out int value )
        {
            text = text?.Trim();

            if ( string.IsNullOrEmpty( text ) )
            {
                value = 0;
                return false;
            }

            if ( text.StartsWith( "0x", StringComparison.OrdinalIgnoreCase ) )
            {
                return int.TryParse( text.Substring( 2 ), NumberStyles.HexNumber, CultureInfo.InvariantCulture,
                    out value );
            }

            // Try hex without prefix if it contains a-f
            if ( text.Any( c => c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F' ) )
            {
                return int.TryParse( text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value );
            }

            return int.TryParse( text, out value );
        }
    }
}
