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

using System;
using System.Collections.Generic;

namespace ClassicAssist.Launcher
{
    public class ShardEntryComparer : IComparer<ShardEntry>
    {
        public int Compare( ShardEntry x, ShardEntry y )
        {
            if ( ReferenceEquals( x, y ) )
            {
                return 0;
            }

            if ( ReferenceEquals( null, y ) )
            {
                return 1;
            }

            if ( ReferenceEquals( null, x ) )
            {
                return -1;
            }

            int result = y.Encryption.CompareTo( x.Encryption );

            if ( result == 0 )
            {
                result = y.IsPreset.CompareTo( x.IsPreset );
            }

            return result == 0 ? string.Compare( x.Name, y.Name, StringComparison.Ordinal ) : result;
        }
    }
}