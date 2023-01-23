using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Views;
using Hardcodet.Wpf.TaskbarNotification;

namespace ClassicAssist.UI.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private ICommand _debugCommand;
        private DebugWindow _debugWindow;
        private ICommand _minimizeCommand;
        private ICommand _restoreWindowCommand;
        private string _status = Strings.Ready___;
        private TaskbarIcon _taskbarIcon;
        private string _title = Strings.ProductName;

        public MainWindowViewModel()
        {
            Engine.Dispatcher = Dispatcher.CurrentDispatcher;
            Engine.UpdateWindowTitleEvent += OnUpdateWindowTitleEvent;
            Engine.ClientClosing += OnClientClosing;
        }

        public ICommand DebugCommand =>
            _debugCommand ?? ( _debugCommand = new RelayCommand( ShowDebugWindow, o => true ) );

        public ICommand MinimizeCommand =>
            _minimizeCommand ?? ( _minimizeCommand = new RelayCommand( Minimize, o => true ) );

        public ICommand RestoreWindowCommand =>
            _restoreWindowCommand ?? ( _restoreWindowCommand = new RelayCommand( RestoreWindow, o => true ) );

        public string Status
        {
            get => _status;
            set => SetProperty( ref _status, value );
        }

        public string Title
        {
            get => _title;
            set => SetProperty( ref _title, value );
        }

        private void OnClientClosing()
        {
            if ( _taskbarIcon != null )
            {
                _dispatcher.Invoke( () => { _taskbarIcon.Visibility = Visibility.Hidden; } );
            }
        }

        private void OnUpdateWindowTitleEvent()
        {
            Title = string.IsNullOrEmpty( Engine.Player?.Name )
                ? Strings.ProductName
                : $"{Engine.Player?.Name} - {( Options.CurrentOptions.ShowProfileNameWindowTitle ? $"({Options.CurrentOptions.Name}) - " : "" )}{Strings.ProductName}";

            if ( _taskbarIcon != null )
            {
                _dispatcher.Invoke( () => { _taskbarIcon.ToolTipText = Title; } );
            }
        }

        private void ShowDebugWindow( object obj )
        {
            _debugWindow = new DebugWindow();
            _debugWindow.Show();
        }

        private void Minimize( object obj )
        {
            if ( !( obj is Window window ) )
            {
                return;
            }

            if ( !Options.CurrentOptions.SysTray )
            {
                window.WindowState = WindowState.Minimized;
                return;
            }

            if ( _taskbarIcon == null )
            {
                _taskbarIcon = new TaskbarIcon
                {
                    Icon = Properties.Resources.cog,
                    Visibility = Visibility.Visible,
                    LeftClickCommand = RestoreWindowCommand,
                    LeftClickCommandParameter = window,
                    NoLeftClickDelay = true
                };
            }

            _taskbarIcon.ToolTipText = Title;
            _taskbarIcon.Visibility = Visibility.Visible;
            window.ShowInTaskbar = false;
            window.Hide();
        }

        private void RestoreWindow( object obj )
        {
            if ( !( obj is Window window ) )
            {
                return;
            }

            window.ShowInTaskbar = true;
            window.WindowState = WindowState.Normal;
            window.Show();
            _taskbarIcon.Visibility = Visibility.Hidden;
        }
    }
}