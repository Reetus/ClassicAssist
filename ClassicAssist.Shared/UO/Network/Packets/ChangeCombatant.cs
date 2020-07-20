using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class ChangeCombatant : BasePacket
    {
        public ChangeCombatant( int serial ) : base( PacketDirection.Incoming )
        {
            _writer = new PacketWriter( 5 );
            _writer.Write( (byte) 0xAA );
            _writer.Write( serial );
        }
    }
}