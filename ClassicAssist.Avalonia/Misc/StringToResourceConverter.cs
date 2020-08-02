using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ClassicAssist.Shared;

namespace ClassicAssist.Avalonia.Misc
{
    public class StringToResourceConverter : IValueConverter
    {
        private static readonly Dictionary<string, Bitmap> _bitmaps = new Dictionary<string, Bitmap>();

        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if ( value is string key )
            {
                if ( targetType.Equals( typeof( IImage ) ) )
                {
                    if ( _bitmaps.TryGetValue( key, out Bitmap bitmap ) )
                    {
                        return bitmap;
                    }

                    bitmap = new Bitmap( Engine.GetResourceStream( key ) );
                    _bitmaps.Add( key, bitmap );
                    return bitmap;
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