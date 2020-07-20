using UOC = ClassicAssist.Shared.UO.Commands;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Name = "Resync" )]
    public class Resync : HotkeyCommand
    {
        public override void Execute()
        {
            UOC.Resync();
        }
    }
}