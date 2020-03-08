using ClassicAssist.Data.Spells;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class CastSpell : BasePacket, IMacroCommandParser
    {
        public CastSpell()
        {
        }

        public CastSpell( int id )
        {
            _writer = new PacketWriter( 9 );
            _writer.Write( (byte) 0xBF );
            _writer.Write( (short) 9 );
            _writer.Write( (short) 0x1C );
            _writer.Write( (short) 0x02 );
            _writer.Write( (short) id );
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
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