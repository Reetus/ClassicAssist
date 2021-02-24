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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassicAssist.Annotations;

namespace ClassicAssist.Data.Vendors
{
    public class VendorBuyAgentEntry : INotifyPropertyChanged
    {
        private bool _enabled;
        private bool _includeBackpackAmount;
        private ObservableCollection<VendorBuyAgentItem> _items = new ObservableCollection<VendorBuyAgentItem>();
        private string _name;

        public bool Enabled
        {
            get => _enabled;
            set => SetProperty( ref _enabled, value );
        }

        public bool IncludeBackpackAmount
        {
            get => _includeBackpackAmount;
            set => SetProperty( ref _includeBackpackAmount, value );
        }

        public ObservableCollection<VendorBuyAgentItem> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public string Name
        {
            get => _name;
            set => SetProperty( ref _name, value );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        // ReSharper disable once RedundantAssignment
        public virtual void SetProperty<T>( ref T obj, T value, [CallerMemberName] string propertyName = "" )
        {
            obj = value;
            OnPropertyChanged( propertyName );
        }

        public override string ToString()
        {
            return Name;
        }
    }
}