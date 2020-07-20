using ClassicAssist.Shared;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class SetWeaponAbility : BasePacket
    {
        public SetWeaponAbility( int abilityIndex )
        {
            _writer = new PacketWriter( 15 );

            _writer.Write( (byte) 0xD7 );
            _writer.Write( (short) 15 );
            _writer.Write( Engine.Player?.Serial ?? 0 );
            _writer.Write( (short) 0x19 );
            _writer.Write( (byte) 0 );
            _writer.Write( abilityIndex );
            _writer.Write( (byte) 0x0A );
        }
    }
}