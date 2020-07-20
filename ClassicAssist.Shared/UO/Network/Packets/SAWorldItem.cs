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

using System.IO;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class SAWorldItem : BasePacket
    {
        public SAWorldItem( byte[] packet, int length, int hueOverride )
        {
            _writer = new PacketWriter( length );
            _writer.Write( packet, 0, length );

            if ( hueOverride <= 0 )
            {
                return;
            }

            _writer.Seek( 21, SeekOrigin.Begin );
            _writer.Write( (short) hueOverride );
            _writer.Seek( 0, SeekOrigin.End );
        }
    }
}