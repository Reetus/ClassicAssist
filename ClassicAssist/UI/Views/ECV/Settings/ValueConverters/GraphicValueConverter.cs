#region License

// Copyright (C) 2024 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System;
using System.Globalization;
using System.Windows.Data;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UI.Views.ECV.Settings.ValueConverters
{
    public class GraphicValueConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if ( !( value is int itemId ) )
            {
                return value;
            }

            string name = TileData.GetStaticTile( itemId ).Name;

            return string.IsNullOrEmpty( name ) ? string.Empty : $"({name})";
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}