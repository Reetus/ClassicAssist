using System.Collections.Generic;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.Data.Vendors
{
    public class XYComparer : IComparer<Item>
    {
        public int Compare( Item x, Item y )
        {
            if ( x == null && y == null )
            {
                return 0;
            }

            if ( x == null )
            {
                return -1;
            }

            if ( y == null )
            {
                return 1;
            }

            if ( x.X > y.X )
            {
                return 1;
            }

            if ( y.X > x.X )
            {
                return -1;
            }

            if ( x.X != y.X )
            {
                return 0;
            }

            if ( x.Y > y.Y )
            {
                return 1;
            }

            if ( y.Y > x.Y )
            {
                return -1;
            }

            return 0;
        }
    }
}