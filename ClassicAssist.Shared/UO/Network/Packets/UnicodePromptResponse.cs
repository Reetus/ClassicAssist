using System.Text;
using ClassicAssist.Resources;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class UnicodePromptResponse : BasePacket, IMacroCommandParser
    {
        public UnicodePromptResponse()
        {
        }

        public UnicodePromptResponse( int senderSerial, int promptId, string text )
        {
            byte[] textBytes = Encoding.Unicode.GetBytes( text );

            int len = 19 + textBytes.Length;

            _writer = new PacketWriter( len );
            _writer.Write( (byte) 0xC2 );
            _writer.Write( (short) len );
            _writer.Write( senderSerial );
            _writer.Write( promptId );
            _writer.Write( 1 );
            _writer.WriteAsciiFixed( Strings.UO_LOCALE, 4 );
            _writer.Write( textBytes, 0, textBytes.Length );
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( packet[0] != 0xC2 || direction != PacketDirection.Outgoing )
            {
                return null;
            }

            if ( packet[14] == 0x00 )
            {
                return null;
            }

            string text = Encoding.Unicode.GetString( packet, 19, packet.Length - 20 ).Trim( '\0' );

            return $"PromptMsg(\"{text}\")\r\n";
        }
    }
}