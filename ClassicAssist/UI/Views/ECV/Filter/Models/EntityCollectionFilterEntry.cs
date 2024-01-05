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
using ClassicAssist.Shared.UI;

namespace ClassicAssist.UI.Views.ECV.Filter.Models
{
    public class EntityCollectionFilterEntry : SetPropertyNotifyChanged
    {
        private ObservableCollection<EntityCollectionFilterGroup> _groups =
            new ObservableCollection<EntityCollectionFilterGroup>();

        private Guid _id = Guid.NewGuid();

        private string _name;

        public ObservableCollection<EntityCollectionFilterGroup> Groups
        {
            get => _groups;
            set => SetProperty( ref _groups, value );
        }

        public Guid ID
        {
            get => _id;
            set => SetProperty( ref _id, value );
        }

        public string Name
        {
            get => _name;
            set => SetProperty( ref _name, value );
        }
    }
}