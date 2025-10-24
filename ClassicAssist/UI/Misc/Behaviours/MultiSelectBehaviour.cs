#region License

// Copyright (C) 2025 Reetus
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

using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace ClassicAssist.UI.Misc.Behaviours
{
    /*
     * https://stackoverflow.com/questions/8088595/synchronizing-multi-select-listbox-with-mvvm
     */

    public class MultiSelectionBehaviour : Behavior<ListBox>
    {
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register( nameof( SelectedItems ), typeof( IList ), typeof( MultiSelectionBehaviour ),
            new UIPropertyMetadata( null, SelectedItemsChanged ) );

        private bool _isUpdatingSource;

        private bool _isUpdatingTarget;

        public IList SelectedItems
        {
            get => (IList) GetValue( SelectedItemsProperty );
            set => SetValue( SelectedItemsProperty, value );
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            if ( SelectedItems == null )
            {
                return;
            }

            AssociatedObject.SelectedItems.Clear();

            foreach ( object item in SelectedItems )
            {
                AssociatedObject.SelectedItems.Add( item );
            }
        }

        private static void SelectedItemsChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
        {
            if ( o == null || !( o is MultiSelectionBehaviour behavior ) )
            {
                return;
            }

            INotifyCollectionChanged oldValue = (INotifyCollectionChanged) e.OldValue;
            INotifyCollectionChanged newValue = (INotifyCollectionChanged) e.NewValue;

            if ( oldValue != null )
            {
                oldValue.CollectionChanged -= behavior.SourceCollectionChanged;
                behavior.AssociatedObject.SelectionChanged -= behavior.ListBoxSelectionChanged;
            }

            if ( newValue == null || behavior.AssociatedObject == null )
            {
                return;
            }

            behavior.AssociatedObject.SelectedItems.Clear();

            if ( newValue is IEnumerable items )
            {
                foreach ( object item in items )
                {
                    behavior.AssociatedObject.SelectedItems.Add( item );
                }
            }

            behavior.AssociatedObject.SelectionChanged += behavior.ListBoxSelectionChanged;
            newValue.CollectionChanged += behavior.SourceCollectionChanged;
        }

        private void SourceCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            if ( _isUpdatingSource )
            {
                return;
            }

            try
            {
                _isUpdatingTarget = true;

                if ( e.OldItems != null )
                {
                    foreach ( object item in e.OldItems )
                    {
                        AssociatedObject.SelectedItems.Remove( item );
                    }
                }

                if ( e.NewItems != null )
                {
                    foreach ( object item in e.NewItems )
                    {
                        AssociatedObject.SelectedItems.Add( item );
                    }
                }

                if ( e.Action == NotifyCollectionChangedAction.Reset )
                {
                    AssociatedObject.SelectedItems.Clear();
                }
            }
            finally
            {
                _isUpdatingTarget = false;
            }
        }

        private void ListBoxSelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            if ( _isUpdatingTarget )
            {
                return;
            }

            IList selectedItems = SelectedItems;

            if ( selectedItems == null )
            {
                return;
            }

            try
            {
                _isUpdatingSource = true;

                foreach ( object item in e.RemovedItems )
                {
                    selectedItems.Remove( item );
                }

                foreach ( object item in e.AddedItems )
                {
                    selectedItems.Add( item );
                }
            }
            finally
            {
                _isUpdatingSource = false;
            }
        }
    }
}