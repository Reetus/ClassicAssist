using System.Windows;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data;

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

            // TODO FIX THIS
            switch ( Data.Options.CurrentOptions.DefaultTabOption )
            {
                case DefaultTabOption.General:
                    MainTabs.SelectedIndex = 0;
                    break;
                case DefaultTabOption.Options:
                    MainTabs.SelectedIndex = 1;
                    break;
                case DefaultTabOption.Hotkeys:
                    MainTabs.SelectedIndex = 2;
                    break;
                case DefaultTabOption.Macros:
                    MainTabs.SelectedIndex = 3;
                    break;
                case DefaultTabOption.Skills:
                    MainTabs.SelectedIndex = 4;
                    break;
                case DefaultTabOption.Agents:
                    MainTabs.SelectedIndex = 5;
                    break;
                case DefaultTabOption.PublicMacros:
                    MainTabs.SelectedIndex = 6;
                    break;
                case DefaultTabOption.About:
                    MainTabs.SelectedIndex = 7;
                    break;
            }
        }
    }
}