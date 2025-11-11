#region License

// Copyright (C) 2025 Reetus
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
using System.Collections.ObjectModel;
using ClassicAssist.Shared.UI;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json;

namespace ClassicAssist.Data.Autoloot
{
    public class PropertyEntry : SetPropertyNotifyChanged, IComparable<PropertyEntry>
    {
        private AutolootAllowedOperators _allowedOperators;
        private Type _allowedValuesEnum;
        private int _clilocIndex;
        private int[] _clilocs;
        private PropertyType _constraintType;
        private string _name;
        private ObservableCollection<string> _options;
        private Func<Entity, AutolootConstraintEntry, bool> _predicate;
        private string _shortName;
        private bool _useMultipleValues;

        public AutolootAllowedOperators AllowedOperators
        {
            get => _allowedOperators;
            set => SetProperty( ref _allowedOperators, value );
        }

        [JsonIgnore]
        public Type AllowedValuesEnum
        {
            get => _allowedValuesEnum;
            set => SetProperty( ref _allowedValuesEnum, value );
        }

        public int ClilocIndex
        {
            get => _clilocIndex;
            set => SetProperty( ref _clilocIndex, value );
        }

        public int[] Clilocs
        {
            get => _clilocs;
            set => SetProperty( ref _clilocs, value );
        }

        public PropertyType ConstraintType
        {
            get => _constraintType;
            set => SetProperty( ref _constraintType, value );
        }

        public string Name
        {
            get => _name;
            set => SetProperty( ref _name, value );
        }

        public ObservableCollection<string> Options
        {
            get => _options;
            set => SetProperty( ref _options, value );
        }

        [JsonIgnore]
        public Func<Entity, AutolootConstraintEntry, bool> Predicate
        {
            get => _predicate;
            set => SetProperty( ref _predicate, value );
        }

        public string ShortName
        {
            get => _shortName;
            set => SetProperty( ref _shortName, value );
        }

        public bool UseMultipleValues
        {
            get => _useMultipleValues;
            set => SetProperty( ref _useMultipleValues, value );
        }

        public int CompareTo( PropertyEntry other )
        {
            if ( ReferenceEquals( this, other ) )
            {
                return 0;
            }

            if ( ReferenceEquals( null, other ) )
            {
                return 1;
            }

            int nameComparison = string.Compare( _name, other._name, StringComparison.InvariantCultureIgnoreCase );

            return nameComparison;
        }
    }
}