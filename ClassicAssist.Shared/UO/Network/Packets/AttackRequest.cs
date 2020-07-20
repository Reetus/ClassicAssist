using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class AttackRequest : BasePacket
    {
        public AttackRequest( int serial )
        {
            _writer = new PacketWriter( 5 );
            _writer.Write( (byte) 0x05 );
            _writer.Write( serial );
        }
    }
}