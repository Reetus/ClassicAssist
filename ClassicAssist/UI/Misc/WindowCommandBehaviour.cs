using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        public static readonly DependencyProperty MinimizeCommandProperty =
            DependencyProperty.Register( nameof( MinimizeCommand ), typeof( ICommand ),
                typeof( WindowCommandBehaviour ),
                new FrameworkPropertyMetadata( default ) );

        public WindowCommand Command
        {
            get => (WindowCommand) GetValue( CommandProperty );
            set => SetValue( CommandProperty, value );
        }

        public ICommand MinimizeCommand
        {
            get => (ICommand) GetValue( MinimizeCommandProperty );
            set => SetValue( MinimizeCommandProperty, value );
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
                    if ( MinimizeCommand != null )
                    {
                        MinimizeCommand.Execute( window );
                        break;
                    }

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