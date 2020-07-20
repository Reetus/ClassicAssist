using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class CloseClientGump : BasePacket
    {
        public CloseClientGump( uint gumpID )
        {
            _writer = new PacketWriter( 13 );
            _writer.Write( (byte) 0xBF );
            _writer.Write( (short) 13 );
            _writer.Write( (short) 0x04 );
            _writer.Write( gumpID );
            _writer.Fill();
        }
    }
}