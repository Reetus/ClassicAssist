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
using System.Collections;
using System.ComponentModel;
using ClassicAssist.Launcher.Properties;

namespace ClassicAssist.Launcher
{
    public class ShardListViewComparer : IComparer
    {
        private readonly ListSortDirection _direction;
        private readonly string _propertyName;

        public ShardListViewComparer( ListSortDirection direction, string propertyName )
        {
            _direction = direction;
            _propertyName = propertyName;
        }

        public int Compare( object x, object y )
        {
            if ( !( x is ShardEntry first ) || !( y is ShardEntry second ) )
            {
                return -1;
            }

            int result = 0;

            switch ( _propertyName )
            {
                case nameof( Resources.Shard ):
                    result = string.Compare( first.Name, second.Name, StringComparison.Ordinal );
                    break;
                case nameof( Resources.Address ):
                    result = string.Compare( first.Address, second.Address, StringComparison.Ordinal );
                    break;
                case nameof( Resources.Port ):
                    result = first.Port.CompareTo( second.Port );
                    break;
                case nameof( Resources.Encryption ):
                    result = first.Encryption.CompareTo( second.Encryption );
                    break;
                case nameof( Resources.Players ):

                    int firstPlayers = string.IsNullOrEmpty( first.Status ) || first.Status.Equals( "-" )
                        ? 0
                        : int.Parse( first.Status );

                    int secondPlayers = string.IsNullOrEmpty( second.Status ) || second.Status.Equals( "-" )
                        ? 0
                        : int.Parse( second.Status );

                    result = firstPlayers.CompareTo( secondPlayers );

                    break;
                case nameof( Resources.Ping ):
                    int firstPing = string.IsNullOrEmpty( first.Ping ) || first.Ping.Equals( "-" )
                        ? 0
                        : int.Parse( first.Ping );

                    int secondPing = string.IsNullOrEmpty( second.Ping ) || second.Ping.Equals( "-" )
                        ? 0
                        : int.Parse( second.Ping );

                    result = firstPing.CompareTo( secondPing );

                    break;
            }

            if ( _direction == ListSortDirection.Descending )
            {
                result = -result;
            }

            return result;
        }
    }
}