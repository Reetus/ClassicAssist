using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace ClassicAssist.UI.Misc
{
    public class ComboBoxBehavior : Behavior<ComboBox>
    {
        public static readonly DependencyProperty CommandBindingProperty =
            DependencyProperty.Register( "CommandBinding", typeof( ICommand ), typeof( ComboBoxBehavior ),
                new FrameworkPropertyMetadata( default( ICommand ),
                    FrameworkPropertyMetadataOptions.None, PropertyChangedCallback ) );

        public ICommand CommandBinding
        {
            get => (ICommand) GetValue( CommandBindingProperty );
            set => SetValue( CommandBindingProperty, value );
        }

        private static void PropertyChangedCallback( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            CommandBinding?.Execute( AssociatedObject.SelectedItem );
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }
    }
}