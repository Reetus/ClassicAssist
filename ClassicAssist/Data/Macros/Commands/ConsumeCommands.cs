using System.Linq;
using Assistant;
using ClassicAssist.Resources;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using static ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class ConsumeCommands
    {
        private const int TIMEOUT = 5000;
        private static readonly int[] _bandageTypes = { 0xe21 };

        [CommandsDisplay( Category = nameof( Strings.Actions ) )]
        public static bool BandageSelf()
        {
            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                SystemMessage( Strings.Error__No_Player );
                return false;
            }

            Item backpack = player.Backpack;

            if ( backpack == null )
            {
                SystemMessage( Strings.Error__Cannot_find_player_backpack );
                return false;
            }

            Item bandage = backpack.Container?.SelectEntity( i => _bandageTypes.Contains( i.ID ) );

            if ( bandage == null )
            {
                SystemMessage( Strings.Error__Cannot_find_type );
                return false;
            }

            ObjectCommands.UseObject( bandage.Serial );

            PacketWaitEntry we = Engine.PacketWaitEntries.Add( new PacketFilterInfo( 0x6C ), PacketDirection.Incoming );

            bool result = we.Lock.WaitOne( TIMEOUT );

            if ( !result )
            {
                SystemMessage( Strings.Target_timeout___ );
                return false;
            }

            byte[] packet = we.Packet;

            int senderSerial = ( packet[2] << 24 ) | ( packet[3] << 16 ) | ( packet[4] << 8 ) | packet[5];

            Engine.SendPacketToServer( new Target( senderSerial, player, true ) );

            return true;
        }
    }
}