using System.Windows;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data;
using Hardcodet.Wpf.TaskbarNotification;

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

            _taskbarIcon = new TaskbarIcon
            {
                Icon = Properties.Resources.cog,
                ToolTipText = Shared.Resources.Strings.ProductName,
                Visibility = Visibility.Visible,
                DoubleClickCommand = TaskbarIconCommand,
                DoubleClickCommandParameter = "DBL"
            };

            if (Options.CurrentOptions.SysTray)
            {
                _taskbarIcon.Visibility = Visibility.Visible;
            }
            else
            {
                this.ShowInTaskbar = false;
            }
        }

        private TaskbarIcon _taskbarIcon;
        private ICommand _taskbarIconCommand;

        public ICommand TaskbarIconCommand =>
            _taskbarIconCommand ?? (_taskbarIconCommand = new Shared.UI.RelayCommand(TaskbarDoubleClick, o => true));

        private void TaskbarDoubleClick(object obj)
        {
            this.Show();
        }
    }
}