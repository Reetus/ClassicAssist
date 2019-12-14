using ClassicAssist.Data.Macros.Commands;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Name = "Add Friend", Category = "Friends" )]
    public class AddFriend : HotkeyCommand
    {
        public override void Execute()
        {
            MobileCommands.AddFriend();
        }
    }

    [HotkeyCommand( Name = "Remove Friend", Category = "Friends" )]
    public class RemoveFriend : HotkeyCommand
    {
        public override void Execute()
        {
            MobileCommands.RemoveFriend();
        }
    }
}