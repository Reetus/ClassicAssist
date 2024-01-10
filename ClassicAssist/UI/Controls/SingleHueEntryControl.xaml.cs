using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UI.Controls
{
    /// <summary>
    ///     Interaction logic for HueEntryControl.xaml
    /// </summary>
    public partial class SingleHueEntryControl
    {
        public static readonly DependencyProperty HueEntryProperty = DependencyProperty.Register( nameof( HueEntry ),
            typeof( HueEntry ), typeof( SingleHueEntryControl ),
            new FrameworkPropertyMetadata( default( HueEntry ), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                PropertyChangedCallback ) );

        public SingleHueEntryControl()
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
            if ( d is SingleHueEntryControl control )
            {
                control.SetHues( (HueEntry) e.NewValue );
            }
        }

        private void SetHues( HueEntry entry )
        {
            // TODO: Rush job, verify logic for single hue later
            SolidColorBrush brush = new SolidColorBrush( Convert555ToARGB( entry.Colors[30] ) );

            StackPanel.Children.Add( new Rectangle { Fill = brush, Width = 320 } );
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