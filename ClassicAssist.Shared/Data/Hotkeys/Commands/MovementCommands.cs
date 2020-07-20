using ClassicAssist.Data.Macros.Commands;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Category = "Movement", Name = "Toggle Force Walk" )]
    public class ToggleForceWalk : HotkeyCommand
    {
        public override void Execute()
        {
            MovementCommands.ToggleForceWalk();
        }
    }

    [HotkeyCommand( Category = "Movement", Name = "Force Walk On" )]
    public class SetForceWalkOn : HotkeyCommand
    {
        public override void Execute()
        {
            MovementCommands.SetForceWalk( true );
        }
    }

    [HotkeyCommand( Category = "Movement", Name = "Force Walk Off" )]
    public class SetForceWalkOff : HotkeyCommand
    {
        public override void Execute()
        {
            MovementCommands.SetForceWalk( false );
        }
    }
}