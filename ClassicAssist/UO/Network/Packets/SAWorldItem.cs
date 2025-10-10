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
using ClassicAssist.UO.Objects;

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

        public SAWorldItem( Item item, int hueOverride = -1 )
        {
            _writer = new PacketWriter( 26 );
            _writer.Write( (byte) 0xF3 );
            _writer.Write( (short) 0x01 );
            _writer.Write( (byte) item.ArtDataID );
            _writer.Write( item.Serial );
            _writer.Write( (short) item.ID );
            _writer.Write( (byte) item.Direction );
            _writer.Write( (short) item.Count );
            _writer.Write( (short) item.Count );
            _writer.Write( (short) item.X );
            _writer.Write( (short) item.Y );
            _writer.Write( (sbyte) item.Z );
            _writer.Write( (byte) item.Light );
            _writer.Write( (short) ( hueOverride > 0 ? hueOverride : item.Hue ) );
            _writer.Write( (byte) item.Flags );
            _writer.Fill();
        }
    }
}