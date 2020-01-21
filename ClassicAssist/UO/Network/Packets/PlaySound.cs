using Assistant;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class PlaySound : BasePacket
    {
        public PlaySound( int id )
        {
            _writer = new PacketWriter( 12 );
            _writer.Write( (byte) 0x54 );
            _writer.Write( (byte) 0x01 );
            _writer.Write( (short) id );
            _writer.Write( (short) 0 );
            _writer.Write( (short) ( Engine.Player?.X ?? 0 ) );
            _writer.Write( (short) ( Engine.Player?.Y ?? 0 ) );
            _writer.Write( (short) ( Engine.Player?.Z ?? 0 ) );
        }
    }
}