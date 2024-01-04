using System;
using System.Windows;
using System.Windows.Controls;
using ClassicAssist.UI.ViewModels.Debug;

namespace ClassicAssist.UI.Views
{
    /// <summary>
    ///     Interaction logic for PacketLogWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        public DebugWindow()
        {
            InitializeComponent();
        }

        public DebugWindow( Type viewModelType, object value )
        {
            InitializeComponent();

            foreach ( object t in TabControl.Items )
            {
                if ( !( t is TabItem tabItem ) || !( tabItem.Content is FrameworkElement frameworkElement ) ||
                     frameworkElement.DataContext?.GetType() != viewModelType )
                {
                    continue;
                }

                TabControl.SelectedItem = tabItem;

                if ( frameworkElement.DataContext is DebugBaseViewModel viewModel )
                {
                    viewModel.Object = value;
                }

                break;
            }
        }
    }
}