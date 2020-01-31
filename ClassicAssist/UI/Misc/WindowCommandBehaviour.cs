using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ClassicAssist.UI.Misc
{
    public enum WindowCommand
    {
        Minimize,
        Maximize,
        Close
    }

    public class WindowCommandBehaviour : Behavior<Button>
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register( "Command",
            typeof( WindowCommand ), typeof( WindowCommandBehaviour ),
            new FrameworkPropertyMetadata( WindowCommand.Close ) );

        public WindowCommand Command
        {
            get => (WindowCommand) GetValue( CommandProperty );
            set => SetValue( CommandProperty, value );
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Click += OnClick;
        }

        private void OnClick( object sender, RoutedEventArgs e )
        {
            if ( !( sender is UIElement element ) )
            {
                return;
            }

            Window window = Window.GetWindow( element );

            if ( window == null )
            {
                return;
            }

            switch ( Command )
            {
                case WindowCommand.Minimize:
                {
                    window.WindowState = WindowState.Minimized;
                    break;
                }
                case WindowCommand.Maximize:
                {
                    window.WindowState = window.WindowState == WindowState.Maximized
                        ? WindowState.Normal
                        : WindowState.Maximized;

                    break;
                }
                case WindowCommand.Close:
                {
                    window.Close();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.Click -= OnClick;
        }
    }
}