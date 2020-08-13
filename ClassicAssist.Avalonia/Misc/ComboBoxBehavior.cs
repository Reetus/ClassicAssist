using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace ClassicAssist.Avalonia.Misc
{
    public class ComboBoxBehavior : Behavior<ComboBox>
    {
        public static readonly AvaloniaProperty<ICommand> CommandBindingProperty =
            AvaloniaProperty.Register<ComboBoxBehavior, ICommand>( "CommandBinding", inherits: true );

        public static readonly AvaloniaProperty<object> CommandParameterProperty =
            AvaloniaProperty.Register<ComboBoxBehavior, object>( "CommandParameter", inherits: true );

        public static readonly AvaloniaProperty<bool> OnlyUserTriggeredProperty =
            AvaloniaProperty.Register<ComboBoxBehavior, bool>( "OnlyUserTriggered", inherits: true );

        private bool _userTriggered;

        public ICommand CommandBinding
        {
            get => this.GetValue<ICommand>( CommandBindingProperty );
            set => SetValue( CommandBindingProperty, value );
        }

        public object CommandParameter
        {
            get => GetValue( CommandParameterProperty );
            set => SetValue( CommandParameterProperty, value );
        }

        public bool OnlyUserTriggered
        {
            get => this.GetValue<bool>( OnlyUserTriggeredProperty );
            set => SetValue( OnlyUserTriggeredProperty, value );
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged += OnSelectionChanged;
            AssociatedObject.GotFocus += OnGotFocus;
            AssociatedObject.LostFocus += OnLostFocus;
        }

        private void OnGotFocus( object sender, GotFocusEventArgs e )
        {
            _userTriggered = true;
        }

        private void OnLostFocus( object sender, RoutedEventArgs e )
        {
            _userTriggered = false;
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
            AssociatedObject.GotFocus -= OnGotFocus;
            AssociatedObject.LostFocus -= OnLostFocus;
        }
    }
}