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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassicAssist.Annotations;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Data.Misc
{
    public class EntityCollectionViewerOptions : INotifyPropertyChanged
    {
        private bool _alwaysOnTop;
        private bool _showChildItems;

        public bool AlwaysOnTop
        {
            get => _alwaysOnTop;
            set => SetProperty( ref _alwaysOnTop, value );
        }

        public bool ShowChildItems
        {
            get => _showChildItems;
            set => SetProperty( ref _showChildItems, value );
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

        public void Deserialize( JToken config )
        {
            if ( config == null )
            {
                return;
            }

            AlwaysOnTop = config["AlwaysOnTop"]?.ToObject<bool>() ?? false;
            ShowChildItems = config["ShowChildItems"]?.ToObject<bool>() ?? false;
        }

        public JToken Serialize()
        {
            JObject config = new JObject { { "AlwaysOnTop", AlwaysOnTop }, { "ShowChildItems", ShowChildItems } };

            return config;
        }
    }
}