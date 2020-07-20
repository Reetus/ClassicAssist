using ClassicAssist.UO.Network;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Name = "Clear Object Queue" )]
    public class ClearObjectQueue : HotkeyCommand
    {
        public override void Execute()
        {
            ActionPacketQueue.Clear();
        }
    }
}