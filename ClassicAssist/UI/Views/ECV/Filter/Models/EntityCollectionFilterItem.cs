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
using ClassicAssist.Data.Autoloot;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.UI.Views.ECV.Filter.Models
{
    public class EntityCollectionFilterItem : SetPropertyNotifyChanged
    {
        private string _additional;
        private PropertyEntry _constraint;
        private bool _enabled = true;
        private AutolootOperator _operator = AutolootOperator.Equal;
        private int _value;
        private ObservableCollection<int> _values = new ObservableCollection<int>();

        public string Additional
        {
            get => _additional;
            set => SetProperty( ref _additional, value );
        }

        public PropertyEntry Constraint
        {
            get => _constraint;
            set => SetProperty( ref _constraint, value );
        }

        public bool Enabled
        {
            get => _enabled;
            set => SetProperty( ref _enabled, value );
        }

        public AutolootOperator Operator
        {
            get => _operator;
            set => SetProperty( ref _operator, value );
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