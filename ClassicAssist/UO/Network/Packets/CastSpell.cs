using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class CastSpell : BasePacket
    {
        public CastSpell( int id )
        {
            _writer = new PacketWriter( 9 );
            _writer.Write( (byte) 0xBF );
            _writer.Write( (short) 9 );
            _writer.Write( (short) 0x1C );
            _writer.Write( (short) 0x02 );
            _writer.Write( (short) id );
        }
    }
}