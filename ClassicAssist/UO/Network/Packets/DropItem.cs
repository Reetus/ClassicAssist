using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class DropItem : BasePacket, IMacroCommandParser
    {
        public DropItem()
        {
        }

        public DropItem( int serial, int containerSerial, int x, int y, int z )
        {
            _writer = new PacketWriter( 15 );
            _writer.Write( (byte) 0x08 );
            _writer.Write( serial );
            _writer.Write( (short) x );
            _writer.Write( (short) y );
            _writer.Write( (sbyte) z );
            _writer.Write( (byte) 0 );
            _writer.Write( containerSerial );
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( packet[0] != 0x08 || direction != PacketDirection.Outgoing )
            {
                return null;
            }

            int serial = ( packet[1] << 24 ) | ( packet[2] << 16 ) | ( packet[3] << 8 ) | packet[4];
            int containerSerial = ( packet[11] << 24 ) | ( packet[12] << 16 ) | ( packet[13] << 8 ) | packet[14];

            if ( serial == 0 || containerSerial == 0 )
            {
                return null;
            }

            return $"MoveItem(0x{serial:x}, 0x{containerSerial:x})\r\n";
        }
    }
}