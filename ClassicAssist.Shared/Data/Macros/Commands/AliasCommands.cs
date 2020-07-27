using System.Collections.Generic;
using ClassicAssist.Shared;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.Shared.UO.Commands;

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

                    if ( serial == -1 && !MacroManager.QuietMode )
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
                case Entity i:
                    serial = i.Serial;
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

        [CommandsDisplay( Category = nameof( Strings.Aliases ),
            Parameters = new[] { nameof( ParameterType.AliasName ), nameof( ParameterType.SerialOrAlias ) } )]
        public static void SetAlias( string aliasName, object obj )
        {
            aliasName = aliasName.ToLower();

            int value = ResolveSerial( obj );

            if ( _aliases.ContainsKey( aliasName ) )
            {
                _aliases[aliasName] = value;
            }
            else
            {
                _aliases.Add( aliasName, value );
            }
        }

        [CommandsDisplay( Category = nameof( Strings.Aliases ),
            Parameters = new[] { nameof( ParameterType.AliasName ), nameof( ParameterType.SerialOrAlias ) } )]
        public static void SetMacroAlias( string aliasName, object obj )
        {
            aliasName = aliasName.ToLower();

            int value = ResolveSerial( obj );

            MacroEntry macro = MacroManager.GetInstance().GetCurrentMacro();

            if ( macro == null )
            {
                SetAlias( aliasName, obj );
                return;
            }

            if ( macro.Aliases.ContainsKey( aliasName ) )
            {
                macro.Aliases[aliasName] = value;
            }
            else
            {
                macro.Aliases.Add( aliasName, value );
            }
        }

        [CommandsDisplay( Category = nameof( Strings.Aliases ),
            Parameters = new[] { nameof( ParameterType.AliasName ) } )]
        public static void UnsetAlias( string aliasName )
        {
            aliasName = aliasName.ToLower();

            MacroEntry macro = MacroManager.GetInstance().GetCurrentMacro();

            if ( macro != null )
            {
                if ( macro.Aliases.ContainsKey( aliasName ) )
                {
                    macro.Aliases.Remove( aliasName );
                }
            }

            if ( _aliases.ContainsKey( aliasName ) )
            {
                _aliases.Remove( aliasName );
            }
        }

        [CommandsDisplay( Category = nameof( Strings.Aliases ),
            Parameters = new[] { nameof( ParameterType.AliasName ) } )]
        public static int GetAlias( string aliasName )
        {
            aliasName = aliasName.ToLower();

            MacroEntry macro = MacroManager.GetInstance().GetCurrentMacro();

            if ( macro != null )
            {
                if ( macro.Aliases.ContainsKey( aliasName ) )
                {
                    return macro.Aliases[aliasName];
                }
            }

            if ( _aliases.ContainsKey( aliasName ) )
            {
                return _aliases[aliasName];
            }

            return -1;
        }

        public static Dictionary<string, int> GetAllAliases()
        {
            return _aliases;
        }

        [CommandsDisplay( Category = nameof( Strings.Aliases ),
            Parameters = new[] { nameof( ParameterType.AliasName ) } )]
        public static int PromptAlias( string aliasName )
        {
            int serial = UOC.GetTargeSerialAsync( string.Format( Strings.Target_object___0_____, aliasName ) ).Result;
            SetAlias( aliasName, serial );
            return serial;
        }

        [CommandsDisplay( Category = nameof( Strings.Aliases ),
            Parameters = new[] { nameof( ParameterType.AliasName ) } )]
        public static bool FindAlias( string aliasName )
        {
            aliasName = aliasName.ToLower();

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