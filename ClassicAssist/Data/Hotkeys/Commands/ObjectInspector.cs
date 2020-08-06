using static ClassicAssist.Shared.UO.Commands;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Name = "Object Inspector" )]
    public class ObjectInspector : HotkeyCommand
    {
        public override async void Execute()
        {
            await InspectObjectAsync();
        }
    }
}