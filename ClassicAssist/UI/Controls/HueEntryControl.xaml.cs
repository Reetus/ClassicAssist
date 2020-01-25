using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UI.Controls
{
    /// <summary>
    ///     Interaction logic for HueEntryControl.xaml
    /// </summary>
    public partial class HueEntryControl : UserControl
    {
        public static readonly DependencyProperty HueEntryProperty =
            DependencyProperty.Register( "HueEntry", typeof( HueEntry ), typeof( HueEntryControl ),
                new FrameworkPropertyMetadata( default( HueEntry ),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback ) );

        public HueEntryControl()
        {
            InitializeComponent();
        }

        public HueEntry HueEntry
        {
            get => (HueEntry) GetValue( HueEntryProperty );
            set => SetValue( HueEntryProperty, value );
        }

        private static void PropertyChangedCallback( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            if ( d is HueEntryControl control )
            {
                control.SetHues( (HueEntry) e.NewValue );
            }
        }

        private void SetHues( HueEntry entry )
        {
            for ( int i = 0; i < 32; i++ )
            {
                SolidColorBrush brush = new SolidColorBrush( Convert555ToARGB( entry.Colors[i] ) );

                StackPanel.Children.Add( new Rectangle { Fill = brush, Width = 10 } );
            }
        }

        protected static Color Convert555ToARGB( short Col )
        {
            int r = ( (short) ( Col >> 10 ) & 31 ) * 8;
            int g = ( (short) ( Col >> 5 ) & 31 ) * 8;
            int b = ( Col & 31 ) * 8;
            return Color.FromArgb( 255, (byte) r, (byte) g, (byte) b );
        }
    }
}