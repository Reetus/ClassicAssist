using System.Linq;
using Assistant;
using ClassicAssist.Resources;
using ClassicAssist.UO.Objects.Gumps;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class GumpCommands
    {
        [CommandsDisplay( Category = nameof( Strings.Gumps ),
            Parameters = new[] { nameof( ParameterType.ItemID ), nameof( ParameterType.Timeout ) } )]
        public static bool WaitForGump( uint gumpId = 0, int timeout = 30000 )
        {
            bool result = UOC.WaitForGump( gumpId, timeout );

            if ( !result )
            {
                UOC.SystemMessage( Strings.Timeout___ );
            }

            return result;
        }

        [CommandsDisplay( Category = nameof( Strings.Gumps ),
            Parameters = new[] { nameof( ParameterType.ItemID ), nameof( ParameterType.GumpButtonIndex ) } )]
        public static void ReplyGump( uint gumpId, int buttonId )
        {
            UOC.GumpButtonClick( gumpId, buttonId );
        }

        [CommandsDisplay( Category = nameof( Strings.Gumps ) )]
        public static bool GumpExists( uint gumpId )
        {
            return Engine.GumpList.ContainsKey( gumpId );
        }

        [CommandsDisplay( Category = nameof( Strings.Gumps ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.String ) } )]
        public static bool InGump( uint gumpId, string text )
        {
            if ( Engine.Gumps.GetGump( gumpId, out Gump gump ) )
            {
                return gump.GumpElements.Any( ge => ge.Text != null && ge.Text.ToLower().Contains( text.ToLower() ) );
            }

            UOC.SystemMessage( Strings.Invalid_gump___ );
            return false;
        }

        [CommandsDisplay( Category = nameof( Strings.Gumps ), Parameters = new[] { nameof( ParameterType.Serial ) } )]
        public static void CloseGump( int serial )
        {
            if ( Engine.Gumps.FindGump( serial, out Gump gump ) )
            {
                gump.CloseGump();
            }
        }
    }
}