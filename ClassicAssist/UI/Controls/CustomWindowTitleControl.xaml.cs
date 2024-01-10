using System.Windows;
using System.Windows.Input;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.UI.Controls
{
    /// <summary>
    ///     Interaction logic for CustomWindowTitleControl.xaml
    /// </summary>
    public partial class CustomWindowTitleControl
    {
        public static readonly DependencyProperty AdditionalContentProperty =
            DependencyProperty.Register( nameof( AdditionalContent ), typeof( object ), typeof( CustomWindowTitleControl ),
                new PropertyMetadata( null ) );

        public static readonly DependencyProperty AdditionalButtonsProperty =
            DependencyProperty.Register( nameof( AdditionalButtons ), typeof( object ), typeof( CustomWindowTitleControl ),
                new PropertyMetadata( null ) );

        public static readonly DependencyProperty CustomTitleProperty = DependencyProperty.Register( nameof( CustomTitle ),
            typeof( string ), typeof( CustomWindowTitleControl ),
            new FrameworkPropertyMetadata( default( string ), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

        public static readonly DependencyProperty CanCloseProperty = DependencyProperty.Register( nameof( CanClose ),
            typeof( bool ), typeof( CustomWindowTitleControl ),
            new FrameworkPropertyMetadata( true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

        public static readonly DependencyProperty CanMinimizeProperty = DependencyProperty.Register( nameof( CanMinimize ),
            typeof( bool ), typeof( CustomWindowTitleControl ),
            new FrameworkPropertyMetadata( true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

        public static readonly DependencyProperty CanMaxmizeProperty = DependencyProperty.Register( nameof( CanMaximize ),
            typeof( bool ), typeof( CustomWindowTitleControl ),
            new FrameworkPropertyMetadata( true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

        public static readonly DependencyProperty MinimizeCommandProperty =
            DependencyProperty.Register( nameof( MinimizeCommand ), typeof( ICommand ),
                typeof( CustomWindowTitleControl ), new FrameworkPropertyMetadata( default ) );

        private ICommand _maximizeCommand;

        public CustomWindowTitleControl()
        {
            InitializeComponent();
        }

        public object AdditionalButtons
        {
            get => GetValue( AdditionalButtonsProperty );
            set => SetValue( AdditionalButtonsProperty, value );
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

        public ICommand MaximizeCommand =>
            _maximizeCommand ?? ( _maximizeCommand = new RelayCommand( Maximize, o => true ) );

        public ICommand MinimizeCommand
        {
            get => (ICommand) GetValue( MinimizeCommandProperty );
            set => SetValue( MinimizeCommandProperty, value );
        }

        private static void Maximize( object obj )
        {
            if ( !( obj is UIElement element ) )
            {
                return;
            }

            Window window = Window.GetWindow( element );

            if ( window == null )
            {
                return;
            }

            window.WindowState =
                window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
    }
}