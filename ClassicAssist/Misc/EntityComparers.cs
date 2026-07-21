#region License

// Copyright (C) 2026 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Assistant;
using ClassicAssist.UO.Data;
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

    public class WeightThenSerialComparer : IComparer<Entity>
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

            if ( !Engine.CharacterListFlags.HasFlag( CharacterListFlags.PaladinNecromancerClassTooltips ) )
            {
                byte weightX = TileData.GetStaticTile( itemX.ID ).Weight;
                byte weightY = TileData.GetStaticTile( itemY.ID ).Weight;

                int result = weightX.CompareTo( weightY );

                return result == 0 ? x.Serial.CompareTo( y.Serial ) : result;
            }
            else
            {
                int itemXWeight = Convert.ToInt32( itemX.Properties?.FirstOrDefault( p => p.Cliloc == 1072225 || p.Cliloc == 1072788 )?.Arguments[0] ?? "-1" );
                int itemYWeight = Convert.ToInt32( itemY.Properties?.FirstOrDefault( p => p.Cliloc == 1072225 || p.Cliloc == 1072788 )?.Arguments[0] ?? "-1" );

                if ( itemXWeight == -1 )
                {
                    itemXWeight = TileData.GetStaticTile( itemX.ID ).Weight;
                }

                if ( itemYWeight == -1 )
                {
                    itemYWeight = TileData.GetStaticTile( itemY.ID ).Weight;
                }

                int result = itemXWeight.CompareTo( itemYWeight );

                return result == 0 ? x.Serial.CompareTo( y.Serial ) : result;
            }
        }
    }
}