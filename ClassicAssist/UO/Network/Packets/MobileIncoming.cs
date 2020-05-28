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

using System;
using System.Linq;
using Assistant;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UO.Network.Packets
{
    public class MobileIncoming : BasePacket
    {
        public MobileIncoming( Mobile mobile, ItemCollection equipment, int overrideHue = 0 )
        {
            bool useNewIncoming = Engine.ClientVersion == null || Engine.ClientVersion >= new Version( 7, 0, 33, 1 );

            int len = 23 + equipment.Count() * 9;

            _writer = new PacketWriter( len );
            _writer.Write( (byte) 0x78 );
            _writer.Write( (short) len );
            _writer.Write( mobile.Serial );
            _writer.Write( (short) mobile.ID );
            _writer.Write( (short) mobile.X );
            _writer.Write( (short) mobile.Y );
            _writer.Write( (sbyte) mobile.Z );
            _writer.Write( (byte) mobile.Direction );
            _writer.Write( (short) ( overrideHue > 0 ? overrideHue : mobile.Hue ) );
            _writer.Write( (byte) mobile.Status );
            _writer.Write( (byte) mobile.Notoriety );

            foreach ( Item item in equipment.GetItems() )
            {
                _writer.Write( item.Serial );
                _writer.Write( (short) ( useNewIncoming ? item.ID : item.ID | 0x8000 ) );
                _writer.Write( (byte) item.Layer );
                _writer.Write( (short) ( overrideHue > 0 ? overrideHue : item.Hue ) );
            }

            _writer.Write( 0 );
        }
    }
}