using ClassicAssist.Shared.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class UnicodePromptRequest : BasePacket, IMacroCommandParser
    {
        public UnicodePromptRequest()
        {
        }

        public UnicodePromptRequest( int id )
        {
            _writer = new PacketWriter( 0x15 );
            _writer.Write( (byte) 0xC2 );
            _writer.Write( (short) 0x15 );
            _writer.Write( id );
            _writer.Write( id );
            _writer.Write( 1 );
            _writer.WriteAsciiFixed( Strings.UO_LOCALE, 4 );
            _writer.Fill();
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( packet[0] == 0xC2 && direction == PacketDirection.Incoming )
            {
                return "WaitForPrompt(5000)\r\n";
            }

            return null;
        }
    }
}