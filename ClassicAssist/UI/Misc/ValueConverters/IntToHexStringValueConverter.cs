﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace ClassicAssist.UI.Misc.ValueConverters
{
    public class IntToHexStringValueConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if ( value is int val )
            {
                return $"0x{val:x8}";
            }

            return value;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if ( value is string val )
            {
                return val.StartsWith( "0x" ) ? System.Convert.ToInt32( val, 16 ) : System.Convert.ToInt32( val );
            }

            return value;
        }
    }
}