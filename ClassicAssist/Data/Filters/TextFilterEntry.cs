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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClassicAssist.Annotations;

namespace ClassicAssist.Data.Filters
{
    public class TextFilterEntry : INotifyPropertyChanged
    {
        private bool _enabled;
        private bool _excludeSelf;
        private string _match;
        private TextFilterMatchType _matchType;
        private TextFilterMessageType _messageTypes = TextFilterMessageType.All;

        public bool Enabled
        {
            get => _enabled;
            set => SetProperty( ref _enabled, value );
        }

        public bool ExcludeSelf
        {
            get => _excludeSelf;
            set => SetProperty( ref _excludeSelf, value );
        }

        public string Match
        {
            get => _match;
            set => SetProperty( ref _match, value );
        }

        public TextFilterMatchType MatchType
        {
            get => _matchType;
            set => SetProperty( ref _matchType, value );
        }

        public TextFilterMessageType MessageTypes
        {
            get => _messageTypes;
            set => SetProperty( ref _messageTypes, value );
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
            CommandManager.InvalidateRequerySuggested();
        }
    }
}