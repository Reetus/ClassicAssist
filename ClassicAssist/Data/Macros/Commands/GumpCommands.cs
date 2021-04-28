using System.Collections.Generic;
using System.Linq;
using Assistant;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO.Gumps;
using ClassicAssist.UO.Network.Packets;
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
            Parameters = new[]
            {
                nameof( ParameterType.ItemID ), nameof( ParameterType.GumpButtonIndex ),
                nameof( ParameterType.IntegerValue )
            } )]
        public static void ReplyGump( uint gumpId, int buttonId, int[] switches = null )
        {
            UOC.GumpButtonClick( gumpId, buttonId, switches );
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
                return gump.Pages.Any( gumpPage => gumpPage.GumpElements.Any( ge =>
                    ge.Text != null && ge.Text.ToLower().Contains( text.ToLower() ) ) );
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

        [CommandsDisplay( Category = nameof( Strings.Gumps ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static void OpenVirtueGump( object obj = null )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                if ( !MacroManager.QuietMode )
                {
                    UOC.SystemMessage( Strings.Mobile_not_found___ );
                }

                return;
            }

            Engine.SendPacketToServer( new GumpButtonClick( 0x1CD, Engine.Player.Serial, 1, new[] { serial } ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Gumps ) )]
        public static void OpenGuildGump()
        {
            Engine.SendPacketToServer( new GuildButtonRequest() );
        }

        [CommandsDisplay( Category = nameof( Strings.Gumps ) )]
        public static void OpenQuestsGump()
        {
            Engine.SendPacketToServer( new QuestsButtonRequest() );
        }

        [CommandsDisplay( Category = nameof( Strings.Gumps ) )]
        public static bool ConfirmPrompt( string message, bool closable = false )
        {
            return ConfirmPromptGump.ConfirmPrompt( message, closable );
        }

        [CommandsDisplay( Category = nameof( Strings.Gumps ),
            Parameters = new[]
            {
                nameof( ParameterType.StringArray ), nameof( ParameterType.String ), nameof( ParameterType.Boolean )
            } )]
        public static (bool Result, int index) SelectionPrompt( IEnumerable<string> options,
            string message = "Please choose an option...", bool closable = false )
        {
            IEnumerable<string> enumerable = options as string[] ?? options.ToArray();

            if ( enumerable.Any() )
            {
                return SelectionPromptGump.SelectionPrompt( enumerable, message, closable );
            }

            UOC.SystemMessage( Strings.Atleast_one_option_must_be_provided___, (int) UOC.SystemMessageHues.Red );
            return ( false, 0 );
        }

        [CommandsDisplay( Category = nameof( Strings.Gumps ),
            Parameters = new[]
            {
                nameof( ParameterType.String ), nameof( ParameterType.String ), nameof( ParameterType.Boolean )
            } )]
        public static (bool Result, string Message) MessagePrompt( string message, string initialText = "",
            bool closable = false )
        {
            return MessagePromptGump.MessagePrompt( message, initialText, closable );
        }
    }
}