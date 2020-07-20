using ClassicAssist.Shared;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class EquipLastWeapon : BasePacket, IMacroCommandParser
    {
        public EquipLastWeapon()
        {
            if ( Engine.Player == null )
            {
                return;
            }

            _writer = new PacketWriter( 10 );
            _writer.Write( (byte) 0xD7 );
            _writer.Write( (short) 10 ); // size
            _writer.Write( Engine.Player.Serial );
            _writer.Write( (short) 0x1E );
            _writer.Write( (byte) 0x0A );
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( packet[0] != 0xD7 || packet[8] != 0x1E || packet[9] != 0x0A || direction != PacketDirection.Outgoing )
            {
                return null;
            }

            return "EquipLastWeapon()\r\n";
        }
    }
}