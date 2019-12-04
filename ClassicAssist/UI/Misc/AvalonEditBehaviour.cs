using System;
using System.Windows;
using System.Windows.Interactivity;
using ICSharpCode.AvalonEdit;

namespace ClassicAssist.UI.Misc
{
    public sealed class AvalonEditBehaviour : Behavior<TextEditor>
    {
        public static readonly DependencyProperty TextBindingProperty =
            DependencyProperty.Register( "TextBinding", typeof( string ), typeof( AvalonEditBehaviour ),
                new FrameworkPropertyMetadata( default( string ), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    PropertyChangedCallback ) );

        public string TextBinding
        {
            get => (string) GetValue( TextBindingProperty );
            set => SetValue( TextBindingProperty, value );
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            if ( AssociatedObject != null )
            {
                AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
                AssociatedObject.LostFocus += AssociatedObjectOnLostFocus;
            }
        }

        private void AssociatedObjectOnLostFocus( object sender, RoutedEventArgs routedEventArgs )
        {
            if ( !( sender is TextEditor textEditor ) )
            {
                return;
            }

            if ( textEditor.Document != null )
            {
                TextBinding = textEditor.Document.Text;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if ( AssociatedObject != null )
            {
                AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
                AssociatedObject.LostFocus -= AssociatedObjectOnLostFocus;
            }
        }

        private void AssociatedObjectOnTextChanged( object sender, EventArgs eventArgs )
        {
            if ( !( sender is TextEditor textEditor ) )
            {
            }

            //if (textEditor.Document != null)
            //    TextBinding = textEditor.Document.Text;
        }

        private static void PropertyChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs )
        {
            AvalonEditBehaviour behavior = dependencyObject as AvalonEditBehaviour;

            TextEditor editor = behavior?.AssociatedObject;

            if ( editor?.Document == null )
            {
                return;
            }

            if ( dependencyPropertyChangedEventArgs.NewValue == null )
            {
                return;
            }

            int caretOffset = editor.CaretOffset;
            editor.Document.Text = dependencyPropertyChangedEventArgs.NewValue.ToString();
            editor.CaretOffset = 0;
        }
    }
}