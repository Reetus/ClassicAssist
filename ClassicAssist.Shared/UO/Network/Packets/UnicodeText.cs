using System.Text;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class UnicodeText : BasePacket
    {
        public UnicodeText( int serial, int graphic, JournalSpeech speechType, int hue, int font, string lang,
            string name, string text )
        {
            byte[] textBytes = Encoding.BigEndianUnicode.GetBytes( text );
            int len = 48 + textBytes.Length;

            _writer = new PacketWriter( len );
            _writer.Write( (byte) 0xAE );
            _writer.Write( (short) len );
            _writer.Write( serial );
            _writer.Write( (ushort) graphic );
            _writer.Write( (byte) speechType );
            _writer.Write( (short) hue );
            _writer.Write( (short) font );
            _writer.WriteAsciiFixed( lang, 4 );
            _writer.WriteAsciiFixed( name, 30 );
            _writer.Write( textBytes, 0, textBytes.Length );
        }
    }
}