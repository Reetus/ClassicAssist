using System;
using System.Globalization;
using System.Windows.Data;

namespace ClassicAssist.UI.Misc.ValueConverters
{
    public class DateTimeToTimeSpanValueConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return !( value is DateTime dt ) ? (object) null : (DateTime.Now - dt).ToString();
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}