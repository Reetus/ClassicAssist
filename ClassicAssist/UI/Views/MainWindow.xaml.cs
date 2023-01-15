using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Assistant;


namespace ClassicAssist.UI.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Engine.KeyboardLayoutId = InputLanguageManager.Current.CurrentInputLanguage.KeyboardLayoutId;
            MaxHeight = SystemParameters.VirtualScreenHeight - 35;
        }
    }
}