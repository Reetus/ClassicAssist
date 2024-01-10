using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using Assistant;
using ClassicAssist.Data.Macros;
using ClassicAssist.UI.Controls;
using ICSharpCode.AvalonEdit;
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
    public partial class MacrosTabControl
    {
        private readonly ToolTip _toolTip = new ToolTip();
        private List<PythonCompletionData> _completionData;
        private CompletionWindow _completionWindow;

        public MacrosTabControl()
        {
            InitializeComponent();
        }

        private void Grid_Initialized( object sender, EventArgs e )
        {
            CodeTextEditor.SyntaxHighlighting = HighlightingLoader.Load(
                new XmlTextReader( Path.Combine( Engine.StartupPath, "Python.Dark.xshd" ) ), HighlightingManager.Instance );

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

                    string fullName = $"{methodInfo.Name}(";
                    bool first = true;

                    foreach ( ParameterInfo parameterInfo in methodInfo.GetParameters() )
                    {
                        if ( first )
                        {
                            first = false;
                        }
                        else
                        {
                            fullName += ", ";
                        }

                        bool optional = parameterInfo.RawDefaultValue == null || parameterInfo.RawDefaultValue.GetType() != typeof( DBNull );

                        fullName += $"{( optional ? "[" : "" )}{parameterInfo.ParameterType.Name} {parameterInfo.Name}{( optional ? "]" : "" )}";
                    }

                    fullName += $"):{methodInfo.ReturnType.Name}";

                    _completionData.Add( new PythonCompletionData( methodInfo.Name, fullName, attr.Description, attr.InsertText ) );
                }
            }

            CodeTextEditor.TextArea.TextEntered += OnTextEntered;
            SearchPanel.Install( CodeTextEditor );
            CodeTextEditor.MouseHover += OnMouseHover;
            CodeTextEditor.MouseHoverStopped += OnMouseHoverStopped;
        }

        private void OnMouseHoverStopped( object sender, MouseEventArgs e )
        {
            _toolTip.IsOpen = false;
        }

        private void OnMouseHover( object sender, MouseEventArgs e )
        {
            //https://stackoverflow.com/questions/51711692/how-to-fire-an-event-when-mouse-hover-over-a-specific-text-in-avalonedit
            TextViewPosition? pos = CodeTextEditor.GetPositionFromPoint( e.GetPosition( CodeTextEditor ) );

            if ( !pos.HasValue )
            {
                return;
            }

            DocumentLine line = CodeTextEditor.TextArea.Document.GetLineByNumber( pos.Value.Line );

            if ( line == null )
            {
                return;
            }

            try
            {
                string fullLine = CodeTextEditor.Document.GetText( line.Offset, line.Length );

                int startPosition = 0;
                int endPosition = fullLine.Length;

                for ( int i = pos.Value.Column; i > -1; i-- )
                {
                    if ( char.IsLetter( fullLine[i] ) )
                    {
                        continue;
                    }

                    startPosition = i + 1;
                    break;
                }

                for ( int i = pos.Value.Column; i < fullLine.Length; i++ )
                {
                    if ( char.IsLetter( fullLine[i] ) )
                    {
                        continue;
                    }

                    endPosition = i;
                    break;
                }

                if ( endPosition <= startPosition )
                {
                    return;
                }

                string word = fullLine.Substring( startPosition, endPosition - startPosition );

                string tooltipText = string.Empty;

                IEnumerable<PythonCompletionData> matches = _completionData.Where( i => i.MethodName.Equals( word ) );

                tooltipText = matches.Aggregate( tooltipText, ( current, match ) => current + ( string.IsNullOrEmpty( current ) ? match.Name : $"\n{match.Name}" ) );

                if ( string.IsNullOrEmpty( tooltipText ) )
                {
                    return;
                }

                _toolTip.PlacementTarget = this;
                _toolTip.Content = tooltipText;
                _toolTip.IsOpen = true;
                e.Handled = true;
            }
            catch ( Exception )
            {
                // ignored
            }
        }

        private void OnTextEntered( object sender, TextCompositionEventArgs e )
        {
            DocumentLine line = CodeTextEditor.TextArea.Document.Lines[CodeTextEditor.TextArea.Caret.Line - 1];

            string trimmed = CodeTextEditor.TextArea.Document.GetText( line ).TrimStart( ' ', '\t' );

            if ( trimmed.TrimStart( ' ', '\t' ).Length < 3 )
            {
                return;
            }

            List<PythonCompletionData> data = _completionData.Where( m => m.Name.StartsWith( trimmed, StringComparison.InvariantCultureIgnoreCase ) )
                .Distinct( new SameNameComparer() ).ToList();

            if ( data.Count <= 0 )
            {
                _completionWindow?.Close();
                return;
            }

            foreach ( PythonCompletionData item in data.Where( item => item.Content == null ) )
            {
                item.Content = new CompletionEntry( item.Name, item.Example, CodeTextEditor.SyntaxHighlighting );
            }

            Style windowStyle = (Style) FindResource( "CompletionWindowStyle" );

            _completionWindow = new CompletionWindow( CodeTextEditor.TextArea )
            {
                CloseWhenCaretAtBeginning = true,
                CloseAutomatically = false,
                Width = 500,
                SizeToContent = SizeToContent.WidthAndHeight,
                Style = windowStyle
            };
            _completionWindow.CompletionList.CompletionData.AddRange( data );
            _completionWindow.Show();
            _completionWindow.Closed += delegate { _completionWindow = null; };
        }

        private void DraggableTreeView_OnPreviewMouseWheel( object sender, MouseWheelEventArgs e )
        {
            /*
             * Cheap hack for our broken template, no scrollbars, bubble event to parent scrollviewer
             */
            if ( !( sender is Control control ) || e.Handled )
            {
                return;
            }

            if ( control.Parent == null )
            {
                return;
            }

            e.Handled = true;
            MouseWheelEventArgs eventArg = new MouseWheelEventArgs( e.MouseDevice, e.Timestamp, e.Delta ) { RoutedEvent = MouseWheelEvent, Source = control };
            UIElement parent = control.Parent as UIElement;
            parent?.RaiseEvent( eventArg );
        }

        internal class SameNameComparer : IEqualityComparer<PythonCompletionData>
        {
            public bool Equals( PythonCompletionData x, PythonCompletionData y )
            {
                return y != null && x != null && x.Name.Equals( y.Name );
            }

            public int GetHashCode( PythonCompletionData obj )
            {
                return obj.Name.GetHashCode();
            }
        }
    }
}