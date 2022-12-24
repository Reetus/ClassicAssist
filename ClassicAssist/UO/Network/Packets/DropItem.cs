using System;
using Assistant;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class DropItem : BasePacket, IMacroCommandParser
    {
        public DropItem()
        {
        }

        public DropItem( int serial, int containerSerial, int x, int y, int z )
        {
            bool isNew = Engine.ClientVersion >= new Version( 6, 0, 1, 7 );

            _writer = new PacketWriter( isNew ? 15 : 14 );
            _writer.Write( (byte) 0x08 );
            _writer.Write( serial );
            _writer.Write( (short) x );
            _writer.Write( (short) y );
            _writer.Write( (sbyte) z );

            if ( isNew )
            {
                _writer.Write( (byte) 0 );
            }

            _writer.Write( containerSerial );
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( packet[0] != 0x08 || direction != PacketDirection.Outgoing )
            {
                return null;
            }

            int serial = ( packet[1] << 24 ) | ( packet[2] << 16 ) | ( packet[3] << 8 ) | packet[4];

            int containerSerial;

            if ( Engine.ClientVersion < new Version( 6, 0, 1, 7 ) )
            {
                containerSerial = ( packet[10] << 24 ) | ( packet[11] << 16 ) | ( packet[12] << 8 ) | packet[13];
            }
            else
            {
                containerSerial = ( packet[11] << 24 ) | ( packet[12] << 16 ) | ( packet[13] << 8 ) | packet[14];
            }

            if ( serial == 0 || containerSerial == 0 )
            {
                return null;
            }

            return $"MoveItem(0x{serial:x}, 0x{containerSerial:x})\r\n";
        }
    }
}