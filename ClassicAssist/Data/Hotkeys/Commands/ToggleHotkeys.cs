using ClassicAssist.Resources;
using UOC = ClassicAssist.UO.Commands;

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
            HotkeyManager manager = HotkeyManager.GetInstance();

            manager.Enabled = !manager.Enabled;

            UOC.SystemMessage( manager.Enabled ? Strings.Hotkeys_enabled___ : Strings.Hotkeys_disabled___, 0x3F );
        }
    }
}