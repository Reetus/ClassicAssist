#region License

// Copyright (C) 2020 Reetus
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

using System.Collections.Concurrent;
using System.Linq;

namespace ClassicAssist.UO.Objects
{
    public class MenuCollection
    {
        private readonly ConcurrentDictionary<int, Menu> _dictionary;

        public MenuCollection()
        {
            _dictionary = new ConcurrentDictionary<int, Menu>();
        }

        public void Add( Menu menu )
        {
            bool result = _dictionary.AddOrUpdate( menu.ID, menu, ( k, v ) => menu ) != null;

            if ( result )
            {
                OnCollectionChanged();
            }
        }

        public bool Remove( int id, int buttonId = 0 )
        {
            bool result = _dictionary.TryRemove( id, out Menu m );

            m?.OnResponse( buttonId );

            if ( result )
            {
                OnCollectionChanged();
            }

            return result;
        }

        public bool GetMenu( int id, out Menu menu )
        {
            return _dictionary.TryGetValue( id, out menu );
        }

        public bool FindMenu( int serial, out Menu menu )
        {
            menu = _dictionary.Values.FirstOrDefault( g => g.Serial == serial );

            return menu != null;
        }

        public bool GetMenus( out Menu[] menus )
        {
            menus = null;

            if ( _dictionary.Values.Count == 0 )
            {
                return false;
            }

            menus = _dictionary.Values.ToArray();

            return menus.Length > 0;
        }

        public void Clear()
        {
            int previousCount = _dictionary.Count;

            _dictionary.Clear();

            if ( previousCount > 0 )
            {
                OnCollectionChanged();
            }
        }

        private void OnCollectionChanged()
        {
        }
    }
}