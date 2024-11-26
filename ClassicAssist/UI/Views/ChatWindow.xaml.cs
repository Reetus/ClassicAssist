using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ClassicAssist.Data;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UO;

namespace ClassicAssist.UI.Views
{
    /// <summary>
    ///     Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow
    {
        public ChatWindow()
        {
            InitializeComponent();
            Width = Options.CurrentOptions.ChatWindowWidth;
            Height = Options.CurrentOptions.ChatWindowHeight;
            SizeChanged += OnSizeChanged;

            if ( DataContext is ChatViewModel vm )
            {
                vm.RightColumnSize = Options.CurrentOptions.ChatWindowRightColumn;
            }
        }

        private static void OnSizeChanged( object sender, SizeChangedEventArgs e )
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

        private void Thumb_OnDragCompleted( object sender, DragCompletedEventArgs e )
        {
            if ( DataContext is ChatViewModel vm )
            {
                Options.CurrentOptions.ChatWindowRightColumn = vm.RightColumnSize;
            }
        }
    }
}