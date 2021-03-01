#region License

// Copyright (C) 2021 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Data.Hotkeys.Commands;
using ClassicAssist.Misc;
using ClassicAssist.UI.Views.Debug;
using ClassicAssist.UO;
using Newtonsoft.Json;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace ClassicAssist.UI.ViewModels.Debug
{
    public class DebugKeyboardViewModel : BaseViewModel
    {
        private readonly DebugKeyboardControl _control;
        private ObservableCollection<FailKey> _failKeys = new ObservableCollection<FailKey>();
        private int _keyboardLayoutId;
        private string _keyboardName;
        private ICommand _removeItemCommand;
        private ICommand _saveCommand;
        private int _sdlKey;
        private int _sdlMod;
        private FailKey _selectedItem;
        private string _status;
        private ICommand _testKeyCommand;
        private Key _uoKey;
        private Key _wpfKey;

        public DebugKeyboardViewModel()
        {
            KeyboardName = InputLanguageManager.Current.CurrentInputLanguage.DisplayName;
            KeyboardLayoutId = InputLanguageManager.Current.CurrentInputLanguage.KeyboardLayoutId;
        }

        public DebugKeyboardViewModel( DebugKeyboardControl control )
        {
            KeyboardName = InputLanguageManager.Current.CurrentInputLanguage.DisplayName;
            KeyboardLayoutId = InputLanguageManager.Current.CurrentInputLanguage.KeyboardLayoutId;
            _control = control;
        }

        public ObservableCollection<FailKey> FailKeys
        {
            get => _failKeys;
            set => SetProperty( ref _failKeys, value );
        }

        public int KeyboardLayoutId
        {
            get => _keyboardLayoutId;
            set => SetProperty( ref _keyboardLayoutId, value );
        }

        public string KeyboardName
        {
            get => _keyboardName;
            set => SetProperty( ref _keyboardName, value );
        }

        public ICommand RemoveItemCommand =>
            _removeItemCommand ?? ( _removeItemCommand = new RelayCommand( RemoveItem, o => true ) );

        public ICommand SaveCommand =>
            _saveCommand ?? ( _saveCommand = new RelayCommandAsync( Save, o => FailKeys.Count > 0 ) );

        public int SDLKey
        {
            get => _sdlKey;
            set => SetProperty( ref _sdlKey, value );
        }

        public int SDLMod
        {
            get => _sdlMod;
            set => SetProperty( ref _sdlMod, value );
        }

        public FailKey SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        public string Status
        {
            get => _status;
            set => SetProperty( ref _status, value );
        }

        public ICommand TestKeyCommand =>
            _testKeyCommand ?? ( _testKeyCommand = new RelayCommandAsync( TestKey, o => Engine.Connected ) );

        public Key UOKey
        {
            get => _uoKey;
            set => SetProperty( ref _uoKey, value );
        }

        public Key WPFKey
        {
            get => _wpfKey;
            set => SetProperty( ref _wpfKey, value );
        }

        private void RemoveItem( object obj )
        {
            if ( !( obj is FailKey failKey ) )
            {
                return;
            }

            FailKeys.Remove( failKey );
        }

        private async Task TestKey( object arg )
        {
            Status = string.Empty;
            NativeMethods.SetForegroundWindow( Engine.WindowHandle );
            Commands.SystemMessage( "Press key to test..." );

            AutoResetEvent are = new AutoResetEvent( false );

            void OnHotkeyPressed( int key, int mod, Key keys, SDLKeys.ModKey modKey )
            {
                Engine.HotkeyPressedEvent -= OnHotkeyPressed;

                UOKey = keys;
                SDLKey = key;
                SDLMod = mod;
                are.Set();
            }

            HotkeyManager manager = HotkeyManager.GetInstance();

            if ( manager.Enabled )
            {
                new ToggleHotkeys().Execute();
            }

            Engine.HotkeyPressedEvent += OnHotkeyPressed;
            bool result = false;

            await Task.Run( () => { result = are.WaitOne( 15000 ); } );

            if ( !manager.Enabled )
            {
                new ToggleHotkeys().Execute();
            }

            if ( !result )
            {
                Status = "No UO keypress detected.";
                return;
            }

            void OnWpfKeyDown( KeyEventArgs args )
            {
                _control.WPFKeyDownEvent -= OnWpfKeyDown;

                Key key = args.Key;

                switch ( key )
                {
                    case Key.DeadCharProcessed:
                        key = args.DeadCharProcessedKey;
                        break;
                    case Key.ImeProcessed:
                        key = args.ImeProcessedKey;
                        break;
                    case Key.System:
                        key = args.SystemKey;
                        break;
                }

                WPFKey = key;
                are.Set();
            }

            Window window = Window.GetWindow( _control );
            window?.Activate();
            _control.Focus();

            if ( _control != null )
            {
                Status = "* Press the same key again *";
                _control.WPFKeyDownEvent += OnWpfKeyDown;
            }

            await Task.Run( () => { result = are.WaitOne( 15000 ); } );

            if ( !result )
            {
                Status = "No WPF keypress detected.";
                return;
            }

            if ( UOKey != WPFKey )
            {
                Status = "Key didn't match";
                FailKeys.Add( new FailKey
                {
                    KeyboardLayoutId = InputLanguageManager.Current.CurrentInputLanguage.KeyboardLayoutId,
                    KeyboardName = InputLanguageManager.Current.CurrentInputLanguage.Name,
                    WPFKey = WPFKey,
                    UOKey = UOKey,
                    SDLKey = SDLKey,
                    SDLMod = SDLMod
                } );
            }
            else
            {
                Status = "Key matched";
            }
        }

        private async Task Save( object arg )
        {
            SaveFileDialog fsd = new SaveFileDialog { OverwritePrompt = true, FileName = "keys.json" };

            DialogResult result = fsd.ShowDialog();

            if ( result == DialogResult.OK )
            {
                string json = JsonConvert.SerializeObject( FailKeys );

                File.WriteAllText( fsd.FileName, json );
            }

            await Task.CompletedTask;
        }
    }

    public class FailKey : SetPropertyNotifyChanged
    {
        private int _keyboardLayoutId;
        private string _keyboardName;
        private int _sdlKey;
        private int _sdlMod;
        private string _uo;
        private Key _uoKey;
        private Key _wpfKey;

        public int KeyboardLayoutId
        {
            get => _keyboardLayoutId;
            set => SetProperty( ref _keyboardLayoutId, value );
        }

        public string KeyboardName
        {
            get => _keyboardName;
            set => SetProperty( ref _keyboardName, value );
        }

        public int SDLKey
        {
            get => _sdlKey;
            set => SetProperty( ref _sdlKey, value );
        }

        public int SDLMod
        {
            get => _sdlMod;
            set => SetProperty( ref _sdlMod, value );
        }

        public Key UOKey
        {
            get => _uoKey;
            set => SetProperty( ref _uoKey, value );
        }

        public string UOKeyString => UOKey.ToString();

        public Key WPFKey
        {
            get => _wpfKey;
            set => SetProperty( ref _wpfKey, value );
        }

        public string WPFKeyString => WPFKey.ToString();
    }
}