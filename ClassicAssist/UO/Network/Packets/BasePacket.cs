using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public abstract class BasePacket
    {
        protected PacketWriter _writer;

        protected BasePacket()
        {
        }

        protected BasePacket( int length )
        {
            _writer = new PacketWriter( length );
        }

        protected BasePacket( PacketDirection direction )
        {
            Direction = direction;
        }

        public PacketDirection Direction { get; set; } = PacketDirection.Any;

        public virtual byte[] ToArray()
        {
            return _writer?.ToArray();
        }

        public virtual void ThrottleBeforeSend()
        {
            // No throtteling by default
        }
    }
}