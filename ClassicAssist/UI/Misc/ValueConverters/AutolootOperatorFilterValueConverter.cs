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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using ClassicAssist.Data.Autoloot;

namespace ClassicAssist.UI.Misc.ValueConverters
{
    public class AutolootOperatorFilterValueConverter : IMultiValueConverter

    {
        public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
        {
            if ( values.Length < 2 || !( values[0] is Type enumType ) )
            {
                return null;
            }

            IEnumerable<object> allValues = Enum.GetValues( enumType ).Cast<object>();

            if ( !(values[1] is PropertyEntry entry) || entry.AllowedOperators == AutolootAllowedOperators.All )
            {
                return allValues;
            }

            List<object> filtered = new List<object>();

            if ( entry.AllowedOperators.HasFlag( AutolootAllowedOperators.Equal ) )
            {
                filtered.Add( AutolootOperator.Equal );
            }

            if ( entry.AllowedOperators.HasFlag( AutolootAllowedOperators.NotEqual ) )
            {
                filtered.Add( AutolootOperator.NotEqual );
            }

            if ( entry.AllowedOperators.HasFlag( AutolootAllowedOperators.LessThan ) )
            {
                filtered.Add( AutolootOperator.LessThan );
            }

            if ( entry.AllowedOperators.HasFlag( AutolootAllowedOperators.GreaterThan ) )
            {
                filtered.Add( AutolootOperator.GreaterThan );
            }

            if ( entry.AllowedOperators.HasFlag( AutolootAllowedOperators.NotPresent ) )
            {
                filtered.Add( AutolootOperator.NotPresent );
            }

            return filtered;
        }

        public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}