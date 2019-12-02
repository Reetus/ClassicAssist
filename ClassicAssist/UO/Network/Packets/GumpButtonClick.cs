using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class GumpButtonClick : Packets, IMacroCommandParser
    {
        public GumpButtonClick()
        {
            
        }

        public GumpButtonClick( int gumpID, int serial, int buttonID )
        {
            //TODO switches etc
            _writer = new PacketWriter( 23 );
            _writer.Write( (byte) 0xB1 );
            _writer.Write( (short) 23 );
            _writer.Write( serial );
            _writer.Write( gumpID );
            _writer.Write( buttonID );
            _writer.Fill();
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( packet[0] != 0xB1 || direction != PacketDirection.Outgoing )
            {
                return null;
            }

            uint gumpId = (uint) ( ( packet[7] << 24 ) | ( packet[8] << 16 ) | ( packet[9] << 8 ) | packet[10] );
            int buttonId = ( packet[11] << 24 ) | ( packet[12] << 16 ) | ( packet[13] << 8 ) | packet[14];

            return $"ReplyGump(0x{gumpId:x}, {buttonId})\r\n";
        }
    }
}