using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class MovementRequest : BasePacket, IMacroCommandParser
    {
        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( packet[0] != 0x02 || direction != PacketDirection.Outgoing )
            {
                return null;
            }

            Direction d = (Direction) packet[1];

            return $"Walk(\"{d.ToString()}\")\r\n";
        }
    }
}