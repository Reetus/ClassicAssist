using ClassicAssist.Data.Macros;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Name = "Stop Current Macro" )]
    public class StopCurrentMacro : HotkeyCommand
    {
        public override void Execute()
        {
            MacroManager manager = MacroManager.GetInstance();
            manager.CurrentMacro()?.Stop();
        }
    }
}