using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class InvokeVirtue : Packets, IMacroCommandParser
    {
        public InvokeVirtue()
        {
        }

        public InvokeVirtue( Virtues virtue )
        {
            _writer = new PacketWriter( 6 );
            _writer.Write( (byte) 0x12 );
            _writer.Write( (short) 6 );
            _writer.Write( (byte) 0xF4 );
            _writer.Write( (byte) virtue );
            _writer.Write( (byte) 0 );
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( packet[0] != 0x12 || packet[3] != 0xF4 || direction != PacketDirection.Outgoing )
            {
                return null;
            }

            return $"InvokeVirtue(\"{(Virtues) packet[4]}\")\r\n";
        }
    }
}