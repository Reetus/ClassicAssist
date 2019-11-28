namespace ClassicAssist.UO.Network.Packets
{
    public class PingPacket : Packets
    {
        public PingPacket(byte value) : base(2)
        {
            _writer.Write( (byte) 0x73 );
            _writer.Write( value );
        }
    }
}