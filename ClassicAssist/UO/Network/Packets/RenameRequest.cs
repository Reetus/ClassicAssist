using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class RenameRequest : Packets
    {
        public RenameRequest(int  serial, string name)
        {
            _writer = new PacketWriter(35);
            _writer.Write( (byte) 0x75 );
            _writer.Write( serial );
            _writer.WriteAsciiFixed( name, 30 );
        }
    }
}