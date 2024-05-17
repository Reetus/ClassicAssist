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
using ClassicAssist.UO.Network.PacketFilter;
using static System.Text.Encoding;

namespace ClassicAssist.UO.Network.Packets
{
    public class UseSkill : BasePacket, IMacroCommandParser
    {
        public UseSkill()
        {
        }

        public UseSkill( int index )
        {
            string args = $"{index} 0";

            _writer = new PacketWriter( 5 + args.Length );
            _writer.Write( (byte) 0x12 );
            _writer.Write( (short) ( 5 + args.Length ) );
            _writer.Write( (byte) 0x24 );
            _writer.WriteAsciiFixed( args, args.Length );
            _writer.Write( (byte) 0 );

            Engine.LastSkillID = index;
            Engine.LastSkillTime = DateTime.Now;
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( packet[0] != 0x12 || packet[3] != 0x24 || direction != PacketDirection.Outgoing )
            {
                return null;
            }

            int len = packet.Length - 4;
            byte[] skillPart = new byte[len];
            Buffer.BlockCopy( packet, 4, skillPart, 0, len );
            string skill = ASCII.GetString( skillPart );

            if ( !int.TryParse( skill.Substring( 0, skill.IndexOf( ' ' ) ), out int id ) )
            {
                return null;
            }

            if ( id == 0 )
            {
                return "UseLastSkill()\r\n";
            }

            string skillName = Skills.GetSkillName( id );

            return string.IsNullOrEmpty( skillName ) ? null : $"UseSkill(\"{skillName}\")\r\n";
        }
    }
}