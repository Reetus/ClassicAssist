using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ClassicAssist.UI.Controls
{
    /// <summary>
    ///     Interaction logic for EditTextBlock.xaml
    /// </summary>
    public partial class EditTextBlock
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register( "Text", typeof( string ),
            typeof( EditTextBlock ),
            new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register( "Label",
            typeof( string ), typeof( EditTextBlock ),
            new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None ) );

        public static readonly DependencyProperty ShowIconProperty = DependencyProperty.Register( "ShowIcon",
            typeof( bool ), typeof( EditTextBlock ), new UIPropertyMetadata() );

        public EditTextBlock()
        {
            InitializeComponent();
        }

        public string Label
        {
            get => (string) GetValue( LabelProperty );
            set => SetValue( LabelProperty, value );
        }

        public bool ShowIcon
        {
            get => (bool) GetValue( ShowIconProperty );
            set => SetValue( ShowIconProperty, value );
        }

        public string Text
        {
            get => (string) GetValue( TextProperty );
            set => SetValue( TextProperty, value );
        }

        private void TextBlock_OnMouseDown( object sender, MouseButtonEventArgs e )
        {
            if ( e.ClickCount <= 1 )
            {
                return;
            }

            ShowTextBox();
        }

        private void ShowTextBox()
        {
            textBlock.Visibility = Visibility.Collapsed;
            pencilButton.Visibility = Visibility.Collapsed;
            textBox.Visibility = Visibility.Visible;
            textBox.CaretIndex = textBox.Text.Length;
            textBox.SelectAll();

            Dispatcher?.BeginInvoke( (Action) ( () => Keyboard.Focus( textBox ) ), DispatcherPriority.Render );
        }

        private void TextBox_OnKeyDown( object sender, KeyEventArgs e )
        {
            if ( e.Key == Key.Return )
            {
                TextBox_LostFocus( sender, null );
            }
        }

        private void TextBox_LostFocus( object sender, RoutedEventArgs e )
        {
            textBlock.Visibility = Visibility.Visible;
            pencilButton.Visibility = ShowIcon ? Visibility.Visible : Visibility.Hidden;
            ( (TextBox) sender ).Visibility = Visibility.Collapsed;
        }

        private void Button_OnClick( object sender, RoutedEventArgs e )
        {
            ShowTextBox();
        }
    }
}