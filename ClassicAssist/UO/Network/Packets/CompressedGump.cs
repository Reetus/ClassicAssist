using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class CompressedGump : IMacroCommandParser
    {
        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( packet[0] != 0xDD || direction != PacketDirection.Incoming )
            {
                return null;
            }

            int serial = ( packet[7] << 24 ) | ( packet[8] << 16 ) | ( packet[9] << 8 ) | packet[10];

            return $"WaitForGump(0x{serial:x}, 5000)\r\n";
        }
    }
}