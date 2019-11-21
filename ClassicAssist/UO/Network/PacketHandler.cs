using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network
{
    internal delegate void OnPacketReceive(PacketReader reader);

    internal class PacketHandler
    {
        public int PacketID { get; set; }
        public int Length { get; set; }
        internal OnPacketReceive OnReceive { get; set; }

        public PacketHandler(int packetId, int length, OnPacketReceive onReceive)
        {
            PacketID = packetId;
            Length = length;
            OnReceive = onReceive;
        }
    }
}