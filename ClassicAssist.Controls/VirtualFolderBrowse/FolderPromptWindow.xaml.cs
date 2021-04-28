using System.Windows;

namespace ClassicAssist.Controls.VirtualFolderBrowse
{
    /// <summary>
    ///     Interaction logic for FolderPromptWindow.xaml
    /// </summary>
    public partial class FolderPromptWindow
    {
        public FolderPromptWindow()
        {
            InitializeComponent();
        }

        public string Text { get; set; }

        private void CancelOnClick( object sender, RoutedEventArgs e )
        {
            DialogResult = false;
            Close();
        }

        private void OKOnClick( object sender, RoutedEventArgs e )
        {
            DialogResult = true;
            Text = TextBox.Text;
            Close();
        }
    }
}