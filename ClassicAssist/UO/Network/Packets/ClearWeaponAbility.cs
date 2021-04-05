using Assistant;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class ClearWeaponAbility : BasePacket
    {
        public ClearWeaponAbility()
        {
            _writer = new PacketWriter( 5 );

            _writer.Write( (byte) 0xD7 );
            _writer.Write( (short) 15 );
            _writer.Write( Engine.Player.Serial );
            _writer.Write( (short) 0x19 );
            _writer.Write( (byte) 0x0 );
            _writer.Write( 0x0 );
            _writer.Write( (byte) 0x0A );
        }
    }
}