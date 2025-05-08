using System;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Assistant;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.Shared.UI;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Path = System.IO.Path;

namespace ClassicAssist.UI.ViewModels.Agents.Autoloot.Python
{
    /// <summary>
    ///     Interaction logic for PythonFunctionsWindow.xaml
    /// </summary>
    public partial class PythonFunctionsWindow : Window
    {
        public PythonFunctionsWindow()
        {
            InitializeComponent();
        }

        private void Grid_Initialized( object sender, EventArgs e )
        {
            CodeTextEditor.SyntaxHighlighting = HighlightingLoader.Load(
                new XmlTextReader( Path.Combine( Engine.StartupPath, "Python.Dark.xshd" ) ), HighlightingManager.Instance );
        }

        private void CodeTextEditor_Initialized( object sender, EventArgs e )
        {
            if ( !( sender is TextEditor editor ) )
            {
                return;
            }

            AutolootManager manager = AutolootManager.GetInstance();

            editor.Document.Text = manager?.GetPythonFunctionText();
        }
    }
}