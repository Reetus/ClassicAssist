using System.Windows;
using System.Windows.Controls;

namespace ClassicAssist.Controls
{
    /// <summary>
    ///     Interaction logic for HeaderTextBox.xaml
    /// </summary>
    public partial class HeaderTextBox
    {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register( nameof( Header ),
            typeof( object ), typeof( HeaderTextBox ),
            new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register( nameof( Value ),
            typeof( object ), typeof( HeaderTextBox ),
            new FrameworkPropertyMetadata( string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTextChanged ) );

        public HeaderTextBox()
        {
            InitializeComponent();
        }

        public object Header
        {
            get => GetValue( HeaderProperty );
            set => SetValue( HeaderProperty, value );
        }

        public object Value
        {
            get => GetValue( ValueProperty );
            set => SetValue( ValueProperty, value );
        }

        private static void OnTextChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
        }

        private void TextBox_OnTextChanged( object sender, TextChangedEventArgs e )
        {
            Value = TextBox.Text;
        }
    }
}