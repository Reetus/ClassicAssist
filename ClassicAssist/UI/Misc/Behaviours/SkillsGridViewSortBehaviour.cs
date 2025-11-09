#region License

// Copyright (C) 2025 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using ClassicAssist.Data.Skills;
using ClassicAssist.Shared.UI;
using Microsoft.Xaml.Behaviors;

namespace ClassicAssist.UI.Misc.Behaviours
{
    public class SkillsGridViewSortBehaviour : Behavior<ListView>
    {
        public static readonly DependencyProperty SortChangedCommandProperty =
            DependencyProperty.Register( nameof( SortChangedCommand ), typeof( ICommand ), typeof( SkillsGridViewSortBehaviour ) );

        public static readonly DependencyProperty SetSortCommandProperty = DependencyProperty.Register( nameof( SetSortCommand ), typeof( ICommand ),
            typeof( SkillsGridViewSortBehaviour ), new PropertyMetadata( null ) );

        private ListSortDirection _lastDirection;

        private SkillsGridViewColumn.Enums _lastHeaderClicked;
        private ICommand _setSortCommandImpl;

        public ICommand SetSortCommand
        {
            get => (ICommand) GetValue( SetSortCommandProperty );
            set => SetValue( SetSortCommandProperty, value );
        }

        public ICommand SetSortCommandImpl => _setSortCommandImpl ?? ( _setSortCommandImpl = new RelayCommand( SetSort ) );

        public ICommand SortChangedCommand
        {
            get => (ICommand) GetValue( SortChangedCommandProperty );
            set => SetValue( SortChangedCommandProperty, value );
        }

        private void SetSort( object obj )
        {
            if ( !( obj is SkillsSortInfo info ) )
            {
                return;
            }

            if ( !( AssociatedObject.View is GridView gridView ) )
            {
                return;
            }

            SkillsGridViewColumn column = gridView.Columns.FirstOrDefault( e => e is SkillsGridViewColumn sgvc && sgvc.SortField == info.SortBy ) as SkillsGridViewColumn;

            ApplySort( column, info.Direction );
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.AddHandler( ButtonBase.ClickEvent, new RoutedEventHandler( OnHeaderClick ) );

            SetValue( SetSortCommandProperty, SetSortCommandImpl );
        }

        protected override void OnDetaching()
        {
            AssociatedObject.RemoveHandler( ButtonBase.ClickEvent, new RoutedEventHandler( OnHeaderClick ) );

            base.OnDetaching();
        }

        private void OnHeaderClick( object sender, RoutedEventArgs e )
        {
            if ( !( e.OriginalSource is GridViewColumnHeader headerClicked ) || headerClicked.Column == null )
            {
                return;
            }

            SkillsGridViewColumn.Enums sortBy = ((SkillsGridViewColumn)headerClicked.Column).SortField;

            ListSortDirection direction;

            if ( sortBy == _lastHeaderClicked )
            {
                direction = _lastDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            }
            else
            {
                direction = ListSortDirection.Ascending;
            }

            ApplySort( headerClicked.Column as SkillsGridViewColumn, direction );

            SortChangedCommand?.Execute( new SkillsSortInfo( sortBy, direction ) );
        }

        private void ApplySort( SkillsGridViewColumn column, ListSortDirection direction )
        {
            _lastDirection = direction;

            _lastHeaderClicked = column.SortField;

            ListCollectionView dataView = (ListCollectionView)CollectionViewSource.GetDefaultView( AssociatedObject.ItemsSource );

            if ( dataView == null )
            {
                return;
            }

            dataView.SortDescriptions.Clear();
            dataView.SortDescriptions.Add( new SortDescription( column.SortField.ToString(), direction ) );
            dataView.CustomSort = new SkillComparer( direction, column.SortField );
            dataView.Refresh();
        }
    }

    public class SkillsSortInfo
    {
        public SkillsSortInfo( SkillsGridViewColumn.Enums sortBy, ListSortDirection direction )
        {
            SortBy = sortBy;
            Direction = direction;
        }

        public ListSortDirection Direction { get; }
        public SkillsGridViewColumn.Enums SortBy { get; }
    }
}