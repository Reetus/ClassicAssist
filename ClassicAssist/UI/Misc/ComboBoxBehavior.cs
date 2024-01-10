using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace ClassicAssist.UI.Misc
{
    public class ComboBoxBehavior : Behavior<ComboBox>
    {
        public static readonly DependencyProperty CommandBindingProperty =
            DependencyProperty.Register( "CommandBinding", typeof( ICommand ), typeof( ComboBoxBehavior ),
                new FrameworkPropertyMetadata( default( ICommand ), FrameworkPropertyMetadataOptions.None,
                    PropertyChangedCallback ) );

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register( "CommandParameter", typeof( object ), typeof( ComboBoxBehavior ),
                new FrameworkPropertyMetadata( default, FrameworkPropertyMetadataOptions.None,
                    PropertyChangedCallback ) );

        public static readonly DependencyProperty OnlyUserTriggeredProperty =
            DependencyProperty.Register( "OnlyUserTriggered", typeof( bool ), typeof( ComboBoxBehavior ),
                new FrameworkPropertyMetadata( false, FrameworkPropertyMetadataOptions.None,
                    PropertyChangedCallback ) );

        private bool _userTriggered;

        public ICommand CommandBinding
        {
            get => (ICommand) GetValue( CommandBindingProperty );
            set => SetValue( CommandBindingProperty, value );
        }

        public object CommandParameter
        {
            get => GetValue( CommandParameterProperty );
            set => SetValue( CommandParameterProperty, value );
        }

        public bool OnlyUserTriggered
        {
            get => (bool) GetValue( OnlyUserTriggeredProperty );
            set => SetValue( OnlyUserTriggeredProperty, value );
        }

        private static void PropertyChangedCallback( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged += OnSelectionChanged;
            AssociatedObject.PreviewMouseDown += OnPreviewMouseDown;
        }

        private void OnPreviewMouseDown( object sender, MouseButtonEventArgs e )
        {
            _userTriggered = true;
        }

        private void OnSelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            if ( OnlyUserTriggered && !_userTriggered )
            {
                return;
            }

            CommandBinding?.Execute( AssociatedObject.SelectedItem );
            _userTriggered = false;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
            AssociatedObject.PreviewMouseDown -= OnPreviewMouseDown;
        }
    }
}