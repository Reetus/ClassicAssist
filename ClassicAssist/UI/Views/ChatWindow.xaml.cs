using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassicAssist.Data;
using ClassicAssist.UO;

namespace ClassicAssist.UI.Views
{
    /// <summary>
    ///     Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        public ChatWindow()
        {
            InitializeComponent();
            Width = Options.CurrentOptions.ChatWindowWidth;
            Height = Options.CurrentOptions.ChatWindowHeight;
            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged( object sender, SizeChangedEventArgs e )
        {
            Options.CurrentOptions.ChatWindowWidth = e.NewSize.Width;
            Options.CurrentOptions.ChatWindowHeight = e.NewSize.Height;
        }

        //TODO Change to command
        private void TextBox_KeyDown( object sender, KeyEventArgs e )
        {
            if ( e.Key != Key.Return && e.Key != Key.Return )
            {
                return;
            }

            if ( !( sender is TextBox textBox ) )
            {
                return;
            }

            Commands.ChatMsg( textBox.Text );
            e.Handled = true;
            textBox.Text = string.Empty;
        }
    }
}