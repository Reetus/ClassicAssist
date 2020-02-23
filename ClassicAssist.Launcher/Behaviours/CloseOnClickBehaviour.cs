using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace ClassicAssist.Launcher.Behaviours
{
    public class CloseOnClickBehaviour : Behavior<Button>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Click += OnClick;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Click -= OnClick;
        }

        private void OnClick( object sender, RoutedEventArgs e )
        {
            if ( !( sender is Button button ) )
            {
                return;
            }

            Window window = Window.GetWindow( button );

            window?.Close();
        }
    }
}