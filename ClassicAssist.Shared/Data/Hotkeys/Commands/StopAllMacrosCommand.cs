using ClassicAssist.Data.Macros;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Name = "Stop All Macros",
        Tooltip = "Stops all running macros including background and autostart macros." )]
    public class StopAllMacros : HotkeyCommand
    {
        public override void Execute()
        {
            MacroManager manager = MacroManager.GetInstance();
            manager.StopAll();
        }
    }
}