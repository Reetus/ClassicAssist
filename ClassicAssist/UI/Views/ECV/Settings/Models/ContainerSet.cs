#region License

// Copyright (C) $CURRENT_YEAR$ Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System.Collections.Generic;
using System.Collections.ObjectModel;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.UI.Views.ECV.Settings.Models
{
    public class ContainerSet : SetPropertyNotifyChanged
    {
        private ObservableCollection<int> _items = new ObservableCollection<int>();
        private string _name;

        public ObservableCollection<int> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public string Name
        {
            get => _name;
            set => SetProperty( ref _name, value );
        }
    }
}