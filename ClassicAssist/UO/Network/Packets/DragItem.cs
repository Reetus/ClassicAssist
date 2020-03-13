using Assistant;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UO.Network.Packets
{
    public class DragItem : BasePacket
    {
        public DragItem( int serial, int amount, bool checkAmount = false )
        {
            if ( checkAmount && amount == -1 )
            {
                Item item = Engine.Items.GetItem( serial );

                if ( item != null )
                {
                    amount = item.Count;
                }
            }

            _writer = new PacketWriter( 7 );
            _writer.Write( (byte) 0x07 );
            _writer.Write( serial );
            _writer.Write( (short) amount );
        }
    }
}