using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ClassicAssist.Shared;

namespace ClassicAssist.UI.Misc.ValueConverters
{
    public class StringToResourceConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if ( value is string )
            {
                if ( targetType.Equals( typeof( ImageSource ) ) )
                {
                    return BitmapFrame.Create( Engine.GetResourceStream( (string) value ), BitmapCreateOptions.None,
                        BitmapCacheOption.OnLoad );
                }
            }

            return value;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}