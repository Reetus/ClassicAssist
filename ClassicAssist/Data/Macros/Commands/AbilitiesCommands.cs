using Assistant;
using ClassicAssist.Data.Abilities;
using ClassicAssist.Shared.Resources;
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
            UOC.ClearWeaponAbility();
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
                            UOC.SystemMessage( Strings.Ability_already_set___, (int) SystemMessageHues.Green,
                                true );

                            return;
                        }

                        break;
                    }
                    case "off":
                    {
                        if ( primary && !manager.IsPrimaryEnabled || !primary && !manager.IsSecondaryEnabled )
                        {
                            UOC.SystemMessage( Strings.Ability_not_set___, (int) SystemMessageHues.Green, true );

                            return;
                        }

                        break;
                    }
                }
            }

            if ( onOffNormalized == "off" || onOffNormalized == "toggle" &&
                ( manager.IsPrimaryEnabled || manager.IsSecondaryEnabled ) )
            {
                UOC.ClearWeaponAbility();
            }
            else
            {
                UOC.SystemMessage( string.Format( Strings.Setting_ability___0_____, ability ),
                    (int) SystemMessageHues.Green );
                manager.SetAbility( primary ? AbilityType.Primary : AbilityType.Secondary );
            }
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
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
                return false;
            }

            Mobile mobile = Engine.Mobiles.GetMobile( serial );

            if ( mobile == null )
            {
                // TODO better message
                UOC.SystemMessage( Strings.Cannot_find_item___, true );
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