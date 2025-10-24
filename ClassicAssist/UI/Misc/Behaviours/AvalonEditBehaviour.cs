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
using Microsoft.Xaml.Behaviors;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace ClassicAssist.UI.Misc.Behaviours
{
    public sealed class AvalonEditBehaviour : Behavior<TextEditor>
    {
        public static readonly DependencyProperty TextBindingProperty = DependencyProperty.Register( nameof( TextBinding ), typeof( string ), typeof( AvalonEditBehaviour ),
            new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback ) );

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
                return;
            }

            if ( textEditor.Document == null )
            {
                return;
            }

            int carot = textEditor.CaretOffset;
            TextBinding = textEditor.Document.Text;
            textEditor.CaretOffset = carot;
        }

        private static void PropertyChangedCallback( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs )
        {
            AvalonEditBehaviour behavior = dependencyObject as AvalonEditBehaviour;

            TextEditor editor = behavior?.AssociatedObject;

            if ( editor?.Document == null )
            {
                return;
            }

            editor.Document.Text = dependencyPropertyChangedEventArgs.NewValue?.ToString() ?? string.Empty;
            editor.CaretOffset = 0;

            if ( editor.Document.UndoStack.SizeLimit == 0 )
            {
                editor.Document.UndoStack.SizeLimit = int.MaxValue;
            }
        }
    }
}