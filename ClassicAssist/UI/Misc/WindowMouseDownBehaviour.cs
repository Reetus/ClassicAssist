using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace ClassicAssist.UI.Misc
{
    public class WindowMouseDownBehaviour : Behavior<UIElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseDown += OnMouseDown;
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

            if ( e.ChangedButton == MouseButton.Left )
            {
                Window.GetWindow( element )?.DragMove();
            }
        }
    }
}