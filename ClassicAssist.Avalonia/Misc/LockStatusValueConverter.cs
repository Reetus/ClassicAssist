using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ClassicAssist.UO.Data;

namespace ClassicAssist.Avalonia.Misc
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

                    return new Bitmap( AvaloniaLocator.Current.GetService<IAssetLoader>()
                        .Open( new Uri( "avares://ClassicAssist.Avalonia/Assets/arrow_up.png" ) ) );
                case LockStatus.Down:

                    return new Bitmap( AvaloniaLocator.Current.GetService<IAssetLoader>()
                        .Open( new Uri( "avares://ClassicAssist.Avalonia/Assets/arrow_down.png" ) ) );

                case LockStatus.Locked:

                    return new Bitmap( AvaloniaLocator.Current.GetService<IAssetLoader>()
                        .Open( new Uri( "avares://ClassicAssist.Avalonia/Assets/lock.png" ) ) );

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