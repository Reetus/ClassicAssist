using System.Windows;

namespace ClassicAssist.UI.Controls
{
    /// <summary>
    ///     Interaction logic for CustomWindowTitleControl.xaml
    /// </summary>
    public partial class CustomWindowTitleControl
    {
        public static readonly DependencyProperty AdditionalContentProperty =
            DependencyProperty.Register( "AdditionalContent", typeof( object ), typeof( CustomWindowTitleControl ),
                new PropertyMetadata( null ) );

        public static readonly DependencyProperty CustomTitleProperty = DependencyProperty.Register( "CustomTitle",
            typeof( string ),
            typeof( CustomWindowTitleControl ),
            new FrameworkPropertyMetadata( default( string ), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

        public static readonly DependencyProperty CanCloseProperty = DependencyProperty.Register( "CanClose",
            typeof( bool ), typeof( CustomWindowTitleControl ),
            new FrameworkPropertyMetadata( true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

        public static readonly DependencyProperty CanMinimizeProperty = DependencyProperty.Register( "CanMinimize",
            typeof( bool ), typeof( CustomWindowTitleControl ),
            new FrameworkPropertyMetadata( true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

        public static readonly DependencyProperty CanMaxmizeProperty = DependencyProperty.Register( "CanMaximize",
            typeof( bool ), typeof( CustomWindowTitleControl ),
            new FrameworkPropertyMetadata( true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

        public CustomWindowTitleControl()
        {
            InitializeComponent();
        }

        public object AdditionalContent
        {
            get => GetValue( AdditionalContentProperty );
            set => SetValue( AdditionalContentProperty, value );
        }

        public bool CanClose
        {
            get => (bool) GetValue( CanCloseProperty );
            set => SetValue( CanCloseProperty, value );
        }

        public bool CanMaximize
        {
            get => (bool) GetValue( CanMaxmizeProperty );
            set => SetValue( CanMaxmizeProperty, value );
        }

        public bool CanMinimize
        {
            get => (bool) GetValue( CanMinimizeProperty );
            set => SetValue( CanMinimizeProperty, value );
        }

        public string CustomTitle
        {
            get => (string) GetValue( CustomTitleProperty );
            set => SetValue( CustomTitleProperty, value );
        }
    }
}