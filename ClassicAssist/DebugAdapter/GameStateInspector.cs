using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Assistant;
using ClassicAssist.UO;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.Debug
{
    public static class GameStateInspector
    {
        public static Tuple<bool, string> Execute( string command )
        {
            string[] parts = command.TrimStart( '!' )
                .Split( new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries );

            if ( parts.Length == 0 )
            {
                return Tuple.Create( false, "" );
            }

            switch ( parts[0].ToLowerInvariant() )
            {
                case "props":
                    return Tuple.Create( true, HandleProps( parts.Length > 1 ? parts[1].Trim() : null ) );
                default:
                    return Tuple.Create( false, $"Unknown command: !{parts[0]}" );
            }
        }

        private static string HandleProps( string serialStr )
        {
            if ( string.IsNullOrEmpty( serialStr ) )
            {
                return "Usage: !props <serial>\nSerial can be hex (0x40001234) or decimal (1073746484)";
            }

            int serial;

            if ( !TryParseSerial( serialStr, out serial ) )
            {
                return $"Invalid serial: {serialStr}";
            }

            Entity entity = FindEntity( serial );

            if ( entity == null )
            {
                return $"Entity 0x{serial:X8} not found";
            }

            return FormatProperties( entity );
        }

        private static string FormatProperties( Entity entity )
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine( $"--- {entity.GetType().Name}: 0x{entity.Serial:X8} ---" );
            sb.AppendLine( $"Name: {entity.Name ?? "(none)"}" );
            sb.AppendLine( $"Graphic: 0x{entity.ID:X4}  Hue: 0x{entity.Hue:X4}" );
            sb.AppendLine( $"Location: {entity.X}, {entity.Y}, {entity.Z}" );

            if ( entity is Mobile mobile )
            {
                sb.AppendLine( $"Hits: {mobile.Hits}/{mobile.HitsMax}" );
                sb.AppendLine( $"Notoriety: {mobile.Notoriety}" );
            }
            else if ( entity is Item item )
            {
                if ( item.Count > 0 )
                {
                    sb.AppendLine( $"Count: {item.Count}" );
                }

                if ( item.Owner != 0 )
                {
                    sb.AppendLine( $"Owner: 0x{item.Owner:X8}" );
                }
            }

            Property[] props = entity.Properties;

            if ( props == null || props.Length == 0 )
            {
                sb.AppendLine( "\n(no properties)" );
                return sb.ToString();
            }

            sb.AppendLine( $"\nProperties ({props.Length}):" );

            for ( int i = 0; i < props.Length; i++ )
            {
                Property p = props[i];
                sb.AppendLine( $"  [{i}] Cliloc: {p.Cliloc}" );
                sb.AppendLine( $"       Text: {p.Text ?? "(null)"}" );

                if ( p.Arguments != null && p.Arguments.Length > 0 )
                {
                    sb.AppendLine( $"       Args: [{string.Join( ", ", p.Arguments )}]" );
                }
            }

            return sb.ToString();
        }

        private static Entity FindEntity( int serial )
        {
            if ( UOMath.IsMobile( serial ) )
            {
                Mobile mobile = Engine.Mobiles.GetMobile( serial );

                if ( mobile != null )
                {
                    return mobile;
                }

                return Engine.Player != null && Engine.Player.Serial == serial ? Engine.Player : null;
            }

            return Engine.Items.GetItem( serial );
        }

        private static bool TryParseSerial( string text, out int serial )
        {
            text = text.Trim();

            if ( text.StartsWith( "0x", StringComparison.OrdinalIgnoreCase ) )
            {
                return int.TryParse( text.Substring( 2 ), NumberStyles.HexNumber, CultureInfo.InvariantCulture,
                    out serial );
            }

            // Try hex without prefix if it contains a-f
            if ( text.Any( c => c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F' ) )
            {
                return int.TryParse( text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out serial );
            }

            return int.TryParse( text, out serial );
        }
    }
}
