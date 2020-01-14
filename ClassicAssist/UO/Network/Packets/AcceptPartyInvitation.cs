using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class AcceptPartyInvitation : BasePacket
    {
        public AcceptPartyInvitation( int partyLeaderSerial )
        {
            _writer = new PacketWriter( 10 );
            _writer.Write( (byte) 0xBF );
            _writer.Write( (short) 10 );
            _writer.Write( (short) 0x06 );
            _writer.Write( (byte) 0x08 );
            _writer.Write( partyLeaderSerial );
        }
    }
}