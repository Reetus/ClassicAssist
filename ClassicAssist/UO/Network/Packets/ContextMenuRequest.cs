using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class ContextMenuRequest : Packets
    {
        public ContextMenuRequest( int serial )
        {
            _writer = new PacketWriter( 9 );
            _writer.Write( (byte) 0xBF );
            _writer.Write( (short) 9 );
            _writer.Write( (short) 0x13 );
            _writer.Write( serial );
        }
    }
}