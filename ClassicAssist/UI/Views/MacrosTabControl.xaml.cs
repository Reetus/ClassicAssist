using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace ClassicAssist.UI.Views
{
    /// <summary>
    /// Interaction logic for MacrosTabControl.xaml
    /// </summary>
    public partial class MacrosTabControl : UserControl
    {
        public MacrosTabControl()
        {
            InitializeComponent();
        }

        private void Grid_Initialized(object sender, System.EventArgs e)
        {
            CodeTextEditor.SyntaxHighlighting = HighlightingLoader.Load(new XmlTextReader(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Python.xshd")), HighlightingManager.Instance);
        }
    }
}
