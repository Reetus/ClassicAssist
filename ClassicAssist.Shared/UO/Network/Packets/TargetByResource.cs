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

namespace ClassicAssist.UO.Network.Packets
{
    public class TargetByResource : BasePacket
    {
        public TargetByResource( int serial, int resourceType )
        {
            _writer = new PacketWriter( 11 );
            _writer.Write( (byte) 0xBF );
            _writer.Write( (short) 11 );
            _writer.Write( (short) 0x30 );
            _writer.Write( serial );
            _writer.Write( (short) resourceType );
        }
    }
}