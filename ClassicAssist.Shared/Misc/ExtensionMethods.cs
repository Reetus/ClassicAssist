#region License

// Copyright (C) 2021 Reetus
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

using System.Collections.Generic;
using System.Linq;

namespace ClassicAssist.Shared.Misc
{
    public static class ExtensionMethods
    {
        public static void AddRangeSorted<T>( this IList<T> list, IEnumerable<T> items )
        {
            foreach ( T item in items )
            {
                list.AddSorted( item );
            }
        }

        public static void AddSorted<T>( this IList<T> list, T item, IComparer<T> comparer = null )
        {
            if ( comparer == null )
            {
                comparer = Comparer<T>.Default;
            }

            int i = 0;

            while ( i < list.Count && comparer.Compare( list[i], item ) < 0 )
            {
                i++;
            }

            list.Insert( i, item );
        }

        public static IEnumerable<IEnumerable<T>> Split<T>( this IEnumerable<T> source, int chunkSize )
        {
            IEnumerable<T> enumerable = source.ToList();

            return enumerable.Where( ( x, i ) => i % chunkSize == 0 )
                .Select( ( x, i ) => enumerable.Skip( i * chunkSize ).Take( chunkSize ) );
        }
    }
}