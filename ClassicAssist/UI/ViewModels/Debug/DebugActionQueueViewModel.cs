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

using System.Collections.ObjectModel;
using System.Windows.Input;
using ClassicAssist.Shared.UI;
using ClassicAssist.UO.Network;

namespace ClassicAssist.UI.ViewModels.Debug
{
    public class DebugActionQueueViewModel : BaseViewModel
    {
        private ICommand _clearCommand;
        private bool _enabled;
        private ObservableCollection<Item> _items = new ObservableCollection<Item>();
        public ICommand ClearCommand => _clearCommand ?? ( _clearCommand = new RelayCommand( Clear, o => true ) );

        public bool Enabled
        {
            get => _enabled;
            set
            {
                SetProperty( ref _enabled, value );
                SetEnabled( value );
            }
        }

        public ObservableCollection<Item> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        private void Clear( object obj )
        {
            Items.Clear();
        }

        private void SetEnabled( bool value )
        {
            if ( value )
            {
                ActionPacketQueue.ActionQueueEvent += OnActionQueueEvent;
            }
            else
            {
                ActionPacketQueue.ActionQueueEvent -= OnActionQueueEvent;
            }
        }

        private void OnActionQueueEvent( ActionQueueEvents actionEvent, BaseQueueItem queueitem )
        {
            _dispatcher.Invoke( () =>
            {
                Item item = new Item
                {
                    ID = queueitem.UUID,
                    Caller = queueitem.Caller,
                    Event = actionEvent.ToString(),
                    Elapsed = queueitem.TimeSpan.ToString()
                };
                Items.Add( item );
            } );
        }

        public class Item
        {
            public string Caller { get; set; }
            public string Elapsed { get; set; }
            public string Event { get; set; }
            public string ID { get; set; }
        }
    }
}