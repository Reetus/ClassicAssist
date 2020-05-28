using System.Collections.Generic;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class GumpButtonClick : BasePacket, IMacroCommandParser
    {
        public GumpButtonClick()
        {
        }

        public GumpButtonClick( int gumpID, int serial, int buttonID, IReadOnlyCollection<int> switches = null )
        {
            //TODO switches etc
            _writer = new PacketWriter( 23 + ( switches?.Count * 4 ?? 0 ) );
            _writer.Write( (byte) 0xB1 );
            _writer.Write( (short) ( 23 + ( switches?.Count * 4 ?? 0 ) ) );
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