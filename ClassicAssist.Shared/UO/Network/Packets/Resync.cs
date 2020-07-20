using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class Resync : BasePacket
    {
        public Resync()
        {
            _writer = new PacketWriter( 3 );
            _writer.Write( (byte) 0x22 );
            _writer.Write( (short) 0 );
        }
    }
}