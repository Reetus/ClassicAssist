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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Xaml.Behaviors;

namespace ClassicAssist.UI.Misc.Behaviours
{
    public class AvalonEditBreakpointMarginBehaviour : Behavior<TextEditor>
    {
        public static readonly DependencyProperty BreakpointsProperty = DependencyProperty.RegisterAttached( "Breakpoints", typeof( ObservableCollection<int> ),
            typeof( AvalonEditBreakpointMarginBehaviour ), new PropertyMetadata( null, OnBreakpointsChanged ) );

        private BreakpointMargin _breakPointMargin;
        private TextEditor _textEditor;

        public static void SetBreakpoints( DependencyObject element, ObservableCollection<int> value )
        {
            element?.SetValue( BreakpointsProperty, value );
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            if ( AssociatedObject != null )
            {
                _textEditor = AssociatedObject;
                // TODO
                //_textEditor.TextArea.TextView.Document.Changed += Document_Changed;
            }
        }

        private void Document_Changed( object sender, DocumentChangeEventArgs e )
        {
            TextDocument doc = _textEditor.Document;
            int startLine = doc.GetLineByOffset( e.Offset ).LineNumber;

            int lineDelta = 0;

            if ( e.InsertionLength == doc.TextLength )
            {
                return;
            }

            if ( e.InsertionLength > 0 )
            {
                // Count inserted newlines
                string insertedText = e.InsertedText.Text;
                lineDelta = insertedText.Count( c => c == '\n' );

                if ( lineDelta > 0 )
                {
                    ShiftBreakpoints( startLine, lineDelta );
                }
            }
            else if ( e.RemovalLength > 0 )
            {
                // Count removed newlines
                string removedText = e.RemovedText.Text;
                lineDelta = -removedText.Count( c => c == '\n' );

                if ( lineDelta < 0 )
                {
                    ShiftBreakpoints( startLine, lineDelta );
                }
            }
        }

        private void ShiftBreakpoints( int fromLine, int delta )
        {
            if ( delta == 0 )
            {
                return;
            }

            ObservableCollection<int> breakpoints = (ObservableCollection<int>) GetValue( BreakpointsProperty );

            if ( breakpoints == null )
            {
                return;
            }

            foreach ( int bp in breakpoints.ToList().Where( bp => bp > fromLine ) )
            {
                breakpoints.Remove( bp );
                breakpoints.Add( bp + delta );
            }

            List<int> sortedBreakpoints = breakpoints.OrderBy( x => x ).ToList();

            breakpoints.Clear();

            foreach ( int bp in sortedBreakpoints )
            {
                breakpoints.Add( bp );
            }
        }

        private static void OnBreakpointsChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            if ( !( d is AvalonEditBreakpointMarginBehaviour editor ) )
            {
                return;
            }

            if ( e.OldValue != null )
            {
                editor.RemoveBreakpointMargin();
            }

            if ( e.NewValue != null )
            {
                editor.AddBreakpointMargin( (ObservableCollection<int>) e.NewValue );
            }
        }

        private void AddBreakpointMargin( ObservableCollection<int> breakpoints )
        {
            if ( breakpoints == null || _textEditor == null )
            {
                return;
            }

            BreakpointMargin margin = new BreakpointMargin { Breakpoints = breakpoints };
            _textEditor.TextArea.LeftMargins.Insert( 0, margin );
            _breakPointMargin = margin;

            if ( _breakPointMargin.Breakpoints != null )
            {
                _breakPointMargin.Breakpoints.CollectionChanged += Breakpoints_CollectionChanged;
            }
        }

        private void Breakpoints_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            SetValue( BreakpointsProperty, (ObservableCollection<int>) sender );
        }

        private void RemoveBreakpointMargin()
        {
            TextEditor editor = _textEditor;

            for ( int i = editor.TextArea.LeftMargins.Count - 1; i >= 0; i-- )
            {
                if ( editor.TextArea.LeftMargins[i] is BreakpointMargin )
                {
                    editor.TextArea.LeftMargins.RemoveAt( i );
                }
            }

            if ( _breakPointMargin == null )
            {
                return;
            }

            _breakPointMargin.Breakpoints.CollectionChanged -= Breakpoints_CollectionChanged;
            _breakPointMargin = null;
        }
    }
}