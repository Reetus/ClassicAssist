using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using ClassicAssist.Misc;
using ClassicAssist.Shared;

namespace ClassicAssist.UI.ViewModels
{
    public class DialogWindowViewModel : BaseViewModel
    {
        private readonly SharedWindow _window;
        private ICommand _buttonCommand;

        public DialogWindowViewModel( SharedWindow window )
        {
            _window = window;
        }

        public ICommand ButtonClick =>
            _buttonCommand ?? ( _buttonCommand = new RelayCommand( OnButtonClick, o => true ) );

        public string Caption => _window.Caption;

        // Maybe make this dynamic, since SizeToContext=Height is having problems...
        public int Height => 200;

        public bool Icon => _window.Icon != null;

        public string ImagePath =>
            _window.Icon == MessageBoxImage.Error ? "dialog-error.png" :
            _window.Icon == MessageBoxImage.Warning ? "dialog-warn.png" :
            _window.Icon == MessageBoxImage.Warning ? "dialog-info.png" : null;

        public bool IsNoShowed => _window.Buttons == MessageBoxButtons.YesNo;
        public bool IsOkShowed => _window.Buttons == MessageBoxButtons.OK;
        public bool IsYesShowed => _window.Buttons == MessageBoxButtons.YesNo;
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.No;
        public string Text => _window.Text;
        public int TextWidth => Icon ? 250 : 330;

        private void OnButtonClick( object obj )
        {
            Result = (MessageBoxResult) obj;
            ((Window)_window.Window).Close();
        }

        public async Task Copy()
        {
            await AvaloniaLocator.Current.GetService<IClipboard>().SetTextAsync(_window.Text);
        }
    }
}