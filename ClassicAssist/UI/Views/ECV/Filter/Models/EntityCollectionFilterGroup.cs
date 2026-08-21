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
using System.Collections.Specialized;
using System.ComponentModel;
using ClassicAssist.Controls.DraggableTreeView;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Misc;

namespace ClassicAssist.UI.Views.ECV.Filter.Models
{
    public class EntityCollectionFilterGroup : SetPropertyNotifyChanged, IDraggableGroup
    {
        private ObservableCollection<IDraggable> _children = new ObservableCollection<IDraggable>();

        private bool _isFirst = true;
        private ObservableCollection<EntityCollectionFilterItem> _items =
            new ObservableCollection<EntityCollectionFilterItem>();

        private string _name;
        private BooleanOperation _operation;

        public EntityCollectionFilterGroup()
        {
            _children.CollectionChanged += OnChildrenCollectionChanged;
            _items.CollectionChanged += OnItemsCollectionChanged;
        }

        public ObservableCollection<IDraggable> Children
        {
            get => _children;
            set
            {
                if ( _children != null )
                {
                    _children.CollectionChanged -= OnChildrenCollectionChanged;
                }

                _children = value ?? new ObservableCollection<IDraggable>();
                OnPropertyChanged( nameof( Children ) );
                OnPropertyChanged( nameof( HasChildren ) );
                OnPropertyChanged( nameof( Name ) );

                if ( _children != null )
                {
                    _children.CollectionChanged += OnChildrenCollectionChanged;
                }

                UpdateChildrenFirstFlags();
            }
        }

        ObservableCollection<IDraggable> IDraggableGroup.Children
        {
            get => _children;
            set => Children = value;
        }

        public bool IsFirst
        {
            get => _isFirst;
            set => SetProperty( ref _isFirst, value );
        }

        public bool HasChildren => _children.Count > 0;

        public ObservableCollection<EntityCollectionFilterItem> Items
        {
            get => _items;
            set
            {
                if ( _items != null )
                {
                    _items.CollectionChanged -= OnItemsCollectionChanged;
                }

                SetProperty( ref _items, value );

                if ( _items != null )
                {
                    _items.CollectionChanged += OnItemsCollectionChanged;
                }

                OnPropertyChanged( nameof( Name ) );
            }
        }

        public string Name
        {
            get
            {
                if ( !string.IsNullOrEmpty( _name ) )
                {
                    return _name;
                }

                if ( HasChildren )
                {
                    return string.Format( Strings.Filter_Group_Subgroups, Children.Count );
                }

                return string.Format( Strings.Filter_Group_Filters, Items.Count );
            }
            set => SetProperty( ref _name, value );
        }

        public BooleanOperation Operation
        {
            get => _operation;
            set => SetProperty( ref _operation, value );
        }

        private void OnChildrenCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            OnPropertyChanged( nameof( HasChildren ) );
            OnPropertyChanged( nameof( Name ) );
            UpdateChildrenFirstFlags();
        }

        private void OnItemsCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            if ( !HasChildren )
            {
                OnPropertyChanged( nameof( Name ) );
            }
        }

        public void UpdateChildrenFirstFlags()
        {
            for ( int i = 0; i < _children.Count; i++ )
            {
                if ( _children[i] is EntityCollectionFilterGroup child )
                {
                    child.IsFirst = i == 0;
                    child.UpdateChildrenFirstFlags();
                }
            }
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
