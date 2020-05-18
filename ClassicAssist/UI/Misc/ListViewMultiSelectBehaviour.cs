using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ClassicAssist.UI.Misc
{
    /*
     * https://stackoverflow.com/questions/8088595/synchronizing-multi-select-listbox-with-mvvm
     */

    public class ListViewMultiSelectionBehaviour : Behavior<ListView>
    {
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register( "SelectedItems",
            typeof( IList ), typeof( ListViewMultiSelectionBehaviour ),
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

            AssociatedObject.SelectionChanged += ListViewSelectionChanged;
        }

        private static void SelectedItemsChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
        {
            if ( o == null || !( o is ListViewMultiSelectionBehaviour behavior ) )
            {
                return;
            }

            INotifyCollectionChanged oldValue = (INotifyCollectionChanged) e.OldValue;
            INotifyCollectionChanged newValue = (INotifyCollectionChanged) e.NewValue;

            if ( oldValue != null )
            {
                oldValue.CollectionChanged -= behavior.SourceCollectionChanged;
                behavior.AssociatedObject.SelectionChanged -= behavior.ListViewSelectionChanged;
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

            behavior.AssociatedObject.SelectionChanged += behavior.ListViewSelectionChanged;
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

        private void ListViewSelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            if ( e.OriginalSource.GetType() != typeof( ListView ) )
            {
                return;
            }

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