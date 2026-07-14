using System;
using System.Globalization;
using System.Windows.Data;

namespace ClassicAssist.UI.Misc.ValueConverters
{
    public class CellWidthValueConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            double subtractValue = 15;

            if ( parameter != null )
            {
                double parsed = double.Parse( parameter.ToString(), CultureInfo.InvariantCulture );
                subtractValue = parsed < 0 ? 0 : parsed;
            }

            return (double?) value - subtractValue;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            double subtractValue = 15;

            if ( parameter != null )
            {
                double parsed = double.Parse( parameter.ToString(), CultureInfo.InvariantCulture );
                subtractValue = parsed < 0 ? 0 : parsed;
            }

            return (double?) value + subtractValue;
        }
    }
}