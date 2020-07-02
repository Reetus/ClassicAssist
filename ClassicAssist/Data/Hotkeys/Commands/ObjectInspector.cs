namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Name = "Object Inspector" )]
    public class ObjectInspector : HotkeyCommand
    {
        public override async void Execute()
        {
            await UO.Commands.InspectObjectAsync();
        }
    }
}