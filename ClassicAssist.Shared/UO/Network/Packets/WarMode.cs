using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class WarMode : BasePacket
    {
        public WarMode( bool onOff )
        {
            _writer = new PacketWriter( 5 );
            _writer.Write( (byte) 0x72 );
            _writer.Write( onOff );
            _writer.Write( (byte) 0x00 );
            _writer.Write( (byte) 0x32 );
            _writer.Write( (byte) 0x00 );
        }
    }
}