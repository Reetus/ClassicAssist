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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace ClassicAssist.Controls.VirtualFolderBrowse
{
    public class TreeViewExpandedBehaviour : Behavior<TreeView>
    {
        public static readonly DependencyProperty OnExpandedActionProperty = DependencyProperty.Register(
            nameof( OnExpandedAction ), typeof( ICommand ), typeof( TreeViewExpandedBehaviour ),
            new FrameworkPropertyMetadata( default ) );

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof( SelectedItem ), typeof( object ), typeof( TreeViewExpandedBehaviour ),
            new FrameworkPropertyMetadata( default, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

        private readonly List<TreeViewItem> _list = new List<TreeViewItem>();

        public ICommand OnExpandedAction
        {
            get => (ICommand) GetValue( OnExpandedActionProperty );
            set => SetValue( OnExpandedActionProperty, value );
        }

        public object SelectedItem
        {
            get => GetValue( SelectedItemProperty );
            set => SetValue( SelectedItemProperty, value );
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += Loaded;
            AssociatedObject.ItemContainerGenerator.StatusChanged += StatusChanged;
            AssociatedObject.SelectedItemChanged += SelectedItemChanged;
        }

        private void SelectedItemChanged( object sender, RoutedPropertyChangedEventArgs<object> e )
        {
            SelectedItem = ( sender as TreeView )?.SelectedItem;
        }

        private void StatusChanged( object sender, EventArgs e )
        {
            if ( !( sender is ItemContainerGenerator container ) )
            {
                return;
            }

            if ( container.Status != GeneratorStatus.ContainersGenerated )
            {
                return;
            }

            AddExpandedListener( container, container.Items.ToList() );
        }

        private void Loaded( object sender, RoutedEventArgs routedEventArgs )
        {
            if ( !( sender is TreeView treeView ) )
            {
                return;
            }

            AddExpandedListener( treeView.ItemContainerGenerator,
                AssociatedObject.ItemsSource.Cast<object>().ToList() );
        }

        private void AddExpandedListener( ItemContainerGenerator container, IEnumerable items )
        {
            foreach ( object item in items )
            {
                if ( !( container.ContainerFromItem( item ) is TreeViewItem treeViewItem ) )
                {
                    return;
                }

                if ( _list.Contains( treeViewItem ) )
                {
                    _list.Remove( treeViewItem );
                }

                treeViewItem.ItemContainerGenerator.StatusChanged += StatusChanged;

                treeViewItem.Expanded += TreeViewItemOnExpanded;
                _list.Add( treeViewItem );
            }
        }

        private void TreeViewItemOnExpanded( object sender, RoutedEventArgs routedEventArgs )
        {
            if ( !( sender is TreeViewItem treeViewItem ) )
            {
                return;
            }

            OnExpandedAction?.Execute( treeViewItem.Header );

            routedEventArgs.Handled = true;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= Loaded;
            _list.ForEach( item => item.Expanded -= TreeViewItemOnExpanded );
            _list.Clear();
        }
    }
}