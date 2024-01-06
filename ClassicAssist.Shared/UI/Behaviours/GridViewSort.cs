#region License

// Copyright (C) 2021 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Microsoft.Xaml.Behaviors;

namespace ClassicAssist.Shared.UI.Behaviours
{
    public class GridViewSort : Behavior<ListView>
    {
        public static readonly DependencyProperty ComparerTypeProperty =
            DependencyProperty.RegisterAttached( nameof( ComparerType ), typeof( Type ), typeof( GridViewSort ),
                new UIPropertyMetadata( null ) );

        private ListSortDirection _lastDirection;

        private GridViewColumnHeader _lastHeaderClicked;

        public Type ComparerType
        {
            get => (Type) GetValue( ComparerTypeProperty );
            set => SetValue( ComparerTypeProperty, value );
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.AddHandler( ButtonBase.ClickEvent, new RoutedEventHandler( ColumnHeader_Click ) );
        }

        private void ColumnHeader_Click( object sender, RoutedEventArgs e )
        {
            if ( !( e.OriginalSource is GridViewColumnHeader headerClicked ) )
            {
                return;
            }

            ListCollectionView dataView =
                (ListCollectionView) CollectionViewSource.GetDefaultView( AssociatedObject.ItemsSource );

            ListSortDirection direction;

            string propertyName = headerClicked.Column.Header.ToString();

            if ( !Equals( headerClicked, _lastHeaderClicked ) )
            {
                direction = ListSortDirection.Ascending;
            }
            else
            {
                direction = _lastDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }

            IComparer comparer = (IComparer) Activator.CreateInstance( ComparerType, direction, propertyName );

            dataView.CustomSort = comparer;
            dataView.Refresh();

            _lastHeaderClicked = headerClicked;
            _lastDirection = direction;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.RemoveHandler( ButtonBase.ClickEvent, new RoutedEventHandler( ColumnHeader_Click ) );
            base.OnDetaching();
        }
    }
}