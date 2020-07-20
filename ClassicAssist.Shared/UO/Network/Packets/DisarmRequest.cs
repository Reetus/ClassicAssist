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

using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class DisarmRequest : BasePacket, IMacroCommandParser
    {
        public DisarmRequest()
        {
            _writer = new PacketWriter( 5 );
            _writer.Write( (byte) 0xBF );
            _writer.Write( (short) 5 );
            _writer.Write( (short) 0x09 );
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( direction == PacketDirection.Outgoing && packet[0] == 0xBF && packet[4] == 0x09 )
            {
                return "SetAbility(\"Disarm\")\r\n";
            }

            return null;
        }
    }
}