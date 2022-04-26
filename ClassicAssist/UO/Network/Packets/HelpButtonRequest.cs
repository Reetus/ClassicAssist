// Copyright (C) $CURRENT_YEAR$ ad960009
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using Assistant;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class HelpButtonRequest : BasePacket, IMacroCommandParser
    {
        public HelpButtonRequest()
        {
            if ( Engine.Player == null )
            {
                return;
            }

            _writer = new PacketWriter( 258 );
            _writer.Write( (byte) 0x9B );
            _writer.Fill();
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( packet[0] != 0x9B || direction != PacketDirection.Outgoing || length != 258 )
            {
                return null;
            }

            for ( int i = 1; i < length; i++ )
            {
                if ( packet[i] != 0x00 )
                    return null;
            }

            return "OpenHelpGump()\r\n";
        }
    }
}
