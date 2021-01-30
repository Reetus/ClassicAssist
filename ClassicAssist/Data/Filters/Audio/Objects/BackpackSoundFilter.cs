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

using System.Linq;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.Data.Filters
{
    [FilterOptions( Name = " -  Backpack Sounds", DefaultEnabled = true )]
    public class AudioFilterBackpackSounds : DynamicFilterEntry
    {
        // spellbook 0x0055

        //private static readonly int[] _audioPackets = { 0x0048, 0x0058, 0x002F, 0x002E, 0x004F, 0x0058, 0x002D, 0x002C, 0x004F, 0x0058, 0x002F, 0x002E, 0x0187, 0x01C9, 0x529, }; // { 0x48, 0x529 };
        private static readonly int[] _audioPackets = { 0x002D, 0x002C, 0x002E, 0x002F, 0x0048, 0x0058, 0x004F, 0x0187, 0x01C9 }; // taken from containers.txt -> not working seems to be client side?
        private static bool _isEnabled;

        protected override void OnChanged( bool enabled )
        {
            _isEnabled = enabled;
        }

        public override bool CheckPacket( ref byte[] packet, ref int length, PacketDirection direction )
        {
            if ( packet == null || !_isEnabled )
            {
                return false;
            }

            if ( packet[0] != 0x54 || direction != PacketDirection.Incoming )
            {
                return false;
            }

            int soundId = ( packet[2] << 8 ) | packet[3];

            return _audioPackets.Contains( soundId );
        }
    }
}