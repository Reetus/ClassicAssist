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

        private TextDocument _subscribedDocument;

        protected override void OnAttached()
        {
            base.OnAttached();

            if ( AssociatedObject != null )
            {
                _textEditor = AssociatedObject;
                _textEditor.DocumentChanged += OnEditorDocumentChanged;
                SubscribeToDocument( _textEditor.Document );
            }
        }

        protected override void OnDetaching()
        {
            if ( _textEditor != null )
            {
                _textEditor.DocumentChanged -= OnEditorDocumentChanged;
                UnsubscribeFromDocument();
            }

            base.OnDetaching();
        }

        private void OnEditorDocumentChanged( object sender, System.EventArgs e )
        {
            UnsubscribeFromDocument();
            SubscribeToDocument( _textEditor?.Document );
        }

        private void SubscribeToDocument( TextDocument document )
        {
            if ( document == null )
            {
                return;
            }

            _subscribedDocument = document;
            _subscribedDocument.Changed += Document_Changed;
        }

        private void UnsubscribeFromDocument()
        {
            if ( _subscribedDocument != null )
            {
                _subscribedDocument.Changed -= Document_Changed;
                _subscribedDocument = null;
            }
        }

        private void Document_Changed( object sender, DocumentChangeEventArgs e )
        {
            if ( AvalonEditBehaviour.IsProgrammaticTextChange )
            {
                return;
            }

            if ( _breakPointMargin?.Breakpoints == null || _breakPointMargin.Breakpoints.Count == 0 )
            {
                return;
            }

            TextDocument doc = _textEditor.Document;

            int startLine = doc.GetLineByOffset( e.Offset ).LineNumber;

            int insertedNewlines = e.InsertionLength > 0
                ? e.InsertedText.Text.Count( c => c == '\n' )
                : 0;

            int removedNewlines = e.RemovalLength > 0
                ? e.RemovedText.Text.Count( c => c == '\n' )
                : 0;

            int lineDelta = insertedNewlines - removedNewlines;

            if ( lineDelta != 0 )
            {
                ShiftBreakpoints( startLine, lineDelta );
            }
        }

        private void ShiftBreakpoints( int fromLine, int delta )
        {
            ObservableCollection<int> breakpoints = _breakPointMargin?.Breakpoints;

            if ( breakpoints == null || breakpoints.Count == 0 )
            {
                return;
            }

            List<int> shifted = new List<int>( breakpoints.Count );

            foreach ( int bp in breakpoints )
            {
                if ( bp > fromLine )
                {
                    int newLine = bp + delta;

                    if ( newLine > 0 )
                    {
                        shifted.Add( newLine );
                    }
                }
                else
                {
                    shifted.Add( bp );
                }
            }

            shifted.Sort();

            breakpoints.Clear();

            foreach ( int bp in shifted )
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

            if ( editor._breakPointMargin != null )
            {
                // Reuse existing margin, just swap breakpoints
                if ( editor._breakPointMargin.Breakpoints != null )
                {
                    editor._breakPointMargin.Breakpoints.CollectionChanged -= editor.Breakpoints_CollectionChanged;
                }

                // Unsubscribe from Document.Changed during macro switch so the
                // full-text replacement doesn't shift/clear the new macro's breakpoints.
                // Resubscribe on next dispatcher idle after the text swap completes.
                editor.UnsubscribeFromDocument();

                editor._breakPointMargin.Breakpoints = (ObservableCollection<int>) e.NewValue;

                if ( editor._breakPointMargin.Breakpoints != null )
                {
                    editor._breakPointMargin.Breakpoints.CollectionChanged += editor.Breakpoints_CollectionChanged;
                }

                editor._breakPointMargin.InvalidateVisual();

                editor._textEditor?.Dispatcher.BeginInvoke(
                    (Action) ( () => editor.SubscribeToDocument( editor._textEditor?.Document ) ),
                    System.Windows.Threading.DispatcherPriority.Loaded );

                return;
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
            _breakPointMargin?.InvalidateVisual();
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