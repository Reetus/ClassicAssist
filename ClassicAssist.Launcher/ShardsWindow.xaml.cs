using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace ClassicAssist.Launcher
{
    /// <summary>
    ///     Interaction logic for ShardsWindow.xaml
    /// </summary>
    public partial class ShardsWindow : Window
    {
        public ShardsWindow()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate( object sender, RequestNavigateEventArgs e )
        {
            Process.Start( e.Uri.ToString() );
        }
    }
}