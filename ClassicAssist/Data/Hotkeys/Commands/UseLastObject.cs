using Assistant;
using ClassicAssist.UO.Network.Packets;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Name = "Use Last Object" )]
    public class UseLastObject : HotkeyCommand
    {
        public override void Execute()
        {
            int serial = Engine.Player?.LastObjectSerial ?? 0;

            if ( serial != 0 )
            {
                Engine.SendPacketToServer( new UseObject( serial ) );
            }
        }
    }
}