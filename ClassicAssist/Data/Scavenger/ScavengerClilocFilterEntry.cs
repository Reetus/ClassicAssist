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
using ClassicAssist.Annotations;

namespace ClassicAssist.Data.Scavenger
{
    public class ScavengerClilocFilterEntry : INotifyPropertyChanged
    {
        private int _cliloc;

        public int Cliloc
        {
            get => _cliloc;
            set
            {
                _cliloc = value;
                OnPropertyChanged( nameof( Property ) );
            }
        }

        public bool Enabled { get; set; }

        public string Property => UO.Data.Cliloc.GetProperty( Cliloc );
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}