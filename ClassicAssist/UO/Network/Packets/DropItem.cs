using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class DropItem : BasePacket
    {
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
    }
}