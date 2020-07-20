using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class EquipRequest : BasePacket, IMacroCommandParser
    {
        public EquipRequest()
        {
        }

        public EquipRequest( int serial, Layer layer, int containerSerial )
        {
            _writer = new PacketWriter( 10 );

            _writer.Write( (byte) 0x13 );
            _writer.Write( serial );
            _writer.Write( (byte) layer );
            _writer.Write( containerSerial );
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( packet[0] != 0x13 || direction != PacketDirection.Outgoing )
            {
                return null;
            }

            int serial = ( packet[1] << 24 ) | ( packet[2] << 16 ) | ( packet[3] << 8 ) | packet[4];
            Layer layer = (Layer) packet[5];

            return $"EquipItem(0x{serial:x}, \"{layer.ToString()}\")\r\n";
        }
    }
}