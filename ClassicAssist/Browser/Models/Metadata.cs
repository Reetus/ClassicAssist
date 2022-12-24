﻿#region License

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

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassicAssist.Annotations;
using ClassicAssist.Browser.Data;
using Newtonsoft.Json;

namespace ClassicAssist.Browser.Models
{
    public class Metadata : IComparable<Metadata>, INotifyPropertyChanged
    {
        private string _macro;
        public string Author { get; set; }

        [JsonConverter( typeof( CategoriesConverter ) )]
        public string[] Categories { get; set; }

        public string Description { get; set; }
        public string Era { get; set; }
        public string FileName { get; set; }
        public string Id { get; set; }

        [JsonIgnore]
        public string Macro
        {
            get => _macro;
            set
            {
                _macro = value;
                OnPropertyChanged();
            }
        }

        public DateTime ModifiedDate { get; set; }
        public string Name { get; set; }
        public string SHA1 { get; set; }
        public string Shard { get; set; }
        public int Size { get; set; }

        public int CompareTo( Metadata other )
        {
            if ( ReferenceEquals( this, other ) )
            {
                return 0;
            }

            if ( ReferenceEquals( null, other ) )
            {
                return 1;
            }

            return string.Compare( Name, other.Name, StringComparison.Ordinal );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}