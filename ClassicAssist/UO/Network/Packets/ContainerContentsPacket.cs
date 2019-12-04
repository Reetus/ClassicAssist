using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UO.Network.Packets
{
    public class ContainerContentsPacket : BasePacket
    {
        public ContainerContentsPacket( ItemCollection collection )
        {
            _writer = new PacketWriter( 5 + collection.GetTotalItemCount() * 20 );

            _writer.Write( (byte) 0x3C );
            _writer.Write( (short) ( collection.GetTotalItemCount() * 20 ) );
            _writer.Write( (short) collection.GetTotalItemCount() );

            foreach ( Item item in collection.GetItems() )
            {
                _writer.Write( item.Serial );
                _writer.Write( (short) item.ID );
                _writer.Write( (byte) 0 ); // itemid offset
                _writer.Write( (short) item.Count );
                _writer.Write( (short) item.X );
                _writer.Write( (short) item.Y );
                _writer.Write( (byte) item.Grid );
                _writer.Write( collection.Serial );
                _writer.Write( (short) item.Hue );
            }
        }
    }
}