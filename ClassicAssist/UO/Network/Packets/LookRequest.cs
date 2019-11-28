using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class LookRequest : Packets
    {
        public LookRequest(int serial)
        {
            _writer = new PacketWriter(5);
            _writer.Write( (byte)0x09 );
            _writer.Write( serial );
        }
    }
}