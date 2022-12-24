using System;
using System.Globalization;
using System.Windows.Data;

namespace ClassicAssist.Shared.UI.ValueConverters
{
    public class InverseBooleanValueConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            try
            {
                bool testValue = value != null && (bool) value;
                return !testValue; // or do whatever you need with this boolean
            }
            catch
            {
                return true;
            } // or false
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return Convert( value, targetType, parameter, culture );
        }
    }
}