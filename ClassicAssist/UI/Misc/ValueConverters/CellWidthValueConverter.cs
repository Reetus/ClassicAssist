using System;
using System.Globalization;
using System.Windows.Data;

namespace ClassicAssist.UI.Misc.ValueConverters
{
    public class CellWidthValueConverter : IValueConverter
    {
        private double substractValue = 15;

        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if ( parameter != null )
            {
                substractValue = double.Parse( parameter.ToString() );
            }

            double? val = (double?) value - substractValue;
            return val;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return (double?) value + substractValue;
        }
    }
}