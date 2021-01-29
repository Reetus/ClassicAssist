using System.Windows;
using System.Windows.Input;
using Assistant;

namespace ClassicAssist.UI.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Engine.KeyboardLayoutId = InputLanguageManager.Current.CurrentInputLanguage.KeyboardLayoutId;
        }
    }
}