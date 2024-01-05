using System.Windows;
using System.Windows.Input;

namespace ClassicAssist.UI.Views.ECV.Filter
{
    /// <summary>
    ///     Interaction logic for EntityCollectionFilterControl.xaml
    /// </summary>
    public partial class EntityCollectionFilterControl
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register( nameof( Command ),
            typeof( ICommand ), typeof( EntityCollectionFilterControl ),
            new PropertyMetadata( default( ICommand ), PropertyChangedCallback ) );

        public EntityCollectionFilterControl()
        {
            InitializeComponent();

            IsVisibleChanged += OnIsVisibleChanged;
        }

        public ICommand Command
        {
            get => (ICommand) GetValue( CommandProperty );
            set => SetValue( CommandProperty, value );
        }

        private void OnIsVisibleChanged( object sender, DependencyPropertyChangedEventArgs e )
        {
            if ( DataContext is EntityCollectionFilterViewModel viewModel )
            {
                viewModel.SaveFilterProfiles();
            }
        }

        private static void PropertyChangedCallback( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            if ( !( d is EntityCollectionFilterControl control ) )
            {
                return;
            }

            if ( control.DataContext is EntityCollectionFilterViewModel viewModel )
            {
                viewModel.Command = (ICommand) e.NewValue;
            }
        }
    }
}