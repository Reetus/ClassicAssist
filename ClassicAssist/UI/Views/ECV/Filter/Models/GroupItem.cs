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

namespace ClassicAssist.UI.Views.ECV.Filter.Models
{
    public class GroupItem
    {
        public ObservableCollection<EntityCollectionFilterItem> Group { get; set; }
        public EntityCollectionFilterItem Item { get; set; }
    }
}