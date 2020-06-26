using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace ClassicAssist.Browser
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MacroBrowserControl : UserControl
    {
        public MacroBrowserControl()
        {
            InitializeComponent();

            CodeTextEditor.SyntaxHighlighting = HighlightingLoader.Load(
                new XmlTextReader( Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ),
                    "Python.Dark.xshd" ) ), HighlightingManager.Instance );
        }

        private void Hyperlink_RequestNavigate( object sender, RequestNavigateEventArgs e )
        {
            Process.Start( e.Uri.ToString() );
        }
    }
}