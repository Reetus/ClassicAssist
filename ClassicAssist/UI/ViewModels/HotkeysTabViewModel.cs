using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using ClassicAssist.Data;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Data.Hotkeys.Commands;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
using ClassicAssist.UI.Misc;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.UI.ViewModels
{
    public class HotkeysTabViewModel : BaseViewModel, ISettingProvider
    {
        private readonly HotkeyManager _hotkeyManager;
        private ICommand _clearHotkeyCommand;
        private HotkeyEntry _commandsCategory;
        private ICommand _executeCommand;
        private HotkeySettable _selectedItem;

        public HotkeysTabViewModel()
        {
            _hotkeyManager = HotkeyManager.GetInstance();
        }

        public ICommand ClearHotkeyCommand =>
            _clearHotkeyCommand ?? ( _clearHotkeyCommand = new RelayCommand( ClearHotkey, o => SelectedItem != null ) );

        public ICommand ExecuteCommand =>
            _executeCommand ?? ( _executeCommand = new RelayCommand( ExecuteHotkey, o => SelectedItem != null ) );

        public ObservableCollectionEx<HotkeyEntry> Items
        {
            get => _hotkeyManager.Items;
            set => _hotkeyManager.Items = value;
        }

        public HotkeySettable SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        public void Serialize( JObject json )
        {
            JObject hotkeys = new JObject();

            JArray commandsArray = new JArray();

            foreach ( HotkeySettable commandsCategoryChild in _commandsCategory.Children )
            {
                JObject entry = new JObject
                {
                    { "Type", commandsCategoryChild.GetType().FullName },
                    { "Keys", commandsCategoryChild.Hotkey.ToJObject() }
                };

                commandsArray.Add( entry );
            }

            hotkeys.Add( "Commands", commandsArray );

            json.Add( "Hotkeys", hotkeys );
        }

        public void Deserialize( JObject json, Options options )
        {
            IEnumerable<Type> hotkeyCommands = Assembly.GetExecutingAssembly().GetTypes()
                .Where( i => i.IsSubclassOf( typeof( HotkeyCommand ) ) );

            _commandsCategory = new HotkeyEntry { IsCategory = true, Name = Strings.Commands };

            ObservableCollectionEx<HotkeySettable> children = new ObservableCollectionEx<HotkeySettable>();

            foreach ( Type hotkeyCommand in hotkeyCommands )
            {
                HotkeyCommand hkc = (HotkeyCommand) Activator.CreateInstance( hotkeyCommand );

                children.Add( hkc );
            }

            _commandsCategory.Children = children;

            _hotkeyManager.Items.AddSorted( _commandsCategory );

            JToken hotkeys = json["Hotkeys"];

            if ( hotkeys?["Commands"] == null )
            {
                return;
            }

            foreach ( JToken token in hotkeys["Commands"] )
            {
                JToken type = token["Type"];

                HotkeySettable entry =
                    _commandsCategory.Children.FirstOrDefault( o => o.GetType().FullName == type.ToObject<string>() );

                if ( entry == null )
                {
                    continue;
                }

                JToken keys = token["Keys"];

                entry.Hotkey = new ShortcutKeys( keys["Modifier"].ToObject<Key>(), keys["Keys"].ToObject<Key>() );
            }
        }

        private static void ClearHotkey( object obj )
        {
            if ( obj is HotkeySettable cmd )
            {
                cmd.Hotkey = ShortcutKeys.Default;
            }
        }

        private static void ExecuteHotkey( object obj )
        {
            if ( obj is HotkeySettable cmd )
            {
                cmd.Action( cmd );
            }
        }
    }
}