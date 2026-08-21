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
using System.Collections.Specialized;
using ClassicAssist.Controls.DraggableTreeView;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.UI.Views.ECV.Filter.Models
{
    public class EntityCollectionFilterEntry : SetPropertyNotifyChanged
    {
        private ObservableCollection<IDraggable> _groups = new ObservableCollection<IDraggable>();

        private Guid _id = Guid.NewGuid();

        private string _name;

        public EntityCollectionFilterEntry()
        {
            _groups.CollectionChanged += OnGroupsChanged;
        }

        public ObservableCollection<IDraggable> Groups
        {
            get => _groups;
            set
            {
                if ( _groups != null )
                {
                    _groups.CollectionChanged -= OnGroupsChanged;
                }

                SetProperty( ref _groups, value );

                if ( _groups != null )
                {
                    _groups.CollectionChanged += OnGroupsChanged;
                }

                UpdateGroupsFirstFlags();
            }
        }

        private void OnGroupsChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            UpdateGroupsFirstFlags();
        }

        public void UpdateGroupsFirstFlags()
        {
            for ( int i = 0; i < _groups.Count; i++ )
            {
                if ( _groups[i] is EntityCollectionFilterGroup g )
                {
                    g.IsFirst = i == 0;
                    g.UpdateChildrenFirstFlags();
                }
            }
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