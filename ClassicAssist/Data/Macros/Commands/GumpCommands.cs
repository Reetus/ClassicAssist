using System.Linq;
using Assistant;
using ClassicAssist.Resources;
using ClassicAssist.UO.Objects.Gumps;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class GumpCommands
    {
        [CommandsDisplay( Category = "Gumps",
            Description = "Pauses until incoming gump packet is received, optional paramters of gump ID and timeout",
            InsertText = "WaitForGump(0xff, 5000)" )]
        public static bool WaitForGump( uint gumpId = 0, int timeout = 30000 )
        {
            bool result = UOC.WaitForGump( gumpId, timeout );

            if ( !result )
            {
                UOC.SystemMessage( Strings.Timeout___ );
            }

            return result;
        }

        [CommandsDisplay( Category = "Gumps",
            Description = "Sends a button reply to server gump, parameters are gumpID and buttonID.",
            InsertText = "ReplyGump(0xff, 0)" )]
        public static void ReplyGump( uint gumpId, int buttonId )
        {
            UOC.GumpButtonClick( gumpId, buttonId );
        }

        [CommandsDisplay( Category = "Gumps", Description = "Checks if a gump id exists or not.",
            InsertText = "if GumpExists(0xff):" )]
        public static bool GumpExists( uint gumpId )
        {
            return Engine.GumpList.ContainsKey( gumpId );
        }

        [CommandsDisplay( Category = "Gumps", Description = "Check for a text in gump.",
            InsertText = "if InGump(0xf00f, \"lethal darts\"):" )]
        public static bool InGump( uint gumpId, string text )
        {
            if ( Engine.Gumps.GetGump( gumpId, out Gump gump ) )
            {
                return gump.GumpElements.Any( ge => ge.Text != null && ge.Text.ToLower().Contains( text.ToLower() ) );
            }

            UOC.SystemMessage( Strings.Invalid_gump___ );
            return false;
        }

        [CommandsDisplay( Category = "Gumps", Description = "Close a specified gump serial",
            InsertText = "CloseGump(0x454ddef)" )]
        public static void CloseGump( int serial )
        {
            if ( Engine.Gumps.FindGump( serial, out Gump gump ) )
            {
                gump.CloseGump();
            }
        }
    }
}