using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class ContextMenuClick : Packets, IMacroCommandParser
    {
        public ContextMenuClick()
        {
        }

        public ContextMenuClick( int serial, int index )
        {
            _writer = new PacketWriter( 11 );
            _writer.Write( (byte) 0xBF );
            _writer.Write( (short) 11 );
            _writer.Write( (short) 0x15 );
            _writer.Write( serial );
            _writer.Write( (short) index );
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( packet[0] != 0xBF || packet[4] != 0x15 || direction != PacketDirection.Outgoing )
            {
                return null;
            }

            PacketReader reader = new PacketReader( packet, length, false );

            reader.ReadInt16();

            int serial = reader.ReadInt32();
            int index = reader.ReadInt16();

            return $"WaitForContext(0x{serial:x}, {index}, 5000)\r\n";
        }
    }
}