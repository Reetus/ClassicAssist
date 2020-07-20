using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class ClearWeaponAbility : BasePacket
    {
        public ClearWeaponAbility()
        {
            _writer = new PacketWriter( 5 );

            _writer.Write( (byte) 0xBF );
            _writer.Write( (short) 5 );
            _writer.Write( (short) 0x21 );
        }
    }
}