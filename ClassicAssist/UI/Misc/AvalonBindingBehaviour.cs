using System;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace ClassicAssist.UI.Misc
{
    public class AvalonBindingBehaviour : Behavior<TextEditor>
    {
        public static readonly DependencyProperty DocumentBindingProperty = DependencyProperty.Register(
            "DocumentBinding", typeof( TextDocument ), typeof( AvalonBindingBehaviour ),
            new FrameworkPropertyMetadata( default( TextDocument ),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback ) );

        public static readonly DependencyProperty CaretBindingProperty = DependencyProperty.Register( "CaretBinding",
            typeof( int ), typeof( AvalonBindingBehaviour ),
            new FrameworkPropertyMetadata( default( int ), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                PropertyChangedCallback ) );

        public int CaretBinding
        {
            get => (int) GetValue( CaretBindingProperty );
            set => SetValue( CaretBindingProperty, value );
        }

        public TextDocument DocumentBinding
        {
            get => (TextDocument) GetValue( DocumentBindingProperty );
            set => SetValue( DocumentBindingProperty, value );
        }

        private static void PropertyChangedCallback( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += OnLoaded;
            AssociatedObject.TextArea.Caret.PositionChanged += CaretOnPositionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.Loaded -= OnLoaded;
            AssociatedObject.TextArea.Caret.PositionChanged -= CaretOnPositionChanged;
        }

        private void CaretOnPositionChanged( object sender, EventArgs e )
        {
            if ( sender is Caret caret )
            {
                CaretBinding = caret.Offset;
            }
        }

        private void OnLoaded( object sender, RoutedEventArgs args )
        {
            if ( sender is TextEditor textEditor )
            {
                DocumentBinding = textEditor.Document;
            }
        }
    }
}