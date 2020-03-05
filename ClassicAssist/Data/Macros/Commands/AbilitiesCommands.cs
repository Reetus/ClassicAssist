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
        [CommandsDisplay( Category = "Abilities", Description = "Clear weapon ability.",
            InsertText = "ClearAbility()" )]
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

        [CommandsDisplay( Category = "Abilities",
            Description = "Set weapon ability, parameter \"primary\" / \"secondary\".",
            InsertText = "SetAbility(\"primary\")" )]
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

        [CommandsDisplay( Category = "Abilities", Description = "(Garoyle) Start flying if not already flying.",
            InsertText = "Fly()" )]
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

        [CommandsDisplay( Category = "Abilities", Description = "Returns true if mobile is currently flying.",
            InsertText = "if Flying(\"self\"):" )]
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

        [CommandsDisplay( Category = "Abilities", Description = "(Garoyle) Stop flying if currently flying.",
            InsertText = "Land()" )]
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