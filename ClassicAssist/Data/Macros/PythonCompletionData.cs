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

            textArea.Document.Replace(
                new TextSegment { StartOffset = line.Offset, EndOffset = line.EndOffset, Length = line.Length }, Text );
        }

        public ImageSource Image => Properties.Resources.python.ToImageSource();
        public string Text { get; }

        public object Content { get; }
        public object Description { get; }
        public double Priority { get; }
    }
}