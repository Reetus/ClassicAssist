using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ClassicAssist.Controls.DraggableTreeView
{
    /*
     * Credit: https://www.codeproject.com/Articles/55168/Drag-and-Drop-Feature-in-WPF-TreeView-Control
     */
    public class DraggableTreeView : TreeView
    {
        public static readonly DependencyProperty BindableSelectedItemProperty = DependencyProperty.RegisterAttached(
            nameof( BindableSelectedItem ), typeof( object ), typeof( DraggableTreeView ),
            new FrameworkPropertyMetadata( default, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedPropertyChanged ) );

        public static readonly DependencyProperty BindableSelectedGroupProperty = DependencyProperty.RegisterAttached(
            nameof( BindableSelectedGroup ), typeof( object ), typeof( DraggableTreeView ),
            new FrameworkPropertyMetadata( default, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedPropertyChanged ) );

        public static readonly DependencyProperty AllowDragGroupsProperty =
            DependencyProperty.RegisterAttached( nameof( AllowDragGroups ), typeof( bool ), typeof( DraggableTreeView ),
                new PropertyMetadata( true ) );

        private IDraggable _draggedItem;
        private Point _lastMouseDown;

        public DraggableTreeView()
        {
            AllowDrop = true;
            MouseMove += OnMouseMove;
            MouseDown += OnMouseDown;
            Drop += OnDrop;
            SelectedItemChanged += OnSelectedItemChanged;
            ItemContainerGenerator.StatusChanged += ItemContainerGeneratorOnStatusChanged;
        }

        public bool AllowDragGroups
        {
            get => (bool) GetValue( AllowDragGroupsProperty );
            set => SetValue( AllowDragGroupsProperty, value );
        }

        public IDraggableGroup BindableSelectedGroup
        {
            get => (IDraggableGroup) GetValue( BindableSelectedGroupProperty );
            set => SetValue( BindableSelectedGroupProperty, value );
        }

        public IDraggableEntry BindableSelectedItem
        {
            get => (IDraggableEntry) GetValue( BindableSelectedItemProperty );
            set => SetValue( BindableSelectedItemProperty, value );
        }

        private void ItemContainerGeneratorOnStatusChanged( object sender, EventArgs e )
        {
            if ( ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated )
            {
                return;
            }

            var selectedItem = (IDraggable) BindableSelectedItem ?? BindableSelectedGroup;

            if ( ItemContainerGenerator.ContainerFromItem( selectedItem ) is TreeViewItem item )
            {
                item.IsSelected = true;
            }
        }

        private static void OnSelectedPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            if ( !( d is DraggableTreeView draggableTreeView ) )
            {
                return;
            }

            if ( draggableTreeView.ItemContainerGenerator.ContainerFromItem( e.NewValue ) is TreeViewItem tvi )
            {
                tvi.IsSelected = true;
            }
        }

        private void OnSelectedItemChanged( object sender, RoutedPropertyChangedEventArgs<object> e )
        {
            switch ( e.NewValue )
            {
                case IDraggableGroup draggableGroup:
                    BindableSelectedGroup = draggableGroup;
                    BindableSelectedItem = null;
                    break;
                case IDraggableEntry draggableEntry:
                    BindableSelectedItem = draggableEntry;
                    BindableSelectedGroup = null;
                    break;
            }
        }

        private void OnDrop( object sender, DragEventArgs e )
        {
            IDraggableGroup group = null;

            if ( _draggedItem == null )
            {
                return;
            }

            TreeViewItem treeViewItem = GetNearestContainer( e.OriginalSource as UIElement );

            if ( !( treeViewItem?.Header is IDraggableEntry ) )
            {
                group = treeViewItem?.Header as IDraggableGroup;
            }

            if ( ReferenceEquals( treeViewItem?.Header, _draggedItem ) )
            {
                // Don't drag onto itself
                return;
            }

            e.Effects = DragDropEffects.None;
            e.Handled = true;

            MoveItem( _draggedItem, group );
        }

        private void MoveItem( IDraggable sourceItem, IDraggableGroup destinationGroup )
        {
            ObservableCollection<IDraggable> parent = GetParent( sourceItem,
                ItemsSource as ObservableCollection<IDraggable> );

            if ( sourceItem is IDraggableGroup draggableGroup && IsParentOf( destinationGroup, draggableGroup ) )
            {
                // No parents into children
                return;
            }

            parent?.Remove( sourceItem );

            if ( destinationGroup != null )
            {
                destinationGroup.Children.Add( sourceItem );
            }
            else
            {
                ( ItemsSource as ObservableCollection<IDraggable> )?.Add( sourceItem );
            }
        }

        private static bool IsParentOf( IDraggable sourceItem, IDraggableGroup destinationGroup )
        {
            return destinationGroup.Children.Any( e => e == sourceItem ) || GetGroups( destinationGroup.Children )
                .Any( draggableGroup => IsParentOf( sourceItem, draggableGroup ) );
        }

        private static ObservableCollection<IDraggable> GetParent( IDraggable draggable,
            ObservableCollection<IDraggable> parent )
        {
            if ( parent.Contains( draggable ) )
            {
                return parent;
            }

            IEnumerable<IDraggableGroup> groups = GetGroups( parent );

            return groups.Select( draggableGroup => GetParent( draggable, draggableGroup.Children ) )
                .FirstOrDefault( childParent => childParent != null );
        }

        private static IEnumerable<IDraggableGroup> GetGroups( IEnumerable<IDraggable> collection )
        {
            return collection.Where( i => i is IDraggableGroup ).Cast<IDraggableGroup>();
        }

        private void OnMouseDown( object sender, MouseButtonEventArgs e )
        {
            if ( e.ChangedButton == MouseButton.Left )
            {
                _lastMouseDown = e.GetPosition( this );
            }
        }

        private void OnMouseMove( object sender, MouseEventArgs e )
        {
            Dispatcher.Invoke( () =>
            {
                if ( e.LeftButton != MouseButtonState.Pressed )
                {
                    return;
                }

                var currentPosition = e.GetPosition( this );

                if ( !( Math.Abs( currentPosition.X - _lastMouseDown.X ) >
                        SystemParameters.MinimumHorizontalDragDistance ) &&
                     !( Math.Abs( currentPosition.Y - _lastMouseDown.Y ) >
                        SystemParameters.MinimumVerticalDragDistance ) )
                {
                    return;
                }

                if ( !( SelectedItem is IDraggable ) )
                {
                    return;
                }

                if ( !AllowDragGroups && SelectedItem is IDraggableGroup )
                {
                    return;
                }

                _draggedItem = (IDraggable) SelectedItem;

                try
                {
                    DragDrop.DoDragDrop( this, SelectedValue, DragDropEffects.Move );
                }
                catch ( Exception )
                {
                    // ignored
                }
            } );
        }

        private static TreeViewItem GetNearestContainer( UIElement element )
        {
            var container = element as TreeViewItem;

            while ( container == null && element != null )
            {
                element = VisualTreeHelper.GetParent( element ) as UIElement;
                container = element as TreeViewItem;
            }

            return container;
        }
    }
}