using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ClassicAssist.Data;
using ClassicAssist.Misc;
using ClassicAssist.UI.ViewModels.Debug;
using Newtonsoft.Json.Linq;

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

            Closing += OnClosing;

            foreach ( object t in TabControl.Items )
            {
                if ( !( t is TabItem tabItem ) || !( tabItem.Content is FrameworkElement frameworkElement ) || !( frameworkElement.DataContext is ISettingProvider provider ) )
                {
                    continue;
                }

                provider.Deserialize( AssistantOptions.DebugWindowOptions, Options.CurrentOptions );
            }
        }

        public DebugWindow( Type viewModelType, object value ) : this()
        {
            foreach ( object t in TabControl.Items )
            {
                if ( !( t is TabItem tabItem ) || !( tabItem.Content is FrameworkElement frameworkElement ) || frameworkElement.DataContext?.GetType() != viewModelType )
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

        private void OnClosing( object sender, CancelEventArgs e )
        {
            Closing -= OnClosing;

            JObject options = new JObject();

            foreach ( object t in TabControl.Items )
            {
                if ( !( t is TabItem tabItem ) || !( tabItem.Content is FrameworkElement frameworkElement ) || !( frameworkElement.DataContext is ISettingProvider provider ) )
                {
                    continue;
                }

                provider.Serialize( options );
            }

            AssistantOptions.DebugWindowOptions = options;
        }
    }
}