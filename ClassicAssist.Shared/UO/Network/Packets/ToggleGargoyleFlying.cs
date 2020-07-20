using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class ToggleGargoyleFlying : BasePacket
    {
        public ToggleGargoyleFlying()
        {
            _writer = new PacketWriter( 11 );

            _writer.Write( (byte) 0xBF );
            _writer.Write( (short) 11 );
            _writer.Write( (short) 0x32 );
            _writer.Write( (short) 1 );
            _writer.Write( 0 );
        }
    }
}