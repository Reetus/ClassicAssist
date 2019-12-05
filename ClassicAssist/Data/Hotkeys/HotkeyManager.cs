using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
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

        private ObservableCollectionEx<HotkeyEntry> _items = new ObservableCollectionEx<HotkeyEntry>();

        private HotkeyManager()
        {
        }

        public ObservableCollectionEx<HotkeyEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

            foreach ( HotkeyEntry hke in Items )
            {
                if ( hke.Children == null )
                {
                    break;
                }

                foreach ( HotkeySettable hks in hke.Children )
                {
                    Key modifier = Key.None;

                    if ( _modifiers.Count > 0 )
                    {
                        modifier = _modifiers[0];
                    }

                    if ( hks.Hotkey.Key != keys || hks.Hotkey.Modifier != modifier )
                    {
                        continue;
                    }

                    filter = !hks.PassToUO;

                    Task.Run( () =>
                        hks.Action.Invoke( hks ) );
                    _modifiers?.Clear();
                }
            }

            // Any key that isn't a modifier with clear the modifiers
            _modifiers?.Clear();

            return filter;
        }
    }
}