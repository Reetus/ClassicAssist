using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClassicAssist.UI.Views.Agents
{
    /// <summary>
    ///     Interaction logic for AutolootTabControl.xaml
    /// </summary>
    public partial class AutolootTabControl : UserControl
    {
        public AutolootTabControl()
        {
            InitializeComponent();
        }

        private void DraggableTreeView_OnPreviewMouseWheel( object sender, MouseWheelEventArgs e )
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
    }
}