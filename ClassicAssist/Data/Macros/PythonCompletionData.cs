using System;
using System.Windows.Media;
using ClassicAssist.Misc;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace ClassicAssist.Data.Macros
{
    public class PythonCompletionData : ICompletionData
    {
        public PythonCompletionData( string text, string description, string insertText )
        {
            Description = description;
            Text = insertText;
            Content = text;
        }

        public void Complete( TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs )
        {
            DocumentLine line = textArea.Document.Lines[textArea.Caret.Line - 1];

            string text = textArea.Document.GetText( line );

            int offset = completionSegment.Offset;

            // Walk backwards decrementing the offset if not ' ' || '\t'
            for ( int i = completionSegment.Offset; i > line.Offset; i-- )
            {
                int stringOffset = i - line.Offset - 1;

                if ( text[stringOffset] != ' ' && text[stringOffset] != '\t' )
                {
                    offset--;
                    continue;
                }

                break;
            }

            ISegment segment = new AnchorSegment( textArea.Document, offset, line.EndOffset - offset );

            textArea.Document.Replace( segment, Text );
        }

        public ImageSource Image => Properties.Resources.python.ToImageSource();
        public string Text { get; }

        public object Content { get; }
        public object Description { get; }
        public double Priority { get; }
    }
}