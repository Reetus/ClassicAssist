using System.Collections.Generic;
using Assistant;
using ClassicAssist.Shared.Resources;
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

        internal static int ResolveSerial( object obj, bool defaultSelf = true )
        {
            int serial;

            switch ( obj )
            {
                case string str:
                    serial = GetAlias( str );

                    if ( serial == 0 )
                    {
                        UOC.SystemMessage( string.Format( Strings.Unknown_alias___0___, str ), true );
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
                case null when defaultSelf:
                    serial = Engine.Player == null ? 0 : Engine.Player.Serial;

                    break;
                default:
                    UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
                    return 0;
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

            return 0;
        }

        internal static Dictionary<string, int> GetAllAliases()
        {
            return _aliases;
        }

        [CommandsDisplay( Category = nameof( Strings.Aliases ),
            Parameters = new[] { nameof( ParameterType.AliasName ) } )]
        public static int PromptAlias( string aliasName )
        {
            int serial = UOC.GetTargetSerialAsync( string.Format( Strings.Target_object___0_____, aliasName ) ).Result;
            SetAlias( aliasName, serial );
            return serial;
        }

        [CommandsDisplay( Category = nameof( Strings.Aliases ),
            Parameters = new[] { nameof( ParameterType.AliasName ) } )]
        public static int PromptMacroAlias(string aliasName)
        {
            int serial = UOC.GetTargetSerialAsync(string.Format(Strings.Target_object___0_____, aliasName)).Result;
            SetMacroAlias(aliasName, serial);
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