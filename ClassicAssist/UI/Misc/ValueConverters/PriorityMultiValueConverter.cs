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

using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ClassicAssist.UI.Misc.ValueConverters
{
    public class PriorityMultiValueConverter : IMultiValueConverter
    {
        //https://stackoverflow.com/questions/1915562/wpf-binding-fallbackvalue-set-to-binding
        public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
        {
            return values.FirstOrDefault( o => o != null );
        }

        public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}