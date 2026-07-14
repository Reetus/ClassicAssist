using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GongSolutions.Wpf.DragDrop;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace ClassicAssist.Controls.DraggableTreeView
{
    /*
     * Credit: https://www.codeproject.com/Articles/55168/Drag-and-Drop-Feature-in-WPF-TreeView-Control
     */
    public class DraggableTreeView : TreeView, IDropTarget
    {
        public static readonly DependencyProperty BindableSelectedItemProperty = DependencyProperty.RegisterAttached( nameof( BindableSelectedItem ), typeof( object ),
            typeof( DraggableTreeView ), new FrameworkPropertyMetadata( default, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedPropertyChanged ) );

        public static readonly DependencyProperty BindableSelectedGroupProperty = DependencyProperty.RegisterAttached( nameof( BindableSelectedGroup ), typeof( object ),
            typeof( DraggableTreeView ), new FrameworkPropertyMetadata( default, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedPropertyChanged ) );

        public static readonly DependencyProperty AllowDragGroupsProperty =
            DependencyProperty.RegisterAttached( nameof( AllowDragGroups ), typeof( bool ), typeof( DraggableTreeView ), new PropertyMetadata( true ) );

        public static readonly DependencyProperty AllowDragItemsOntoItemsProperty =
            DependencyProperty.RegisterAttached( nameof( AllowDragItemsOntoItems ), typeof( bool ), typeof( DraggableTreeView ), new PropertyMetadata( true ) );

        public DraggableTreeView()
        {
            DragDrop.SetIsDragSource( this, true );
            DragDrop.SetIsDropTarget( this, true );
            DragDrop.SetDropHandler( this, this );
            SelectedItemChanged += OnSelectedItemChanged;
            ItemContainerGenerator.StatusChanged += OnStatusChanged;
        }

        public bool AllowDragGroups
        {
            get => (bool) GetValue( AllowDragGroupsProperty );
            set => SetValue( AllowDragGroupsProperty, value );
        }

        public bool AllowDragItemsOntoItems
        {
            get => (bool) GetValue( AllowDragItemsOntoItemsProperty );
            set => SetValue( AllowDragItemsOntoItemsProperty, value );
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

        public new void DragEnter( IDropInfo dropInfo )
        {
        }

        public new void DragOver( IDropInfo dropInfo )
        {
            if ( !( dropInfo.DragInfo.SourceItem is IDraggable sourceItem ) )
            {
                return;
            }

            if ( !( ItemsSource is ObservableCollection<IDraggable> items ) )
            {
                return;
            }

            ObservableCollection<IDraggable> sourceParent = GetParent( sourceItem, items );

            if ( !AllowDragItemsOntoItems && sourceItem is IDraggableEntry && sourceParent == items && !( dropInfo?.TargetItem is IDraggableGroup ) )
            {
                dropInfo.Effects = DragDropEffects.None;

                return;
            }

            dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
            dropInfo.Effects = DragDropEffects.Move;
        }

        public new void DragLeave( IDropInfo dropInfo )
        {
        }

        public new void Drop( IDropInfo dropInfo )
        {
            switch ( dropInfo.TargetItem )
            {
                case IDraggableGroup destinationGroup:
                {
                    IDraggable sourceItem = dropInfo.DragInfo.SourceItem as IDraggable;

                    ObservableCollection<IDraggable> parent = GetParent( sourceItem, ItemsSource as ObservableCollection<IDraggable> );

                    switch ( sourceItem )
                    {
                        case IDraggableGroup _ when !AllowDragGroups:
                        // No parents into children
                        case IDraggableGroup draggableGroup when IsParentOf( destinationGroup, draggableGroup ):
                            return;
                    }

                    int? previousIndex = parent?.IndexOf( sourceItem );

                    int newSelectedIndex = 0;

                    if ( previousIndex.HasValue )
                    {
                        if ( previousIndex.Value > 0 )
                        {
                            newSelectedIndex = previousIndex.Value - 1;
                        }
                        else if ( previousIndex.Value < parent.Count - 1 )
                        {
                            newSelectedIndex = previousIndex.Value;
                        }
                        else
                        {
                            newSelectedIndex = -1;
                        }
                    }

                    parent?.Remove( sourceItem );

                    if ( newSelectedIndex != -1 && parent != null )
                    {
                        IDraggable item = parent[newSelectedIndex];

                        if ( item != null && ItemContainerGenerator.ContainerFromItem( item ) is TreeViewItem tvi && !tvi.IsSelected )
                        {
                            tvi.IsSelected = true;
                        }
                    }

                    destinationGroup.Children.Add( sourceItem );

                    break;
                }
                case IDraggableEntry destinationItem:
                {
                    IDraggable sourceItem = dropInfo.DragInfo.SourceItem as IDraggable;

                    if ( !( ItemsSource is ObservableCollection<IDraggable> items ) )
                    {
                        return;
                    }

                    ObservableCollection<IDraggable> sourceParent = GetParent( sourceItem, items );

                    if ( !AllowDragItemsOntoItems && sourceItem is IDraggableEntry && sourceParent == items )
                    {
                        return;
                    }

                    if ( sourceItem is IDraggableGroup )
                    {
                        // No groups onto items
                        return;
                    }

                    ObservableCollection<IDraggable> destinationParent = GetParent( destinationItem, ItemsSource as ObservableCollection<IDraggable> );

                    if ( sourceParent != destinationParent )
                    {
                        sourceParent.Remove( sourceItem );
                        destinationParent.Add( sourceItem );
                    }

                    int sourceIndex = destinationParent.IndexOf( sourceItem );
                    int targetIndex = destinationParent.IndexOf( destinationItem );

                    destinationParent.Move( sourceIndex, targetIndex );
                    break;
                }
                case null:
                {
                    IDraggable sourceItem = dropInfo.DragInfo.SourceItem as IDraggable;

                    if ( !( ItemsSource is ObservableCollection<IDraggable> items ) )
                    {
                        return;
                    }

                    ObservableCollection<IDraggable> sourceParent = GetParent( sourceItem, items );

                    sourceParent?.Remove( sourceItem );

                    items.Add( sourceItem );
                    break;
                }
            }
        }

        protected override void PrepareContainerForItemOverride( DependencyObject element, object item )
        {
            base.PrepareContainerForItemOverride( element, item );

            // Under container recycling a reused TreeViewItem can keep a stale IsSelected from the
            // item it previously displayed, making several rows appear selected at once. Sync it to
            // the bound selection every time a container is (re)assigned to an item.
            if ( element is TreeViewItem tvi )
            {
                bool selected = ReferenceEquals( item, BindableSelectedItem ) || ReferenceEquals( item, BindableSelectedGroup );

                if ( tvi.IsSelected != selected )
                {
                    tvi.IsSelected = selected;
                }
            }
        }

        private void OnStatusChanged( object sender, EventArgs e )
        {
            foreach ( object item in ItemContainerGenerator.Items )
            {
                if ( !item.Equals( BindableSelectedItem ) )
                {
                    continue;
                }

                if ( ItemContainerGenerator.ContainerFromItem( item ) is TreeViewItem tvi && !tvi.IsSelected )
                {
                    tvi.IsSelected = true;
                }
            }
        }

        private static bool IsParentOf( IDraggable sourceItem, IDraggableGroup destinationGroup )
        {
            return destinationGroup.Children.Any( e => e == sourceItem ) || GetGroups( destinationGroup.Children )
                .Any( draggableGroup => IsParentOf( sourceItem, draggableGroup ) );
        }

        private static IEnumerable<IDraggableGroup> GetGroups( IEnumerable<IDraggable> collection )
        {
            return collection.Where( i => i is IDraggableGroup ).Cast<IDraggableGroup>();
        }

        private static void OnSelectedPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            if ( !( d is DraggableTreeView draggableTreeView ) || e.NewValue == null )
            {
                return;
            }

            draggableTreeView.SelectAndBringIntoView( e.NewValue );
        }

        private void SelectAndBringIntoView( object item )
        {
            // Already realized (non-virtualized or currently on-screen).
            if ( ItemContainerGenerator.ContainerFromItem( item ) is TreeViewItem container )
            {
                container.IsSelected = true;
                container.BringIntoView();

                return;
            }

            // The container hasn't been generated yet - either layout is still pending or, under
            // virtualization, the item is scrolled out of view. For a top-level item we force its
            // container to be realized by scrolling its index into view; nested items are handled
            // by OnStatusChanged once the generator produces the container.
            int index = Items.IndexOf( item );

            if ( index < 0 )
            {
                return;
            }

            Dispatcher.BeginInvoke( (Action) ( () =>
            {
                if ( GetItemsHost() is VirtualizingPanel panel )
                {
                    ScrollIndexIntoView( panel, index );
                    UpdateLayout();
                }

                if ( ItemContainerGenerator.ContainerFromItem( item ) is TreeViewItem tvi )
                {
                    tvi.IsSelected = true;
                    tvi.BringIntoView();
                }
            } ), DispatcherPriority.Loaded );
        }

        private Panel GetItemsHost()
        {
            return typeof( ItemsControl ).GetProperty( "ItemsHost", BindingFlags.Instance | BindingFlags.NonPublic )?.GetValue( this ) as Panel;
        }

        private static void ScrollIndexIntoView( VirtualizingPanel panel, int index )
        {
            // VirtualizingPanel.BringIndexIntoView is protected internal, so there is no public
            // way to realize an off-screen container; reflection is the standard workaround.
            typeof( VirtualizingPanel ).GetMethod( "BringIndexIntoView", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof( int ) }, null )
                ?.Invoke( panel, new object[] { index } );
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

        private static ObservableCollection<IDraggable> GetParent( IDraggable draggable, ObservableCollection<IDraggable> parent )
        {
            if ( parent.Contains( draggable ) )
            {
                return parent;
            }

            IEnumerable<IDraggableGroup> groups = GetGroups( parent );

            return groups.Select( draggableGroup => GetParent( draggable, draggableGroup.Children ) ).FirstOrDefault( childParent => childParent != null );
        }
    }
}