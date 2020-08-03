using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace ClassicAssist.Updater
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Hyperlink_OnRequestNavigate( object sender, RequestNavigateEventArgs e )
        {
            Process.Start( e.Uri.ToString() );
        }
    }
}