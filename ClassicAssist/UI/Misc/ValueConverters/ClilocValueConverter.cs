#region License

// Copyright (C) $CURRENT_YEAR$ Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System;
using System.Globalization;
using System.Windows.Data;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UI.Misc.ValueConverters
{
    public class ClilocValueConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if ( !( value is int cliloc ) )
            {
                return null;
            }

            if ( cliloc == -1 )
            {
                return cliloc;
            }

            return Cliloc.GetProperty( cliloc );
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}