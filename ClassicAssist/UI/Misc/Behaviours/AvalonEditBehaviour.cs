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

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Xaml.Behaviors;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;

namespace ClassicAssist.UI.Misc.Behaviours
{
    public sealed class AvalonEditBehaviour : Behavior<TextEditor>
    {
        internal static bool IsProgrammaticTextChange { get; private set; }

        public static readonly DependencyProperty TextBindingProperty = DependencyProperty.Register( nameof( TextBinding ), typeof( string ), typeof( AvalonEditBehaviour ),
            new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback ) );

        private object _textSource;

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
                AssociatedObject.DataContextChanged += AssociatedObjectOnDataContextChanged;
            }
        }

        private void AssociatedObjectOnDataContextChanged( object sender, DependencyPropertyChangedEventArgs e )
        {
            // A different item is being edited, don't allow undo back into the previous item's text
            AssociatedObject?.Document?.UndoStack.ClearAll();
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
                AssociatedObject.DataContextChanged -= AssociatedObjectOnDataContextChanged;
            }
        }

        private void AssociatedObjectOnTextChanged( object sender, EventArgs eventArgs )
        {
            if ( !( sender is TextEditor textEditor ) )
            {
                return;
            }

            if ( textEditor.Document == null )
            {
                return;
            }

            TextBinding = textEditor.Document.Text;
            _textSource = GetTextSource();
        }

        private object GetTextSource()
        {
            return BindingOperations.GetBindingExpression( this, TextBindingProperty )?.ResolvedSource;
        }

        private static void PropertyChangedCallback( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs )
        {
            AvalonEditBehaviour behavior = dependencyObject as AvalonEditBehaviour;

            TextEditor editor = behavior?.AssociatedObject;

            if ( editor?.Document == null )
            {
                return;
            }

            string text = dependencyPropertyChangedEventArgs.NewValue?.ToString() ?? string.Empty;

            // The change originated from the editor itself, replacing the document would add a whole document
            // replace to the undo stack for every keystroke and clear the redo stack
            if ( editor.Document.Text == text )
            {
                return;
            }

            object textSource = behavior.GetTextSource();

            if ( textSource != null && textSource == behavior._textSource )
            {
                // Same item changed externally (e.g. recording), apply only the changed region so the
                // change is undoable and existing undo history is kept
                ReplaceChangedRegion( editor.Document, text );
                return;
            }

            behavior._textSource = textSource;

            IsProgrammaticTextChange = true;

            try
            {
                editor.Document.Text = text;
            }
            finally
            {
                IsProgrammaticTextChange = false;
            }

            editor.CaretOffset = 0;
            editor.Document.UndoStack.ClearAll();
        }

        private static void ReplaceChangedRegion( TextDocument document, string text )
        {
            string current = document.Text;
            int maxLength = Math.Min( current.Length, text.Length );

            int prefix = 0;

            while ( prefix < maxLength && current[prefix] == text[prefix] )
            {
                prefix++;
            }

            int suffix = 0;

            while ( suffix < maxLength - prefix && current[current.Length - 1 - suffix] == text[text.Length - 1 - suffix] )
            {
                suffix++;
            }

            document.Replace( prefix, current.Length - prefix - suffix, text.Substring( prefix, text.Length - prefix - suffix ) );
        }
    }
}