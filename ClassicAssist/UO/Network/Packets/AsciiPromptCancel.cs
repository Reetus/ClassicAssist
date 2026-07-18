#region License
// Copyright (C) 2026 Reetus
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

using ClassicAssist.Shared.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class AsciiPromptCancel : BasePacket, IMacroCommandParser
    {
        public AsciiPromptCancel()
        {
            
        }

        public AsciiPromptCancel( int senderSerial, int promptId )
        {
            _writer = new PacketWriter( 15 );
            _writer.Write( (byte) 0x9A );
            _writer.Write( (short) 15 );
            _writer.Write( senderSerial );
            _writer.Write( promptId );
            _writer.Write( 0 );
            _writer.Fill();

        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( packet[0] != 0x9A || direction != PacketDirection.Outgoing )
            {
                return null;
            }

            return packet[14] != 0x00 ? null : "CancelPrompt()\r\n";
        }
    }
}