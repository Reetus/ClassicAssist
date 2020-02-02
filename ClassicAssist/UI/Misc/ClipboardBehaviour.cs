using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace ClassicAssist.UI.Misc
{
    public class ClipboardBehaviour : Behavior<UIElement>
    {
        public static readonly DependencyProperty CopyCommandProperty = DependencyProperty.Register( "CopyCommand",
            typeof( ICommand ), typeof( ClipboardBehaviour ),
            new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None ) );

        public static readonly DependencyProperty PasteCommandProperty = DependencyProperty.Register( "PasteCommand",
            typeof( ICommand ), typeof( ClipboardBehaviour ),
            new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None ) );

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            "CommandParameter",
            typeof( object ), typeof( ClipboardBehaviour ),
            new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None ) );

        public object CommandParameter
        {
            get => GetValue( CommandParameterProperty );
            set => SetValue( CommandParameterProperty, value );
        }

        public ICommand CopyCommand
        {
            get => (ICommand) GetValue( CopyCommandProperty );
            set => SetValue( CopyCommandProperty, value );
        }

        public ICommand PasteCommand
        {
            get => (ICommand) GetValue( PasteCommandProperty );
            set => SetValue( PasteCommandProperty, value );
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            CommandBinding CopyCommandBinding = new CommandBinding(
                ApplicationCommands.Copy, CopyCommandExecuted, CopyCommandCanExecute );
            AssociatedObject.CommandBindings.Add( CopyCommandBinding );

            CommandBinding CutCommandBinding = new CommandBinding(
                ApplicationCommands.Cut, CutCommandExecuted, CutCommandCanExecute );
            AssociatedObject.CommandBindings.Add( CutCommandBinding );

            CommandBinding PasteCommandBinding = new CommandBinding( ApplicationCommands.Paste, PasteCommandExecuted,
                PasteCommandCanExecute );
            AssociatedObject.CommandBindings.Add( PasteCommandBinding );
        }

        private void CopyCommandExecuted( object target, ExecutedRoutedEventArgs e )
        {
            CopyCommand?.Execute( CommandParameter );
        }

        private void CopyCommandCanExecute( object target, CanExecuteRoutedEventArgs e )
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void CutCommandExecuted( object target, ExecutedRoutedEventArgs e )
        {
        }

        private void CutCommandCanExecute( object target, CanExecuteRoutedEventArgs e )
        {
        }

        private void PasteCommandExecuted( object target, ExecutedRoutedEventArgs e )
        {
            PasteCommand?.Execute( CommandParameter );
        }

        private void PasteCommandCanExecute( object target, CanExecuteRoutedEventArgs e )
        {
            e.CanExecute = true;
            e.Handled = true;
        }
    }
}