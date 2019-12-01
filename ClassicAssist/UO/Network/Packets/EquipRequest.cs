using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class EquipRequest : Packets
    {
        public EquipRequest( int serial, Layer layer, int containerSerial )
        {
            _writer = new PacketWriter( 10 );

            _writer.Write( (byte) 0x13 );
            _writer.Write( serial );
            _writer.Write( (byte) layer );
            _writer.Write( containerSerial );
        }
    }
}