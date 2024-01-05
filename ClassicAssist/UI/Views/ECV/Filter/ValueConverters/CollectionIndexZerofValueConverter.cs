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
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ClassicAssist.UI.Views.ECV.Filter.Models;

namespace ClassicAssist.UI.Views.ECV.Filter.ValueConverters
{
    public class CollectionIndexZeroVisibilityConverter : IMultiValueConverter
    {
        public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
        {
            if ( !( values[0] is ObservableCollection<EntityCollectionFilterGroup> collection ) ||
                 !( values[1] is EntityCollectionFilterGroup item ) )
            {
                return default( Visibility );
            }

            int index = collection.IndexOf( item );

            return index == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}