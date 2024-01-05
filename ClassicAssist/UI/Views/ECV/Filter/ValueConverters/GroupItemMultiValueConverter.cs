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
using System.Windows.Data;
using ClassicAssist.UI.Views.ECV.Filter.Models;

namespace ClassicAssist.UI.Views.ECV.Filter.ValueConverters
{
    public class GroupItemMultiValueConverter : IMultiValueConverter
    {
        public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
        {
            return new GroupItem
            {
                Group = (ObservableCollection<EntityCollectionFilterItem>) values[0],
                Item = (EntityCollectionFilterItem) values[1]
            };
        }

        public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}