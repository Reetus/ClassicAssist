using Assistant;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UO.Network.Packets
{
    public enum TargetFlags : byte
    {
        None,
        Harmful,
        Beneficial,
        Cancel
    }

    public class Target : Packets
    {
        public Target(int senderSerial, Entity entity) : this(TargetType.Object, senderSerial, TargetFlags.None, entity.Serial, -1, -1, -1, entity.ID)
        {
        }

        public Target( TargetType targetType, int senderSerial, TargetFlags flags, int targetSerial, int x, int y,
            int z, int id )
        {
            if ( senderSerial == -1 )
                senderSerial = Engine.TargetSerial;

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
        }
    }
}