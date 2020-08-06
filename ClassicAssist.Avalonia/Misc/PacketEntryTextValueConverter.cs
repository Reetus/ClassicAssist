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
using System.Text;
using Avalonia.Data.Converters;
using ClassicAssist.UI.Misc;

namespace ClassicAssist.Avalonia.Misc
{
    public class PacketEntryTextValueConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if ( !( value is PacketEntry packetEntry ) )
                return string.Empty;

            if (packetEntry?.Data == null)
            {
                return string.Empty;
            }

            StringBuilder textBuilder = new StringBuilder();

            for (int i = 0; i < packetEntry.Length; i++)
            {
                byte b1 = packetEntry.Data[i];

                if (b1 < 0x20 || b1 == 0xB7 || b1 == 0xFF)
                {
                    b1 = (byte)'.';
                }

                textBuilder.Append( (char)b1 );

                if ((i + 1) % 16 == 0)
                {
                    textBuilder.AppendLine();
                }
            }

            return textBuilder.ToString();
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}