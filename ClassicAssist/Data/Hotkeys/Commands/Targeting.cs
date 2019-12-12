using Assistant;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Data.Targeting;
using ClassicAssist.UO;
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
                TargetCommands.Target( "enemy" );
            }
        }

        [HotkeyCommand( Name = "Target Last", Category = "Targeting" )]
        public class TargetLastCommand : HotkeyCommand
        {
            public override void Execute()
            {
                TargetCommands.Target( "last" );
            }
        }

        [HotkeyCommand( Name = "Target Friend", Category = "Targeting" )]
        public class TargetFriendCommand : HotkeyCommand
        {
            public override void Execute()
            {
                TargetCommands.Target( "friend" );
            }
        }
    }
}