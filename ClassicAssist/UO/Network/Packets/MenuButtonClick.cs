#region License

// Copyright (C) 2020 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using Assistant;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UO.Network.Packets
{
    public class MenuButtonClick : BasePacket, IMacroCommandParser
    {
        public MenuButtonClick()
        {
        }

        public MenuButtonClick( int gumpId, int serial, int index, int id = 0, int hue = 0 )
        {
            if ( serial == -1 )
            {
                if ( !Engine.Menus.GetMenu( gumpId, out Menu menu ) )
                    return;

                serial = menu.Serial;
            }

            _writer = new PacketWriter( 13 );
            _writer.Write( (byte) 0x7D );
            _writer.Write( serial );
            _writer.Write( (short) gumpId );
            _writer.Write( (short) index );
            _writer.Write( (short) id );
            _writer.Write( (short) hue );
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( packet[0] != 0x7D || direction != PacketDirection.Outgoing )
            {
                return null;
            }

            int gumpId = ( packet[5] << 8 ) | packet[6];
            int index = ( packet[7] << 8 ) | packet[8];
            int id = ( packet[9] << 8 ) | packet[10];
            int hue = ( packet[11] << 8 ) | packet[12];

            return $"ReplyMenu(0x{gumpId:x}, {index}, 0x{id:x}, {hue})\r\n";

        }
    }
}