using Assistant;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Name = "Clear Object Queue" )]
    public class ClearObjectQueue : HotkeyCommand
    {
        public override void Execute()
        {
            Engine.UseObjectQueue?.Clear();
        }
    }
}