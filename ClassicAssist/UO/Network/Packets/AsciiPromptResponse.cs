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

using System.Text;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class AsciiPromptResponse : BasePacket, IMacroCommandParser
    {
        public AsciiPromptResponse()
        {
        }

        public AsciiPromptResponse( int senderSerial, int promptId, string text )
        {
            byte[] textBytes = Encoding.ASCII.GetBytes( text + '\0' );

            int len = 15 + textBytes.Length;

            _writer = new PacketWriter( len );
            _writer.Write( (byte) 0x9A );
            _writer.Write( (short) len );
            _writer.Write( senderSerial );
            _writer.Write( promptId );
            _writer.Write( 1 );
            _writer.Write( textBytes, 0, textBytes.Length );
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( packet[0] != 0x9A || direction != PacketDirection.Outgoing )
            {
                return null;
            }

            if ( packet[14] == 0x00 )
            {
                return null;
            }

            string text = Encoding.ASCII.GetString( packet, 15, packet.Length - 15 ).Trim( '\0' );

            return $"PromptMsg(\"{text}\")\r\n";
        }
    }
}