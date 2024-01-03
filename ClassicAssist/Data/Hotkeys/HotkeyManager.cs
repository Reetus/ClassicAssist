using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data.Hotkeys.Commands;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Shared.UI;
using static ClassicAssist.Misc.SDLKeys;

namespace ClassicAssist.Data.Hotkeys
{
    public class HotkeyManager : SetPropertyNotifyChanged
    {
        public delegate void dHotkeysStatus( bool enabled );

        private static HotkeyManager _instance;
        private static readonly object _instanceLock = new object();
        private readonly object _lock = new object();

        private readonly Key[] _modifierKeys =
        {
            Key.LeftCtrl, Key.RightCtrl, Key.LeftShift, Key.RightShift, Key.LeftAlt, Key.RightAlt
        };

        private bool _enabled = true;

        private ObservableCollectionEx<HotkeyCommand> _items = new ObservableCollectionEx<HotkeyCommand>();

        private HotkeyManager()
        {
        }

        public Action ClearAllHotkeys { get; set; }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if ( _enabled != value )
                {
                    HotkeysStatusChanged?.Invoke( value );
                }

                SetProperty( ref _enabled, value );
            }
        }

        public Action<string, Type> InvokeByName { get; set; }

        public ObservableCollectionEx<HotkeyCommand> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public static event dHotkeysStatus HotkeysStatusChanged;

        public void AddCategory( HotkeyCommand item, IComparer<HotkeyEntry> comparer = null )
        {
            if ( Items.Contains( item ) )
            {
                Items.Remove( item );
            }

            if ( comparer == null )
            {
                comparer = Comparer<HotkeyEntry>.Default;
            }

            int i = 0;

            while ( i < Items.Count && comparer.Compare( Items[i], item ) < 0 )
            {
                i++;
            }

            Items.Insert( i, item );
        }

        public void ClearPreviousHotkey( ShortcutKeys keys )
        {
            foreach ( HotkeyCommand hotkeyEntry in Items )
            {
                if ( hotkeyEntry.Children == null )
                {
                    continue;
                }

                foreach ( HotkeyEntry hotkeyEntryChild in hotkeyEntry.Children )
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

        public (bool, bool) OnHotkeyPressed( Key keys, ModKey modifier, bool noexecute )
        {
            lock ( _lock )
            {
                bool filter = false;
                bool found = false;

                // Sanity check
                if ( keys == Key.None )
                {
                    return ( false, false );
                }

                foreach ( HotkeyCommand hke in Items.ToList().Where( hke => hke.Children != null ) )
                {
                    try
                    {
                        IEnumerable<HotkeyEntry> hotkeyEntries = hke.Children.Where( t =>
                            t.Hotkey.Modifier == modifier && t.Hotkey.Key == keys &&
                            t.Hotkey.Mouse == MouseOptions.None );

                        foreach ( HotkeyEntry hks in hotkeyEntries )
                        {
                            if ( hks.Disableable && !Enabled )
                            {
                                continue;
                            }

                            filter = !hks.PassToUO;
                            found = true;

                            if ( !noexecute )
                            {
                                AliasCommands.SetDefaultAliases();

                                Task.Run( () => { hks.Action.Invoke( hks, null ); } );
                            }

                            break;
                        }
                    }
                    catch ( InvalidOperationException )
                    {
                        // When spamming keys
                    }
                }

                return ( found, filter );
            }
        }

        public void OnMouseAction( MouseOptions mouse )
        {
            // Sanity check
            if ( mouse == MouseOptions.None )
            {
                return;
            }

            lock ( _lock )
            {
                foreach ( HotkeyCommand hke in Items.ToList().Where( hke => hke.Children != null ) )
                {
                    try
                    {
                        ModKey modifier =
                            KeymodFromKeyList(
                                Engine.Dispatcher.Invoke( () => _modifierKeys.Where( Keyboard.IsKeyDown ) ) );

                        IEnumerable<HotkeyEntry> hotkeyEntries = hke.Children.Where( t =>
                            t.Hotkey.Modifier == modifier && t.Hotkey.Key == Key.None && t.Hotkey.Mouse == mouse );

                        foreach ( HotkeyEntry hks in hotkeyEntries )
                        {
                            if ( hks.Disableable && !Enabled )
                            {
                                continue;
                            }

                            AliasCommands.SetDefaultAliases();

                            Task.Run( () => { hks.Action.Invoke( hks, null ); } );

                            break;
                        }
                    }
                    catch ( InvalidOperationException )
                    {
                        // When spamming wheel
                    }
                }
            }
        }

        public void ClearItems()
        {
            foreach ( HotkeyEntry entry in Items )
            {
                ClearHotkeys( entry );
            }

            Items.Clear();
        }

        private void ClearHotkeys( HotkeyEntry entry )
        {
            entry.Hotkey = ShortcutKeys.Default;

            if ( !entry.IsCategory )
            {
                return;
            }

            foreach ( HotkeyEntry hotkeyEntry in entry.Children )
            {
                ClearHotkeys( hotkeyEntry );
            }
        }
    }
}