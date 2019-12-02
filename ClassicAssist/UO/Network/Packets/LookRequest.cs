using ClassicAssist.Data;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class LookRequest : Packets, IMacroCommandParser
    {
        public LookRequest()
        {
            
        }

        public LookRequest( int serial )
        {
            _writer = new PacketWriter( 5 );
            _writer.Write( (byte) 0x09 );
            _writer.Write( serial );
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            //if ( packet[0] != 0x09 || direction != PacketDirection.Outgoing )
            //{
            //    return null;
            //}

            //int serial = ( packet[1] << 24 ) | ( packet[2] << 16 ) | ( packet[3] << 8 ) | packet[4];

            //return $"ClickObject(0x{serial:x})\r\nPause({Options.CurrentOptions.ActionDelayMS})\r\n";
            return null;
        }
    }
}