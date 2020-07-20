using System;

namespace ClassicAssist.UO.Objects
{
    public class Packet
    {
        private readonly byte[] _data;
        private readonly int _length;

        public Packet( byte[] data, int length )
        {
            _data = new byte[length];
            _length = length;

            Buffer.BlockCopy( data, 0, _data, 0, length );
        }

        public byte GetPacketID()
        {
            return _data?[0] ?? 0;
        }

        public byte[] GetPacket()
        {
            return _data;
        }

        public int GetLength()
        {
            return _length;
        }
    }
}