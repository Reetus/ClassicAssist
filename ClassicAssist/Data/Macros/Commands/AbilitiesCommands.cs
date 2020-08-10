using Assistant;
using ClassicAssist.Data.Abilities;
using ClassicAssist.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class AbilitiesCommands
    {
        [CommandsDisplay( Category = nameof( Strings.Abilities ) )]
        public static void ClearAbility()
        {
            AbilitiesManager manager = AbilitiesManager.GetInstance();

            if ( manager.IsPrimaryEnabled )
            {
                manager.SetAbility( AbilityType.Primary );
            }
            else if ( manager.IsSecondaryEnabled )
            {
                manager.SetAbility( AbilityType.Secondary );
            }
        }

        [CommandsDisplay( Category = nameof( Strings.Abilities ) )]
        public static bool ActiveAbility()
        {
            AbilitiesManager manager = AbilitiesManager.GetInstance();

            bool result = false;

            if ( manager.IsPrimaryEnabled )
            {
                result = true;
            }
            else if ( manager.IsSecondaryEnabled )
            {
                result = true;
            }

            return result;
        }

        [CommandsDisplay( Category = nameof( Strings.Abilities ),
            Parameters = new[] { nameof( ParameterType.Ability ), nameof( ParameterType.OnOff ) } )]
        public static void SetAbility( string ability, string onOff = "toggle" )
        {
            AbilitiesManager manager = AbilitiesManager.GetInstance();

            if ( ability.ToLower().Equals( "stun" ) )
            {
                Engine.SendPacketToServer( new StunRequest() );
                return;
            }

            if ( ability.ToLower().Equals( "disarm" ) )
            {
                Engine.SendPacketToServer( new DisarmRequest() );
                return;
            }

            bool primary;

            switch ( ability.ToLower() )
            {
                case "primary":
                    primary = true;
                    break;
                default:
                    primary = false;
                    break;
            }

            string onOffNormalized = onOff.Trim().ToLower();

            if ( onOffNormalized != "toggle" )
            {
                switch ( onOffNormalized )
                {
                    case "on":
                    {
                        if ( primary && manager.IsPrimaryEnabled || !primary && manager.IsSecondaryEnabled )
                        {
                            if ( !MacroManager.QuietMode )
                            {
                                UOC.SystemMessage( Strings.Ability_already_set___, 0x3F );
                            }

                            return;
                        }

                        break;
                    }
                    case "off":
                    {
                        if ( primary && !manager.IsPrimaryEnabled || !primary && !manager.IsSecondaryEnabled )
                        {
                            if ( !MacroManager.QuietMode )
                            {
                                UOC.SystemMessage( Strings.Ability_not_set___, 0x3F );
                            }

                            return;
                        }

                        break;
                    }
                }
            }

            UOC.SystemMessage( string.Format( Strings.Setting_ability___0_____, ability ), 0x3F );
            manager.SetAbility( primary ? AbilityType.Primary : AbilityType.Secondary );
        }

        [CommandsDisplay( Category = nameof( Strings.Abilities ) )]
        public static void Fly()
        {
            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                return;
            }

            if ( !player.Status.HasFlag( MobileStatus.Flying ) )
            {
                UOC.ToggleGargoyleFlying();
            }
        }

        [CommandsDisplay( Category = nameof( Strings.Abilities ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static bool Flying( object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return false;
            }

            Mobile mobile = Engine.Mobiles.GetMobile( serial );

            if ( mobile == null )
            {
                // TODO better message
                UOC.SystemMessage( Strings.Cannot_find_item___ );
                return false;
            }

            return mobile.Status.HasFlag( MobileStatus.Flying );
        }

        [CommandsDisplay( Category = nameof( Strings.Abilities ) )]
        public static void Land()
        {
            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                return;
            }

            if ( player.Status.HasFlag( MobileStatus.Flying ) )
            {
                UOC.ToggleGargoyleFlying();
            }
        }
    }
}