using System.Collections.Generic;
using System.Linq;
using ClassicAssist.Data.Vendors;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class VendorBuy : BasePacket
    {
        public VendorBuy( IReadOnlyCollection<ShopListEntry> entries )
        {
            int len = 8 + entries.Count * 7;

            _writer = new PacketWriter( len );
            _writer.Write( (byte) 0x3B );
            _writer.Write( (short) len );
            _writer.Write( entries.FirstOrDefault()?.VendorSerial ?? 0 );
            _writer.Write( (byte) 2 ); // item list

            foreach ( ShopListEntry entry in entries )
            {
                _writer.Write( (byte) Layer.ShopBuy );
                _writer.Write( entry.Item.Serial );
                _writer.Write( (short) entry.Amount );
            }
        }
    }
}