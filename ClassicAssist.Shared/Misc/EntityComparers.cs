using System;
using System.Collections.Generic;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.Misc
{
    public class NameThenSerialComparer : IComparer<Entity>
    {
        public int Compare( Entity x, Entity y )
        {
            int result = string.Compare( x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase );

            return result == 0 ? x.Serial.CompareTo( y.Serial ) : result;
        }
    }

    public class SerialComparer : IComparer<Entity>
    {
        public int Compare( Entity x, Entity y )
        {
            if ( x == null || y == null )
            {
                return -1;
            }

            return x.Serial.CompareTo( y.Serial );
        }
    }

    public class HueThenAmountComparer : IComparer<Entity>
    {
        public int Compare( Entity x, Entity y )
        {
            if ( x == null || y == null )
            {
                return -1;
            }

            int result = x.Hue.CompareTo( y.Hue );

            return result == 0 ? new QuantityThenSerialComparer().Compare( x, y ) : result;
        }
    }

    public class IDThenSerialComparer : IComparer<Entity>
    {
        public int Compare( Entity x, Entity y )
        {
            if ( x == null || y == null )
            {
                return -1;
            }

            int result = x.ID.CompareTo( y.ID );

            return result == 0 ? x.Serial.CompareTo( y.Serial ) : result;
        }
    }

    public class QuantityThenSerialComparer : IComparer<Entity>
    {
        public int Compare( Entity x, Entity y )
        {
            if ( x == null || y == null )
            {
                return -1;
            }

            if ( !( x is Item itemX ) || !( y is Item itemY ) )
            {
                return 0;
            }

            int result = itemX.Count.CompareTo( itemY.Count );

            return result == 0 ? x.Serial.CompareTo( y.Serial ) : result;
        }
    }
}