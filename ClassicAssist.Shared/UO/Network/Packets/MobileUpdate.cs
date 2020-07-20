using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class MobileUpdate : BasePacket
    {
        public MobileUpdate( int serial, int id, int hue, MobileStatus flags, int x, int y, int z, Direction direction )
        {
            _writer = new PacketWriter( 19 );
            _writer.Write( (byte) 0x20 );
            _writer.Write( serial );
            _writer.Write( (short) id );
            _writer.Write( (byte) 0 );
            _writer.Write( (short) hue );
            _writer.Write( (byte) flags );
            _writer.Write( (short) x );
            _writer.Write( (short) y );
            _writer.Write( (short) 0 );
            _writer.Write( (byte) direction );
            _writer.Write( (sbyte) z );
        }
    }
}