using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace ClassicAssist.UI.Misc
{
    public class WindowMouseDownBehaviour : Behavior<UIElement>
    {
        private static Point _lastMouseDown;

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseDown += OnMouseDown;
            AssociatedObject.MouseMove += OnMouseMove;
        }

        private void OnMouseMove( object sender, MouseEventArgs e )
        {
            if ( e.LeftButton != MouseButtonState.Pressed )
            {
                return;
            }

            if ( !( sender is UIElement element ) )
            {
                return;
            }

            Point currentPosition = e.GetPosition( element );

            if ( !( Math.Abs( currentPosition.X - _lastMouseDown.X ) >
                    SystemParameters.MinimumHorizontalDragDistance ) &&
                 !( Math.Abs( currentPosition.Y - _lastMouseDown.Y ) > SystemParameters.MinimumVerticalDragDistance ) )
            {
                return;
            }

            Window window = Window.GetWindow( element );

            if ( window?.WindowState != WindowState.Maximized )
            {
                return;
            }

            Point p = Mouse.GetPosition( window );
            window.Top = window.PointToScreen( p ).Y;
            window.WindowState = WindowState.Normal;

            window.DragMove();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.MouseDown -= OnMouseDown;
        }

        private static void OnMouseDown( object sender, MouseButtonEventArgs e )
        {
            if ( !( sender is UIElement element ) )
            {
                return;
            }

            if ( e.ChangedButton != MouseButton.Left )
            {
                return;
            }

            if ( e.ChangedButton == MouseButton.Left )
            {
                _lastMouseDown = e.GetPosition( element );
            }

            Window window = Window.GetWindow( element );

            window?.DragMove();
        }
    }
}