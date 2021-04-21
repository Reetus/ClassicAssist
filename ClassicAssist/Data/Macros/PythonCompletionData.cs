using System;
using System.Windows.Media;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
using ClassicAssist.UI.Controls;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace ClassicAssist.Data.Macros
{
    public class PythonCompletionData : ICompletionData
    {
        public PythonCompletionData( string name, string fullName, string description, string insertText )
        {
            Name = fullName;
            Description = description;
            Text = insertText;

            Example = MacroCommandHelp.ResourceManager.GetString( $"{name.ToUpper()}_COMMAND_EXAMPLE" );

            Content = new CompletionEntry( fullName, Example );
        }

        public string Example { get; set; }
        public string Name { get; set; }

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