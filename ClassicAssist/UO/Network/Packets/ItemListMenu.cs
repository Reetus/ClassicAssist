using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class ItemListMenu : BasePacket, IMacroCommandParser
    {
        public ItemListMenu()
        {
            
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( packet[0] != 0x7C || direction != PacketDirection.Incoming )
            {
                return null;
            }

            int gumpId = packet[7] << 8 | packet[8];
            return $"WaitForMenu(0x{gumpId:x}, 5000)\r\n";
        }
    }
}
