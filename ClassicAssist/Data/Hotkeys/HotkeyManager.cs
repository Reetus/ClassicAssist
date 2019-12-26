using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Annotations;
using ClassicAssist.UI.Misc;

namespace ClassicAssist.Data.Hotkeys
{
    public class HotkeyManager : INotifyPropertyChanged
    {
        private static HotkeyManager _instance;
        private static readonly object _instanceLock = new object();

        private readonly Key[] _modifierKeys =
        {
            Key.LeftCtrl, Key.RightCtrl, Key.LeftShift, Key.RightShift, Key.LeftAlt, Key.RightAlt
        };

        private readonly List<Key> _modifiers = new List<Key>();
        private bool _enabled = true;

        private ObservableCollectionEx<HotkeyEntry> _items = new ObservableCollectionEx<HotkeyEntry>();

        private HotkeyManager()
        {
        }

        public bool Enabled
        {
            get => _enabled;
            set => SetProperty( ref _enabled, value );
        }

        public ObservableCollectionEx<HotkeyEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void ClearPreviousHotkey( ShortcutKeys keys )
        {
            foreach ( HotkeyEntry hotkeyEntry in Items )
            {
                if ( hotkeyEntry.Children == null )
                {
                    continue;
                }

                foreach ( HotkeySettable hotkeyEntryChild in hotkeyEntry.Children )
                {
                    if ( Equals( hotkeyEntryChild.Hotkey, keys ) )
                    {
                        hotkeyEntryChild.Hotkey = ShortcutKeys.Default;
                    }
                }
            }
        }

        public static HotkeyManager GetInstance()
        {
            // ReSharper disable once InvertIf
            if ( _instance == null )
            {
                lock ( _instanceLock )
                {
                    if ( _instance == null )
                    {
                        _instance = new HotkeyManager();
                    }
                }
            }

            return _instance;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        // ReSharper disable once RedundantAssignment
        public void SetProperty<T>( ref T obj, T value, [CallerMemberName] string propertyName = "" )
        {
            obj = value;
            OnPropertyChanged( propertyName );
        }

        public bool OnHotkeyPressed( Key keys )
        {
            bool filter = false;

            if ( _modifierKeys.Contains( keys ) )
            {
                _modifiers.Clear();
                _modifiers.Add( keys );
                return false;
            }

            Key modifier = Key.None;

            if ( _modifiers.Count > 0 )
            {
                modifier = _modifiers[0];
            }

            bool down = modifier != Key.None
                ? Engine.Dispatcher.Invoke( () => Keyboard.IsKeyDown( modifier ) )
                : false;

            if ( !down )
            {
                modifier = Key.None;
            }

            foreach ( HotkeyEntry hke in Items )
            {
                if ( hke.Children == null )
                {
                    continue;
                }

                foreach ( HotkeySettable hks in hke.Children )
                {
                    if ( hks.Hotkey.Key != keys || hks.Hotkey.Modifier != modifier )
                    {
                        continue;
                    }

                    if ( hks.Disableable && !Enabled )
                    {
                        continue;
                    }

                    filter = !hks.PassToUO;

                    Task.Run( () =>
                        hks.Action.Invoke( hks ) );

                    break;
                }
            }

            return filter;
        }

        public void OnMouseAction( MouseOptions mouse )
        {
            foreach ( HotkeyEntry hke in Items )
            {
                if ( hke.Children == null )
                {
                    continue;
                }

                foreach ( HotkeySettable hks in hke.Children )
                {
                    if ( hks.Hotkey.Mouse != mouse )
                    {
                        continue;
                    }

                    if ( hks.Disableable && !Enabled )
                    {
                        continue;
                    }

                    Key modifier = _modifierKeys.FirstOrDefault( key =>
                        Engine.Dispatcher.Invoke( () => Keyboard.IsKeyDown( key ) ) );

                    if ( hks.Hotkey.Modifier != modifier )
                    {
                        continue;
                    }

                    Task.Run( () =>
                        hks.Action.Invoke( hks ) );

                    break;
                }
            }
        }
    }
}