using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class RenameRequest : BasePacket, IMacroCommandParser
    {
        public RenameRequest()
        {
        }

        public RenameRequest( int serial, string name )
        {
            _writer = new PacketWriter( 35 );
            _writer.Write( (byte) 0x75 );
            _writer.Write( serial );
            _writer.WriteAsciiFixed( name, 30 );
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( packet[0] != 0x75 || direction != PacketDirection.Outgoing )
            {
                return null;
            }

            PacketReader reader = new PacketReader( packet, length, true );

            int serial = reader.ReadInt32();
            string name = reader.ReadString( 30 );

            return $"Rename(0x{serial:x}, \"{name}\")\r\n";
        }
    }
}