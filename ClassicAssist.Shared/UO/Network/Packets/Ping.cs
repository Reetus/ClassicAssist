namespace ClassicAssist.UO.Network.Packets
{
    public class Ping : BasePacket
    {
        public Ping( byte value ) : base( 2 )
        {
            _writer.Write( (byte) 0x73 );
            _writer.Write( value );
        }
    }
}