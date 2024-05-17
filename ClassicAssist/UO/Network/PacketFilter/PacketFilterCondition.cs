using System.Collections.Generic;
using System.Linq;

namespace ClassicAssist.UO.Network.PacketFilter
{
    public class PacketFilterCondition
    {
        private readonly byte[] _bytes;

        public PacketFilterCondition( int position, byte[] bytes, int length, bool negate = false )
        {
            Position = position;
            _bytes = bytes;
            Length = length;
            Negate = negate;
        }

        public int Length { get; }
        public bool Negate { get; }
        public int Position { get; }

        public override bool Equals( object obj )
        {
            if ( !( obj is PacketFilterCondition pfc ) )
            {
                return false;
            }

            return Position == pfc.Position && Length == pfc.Length && _bytes.SequenceEqual( pfc.GetBytes() ) && Negate == pfc.Negate;
        }

        public byte[] GetBytes()
        {
            return _bytes;
        }

        public override int GetHashCode()
        {
            int hashCode = -285373213;
            hashCode = hashCode * -1521134295 + Position.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<byte[]>.Default.GetHashCode( _bytes );
            hashCode = hashCode * -1521134295 + Length.GetHashCode();

            return hashCode;
        }
    }
}