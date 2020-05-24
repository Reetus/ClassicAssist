using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using ClassicAssist.Data.Macros;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;
using Microsoft.Scripting.Utils;

namespace ClassicAssist.UI.Views
{
    /// <summary>
    ///     Interaction logic for MacrosTabControl.xaml
    /// </summary>
    public partial class MacrosTabControl : UserControl
    {
        private List<PythonCompletionData> _completionData;
        private CompletionWindow _completionWindow;

        public MacrosTabControl()
        {
            InitializeComponent();
        }

        private void Grid_Initialized( object sender, EventArgs e )
        {
            CodeTextEditor.SyntaxHighlighting = HighlightingLoader.Load(
                new XmlTextReader( Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ),
                    "Python.Dark.xshd" ) ), HighlightingManager.Instance );

            IEnumerable<Type> namespaces = Assembly.GetExecutingAssembly().GetTypes().Where( t =>
                t.Namespace != null && t.IsPublic && t.IsClass && t.Namespace.EndsWith( "Macros.Commands" ) );

            _completionData = new List<PythonCompletionData>();

            foreach ( Type type in namespaces )
            {
                MethodInfo[] methods = type.GetMethods( BindingFlags.Public | BindingFlags.Static );

                foreach ( MethodInfo methodInfo in methods )
                {
                    CommandsDisplayAttribute attr = methodInfo.GetCustomAttribute<CommandsDisplayAttribute>();

                    if ( attr == null )
                    {
                        continue;
                    }

                    _completionData.Add(
                        new PythonCompletionData( methodInfo.Name, attr.Description, attr.InsertText ) );
                }
            }

            CodeTextEditor.TextArea.TextEntered += OnTextEntered;
            SearchPanel.Install( CodeTextEditor );
        }

        private void OnTextEntered( object sender, TextCompositionEventArgs e )
        {
            DocumentLine line = CodeTextEditor.TextArea.Document.Lines[CodeTextEditor.TextArea.Caret.Line - 1];

            string trimmed = CodeTextEditor.TextArea.Document.GetText( line ).TrimStart( ' ', '\t' );

            if ( trimmed.TrimStart( ' ', '\t' ).Length < 3 )
            {
                return;
            }

            List<PythonCompletionData> data = _completionData.Where( m =>
                ( (string) m.Content ).StartsWith( trimmed, StringComparison.InvariantCultureIgnoreCase ) ).ToList();

            if ( data.Count <= 0 )
            {
                return;
            }

            _completionWindow = new CompletionWindow( CodeTextEditor.TextArea );
            _completionWindow.CompletionList.CompletionData.AddRange( data );
            _completionWindow.Show();
            _completionWindow.Closed += delegate { _completionWindow = null; };
        }
    }
}