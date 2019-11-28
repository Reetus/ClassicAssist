using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public abstract class Packets
    {
        protected PacketWriter _writer;

        protected Packets()
        {
            
        }
        protected Packets(int length)
        {
            _writer = new PacketWriter( length );
        }

        public virtual byte[] ToArray()
        {
            return _writer.ToArray();
        }
    }
}