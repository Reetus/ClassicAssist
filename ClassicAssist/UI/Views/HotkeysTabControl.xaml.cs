using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ClassicAssist.UI.Views
{
    /// <summary>
    ///     Interaction logic for HotkeysTab.xaml
    /// </summary>
    public partial class HotkeysTabControl : UserControl
    {
        public HotkeysTabControl()
        {
            InitializeComponent();
        }

        private void UIElement_OnPreviewMouseWheel( object sender, MouseWheelEventArgs e )
        {
            if ( e.Handled )
            {
                return;
            }

            e.Handled = true;
            MouseWheelEventArgs eventArg = new MouseWheelEventArgs( e.MouseDevice, e.Timestamp, e.Delta ) { RoutedEvent = MouseWheelEvent, Source = sender };
            ScrollViewer parent = GetScrollViewer( (Control) sender );
            parent.RaiseEvent( eventArg );
        }

        public static ScrollViewer GetScrollViewer( DependencyObject depObj )
        {
            if ( depObj is ScrollViewer scrollViewer )
            {
                return scrollViewer;
            }

            for ( int i = 0; i < VisualTreeHelper.GetChildrenCount( depObj ); i++ )
            {
                DependencyObject child = VisualTreeHelper.GetChild( depObj, i );

                ScrollViewer result = GetScrollViewer( child );

                if ( result != null )
                {
                    return result;
                }
            }

            return null;
        }
    }
}