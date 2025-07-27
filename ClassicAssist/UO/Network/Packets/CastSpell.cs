using Assistant;
using ClassicAssist.Data.Spells;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;
using IronPython.Compiler.Ast;
using System;
using static System.Text.Encoding;

namespace ClassicAssist.UO.Network.Packets
{
    public class CastSpell : BasePacket, IMacroCommandParser
    {
        public CastSpell()
        {
        }

        public CastSpell( int id )
        {
            if ( Engine.ClientVersion < new Version( 6, 0, 14, 2 ) )
            {
                WriteLegacyPacket( id );
            }
            else
            {
                WriteModernPacket( id );
            }
        }

        private void WriteModernPacket( int id )
        {
            // modern cast spell packet
            _writer = new PacketWriter( 9 );
            _writer.Write( (byte) 0xBF );
            _writer.Write( (short) 9 );
            _writer.Write( (short) 0x1C );
            _writer.Write( (short) 0x02 );
            _writer.Write( (short) id );
        }

        private void WriteLegacyPacket( int id )
        {
            // legacy cast spell packet
            string idText = $"{id}";

            _writer = new PacketWriter( 5 + idText.Length );
            _writer.Write( (byte) 0x12 );
            _writer.Write( (short) ( 5 + idText.Length ) );
            _writer.Write( (byte) 0x56 );
            _writer.WriteAsciiFixed( idText, idText.Length );
            _writer.Write( (byte) 0 );
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( Engine.ClientVersion < new Version( 6, 0, 14, 2 ) )
            {
                return ParseLegacyPacket( packet, direction );
            }
            else
            {
                return ParseModernPacket( packet, direction );
            }
        }

        private static string ParseLegacyPacket( byte[] packet, PacketDirection direction )
        {
            if ( packet[0] != 0x12 || packet[3] != 0x56 || direction != PacketDirection.Outgoing )
            {
                return null;
            }

            int len = packet.Length - 4;
            byte[] spellPart = new byte[len];
            Buffer.BlockCopy( packet, 4, spellPart, 0, len );

            if ( !int.TryParse( ASCII.GetString( spellPart ), out int spellId ) )
            {
                return null;
            }

            SpellManager manager = SpellManager.GetInstance();

            SpellData sd = manager.GetSpellData( spellId ) ?? manager.GetMasteryData( spellId );

            return sd != null ? $"Cast(\"{sd.Name}\")\r\n" : null;
        }

        private static string ParseModernPacket( byte[] packet, PacketDirection direction )
        {
            if ( packet[0] != 0xBF || packet[4] != 0x1C || direction != PacketDirection.Outgoing )
            {
                return null;
            }

            int spellId = ( packet[7] << 8 ) | packet[8];

            SpellManager manager = SpellManager.GetInstance();

            SpellData sd = manager.GetSpellData( spellId ) ?? manager.GetMasteryData( spellId );

            return sd != null ? $"Cast(\"{sd.Name}\")\r\n" : null;
        }
    }
}