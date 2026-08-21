using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using ClassicAssist.Data.Macros;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Mcp
{
    public static class McpCommandInvoker
    {
        private static readonly Type[] _commandClasses = Assembly.GetExecutingAssembly().GetTypes()
            .Where( t => t.Namespace != null && t.IsPublic && t.IsClass &&
                         t.Namespace.EndsWith( "Macros.Commands" ) )
            .ToArray();

        public static IReadOnlyList<McpTool> GetTools()
        {
            return new List<McpTool>
            {
                new McpTool
                {
                    Name = "invokeCommand",
                    Description = "Invoke any ClassicAssist macro command function directly by name (e.g. Pathfind, UseObject, Cast, FindType) without writing a macro. Arguments are passed as an object keyed by parameter name.",
                    InputSchema = McpTools.ObjectSchema(
                        new JObject
                        {
                            ["command"] = McpTools.StringProperty( "The macro command name (e.g. 'Pathfind')." ),
                            ["arguments"] = new JObject
                            {
                                ["type"] = "object",
                                ["description"] = "Arguments keyed by parameter name (e.g. {\"x\":990,\"y\":524,\"z\":-50})."
                            }
                        }, "command" )
                },
                new McpTool
                {
                    Name = "listCommands",
                    Description = "List available ClassicAssist macro commands with their signatures.",
                    InputSchema = McpTools.ObjectSchema(
                        new JObject { ["filter"] = McpTools.StringProperty( "Optional case-insensitive substring to filter command names." ) } )
                }
            };
        }

        public static CallToolResult Invoke( string name, JObject args )
        {
            try
            {
                switch ( name )
                {
                    case "invokeCommand":
                        return McpTools.Text( InvokeCommand(
                            McpTools.RequireString( args, "command" ),
                            args["arguments"] ) );
                    case "listCommands":
                        return McpTools.Text( ListCommands( McpTools.GetString( args, "filter" ) ) );
                    default:
                        return null;
                }
            }
            catch ( Exception e )
            {
                return McpTools.Error( e.Message );
            }
        }

        private static string InvokeCommand( string command, JToken args )
        {
            if ( string.IsNullOrWhiteSpace( command ) )
            {
                throw new InvalidOperationException( "Missing required argument 'command'." );
            }

            MethodInfo[] methods = _commandClasses
                .SelectMany( t => t.GetMethods( BindingFlags.Public | BindingFlags.Static ) )
                .Where( m => m.Name.Equals( command, StringComparison.OrdinalIgnoreCase ) )
                .ToArray();

            if ( methods.Length == 0 )
            {
                throw new InvalidOperationException(
                    $"Command '{command}' not found. Use listCommands to see available commands." );
            }

            string[] errors = new string[methods.Length];

            for ( int i = 0; i < methods.Length; i++ )
            {
                if ( TryBind( methods[i], args, out object[] parameters, out string bindError ) )
                {
                    object result = methods[i].Invoke( null, parameters );

                    return FormatResult( methods[i], result );
                }

                errors[i] = bindError;
            }

            throw new InvalidOperationException(
                $"Could not match arguments for command '{command}'. Expected signatures:\n{string.Join( "\n", errors )}" );
        }

        private static bool TryBind( MethodInfo method, JToken args, out object[] parameters, out string error )
        {
            ParameterInfo[] methodParams = method.GetParameters();
            parameters = new object[methodParams.Length];

            for ( int i = 0; i < methodParams.Length; i++ )
            {
                ParameterInfo parameter = methodParams[i];
                JToken value = null;

                if ( args is JObject named )
                {
                    value = named.Properties()
                        .FirstOrDefault( x => x.Name.Equals( parameter.Name, StringComparison.OrdinalIgnoreCase ) )
                        ?.Value;
                }
                else if ( args is JArray array && i < array.Count )
                {
                    value = array[i];
                }

                if ( value == null || value.Type == JTokenType.Null )
                {
                    if ( parameter.HasDefaultValue )
                    {
                        parameters[i] = parameter.DefaultValue;
                        continue;
                    }

                    error = $"{method.Name}({DescribeParams( methodParams )}): missing required argument '{parameter.Name}'";
                    return false;
                }

                if ( !TryConvert( value, parameter.ParameterType, out object converted, out string convertError ) )
                {
                    error = $"{method.Name}({DescribeParams( methodParams )}): {convertError}";
                    return false;
                }

                parameters[i] = converted;
            }

            error = null;
            return true;
        }

        private static bool TryConvert( JToken value, Type targetType, out object converted, out string error )
        {
            converted = null;
            error = null;

            Type underlying = Nullable.GetUnderlyingType( targetType ) ?? targetType;

            if ( underlying == typeof( object ) )
            {
                // Serial/alias: pass numeric serials as int, otherwise as string (alias).
                if ( value.Type == JTokenType.Integer )
                {
                    converted = value.ToObject<int>();
                    return true;
                }

                string text = value.ToObject<string>();

                if ( McpTools.TryParseInt( text, out int serial ) )
                {
                    converted = serial;
                }
                else
                {
                    converted = text;
                }

                return true;
            }

            if ( underlying == typeof( string ) )
            {
                if ( value.Type == JTokenType.String )
                {
                    converted = value.ToObject<string>();
                }
                else
                {
                    converted = value.ToString();
                }

                return true;
            }

            string raw = value.Type == JTokenType.String ? value.ToObject<string>() : value.ToString();

            if ( underlying.IsEnum )
            {
                try
                {
                    object enumValue = Enum.Parse( underlying, raw, true );

                    converted = enumValue;
                    return true;
                }
                catch
                {
                    // try numeric below
                }

                if ( McpTools.TryParseInt( raw, out int enumNumber ) && Enum.IsDefined( underlying, enumNumber ) )
                {
                    converted = Enum.ToObject( underlying, enumNumber );
                    return true;
                }

                error = $"'{raw}' is not a valid {underlying.Name}";
                return false;
            }

            if ( underlying == typeof( bool ) )
            {
                switch ( raw.ToLowerInvariant() )
                {
                    case "true":
                    case "1":
                    case "on":
                    case "yes":
                        converted = true;
                        return true;
                    case "false":
                    case "0":
                    case "off":
                    case "no":
                        converted = false;
                        return true;
                    default:
                        error = $"'{raw}' is not a valid boolean";
                        return false;
                }
            }

            if ( underlying == typeof( uint ) )
            {
                if ( raw.StartsWith( "0x", StringComparison.OrdinalIgnoreCase ) )
                {
                    if ( uint.TryParse( raw.Substring( 2 ), NumberStyles.HexNumber, CultureInfo.InvariantCulture,
                             out uint uintValue ) )
                    {
                        converted = uintValue;
                        return true;
                    }
                }
                else if ( uint.TryParse( raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint uintValue ) )
                {
                    converted = uintValue;
                    return true;
                }

                error = $"'{raw}' is not a valid UInt32";
                return false;
            }

            if ( underlying == typeof( long ) )
            {
                if ( raw.StartsWith( "0x", StringComparison.OrdinalIgnoreCase ) )
                {
                    if ( long.TryParse( raw.Substring( 2 ), NumberStyles.HexNumber, CultureInfo.InvariantCulture,
                             out long longValue ) )
                    {
                        converted = longValue;
                        return true;
                    }
                }
                else if ( long.TryParse( raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out long longValue ) )
                {
                    converted = longValue;
                    return true;
                }

                error = $"'{raw}' is not a valid Int64";
                return false;
            }

            if ( underlying == typeof( int ) || underlying == typeof( short ) ||
                 underlying == typeof( byte ) || underlying == typeof( sbyte ) || underlying == typeof( ushort ) )
            {
                if ( McpTools.TryParseInt( raw, out int intValue ) )
                {
                    converted = Convert.ChangeType( intValue, underlying, CultureInfo.InvariantCulture );
                    return true;
                }

                error = $"'{raw}' is not a valid {underlying.Name}";
                return false;
            }

            if ( underlying == typeof( double ) || underlying == typeof( float ) || underlying == typeof( decimal ) )
            {
                try
                {
                    converted = Convert.ChangeType( value.ToObject<string>(), underlying, CultureInfo.InvariantCulture );
                    return true;
                }
                catch
                {
                    error = $"'{raw}' is not a valid {underlying.Name}";
                    return false;
                }
            }

            if ( underlying == typeof( DateTime ) )
            {
                if ( DateTime.TryParse( value.ToObject<string>(), out DateTime date ) )
                {
                    converted = date;
                    return true;
                }

                error = $"'{raw}' is not a valid DateTime";
                return false;
            }

            // Fallback: try ChangeType.
            try
            {
                converted = Convert.ChangeType( value.ToObject<string>(), underlying, CultureInfo.InvariantCulture );
                return true;
            }
            catch
            {
                error = $"cannot convert '{raw}' to {underlying.Name}";
                return false;
            }
        }

        private static string FormatResult( MethodInfo method, object result )
        {
            if ( method.ReturnType == typeof( void ) )
            {
                return "OK";
            }

            if ( result == null )
            {
                return "null";
            }

            if ( method.ReturnType == typeof( bool ) )
            {
                return (bool) result ? "True" : "False";
            }

            return result.ToString();
        }

        private static string ListCommands( string filter )
        {
            JArray array = new JArray();

            IEnumerable<MethodInfo> methods = _commandClasses
                .SelectMany( t => t.GetMethods( BindingFlags.Public | BindingFlags.Static ) )
                .Where( m => m.GetCustomAttributes<CommandsDisplayAttribute>().Any() );

            if ( !string.IsNullOrEmpty( filter ) )
            {
                methods = methods.Where( m => m.Name.IndexOf( filter, StringComparison.OrdinalIgnoreCase ) >= 0 );
            }

            foreach ( MethodInfo method in methods.OrderBy( m => m.Name ) )
            {
                JObject entry = new JObject
                {
                    ["name"] = method.Name,
                    ["signature"] = $"{method.Name}({DescribeParams( method.GetParameters() )})"
                };

                CommandsDisplayAttribute attr = method.GetCustomAttributes<CommandsDisplayAttribute>().FirstOrDefault();

                if ( attr != null )
                {
                    if ( !string.IsNullOrEmpty( attr.Description ) )
                    {
                        entry["description"] = attr.Description;
                    }

                    if ( !string.IsNullOrEmpty( attr.Example ) )
                    {
                        entry["example"] = attr.Example;
                    }

                    ParameterInfo[] methodParams = method.GetParameters();

                    if ( attr.Parameters != null && attr.Parameters.Length > 0 )
                    {
                        JArray paramArray = new JArray();

                        for ( int i = 0; i < methodParams.Length; i++ )
                        {
                            string hint = i < attr.Parameters.Length ? GetParameterHint( attr.Parameters[i] ) : null;

                            paramArray.Add( new JObject
                            {
                                ["name"] = methodParams[i].Name,
                                ["hint"] = hint
                            } );
                        }

                        entry["parameters"] = paramArray;
                    }
                }

                array.Add( entry );
            }

            return JsonConvert.SerializeObject( array, Formatting.Indented );
        }

        private static string GetParameterHint( string parameterTypeName )
        {
            if ( !Enum.TryParse( parameterTypeName, out ParameterType type ) )
            {
                return parameterTypeName;
            }

            switch ( type )
            {
                case ParameterType.Serial:
                    return "serial (0x hex or decimal)";
                case ParameterType.SerialOrAlias:
                    return "serial or alias (e.g. 0x400d379d or 'backpack')";
                case ParameterType.ItemID:
                    return "item/gump id";
                case ParameterType.GumpButtonIndex:
                    return "gump button id";
                case ParameterType.Timeout:
                    return "timeout in ms";
                case ParameterType.Amount:
                    return "amount (-1 for all)";
                case ParameterType.AliasName:
                    return "alias name";
                case ParameterType.Range:
                    return "range in tiles (-1 for any)";
                case ParameterType.Distance:
                    return "distance in tiles";
                case ParameterType.Hue:
                    return "hue";
                case ParameterType.Direction:
                    return "direction (e.g. 'East')";
                case ParameterType.String:
                    return "string";
                case ParameterType.IntegerValue:
                    return "integer";
                case ParameterType.Boolean:
                    return "bool";
                case ParameterType.MacroName:
                    return "macro name";
                case ParameterType.SpellName:
                    return "spell name (e.g. 'Recall')";
                case ParameterType.SkillName:
                    return "skill name";
                case ParameterType.Layer:
                    return "layer (e.g. 'Backpack')";
                case ParameterType.ContextMenuIndex:
                    return "context menu entry index";
                case ParameterType.XCoordinate:
                case ParameterType.XCoordinateOffset:
                    return "x";
                case ParameterType.YCoordinate:
                case ParameterType.YCoordinateOffset:
                    return "y";
                case ParameterType.ZCoordinate:
                case ParameterType.ZCoordinateOffset:
                    return "z";
                default:
                    return type.ToString();
            }
        }

        private static string DescribeParams( ParameterInfo[] parameters )
        {
            return string.Join( ", ", parameters.Select( p =>
            {
                string typeName = p.ParameterType == typeof( object )
                    ? "serialOrAlias"
                    : p.ParameterType.Name;
                string optional = p.HasDefaultValue ? " = " + ( p.DefaultValue ?? "null" ) : "";

                return $"{typeName} {p.Name}{optional}";
            } ) );
        }
    }
}
