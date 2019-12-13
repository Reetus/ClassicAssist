using Assistant;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UO.Network.Packets
{
    public class Target : BasePacket, IMacroCommandParser
    {
        public Target()
        {
        }

        public Target( int senderSerial, Entity entity ) : this( TargetType.Object, senderSerial, TargetFlags.None,
            entity.Serial, -1, -1, -1, entity.ID )
        {
        }

        public Target( TargetType targetType, int senderSerial, TargetFlags flags, int targetSerial, int x, int y,
            int z, int id )
        {
            if ( senderSerial == -1 )
            {
                senderSerial = Engine.TargetSerial;
            }

            if ( targetSerial > 0 )
            {
                Engine.Player.LastTargetSerial = targetSerial;
                Engine.Player.LastTargetType = targetType;
            }

            _writer = new PacketWriter( 19 );
            _writer.Write( (byte) 0x6C );
            _writer.Write( (byte) targetType );
            _writer.Write( senderSerial );
            _writer.Write( (byte) flags );
            _writer.Write( targetSerial );
            _writer.Write( (short) x );
            _writer.Write( (short) y );
            _writer.Write( (short) z );
            _writer.Write( (short) id );

            Engine.TargetExists = false;
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( packet[0] != 0x6C )
            {
                return null;
            }

            if ( direction == PacketDirection.Incoming )
            {
                return "WaitForTarget(5000)\r\n";
            }

            uint serial = (uint) ( ( packet[7] << 24 ) | ( packet[8] << 16 ) | ( packet[9] << 8 ) | packet[10] );

            return $"Target(0x{serial:x})\r\n";
        }
    }
}