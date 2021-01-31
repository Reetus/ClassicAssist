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
    [FilterOptions( Name = " -  EmoteFemale Sounds", DefaultEnabled = true )]
    public class AudioFilterEmoteFemaleSounds : DynamicFilterEntry
    {
                                                         
        private static readonly int[] _audioPackets = { 0x30A, 0x30B, 0x30C, 0x30D, 0x312, 0x30E, 0x311, 0x313, // Human Female emotes // qk 
                                                        0x317, 0x319, 0x31A, 0x31B, 0x31C, 0x31D, 0x31E, 0x31F, 
                                                        0x320, 0x321, 0x322, 0x323, 0x32B, 0x32C, 0x32D, 0x32E, 
                                                        0x32F, 0x330, 0x331, 0x332, 0x333, 0x334, 0x335, 0x30F, 
                                                        0x336, 0x337 }; 
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