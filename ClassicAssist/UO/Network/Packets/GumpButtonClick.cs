using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class GumpButtonClick : Packets
    {
        public GumpButtonClick(int gumpID, int serial, int buttonID)
        {
            //TODO switches etc
            _writer = new PacketWriter(23);
            _writer.Write((byte)0xB1);
            _writer.Write((short)23);
            _writer.Write(serial);
            _writer.Write(gumpID);
            _writer.Write(buttonID);
            _writer.Fill();
        }
    }
}