using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClassicAssist.Data;
using ClassicAssist.Misc;
using ClassicAssist.UI.ViewModels;

namespace ClassicAssist.UI.Controls.DraggableTreeView
{
    public class DraggableTreeView : TreeView
    {
        public static readonly DependencyProperty BindableSelectedItemProperty =
            DependencyProperty.RegisterAttached( nameof( BindableSelectedItem ), typeof( object ),
                typeof( DraggableTreeView ),
                new FrameworkPropertyMetadata( default, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

        public static readonly DependencyProperty BindableSelectedGroupProperty =
            DependencyProperty.RegisterAttached( nameof( BindableSelectedGroup ), typeof( object ),
                typeof( DraggableTreeView ),
                new FrameworkPropertyMetadata( default, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

        private IDraggableEntry _draggedItem;
        private Point _lastMouseDown;

        public DraggableTreeView()
        {
            AllowDrop = true;
            MouseMove += OnMouseMove;
            MouseDown += OnMouseDown;
            Drop += OnDrop;
            SelectedItemChanged += OnSelectedItemChanged;
            PreviewMouseWheel += OnPreviewMouseWheel;
            BorderThickness = new Thickness( 0 );
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

        private static void OnPreviewMouseWheel( object sender, MouseWheelEventArgs e )
        {
            /*
             * Cheap hack for our broken template, no scrollbars, bubble event to parent scrollviewer
             */
            if ( !( sender is Control control ) || e.Handled )
            {
                return;
            }

            if ( control.Parent == null )
            {
                return;
            }

            e.Handled = true;
            MouseWheelEventArgs eventArg = new MouseWheelEventArgs( e.MouseDevice, e.Timestamp, e.Delta )
            {
                RoutedEvent = MouseWheelEvent, Source = control
            };
            UIElement parent = control.Parent as UIElement;
            parent?.RaiseEvent( eventArg );
        }

        private void OnSelectedItemChanged( object sender, RoutedPropertyChangedEventArgs<object> e )
        {
            switch ( e.NewValue )
            {
                case IDraggableEntry draggableEntry:
                    BindableSelectedItem = draggableEntry;
                    BindableSelectedGroup = null;
                    break;
                case IDraggableGroup draggableGroup:
                    BindableSelectedGroup = draggableGroup;
                    BindableSelectedItem = null;
                    break;
            }
        }

        private void OnDrop( object sender, DragEventArgs e )
        {
            if ( _draggedItem == null )
            {
                return;
            }

            TreeViewItem nearestContainer = GetNearestContainer( e.OriginalSource as UIElement );

            if ( !( nearestContainer?.Header is IDraggableGroup group ) )
            {
                MoveItem( _draggedItem, null );
                return;
            }

            e.Effects = DragDropEffects.None;
            e.Handled = true;

            MoveItem( _draggedItem, group );
        }

        private void MoveItem( IDraggableEntry draggedItem, IDraggableGroup newParentGroup )
        {
            if ( !( ItemsSource is ObservableCollection<IDraggable> draggables ) )
            {
                return;
            }

            IDraggableGroup parent = null;

            if ( draggables.Contains( draggedItem ) )
            {
                draggables.Remove( draggedItem );
            }
            else
            {
                foreach ( IDraggableGroup group in draggables.Where( e => e is IDraggableGroup ).Cast<IDraggableGroup>()
                )
                {
                    if ( CheckGroupsRecursive( draggedItem, group, out parent ) )
                    {
                        break;
                    }
                }
            }

            parent?.Children.Remove( draggedItem );

            if ( newParentGroup != null )
            {
                if ( Options.CurrentOptions.SortMacrosAlphabetical )
                {
                    newParentGroup.Children.AddSorted( draggedItem, new GroupsBeforeMacrosComparer() );
                }
                else
                {
                    newParentGroup.Children.Add( draggedItem );
                }

                draggedItem.Group = newParentGroup.Name;
            }
            else
            {
                if ( Options.CurrentOptions.SortMacrosAlphabetical )
                {
                    draggables.AddSorted( draggedItem, new GroupsBeforeMacrosComparer() );
                }
                else
                {
                    draggables.Add( draggedItem );
                }

                draggedItem.Group = null;
            }
        }

        private static bool CheckGroupsRecursive( IDraggableEntry draggedItem, IDraggableGroup group,
            out IDraggableGroup parentGroup )
        {
            foreach ( IDraggable groupChild in group.Children )
            {
                switch ( groupChild )
                {
                    case IDraggableEntry _ when groupChild == draggedItem:
                        parentGroup = group;
                        return true;
                    case IDraggableGroup childGroup
                        when CheckGroupsRecursive( draggedItem, childGroup, out parentGroup ):
                        return true;
                }
            }

            parentGroup = null;
            return false;
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
            if ( e.LeftButton != MouseButtonState.Pressed )
            {
                return;
            }

            Point currentPosition = e.GetPosition( this );

            if ( !( Math.Abs( currentPosition.X - _lastMouseDown.X ) >
                    SystemParameters.MinimumHorizontalDragDistance ) &&
                 !( Math.Abs( currentPosition.Y - _lastMouseDown.Y ) > SystemParameters.MinimumVerticalDragDistance ) )
            {
                return;
            }

            if ( !( SelectedItem is IDraggableEntry ) )
            {
                return;
            }

            _draggedItem = (IDraggableEntry) SelectedItem;

            DragDrop.DoDragDrop( this, SelectedValue, DragDropEffects.Move );
        }

        private static TreeViewItem GetNearestContainer( UIElement element )
        {
            TreeViewItem container = element as TreeViewItem;

            while ( container == null && element != null )
            {
                element = VisualTreeHelper.GetParent( element ) as UIElement;
                container = element as TreeViewItem;
            }

            return container;
        }
    }
}