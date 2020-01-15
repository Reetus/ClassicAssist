using ClassicAssist.Data.Macros.Commands;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Name = "WarMode On" )]
    public class WarModeOnCommand : HotkeyCommand
    {
        public override void Execute()
        {
            MainCommands.WarMode( "on" );
        }
    }

    [HotkeyCommand( Name = "WarMode Off" )]
    public class WarModeOffCommand : HotkeyCommand
    {
        public override void Execute()
        {
            MainCommands.WarMode( "off" );
        }
    }

    [HotkeyCommand( Name = "WarMode Toggle" )]
    public class WarModeCommand : HotkeyCommand
    {
        public override void Execute()
        {
            MainCommands.WarMode();
        }
    }
}