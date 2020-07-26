using ClassicAssist.Shared;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.Shared.UO.Commands;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    public static class UseHands
    {
        public static void UseHand( Layer layer )
        {
            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                return;
            }

            int serial = player.GetLayer( layer );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_layer_value___ );
                return;
            }

            ObjectCommands.UseObject( serial );
        }

        [HotkeyCommand( Name = "Use Left Hand" )]
        public class UseLeftHand : HotkeyCommand
        {
            public override void Execute()
            {
                UseHand( Layer.OneHanded );
            }
        }

        [HotkeyCommand( Name = "Use Right Hand" )]
        public class UseRightHand : HotkeyCommand
        {
            public override void Execute()
            {
                UseHand( Layer.TwoHanded );
            }
        }
    }
}