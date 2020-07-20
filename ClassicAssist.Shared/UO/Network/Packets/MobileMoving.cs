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
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UO.Network.Packets
{
    public class MobileMoving : BasePacket
    {
        public MobileMoving( Mobile mobile, int hueOverride = 0 )
        {
            _writer = new PacketWriter( 17 );
            _writer.Write( (byte) 0x77 );
            _writer.Write( mobile.Serial );
            _writer.Write( (short) mobile.ID );
            _writer.Write( (short) mobile.X );
            _writer.Write( (short) mobile.Y );
            _writer.Write( (sbyte) mobile.Z );
            _writer.Write( (byte) mobile.Direction );
            _writer.Write( (short) ( hueOverride > 0 ? hueOverride : mobile.Hue ) );
            _writer.Write( (byte) mobile.Status );
            _writer.Write( (byte) mobile.Notoriety );
        }
    }
}