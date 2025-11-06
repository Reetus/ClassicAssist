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
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;

namespace ClassicAssist.UI.Misc
{
    public class BreakpointMargin : AbstractMargin
    {
        private const double _radius = 5;
        private int? _hoveredLine;

        public BreakpointMargin()
        {
            ClipToBounds = true;
            IsHitTestVisible = true;
            Cursor = Cursors.Hand;
        }

        public ObservableCollection<int> Breakpoints { get; set; }

        public event Action<int> BreakpointToggled;

        protected override Size MeasureOverride( Size availableSize )
        {
            return new Size( 20, 0 );
        }

        protected override HitTestResult HitTestCore( PointHitTestParameters hitTestParameters )
        {
            return new PointHitTestResult( this, hitTestParameters.HitPoint );
        }

        protected override void OnMouseDown( MouseButtonEventArgs e )
        {
            base.OnMouseDown( e );

            TextView textView = TextView;

            if ( textView == null || !textView.VisualLinesValid )
            {
                return;
            }

            Point pos = e.GetPosition( this );
            int lineNumber = (int) ( ( pos.Y + textView.ScrollOffset.Y ) / textView.DefaultLineHeight ) + 1;

            if ( Breakpoints.Contains( lineNumber ) )
            {
                Breakpoints.Remove( lineNumber );
            }
            else
            {
                Breakpoints.Add( lineNumber );
            }

            BreakpointToggled?.Invoke( lineNumber );
            InvalidateVisual();

            e.Handled = true;
        }

        protected override void OnMouseMove( MouseEventArgs e )
        {
            base.OnMouseMove( e );

            TextView textView = TextView;

            if ( textView == null || !textView.VisualLinesValid )
            {
                return;
            }

            Point pos = e.GetPosition( textView );
            VisualLine visualLine = textView.GetVisualLineFromVisualTop( pos.Y );
            int? newHoveredLine = visualLine?.FirstDocumentLine.LineNumber;

            if ( newHoveredLine != _hoveredLine )
            {
                _hoveredLine = newHoveredLine;
                InvalidateVisual();
            }
        }

        protected override void OnTextViewChanged( TextView oldTextView, TextView newTextView )
        {
            base.OnTextViewChanged( oldTextView, newTextView );

            if ( oldTextView != null )
            {
                oldTextView.VisualLinesChanged -= TextView_VisualLinesChanged;
                oldTextView.ScrollOffsetChanged -= TextView_ScrollOffsetChanged;
            }

            if ( newTextView != null )
            {
                newTextView.VisualLinesChanged += TextView_VisualLinesChanged;
                newTextView.ScrollOffsetChanged += TextView_ScrollOffsetChanged;
            }
        }

        private void TextView_VisualLinesChanged( object sender, EventArgs e )
        {
            InvalidateVisual(); // redraw when lines are regenerated
        }

        private void TextView_ScrollOffsetChanged( object sender, EventArgs e )
        {
            InvalidateVisual(); // redraw when scrolling
        }

        protected override void OnMouseLeave( MouseEventArgs e )
        {
            base.OnMouseLeave( e );
            _hoveredLine = null;
            InvalidateVisual();
        }

        protected override void OnRender( DrawingContext dc )
        {
            base.OnRender( dc );

            TextView textView = TextView;

            if ( textView == null || !textView.VisualLinesValid )
            {
                return;
            }

            if ( Breakpoints == null )
            {
                return;
            }

            foreach ( VisualLine visualLine in textView.VisualLines )
            {
                int lineNumber = visualLine.FirstDocumentLine.LineNumber;

                if ( !Breakpoints.Contains( lineNumber ) )
                {
                    continue;
                }

                double y = ( lineNumber - 1 ) * textView.DefaultLineHeight - textView.ScrollOffset.Y + textView.Margin.Top;

                Point center = new Point( 10, y + 7 );
                dc.DrawEllipse( Brushes.Red, null, center, _radius, _radius );
            }
        }
    }
}