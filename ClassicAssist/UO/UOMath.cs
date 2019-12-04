using System;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UO
{
    public static class UOMath
    {
        public static Direction MapDirection( float x1, float y1, float x2, float y2 )
        {
            float slope = ( y2 - y1 ) / ( x2 - x1 );

            if ( float.IsNaN( slope ) )
            {
                return Direction.Invalid;
            }

            if ( slope >= 2f && slope <= float.PositiveInfinity ||
                 slope <= -2f && slope >= float.NegativeInfinity )
            {
                return y2 < y1 ? Direction.North : Direction.South;
            }

            if ( slope <= .5f && slope >= 0f || slope >= -.5f && slope <= 0f )
            {
                return x2 < x1 ? Direction.West : Direction.East;
            }

            if ( slope <= -.5f && slope >= -1f || slope >= -2f && slope <= -1f )
            {
                return y2 < y1 ? Direction.Northeast : Direction.Southwest;
            }

            if ( slope <= 2f && slope >= 1 || slope >= .5f && slope <= 1 )
            {
                return y2 < y1 ? Direction.Northwest : Direction.Southeast;
            }

            return Direction.Invalid;
        }

        public static double Distance( int x1, int y1, int z1, int x2, int y2, int z2 )
        {
            int d1 = Math.Abs( x1 - x2 );
            int d2 = Math.Abs( y1 - y2 );
            int d3 = Math.Abs( z1 - z2 );
            return Math.Sqrt( d1 * d1 + d2 * d2 + d3 * d3 );
        }

        public static double Distance( int x1, int y1, int x2, int y2 )
        {
            int d1 = Math.Abs( x1 - x2 );
            int d2 = Math.Abs( y1 - y2 );
            return Math.Sqrt( d1 * d1 + d2 * d2 );
        }

        public static bool IsMobile( int serial )
        {
            return (uint) serial < 0x40000000;
        }
    }
}