using Assistant;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Data.Targeting;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

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
    }
}