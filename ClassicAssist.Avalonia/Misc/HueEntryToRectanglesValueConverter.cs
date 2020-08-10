#region License

// Copyright (C) 2020 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using ClassicAssist.UO.Data;

namespace ClassicAssist.Avalonia.Misc
{
    public class HueEntryToRectanglesValueConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if ( !( value is HueEntry entry ) )
            {
                return null;
            }

            StackPanel panel = new StackPanel { Orientation = Orientation.Horizontal };

            for ( int i = 0; i < 32; i++ )
            {
                SolidColorBrush brush = new SolidColorBrush( Convert555ToARGB( entry.Colors[i] ) );

                panel.Children.Add( new Rectangle { Fill = brush, Width = 10, Height = 10 } );
            }

            return panel;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }

        protected static Color Convert555ToARGB( short Col )
        {
            int r = ( (short) ( Col >> 10 ) & 31 ) * 8;
            int g = ( (short) ( Col >> 5 ) & 31 ) * 8;
            int b = ( Col & 31 ) * 8;
            return Color.FromArgb( 255, (byte) r, (byte) g, (byte) b );
        }
    }
}