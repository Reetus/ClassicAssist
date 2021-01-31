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
    [FilterOptions( Name = " -  EmoteMale Sounds", DefaultEnabled = true )]
    public class AudioFilterEmoteMaleSounds : DynamicFilterEntry
    {
                                                         
        private static readonly int[] _audioPackets = { 0x419, 0x41A, 0x41B, 0x41C, 0x41D, 0x420, 0x421, 0x422, // Human Male emotes // qk 
                                                        0x427, 0x428, 0x429, 0x42A, 0x42B, 0x42C, 0x42D, 0x42E,
                                                        0x42F, 0x430, 0x431, 0x432, 0x433, 0x43D, 0x43E, 0x43F,
                                                        0x13B, 0x440, 0x441, 0x442, 0x3B4, 0x443, 0x444, 0x445,
                                                        0x318, 0x36A, 0x447, 0x41E, 0x448, 0x449, 0x44A };
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