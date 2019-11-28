using System;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Resources;
using ClassicAssist.UI.Views;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UI.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private string _status = Strings.Ready___;
        private bool _alwaysOnTop;
        private string _title = Strings.ProductName;
        private DebugWindow _debugWindow;
        private ICommand _debugCommand;

        [OptionsBinding(Property = "AlwaysOnTop")]
        public bool AlwaysOnTop
        {
            get => _alwaysOnTop;
            set => SetProperty(ref _alwaysOnTop, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public MainWindowViewModel()
        {
            Engine.PlayerInitializedEvent += PlayerInitialized;
        }

        private void PlayerInitialized( PlayerMobile player )
        {
            Title = string.IsNullOrEmpty( player.Name ) ? Strings.ProductName : $"{Strings.ProductName} - {player.Name}";
        }

        public string Title 
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public ICommand DebugCommand => _debugCommand ?? ( _debugCommand = new RelayCommand( ShowDebugWindow, o => true ) );

        private void ShowDebugWindow( object obj )
        {
            _debugWindow = new DebugWindow();
            _debugWindow.Show();
        }
    }

    public class OptionsBindingAttribute : Attribute
    {
        public string Property { get; set; }
    }
}