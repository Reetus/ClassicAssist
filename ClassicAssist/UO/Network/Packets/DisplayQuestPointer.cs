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
using Assistant;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class DisplayQuestPointer : BasePacket
    {
        public DisplayQuestPointer( bool active, int x, int y, int serial = 0 )
        {
            _writer = new PacketWriter( 10 );
            _writer.Write( (byte) 0xBA );
            _writer.Write( (byte) ( active ? 1 : 0 ) );
            _writer.Write( (short) x );
            _writer.Write( (short) y );

            if ( Engine.ClientVersion >= new Version( 7, 0, 9, 0 ) )
            {
                _writer.Write( serial );
            }
        }
    }
}