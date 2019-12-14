using System.Linq;
using Assistant;
using ClassicAssist.Resources;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Name = "Bandage Self" )]
    public class BandageSelf : HotkeyCommand
    {
        private const int TIMEOUT = 5000;
        private readonly int[] _bandageTypes = { 0xe21 };

        public override void Execute()
        {
            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                UOC.SystemMessage( Strings.Error__No_Player );
                return;
            }

            Item backpack = player.Backpack;

            if ( backpack == null )
            {
                UOC.SystemMessage( Strings.Error__Cannot_find_player_backpack );
                return;
            }

            Item bandage = backpack.Container.SelectEntity( i => _bandageTypes.Contains( i.ID ) );

            if ( bandage == null )
            {
                UOC.SystemMessage( Strings.Error__Cannot_find_type );
                return;
            }

            Engine.SendPacketToServer( new UseObject( bandage.Serial ) );

            PacketWaitEntry we = Engine.PacketWaitEntries.Add( new PacketFilterInfo( 0x6C ), PacketDirection.Incoming );

            try
            {
                bool result = we.Lock.WaitOne( TIMEOUT );

                if ( !result )
                {
                    UOC.SystemMessage( Strings.Target_timeout___ );
                    return;
                }

                byte[] packet = we.Packet;

                int senderSerial = ( packet[2] << 24 ) | ( packet[3] << 16 ) | ( packet[4] << 8 ) | packet[5];

                Engine.SendPacketToServer( new Target( senderSerial, player, true ) );
            }
            finally
            {
                Engine.PacketWaitEntries.Remove( we );
            }
        }
    }
}