using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class DragItem : BasePacket
    {
        public DragItem( int serial, int amount )
        {
            _writer = new PacketWriter( 7 );
            _writer.Write( (byte) 0x07 );
            _writer.Write( serial );
            _writer.Write( (short) amount );
        }
    }
}