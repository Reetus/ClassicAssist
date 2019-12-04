using System;
using System.Globalization;
using System.Windows.Data;
using ClassicAssist.Misc;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UI.Misc.ValueConverters
{
    public class LockStatusValueConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if ( !( value is LockStatus ) )
            {
                return value;
            }

            LockStatus lockStatus = (LockStatus) value;

            switch ( lockStatus )
            {
                case LockStatus.Up:

                    return Properties.Resources.arrow_up.ToImageSource();
                case LockStatus.Down:

                    return Properties.Resources.arrow_down.ToImageSource();
                case LockStatus.Locked:

                    return Properties.Resources.padlock.ToImageSource();
                default:

                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}