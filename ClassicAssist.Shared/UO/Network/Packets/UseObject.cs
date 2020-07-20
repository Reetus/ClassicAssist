using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class UseObject : BasePacket, IMacroCommandParser
    {
        public UseObject()
        {
        }

        public UseObject( int serial )
        {
            _writer = new PacketWriter( 5 );
            _writer.Write( (byte) 0x06 );
            _writer.Write( serial );
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( packet[0] != 0x06 || direction != PacketDirection.Outgoing )
            {
                return null;
            }

            int serial = ( packet[1] << 24 ) | ( packet[2] << 16 ) | ( packet[3] << 8 ) | packet[4];

            return $"UseObject(0x{serial:x})\r\n";
        }
    }
}