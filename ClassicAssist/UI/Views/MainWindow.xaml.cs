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

            // TODO FIX THIS
            switch ( Data.Options.CurrentOptions.DefaultTabOption )
            {
                case Data.DefaultTabOption.General:
                    MainTabs.SelectedIndex = 0;
                    break;
                case Data.DefaultTabOption.Options:
                    MainTabs.SelectedIndex = 1;
                    break;
                case Data.DefaultTabOption.Hotkeys:
                    MainTabs.SelectedIndex = 2;
                    break;
                case Data.DefaultTabOption.Macros:
                    MainTabs.SelectedIndex = 3;
                    break;
                case Data.DefaultTabOption.Skills:
                    MainTabs.SelectedIndex = 4;
                    break;
                case Data.DefaultTabOption.Agents:
                    MainTabs.SelectedIndex = 5;
                    break;
                case Data.DefaultTabOption.PublicMacros:
                    MainTabs.SelectedIndex = 6;
                    break;
                case Data.DefaultTabOption.About:
                    MainTabs.SelectedIndex = 7;
                    break;
            }
        }
    }
}