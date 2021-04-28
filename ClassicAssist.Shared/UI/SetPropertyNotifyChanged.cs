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
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClassicAssist.Shared.UI
{
    public abstract class SetPropertyNotifyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        protected virtual void SetProperty<T>( ref T obj, T value, bool skipIfUnchanged = true,
            Action<T> afterChange = null, [CallerMemberName] string propertyName = "" )
        {
            if ( skipIfUnchanged && obj != null && obj.Equals( value ) )
            {
                return;
            }

            obj = value;
            OnPropertyChanged( propertyName );

            afterChange?.Invoke( obj );
        }
    }
}