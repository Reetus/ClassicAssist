using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class MobileQuery : BasePacket
    {
        public MobileQuery( int serial, MobileQueryType queryType = MobileQueryType.StatsRequest )
        {
            _writer = new PacketWriter( 10 );
            _writer.Write( (byte) 0x34 );
            _writer.Write( 0xEDEDEDED );
            _writer.Write( (byte) queryType );
            _writer.Write( serial );
        }
    }
}