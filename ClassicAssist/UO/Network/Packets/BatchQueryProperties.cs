using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class BatchQueryProperties : BasePacket
    {
        public BatchQueryProperties( int serial )
        {
            _writer = new PacketWriter();
            _writer.Write( (byte) 0xD6 );
            _writer.Write( (short) 7 );
            _writer.Write( serial );
        }
    }
}