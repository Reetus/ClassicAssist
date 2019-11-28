using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class UseObject : Packets
    {
        public UseObject( int serial )
        {
            _writer = new PacketWriter( 5 );
            _writer.Write( (byte) 0x06 );
            _writer.Write( serial );
        }
    }
}