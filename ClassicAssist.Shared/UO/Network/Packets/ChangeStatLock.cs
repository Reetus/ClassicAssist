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
    public class ChangeStatLock : BasePacket, IMacroCommandParser
    {
        public ChangeStatLock()
        {
        }

        public ChangeStatLock( StatType stat, LockStatus lockStatus )
        {
            _writer = new PacketWriter( 7 );
            _writer.Write( (byte) 0xBF );
            _writer.Write( (short) 7 );
            _writer.Write( (short) 0x1A );
            _writer.Write( (byte) stat );
            _writer.Write( (byte) lockStatus );
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( direction != PacketDirection.Outgoing || packet[0] != 0xBF || packet[4] != 0x1A )
            {
                return null;
            }

            StatType stat = (StatType) packet[5];
            LockStatus lockStatus = (LockStatus) packet[6];

            return $"SetStatus(\"{stat.ToString().ToLower()}\", \"{lockStatus.ToString().ToLower()}\")\r\n";
        }
    }
}