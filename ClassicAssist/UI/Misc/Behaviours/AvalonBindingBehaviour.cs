#region License

// Copyright (C) 2025 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Windows;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Microsoft.Xaml.Behaviors;

namespace ClassicAssist.UI.Misc.Behaviours
{
    public class AvalonBindingBehaviour : Behavior<TextEditor>
    {
        public static readonly DependencyProperty DocumentBindingProperty = DependencyProperty.Register( nameof( DocumentBinding ), typeof( TextDocument ),
            typeof( AvalonBindingBehaviour ), new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback ) );

        public static readonly DependencyProperty CaretBindingProperty = DependencyProperty.Register( nameof( CaretBinding ), typeof( int ), typeof( AvalonBindingBehaviour ),
            new FrameworkPropertyMetadata( 0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback ) );

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