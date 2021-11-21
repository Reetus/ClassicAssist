using System.Windows;
using System.Windows.Input;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.Controls
{
    /// <summary>
    ///     Interaction logic for FilterControl.xaml
    /// </summary>
    public partial class FilterControl
    {
        public static readonly DependencyProperty FilterTextProperty = DependencyProperty.Register(
            nameof( FilterText ), typeof( string ), typeof( FilterControl ),
            new FrameworkPropertyMetadata( default( string ), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

        public static readonly DependencyProperty IsFilterVisibleProperty = DependencyProperty.Register(
            nameof( IsFilterVisible ), typeof( bool ), typeof( FilterControl ),
            new FrameworkPropertyMetadata( default( bool ), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

        public static readonly DependencyProperty ShowCloseButtonProperty = DependencyProperty.Register(
            nameof( ShowCloseButton ), typeof( bool ), typeof( FilterControl ),
            new FrameworkPropertyMetadata( true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

        private ICommand _closeCommand;

        public FilterControl()
        {
            InitializeComponent();
        }

        public ICommand CloseCommand => _closeCommand ?? ( _closeCommand = new RelayCommand( Close ) );

        public string FilterText
        {
            get => (string)GetValue( FilterTextProperty );
            set => SetValue( FilterTextProperty, value );
        }

        public bool IsFilterVisible
        {
            get => (bool)GetValue( IsFilterVisibleProperty );
            set => SetValue( IsFilterVisibleProperty, value );
        }

        public bool ShowCloseButton
        {
            get => (bool)GetValue( ShowCloseButtonProperty );
            set => SetValue( ShowCloseButtonProperty, value );
        }

        private void Close( object obj )
        {
            FilterText = string.Empty;
            IsFilterVisible = false;
        }
    }
}