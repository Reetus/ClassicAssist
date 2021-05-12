using System.Collections.Generic;
using System.IO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class GumpButtonClick : BasePacket, IMacroCommandParser
    {
        public GumpButtonClick()
        {
        }

        public GumpButtonClick( int gumpID, int serial, int buttonID, IReadOnlyCollection<int> switches = null,
            Dictionary<int, string> textEntries = null )
        {
            _writer = new PacketWriter();
            _writer.Write( (byte) 0xB1 );
            _writer.Write( (short) 0 );
            _writer.Write( serial );
            _writer.Write( gumpID );
            _writer.Write( buttonID );

            if ( switches != null )
            {
                _writer.Write( switches.Count );

                foreach ( int @switch in switches )
                {
                    _writer.Write( @switch );
                }
            }
            else
            {
                _writer.Write( 0 );
            }

            if ( textEntries != null )
            {
                _writer.Write( textEntries.Count );

                foreach ( KeyValuePair<int, string> keyValuePair in textEntries )
                {
                    int length = keyValuePair.Value.Length * 2;

                    _writer.Write( (short) keyValuePair.Key );
                    _writer.Write( (short) length );
                    _writer.WriteBigUniFixed( keyValuePair.Value, length );
                }
            }

            _writer.Seek( 1, SeekOrigin.Begin );
            _writer.Write( (short) _writer.Length );
            _writer.Seek( 0, SeekOrigin.End );
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( packet[0] != 0xB1 || direction != PacketDirection.Outgoing )
            {
                return null;
            }

            uint gumpId = (uint) ( ( packet[7] << 24 ) | ( packet[8] << 16 ) | ( packet[9] << 8 ) | packet[10] );
            int buttonId = ( packet[11] << 24 ) | ( packet[12] << 16 ) | ( packet[13] << 8 ) | packet[14];

            int switchesCount = 0;

            if ( packet.Length > 15 )
            {
                switchesCount = ( packet[15] << 24 ) | ( packet[16] << 16 ) | ( packet[17] << 8 ) | packet[18];
            }

            int pos = 19;
            List<int> switches = new List<int>();

            for ( int i = 0; i < switchesCount; i++ )
            {
                int switchId = ( packet[pos] << 24 ) | ( packet[pos + 1] << 16 ) | ( packet[pos + 2] << 8 ) |
                               packet[pos + 3];
                switches.Add( switchId );
                pos += 4;
            }

            return switchesCount > 0
                ? $"ReplyGump(0x{gumpId:x}, {buttonId}, Array[int]([{string.Join( ",", switches )}]))\r\n"
                : $"ReplyGump(0x{gumpId:x}, {buttonId})\r\n";
        }
    }
}