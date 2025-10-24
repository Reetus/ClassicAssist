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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassicAssist.Data.Macros;
using ClassicAssist.UI.Controls;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using IronPython.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Xaml.Behaviors;
using static ClassicAssist.UI.Views.MacrosTabControl;

namespace ClassicAssist.UI.Misc.Behaviours
{
    public class AvalonEditShowCompletionTooltipBehaviour : Behavior<TextEditor>
    {
        public static readonly DependencyProperty FrameVariablesProperty = DependencyProperty.Register( nameof( FrameVariables ), typeof( Dictionary<string, object> ),
            typeof( AvalonEditShowCompletionTooltipBehaviour ), new PropertyMetadata( default( Dictionary<string, object> ) ) );

        public static readonly DependencyProperty IsPausedProperty =
            DependencyProperty.Register( nameof( IsPaused ), typeof( bool ), typeof( AvalonEditShowCompletionTooltipBehaviour ), new PropertyMetadata( false ) );

        private readonly ToolTip _toolTip = new ToolTip();
        private List<PythonCompletionData> _completionData;
        private CompletionWindow _completionWindow;
        private TextEditor _textEditor;

        public Dictionary<string, object> FrameVariables
        {
            get => (Dictionary<string, object>) GetValue( FrameVariablesProperty );
            set => SetValue( FrameVariablesProperty, value );
        }

        public bool IsPaused
        {
            get => (bool) GetValue( IsPausedProperty );
            set => SetValue( IsPausedProperty, value );
        }

        private static bool IsWordChar( char c )
        {
            return char.IsLetterOrDigit( c ) || c == '_';
        }

        protected override void OnAttached()
        {
            _textEditor = AssociatedObject;

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

            _textEditor.TextArea.TextEntered += OnTextEntered;
            _textEditor.MouseHover += OnMouseHover;
            _textEditor.MouseHoverStopped += OnMouseHoverStopped;
        }

        private void OnMouseHoverStopped( object sender, MouseEventArgs e )
        {
            _toolTip.IsOpen = false;
        }

        private void OnMouseHover( object sender, MouseEventArgs e )
        {
            //https://stackoverflow.com/questions/51711692/how-to-fire-an-event-when-mouse-hover-over-a-specific-text-in-avalonedit
            TextViewPosition? pos = _textEditor.GetPositionFromPoint( e.GetPosition( _textEditor ) );

            if ( !pos.HasValue )
            {
                return;
            }

            DocumentLine line = _textEditor.TextArea.Document.GetLineByNumber( pos.Value.Line );

            if ( line == null )
            {
                return;
            }

            try
            {
                string fullLine = _textEditor.Document.GetText( line.Offset, line.Length );

                int startPosition = 0;
                int endPosition = fullLine.Length;

                for ( int i = pos.Value.Column; i > -1; i-- )
                {
                    if ( IsWordChar( fullLine[i] ) )
                    {
                        continue;
                    }

                    startPosition = i + 1;
                    break;
                }

                for ( int i = pos.Value.Column; i < fullLine.Length; i++ )
                {
                    if ( IsWordChar( fullLine[i] ) )
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

                IEnumerable<PythonCompletionData> matches = _completionData.Where( i => i.MethodName.Equals( word ) ).ToArray();

                if ( matches.Any() )
                {
                    StackPanel panel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness( 5 ) };

                    foreach ( PythonCompletionData match in matches )
                    {
                        TextBlock descriptionText = new TextBlock { Text = match.Description.ToString(), TextWrapping = TextWrapping.Wrap, Margin = new Thickness( 0, 2, 0, 5 ) };
                        panel.Children.Add( descriptionText );
                        TextBlock nameText = new TextBlock { Text = match.Name, FontWeight = FontWeights.Bold, Margin = new Thickness( 0, 5, 0, 0 ) };
                        panel.Children.Add( nameText );
                    }

                    _toolTip.PlacementTarget = _textEditor;
                    _toolTip.Content = panel;
                    _toolTip.IsOpen = true;
                }
                else if ( IsPaused )
                {
                    KeyValuePair<string, object>[] frameVariables = FrameVariables.Where( kvp => kvp.Key.Equals( word ) ).ToArray();

                    if ( frameVariables.Any() )
                    {
                        StackPanel panel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness( 5 ) };

                        foreach ( KeyValuePair<string, object> variable in frameVariables )
                        {
                            TextBlock nameText = new TextBlock
                            {
                                Text = $"{variable.Key} : {variable.Value?.GetType().Name ?? "null"}", FontWeight = FontWeights.Bold, Margin = new Thickness( 0, 5, 0, 0 )
                            };
                            panel.Children.Add( nameText );
                            TextBlock valueText = new TextBlock
                            {
                                Text = MacroInvoker.GetDisplayValue( variable.Value ), TextWrapping = TextWrapping.Wrap, Margin = new Thickness( 0, 2, 0, 5 )
                            };
                            panel.Children.Add( valueText );
                        }

                        _toolTip.PlacementTarget = _textEditor;
                        _toolTip.Content = panel;
                        _toolTip.IsOpen = true;
                    }
                }

                e.Handled = true;
            }
            catch ( Exception )
            {
                // ignored
            }
        }

        private void OnTextEntered( object sender, TextCompositionEventArgs e )
        {
            DocumentLine line = _textEditor.TextArea.Document.Lines[_textEditor.TextArea.Caret.Line - 1];

            string trimmed = _textEditor.TextArea.Document.GetText( line ).TrimStart( ' ', '\t' );

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
                item.Content = new CompletionEntry( item.Name, item.Example, _textEditor.SyntaxHighlighting );
            }

            _completionWindow = new CompletionWindow( _textEditor.TextArea )
            {
                CloseWhenCaretAtBeginning = true, CloseAutomatically = false, Width = 500, SizeToContent = SizeToContent.WidthAndHeight
            };
            _completionWindow.CompletionList.CompletionData.AddRange( data );
            _completionWindow.Show();
            _completionWindow.Closed += delegate { _completionWindow = null; };
        }
    }
}