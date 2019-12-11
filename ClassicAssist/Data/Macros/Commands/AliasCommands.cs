using System.Collections.Generic;
using Assistant;
using ClassicAssist.Resources;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class AliasCommands
    {
        public static Dictionary<string, int> _aliases = new Dictionary<string, int>();

        internal static void SetDefaultAliases()
        {
            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                return;
            }

            SetAlias( "bank", player.GetLayer( Layer.Bank ) );
            SetAlias( "backpack", player.GetLayer( Layer.Backpack ) );
            SetAlias( "self", player.Serial );
        }

        internal static int ResolveSerial( object obj )
        {
            int serial;

            switch ( obj )
            {
                case string str:
                    serial = GetAlias( str );

                    if ( serial == -1 )
                    {
                        UOC.SystemMessage( string.Format( Strings.Unknown_alias___0___, str ) );
                    }

                    break;
                case int i:
                    serial = i;
                    break;
                case uint i:
                    serial = (int) i;
                    break;
                case null:
                    serial = Engine.Player == null ? 0 : Engine.Player.Serial;

                    break;
                default:
                    UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                    return -1;
            }

            return serial;
        }

        [CommandsDisplay( Category = "Aliases", Description = "Sets the value of the given alias name.",
            InsertText = "SetAlias(\"mount\", 0x40000001\")" )]
        public static void SetAlias( string aliasName, int value )
        {
            if ( _aliases.ContainsKey( aliasName ) )
            {
                _aliases[aliasName] = value;
            }
            else
            {
                _aliases.Add( aliasName, value );
            }
        }

        [CommandsDisplay( Category = "Aliases", Description = "Removes the alias name given.",
            InsertText = "UnsetAlias(\"mount\")" )]
        public static void UnsetAlias( string aliasName )
        {
            if ( _aliases.ContainsKey( aliasName ) )
            {
                _aliases.Remove( aliasName );
            }
        }

        [CommandsDisplay( Category = "Aliases", Description = "Gets the value of the given alias name.",
            InsertText = "GetAlias(\"mount\")" )]
        public static int GetAlias( string aliasName )
        {
            if ( _aliases.ContainsKey( aliasName ) )
            {
                return _aliases[aliasName];
            }

            return -1;
        }

        internal static Dictionary<string, int> GetAllAliases()
        {
            return _aliases;
        }

        [CommandsDisplay( Category = "Aliases",
            Description = "Prompt with an in-game target cursor to supply value for given alias name.",
            InsertText = "PromptAlias(\"mount\")" )]
        public static void PromptAlias( string aliasName )
        {
            int serial = UOC.GetTargeSerialAsync( Strings.Target_object___ ).Result;
            SetAlias( aliasName, serial );
        }

        [CommandsDisplay( Category = "Aliases",
            Description = "Returns true if alias serial can be found on screen, false if not.",
            InsertText = "if FindAlias(\"mount\"):" )]
        public static bool FindAlias( string aliasName )
        {
            int serial;

            if ( ( serial = GetAlias( aliasName ) ) == -1 )
            {
                return false;
            }

            if ( UOMath.IsMobile( serial ) )
            {
                return Engine.Mobiles.GetMobile( serial ) != null;
            }

            return Engine.Items.GetItem( serial ) != null;
        }
    }
}