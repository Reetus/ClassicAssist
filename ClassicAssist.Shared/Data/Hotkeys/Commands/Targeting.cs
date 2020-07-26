using ClassicAssist.Shared;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Data.Targeting;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.Shared.UO.Commands;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    public class Targeting
    {
        [HotkeyCommand( Name = "Set Enemy", Category = "Targeting" )]
        public class SetEnemyCommand : HotkeyCommand
        {
            public override void Execute()
            {
                TargetManager manager = TargetManager.GetInstance();

                Entity mobile = manager.PromptTarget();

                if ( mobile == null )
                {
                    return;
                }

                manager.SetEnemy( mobile );
                Engine.SendPacketToServer( new LookRequest( mobile.Serial ) );
            }
        }

        [HotkeyCommand( Name = "Set Friend", Category = "Targeting" )]
        public class SetFriendCommand : HotkeyCommand
        {
            public override void Execute()
            {
                TargetManager manager = TargetManager.GetInstance();

                Entity mobile = manager.PromptTarget();

                if ( mobile == null )
                {
                    return;
                }

                manager.SetFriend( mobile );
                Engine.SendPacketToServer( new LookRequest( mobile.Serial ) );
            }
        }

        [HotkeyCommand( Name = "Set Last Target", Category = "Targeting" )]
        public class SetLastTargetCommand : HotkeyCommand
        {
            public override void Execute()
            {
                TargetManager manager = TargetManager.GetInstance();

                Entity entity = manager.PromptTarget();

                if ( entity == null )
                {
                    return;
                }

                manager.SetLastTarget( entity );

                if ( UOMath.IsMobile( entity.Serial ) )
                {
                    Engine.SendPacketToServer( new LookRequest( entity.Serial ) );
                }
            }
        }

        [HotkeyCommand( Name = "Target Enemy", Category = "Targeting" )]
        public class TargetEnemyCommand : HotkeyCommand
        {
            public override void Execute()
            {
                TargetCommands.Target( "enemy", Options.CurrentOptions.RangeCheckLastTarget,
                    Options.CurrentOptions.QueueLastTarget );
            }
        }

        [HotkeyCommand( Name = "Target Last", Category = "Targeting" )]
        public class TargetLastCommand : HotkeyCommand
        {
            public override void Execute()
            {
                if ( Options.CurrentOptions.SmartTargetOption == SmartTargetOption.None )
                {
                    TargetCommands.Target( "last", Options.CurrentOptions.RangeCheckLastTarget,
                        Options.CurrentOptions.QueueLastTarget );
                }
                else
                {
                    if ( Engine.TargetExists )
                    {
                        if ( Options.CurrentOptions.SmartTargetOption.HasFlag( SmartTargetOption.Friend ) &&
                             Engine.TargetFlags == TargetFlags.Beneficial && AliasCommands.FindAlias( "friend" ) )
                        {
                            TargetCommands.Target( "friend", Options.CurrentOptions.RangeCheckLastTarget,
                                Options.CurrentOptions.QueueLastTarget );
                            return;
                        }

                        if ( Options.CurrentOptions.SmartTargetOption.HasFlag( SmartTargetOption.Enemy ) &&
                             Engine.TargetFlags == TargetFlags.Harmful && AliasCommands.FindAlias( "enemy" ) )
                        {
                            TargetCommands.Target( "enemy", Options.CurrentOptions.RangeCheckLastTarget,
                                Options.CurrentOptions.QueueLastTarget );
                            return;
                        }
                    }

                    TargetCommands.Target( "last", Options.CurrentOptions.RangeCheckLastTarget,
                        Options.CurrentOptions.QueueLastTarget );
                }
            }
        }

        [HotkeyCommand( Name = "Target Friend", Category = "Targeting" )]
        public class TargetFriendCommand : HotkeyCommand
        {
            public override void Execute()
            {
                TargetCommands.Target( "friend", Options.CurrentOptions.RangeCheckLastTarget,
                    Options.CurrentOptions.QueueLastTarget );
            }
        }

        [HotkeyCommand( Name = "Target Self", Category = "Targeting" )]
        public class TargetSelfCommand : HotkeyCommand
        {
            public override void Execute()
            {
                TargetCommands.Target( "self", Options.CurrentOptions.RangeCheckLastTarget,
                    Options.CurrentOptions.QueueLastTarget );
            }
        }

        [HotkeyCommand( Name = "Clear Target Queue", Category = "Targeting" )]
        public class ClearTargetQueueCommand : HotkeyCommand
        {
            public override void Execute()
            {
                TargetCommands.ClearTargetQueue();
            }
        }

        [HotkeyCommand( Name = "Attack Last", Category = "Targeting" )]
        public class AttackLastCommand : HotkeyCommand
        {
            public override void Execute()
            {
                ActionCommands.Attack( "last" );
            }
        }

        [HotkeyCommand( Name = "Attack Enemy", Category = "Targeting" )]
        public class AttackEnemyCommand : HotkeyCommand
        {
            public override void Execute()
            {
                ActionCommands.Attack( "enemy" );
            }
        }

        [HotkeyCommand( Name = "Show Next Target In Queue", Category = "Targeting" )]
        public class ShowNextTargetQueue : HotkeyCommand
        {
            public override void Execute()
            {
                if ( !Options.CurrentOptions.QueueLastTarget )
                {
                    UOC.SystemMessage( Strings.Target_queue_is_not_enabled___ );
                    return;
                }

                object nextTarget = Engine.LastTargetQueue.Peek();

                switch ( nextTarget )
                {
                    case string targetAlias:
                        MsgCommands.HeadMsg( string.Format( Strings.Next_Target___0_, targetAlias ),
                            Engine.Player?.Serial );
                        break;
                    case int targetSerial:
                    {
                        Mobile entity = Engine.Mobiles.GetMobile( targetSerial );

                        MsgCommands.HeadMsg(
                            string.Format( Strings.Next_Target___0_,
                                $"0x{targetSerial:x} - {entity?.Name ?? "Unknown"}" ), Engine.Player?.Serial );
                        break;
                    }
                }
            }
        }
    }
}