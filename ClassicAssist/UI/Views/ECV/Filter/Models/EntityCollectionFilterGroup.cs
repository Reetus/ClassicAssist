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

using System.Collections.ObjectModel;
using System.ComponentModel;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Misc;

namespace ClassicAssist.UI.Views.ECV.Filter.Models
{
    public class EntityCollectionFilterGroup : SetPropertyNotifyChanged
    {
        private ObservableCollection<EntityCollectionFilterItem> _items =
            new ObservableCollection<EntityCollectionFilterItem>();

        private BooleanOperation _operation;

        public ObservableCollection<EntityCollectionFilterItem> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public BooleanOperation Operation
        {
            get => _operation;
            set => SetProperty( ref _operation, value );
        }
    }

    [TypeConverter( typeof( EnumDescriptionTypeConverter ) )]
    public enum BooleanOperation
    {
        [Description( "And (&&)" )]
        And,

        [Description( "Or (||)" )]
        Or,

        [Description( "Not (!)" )]
        Not
    }
}