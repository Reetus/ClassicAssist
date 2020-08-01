#region License

// Copyright (C) 2020 Reetus
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
using Avalonia;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;

namespace ClassicAssist.Avalonia.Misc
{
    /*
     * Feels a bit hacky, there must be a proper way to do this with Observables?
     */
    public class AvalonEditBehaviour : Behavior<TextEditor>
    {
        public static readonly DirectProperty<AvalonEditBehaviour, string> TextProperty =
            AvaloniaProperty.RegisterDirect<AvalonEditBehaviour, string>( nameof( Text ), o => o.Text,
                ( o, v ) => { o.Text = v; }, defaultBindingMode: BindingMode.TwoWay );

        private string _text;

        public string Text
        {
            get => _text;
            set
            {
                SetAndRaise( TextProperty, ref _text, value );
                SetText( value );
            }
        }

        private void SetText( string value )
        {
            if ( AssociatedObject != null )
            {
                AssociatedObject.Document.Text = value ?? string.Empty;
            }
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

            if ( Text == null || Text.Equals( textEditor.Document.Text ) )
            {
                return;
            }

            int carot = textEditor.CaretOffset;
            Text = textEditor.Document.Text;
            textEditor.CaretOffset = carot;
        }

        private void AssociatedObjectOnLostFocus( object sender, RoutedEventArgs routedEventArgs )
        {
            if ( !( sender is TextEditor textEditor ) )
            {
                return;
            }

            if ( textEditor.Document != null )
            {
                Text = textEditor.Document.Text;
            }
        }
    }
}