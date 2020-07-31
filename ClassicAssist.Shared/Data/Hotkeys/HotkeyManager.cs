﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ClassicAssist.Annotations;
using ClassicAssist.Data.Hotkeys.Commands;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Misc;
using ClassicAssist.UI.Misc;
using static ClassicAssist.Misc.SDLKeys;

namespace ClassicAssist.Data.Hotkeys
{
    public class KeyState
    {
        [DllImport("SDL2.dll", EntryPoint = "SDL_GetKeyboardState", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern byte* GetKeyboardState(out int numKeys);

        public static unsafe bool GetKeyState(SDL_Scancode scanCode)
        {
            var result = GetKeyboardState(out int numKeys);
            int code = (int) scanCode;
            return code < numKeys && result[(int)code] == 1;
        }
    }
    public class HotkeyManager : INotifyPropertyChanged
    {
        private static HotkeyManager _instance;
        private static readonly object _instanceLock = new object();
        private readonly object _lock = new object();

        private readonly Key[] _modifierKeys =
        {
            Key.LeftCtrl, Key.RightCtrl, Key.LeftShift, Key.RightShift, Key.LeftAlt, Key.RightAlt
        };

        private readonly List<Key> _modifiers = new List<Key>();
        private bool _enabled = true;

        private ObservableCollectionEx<HotkeyCommand> _items = new ObservableCollectionEx<HotkeyCommand>();

        private HotkeyManager()
        {
        }

        public Action ClearAllHotkeys { get; set; }

        public bool Enabled
        {
            get => _enabled;
            set => SetProperty( ref _enabled, value );
        }

        public ObservableCollectionEx<HotkeyCommand> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
            lock ( _lock )
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

                bool down = KeyState.GetKeyState(KeysToSDLScancode(modifier));

                if ( !down )
                {
                    modifier = Key.None;
                }

                // Sanity check
                if ( keys == Key.None )
                {
                    return false;
                }

                foreach ( HotkeyCommand hke in Items )
                {
                    if ( hke.Children == null )
                    {
                        continue;
                    }

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

                            AliasCommands.SetDefaultAliases();

                            Task.Run( () => hks.Action.Invoke( hks ) );

                            break;
                        }
                    }
                    catch ( InvalidOperationException )
                    {
                        // When spamming keys
                    }
                }

                return filter;
            }
        }

        private SDL_Scancode KeysToSDLScancode(Key modifier )
        {
            //TODO Enter keys / find better way
            switch ( modifier )
            {
                case Key.LeftAlt:
                    return SDL_Scancode.SDL_SCANCODE_LALT;
                case Key.Right:
                    return SDL_Scancode.SDL_SCANCODE_RIGHT;
                case Key.LeftCtrl:
                    return SDL_Scancode.SDL_SCANCODE_LCTRL;
                case Key.RightCtrl:
                    return SDL_Scancode.SDL_SCANCODE_RCTRL;
                case Key.LeftShift:
                    return SDL_Scancode.SDL_SCANCODE_LSHIFT;
                case Key.RightShift:
                    return SDL_Scancode.SDL_SCANCODE_RSHIFT;
            }

            return 0;
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
                foreach ( HotkeyCommand hke in Items )
                {
                    if ( hke.Children == null )
                    {
                        continue;
                    }

                    try
                    {
                        Key modifier = Key.None;
                        //Key modifier = _modifierKeys.FirstOrDefault( key =>
                        //    Engine.Dispatcher.Invoke( () => Keyboard.IsKeyDown( key ) ) );

                        IEnumerable<HotkeyEntry> hotkeyEntries = hke.Children.Where( t =>
                            t.Hotkey.Modifier == modifier && t.Hotkey.Key == Key.None && t.Hotkey.Mouse == mouse );

                        foreach ( HotkeyEntry hks in hotkeyEntries )
                        {
                            if ( hks.Disableable && !Enabled )
                            {
                                continue;
                            }

                            AliasCommands.SetDefaultAliases();

                            Task.Run( () => hks.Action.Invoke( hks ) );

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