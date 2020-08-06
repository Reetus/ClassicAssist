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

using System.Collections.ObjectModel;
using System.Text;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.Shared.UI.ViewModels.Debug
{
    public class DebugMenusViewModel : BaseViewModel
    {
        private ObservableCollection<Menu> _items = new ObservableCollection<Menu>();
        private Menu _selectedItem;
        private string _text;

        public DebugMenusViewModel()
        {
            if ( Engine.Menus.GetMenus( out Menu[] menus ) )
            {
                foreach ( Menu menu in menus )
                {
                    Items.Add( menu );
                }
            }

            Engine.Menus.CollectionChangedEvent += OnCollectionChangedEvent;
        }

        public ObservableCollection<Menu> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public Menu SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty( ref _selectedItem, value );
                UpdateText( value );
            }
        }

        public string Text
        {
            get => _text;
            set => SetProperty( ref _text, value );
        }

        private void UpdateText( Menu menu )
        {
            if ( menu == null )
            {
                Text = string.Empty;
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine( $"GumpID: 0x{menu.ID:x4}" );
            sb.AppendLine( $"Serial: 0x{menu.Serial:x8}" );
            sb.AppendLine( $"Title: {menu.Title}" );
            sb.AppendLine( $"Entries: {menu.Entries?.Length ?? 0}" );
            sb.AppendLine();
            sb.AppendLine( "Entries:" );

            if ( menu.Entries != null )
            {
                foreach ( MenuEntry menuEntry in menu.Entries )
                {
                    sb.AppendLine( $"Index: {menuEntry.Index}" );
                    sb.AppendLine( $"ID: 0x{menuEntry.ID:x4}" );
                    sb.AppendLine( $"Hue: {menuEntry.Hue}" );
                    sb.AppendLine( $"Title: {menuEntry.Title}" );
                    sb.AppendLine();
                }
            }

            Text = sb.ToString();
        }

        private void OnCollectionChangedEvent( Menu[] menus )
        {
            _dispatcher.Invoke( () =>
            {
                Items.Clear();

                if ( menus == null )
                {
                    return;
                }

                foreach ( Menu menu in menus )
                {
                    Items.Add( menu );
                }
            } );
        }
    }
}