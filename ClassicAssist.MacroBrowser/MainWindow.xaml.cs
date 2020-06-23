using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using ClassicAssist.Resources;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace ClassicAssist.MacroBrowser
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += ( sender, args ) =>
            {
                MessageBox.Show( ( (Exception) args.ExceptionObject ).ToString(), Strings.Error );
            };

            TaskScheduler.UnobservedTaskException += ( sender, args ) =>
            {
                MessageBox.Show( args.Exception.ToString(), Strings.Error );
            };

            CodeTextEditor.SyntaxHighlighting = HighlightingLoader.Load(
                new XmlTextReader( Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ),
                    "Python.Dark.xshd" ) ), HighlightingManager.Instance );
        }
    }
}