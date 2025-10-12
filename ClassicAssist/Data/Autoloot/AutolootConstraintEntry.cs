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

using System.Collections.ObjectModel;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.Data.Autoloot
{
    public class AutolootConstraintEntry : SetPropertyNotifyChanged
    {
        private string _additional;
        private AutolootOperator _operator = AutolootOperator.Equal;
        private PropertyEntry _property;
        private int _value;
        private ObservableCollection<int> _values;

        public string Additional
        {
            get => _additional;
            set => SetProperty( ref _additional, value );
        }

        public AutolootOperator Operator
        {
            get => _operator;
            set => SetProperty( ref _operator, value );
        }

        public PropertyEntry Property
        {
            get => _property;
            set => SetProperty( ref _property, value );
        }

        public int Value
        {
            get => _value;
            set => SetProperty( ref _value, value );
        }

        public ObservableCollection<int> Values
        {
            get => _values;
            set => SetProperty( ref _values, value );
        }
    }
}