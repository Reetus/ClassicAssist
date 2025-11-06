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
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using Microsoft.Xaml.Behaviors;

namespace ClassicAssist.UI.Misc.Behaviours
{
    public class AvalonEditPausedLineBehaviour : Behavior<TextEditor>
    {
        public static readonly DependencyProperty IsPausedProperty = DependencyProperty.Register( nameof( IsPaused ), typeof( bool ), typeof( AvalonEditPausedLineBehaviour ),
            new PropertyMetadata( false, OnPausedStateChanged ) );

        public static readonly DependencyProperty PausedLineNumberProperty = DependencyProperty.Register( nameof( PausedLineNumber ), typeof( int ),
            typeof( AvalonEditPausedLineBehaviour ), new PropertyMetadata( 0, OnPausedStateChanged ) );

        private TextEditor _textEditor;

        public bool IsPaused
        {
            get => (bool) GetValue( IsPausedProperty );
            set => SetValue( IsPausedProperty, value );
        }

        public int PausedLineNumber
        {
            get => (int) GetValue( PausedLineNumberProperty );
            set => SetValue( PausedLineNumberProperty, value );
        }

        private static void OnPausedStateChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            if ( !( d is AvalonEditPausedLineBehaviour behaviour ) )
            {
                return;
            }

            if ( e.Property == IsPausedProperty && (bool) e.NewValue || behaviour.IsPaused && e.Property == PausedLineNumberProperty )
            {
                behaviour.AddHighlighter();
            }
            else
            {
                behaviour.RemoveHighlighter();
            }
        }

        private void RemoveHighlighter()
        {
            PausedLineHighlighter existing = _textEditor.TextArea.TextView.BackgroundRenderers.OfType<PausedLineHighlighter>().FirstOrDefault();

            if ( existing == null )
            {
                return;
            }

            _textEditor.TextArea.TextView.BackgroundRenderers.Remove( existing );
        }

        private void AddHighlighter()
        {
            PausedLineHighlighter highlighter = GetOrCreateHighlighter( _textEditor );
            highlighter.IsPaused = true;
            highlighter.PausedLine = PausedLineNumber;
        }

        private static PausedLineHighlighter GetOrCreateHighlighter( TextEditor editor )
        {
            PausedLineHighlighter existing = editor.TextArea.TextView.BackgroundRenderers.OfType<PausedLineHighlighter>().FirstOrDefault();

            if ( existing != null )
            {
                return existing;
            }

            PausedLineHighlighter h = new PausedLineHighlighter( editor );
            editor.TextArea.TextView.BackgroundRenderers.Add( h );
            return h;
        }

        protected override void OnAttached()
        {
            _textEditor = AssociatedObject;
        }
    }

    internal class PausedLineHighlighter : IBackgroundRenderer
    {
        private readonly TextEditor _editor;
        private bool _isPaused;
        private int _pausedLine;

        public PausedLineHighlighter( TextEditor editor )
        {
            _editor = editor;
        }

        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                if ( _isPaused == value )
                {
                    return;
                }

                _isPaused = value;
                _editor.TextArea.TextView.InvalidateLayer( KnownLayer.Background );
                ScrollToLineIfPaused();
            }
        }

        public int PausedLine
        {
            get => _pausedLine;
            set
            {
                if ( _pausedLine == value )
                {
                    return;
                }

                _pausedLine = value;
                _editor.TextArea.TextView.InvalidateLayer( KnownLayer.Background );
                ScrollToLineIfPaused();
            }
        }

        public KnownLayer Layer => KnownLayer.Background;

        public void Draw( TextView textView, DrawingContext drawingContext )
        {
            if ( !_isPaused || _pausedLine <= 0 || _pausedLine > _editor.Document.LineCount )
            {
                return;
            }

            textView.EnsureVisualLines();
            DocumentLine line = _editor.Document.GetLineByNumber( _pausedLine );
            IEnumerable<Rect> rects = BackgroundGeometryBuilder.GetRectsForSegment( textView, line );

            foreach ( Rect r in rects )
            {
                drawingContext.DrawRectangle( new SolidColorBrush( Color.FromArgb( 80, 255, 255, 0 ) ), // translucent yellow
                    null, new Rect( r.Location, new Size( textView.ActualWidth, r.Height ) ) );
            }
        }

        private void ScrollToLineIfPaused()
        {
            if ( _isPaused && _pausedLine > 0 && _pausedLine <= _editor.Document.LineCount )
            {
                _editor.ScrollTo( _pausedLine, 0 );
            }
        }
    }
}