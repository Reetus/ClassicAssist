using System.Collections.Generic;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class BatchQueryProperties : BasePacket
    {
        public BatchQueryProperties( int serial )
        {
            _writer = new PacketWriter();
            _writer.Write( (byte) 0xD6 );
            _writer.Write( (short) 7 );
            _writer.Write( serial );
        }

        public BatchQueryProperties( IReadOnlyCollection<int> serials )
        {
            _writer = new PacketWriter();
            _writer.Write( (byte) 0xD6 );
            _writer.Write( (short) ( 3 + serials.Count * 4 ) );

            foreach ( int serial in serials )
            {
                _writer.Write( serial );
            }
        }
    }
}