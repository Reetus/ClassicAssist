using ClassicAssist.Data.Macros.Commands;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Name = "Toggle hotkeys" )]
    public sealed class ToggleHotkeys : HotkeyCommand
    {
        public ToggleHotkeys()
        {
            Disableable = false;
        }

        public override void Execute()
        {
            MainCommands.Hotkeys();
        }
    }
}