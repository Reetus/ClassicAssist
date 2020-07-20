using System;
using ClassicAssist.Shared;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class ContainerContentUpdate : BasePacket
    {
        public ContainerContentUpdate( int serial, int id, Direction direction, int amount, int x, int y, int grid,
            int parentSerial, int hue ) : base( PacketDirection.Incoming )
        {
            bool isNew = Engine.ClientVersion == null || Engine.ClientVersion >= new Version( 6, 0, 1, 7 );

            _writer = new PacketWriter( isNew ? 21 : 20 );
            _writer.Write( (byte) 0x25 );
            _writer.Write( serial );
            _writer.Write( (short) id );
            _writer.Write( (byte) direction );
            _writer.Write( (short) amount );
            _writer.Write( (short) x );
            _writer.Write( (short) y );

            if ( isNew )
            {
                _writer.Write( (byte) grid );
            }

            _writer.Write( parentSerial );
            _writer.Write( (short) hue );
        }
    }
}