using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using ClassicAssist.Data;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Data.Hotkeys.Commands;
using ClassicAssist.Data.Spells;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
using ClassicAssist.UI.Misc;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.UI.ViewModels
{
    public class HotkeysTabViewModel : BaseViewModel, ISettingProvider
    {
        private readonly HotkeyManager _hotkeyManager;
        private readonly List<HotkeyEntry> _serializeCategories = new List<HotkeyEntry>();
        private ICommand _clearHotkeyCommand;
        private HotkeyEntry _commandsCategory;
        private ICommand _executeCommand;
        private HotkeySettable _selectedItem;
        private HotkeyEntry _spellsCategory;

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

            foreach ( HotkeyEntry category in _serializeCategories )
            {
                foreach ( HotkeySettable categoryChild in category.Children )
                {
                    if ( Equals( categoryChild.Hotkey, ShortcutKeys.Default ) )
                    {
                        continue;
                    }

                    JObject entry = new JObject
                    {
                        { "Type", categoryChild.GetType().FullName },
                        { "Keys", categoryChild.Hotkey.ToJObject() },
                        { "PassToUO", categoryChild.PassToUO }
                    };

                    commandsArray.Add( entry );
                }
            }

            hotkeys.Add( "Commands", commandsArray );

            JArray spellsArray = new JArray();

            foreach ( HotkeySettable spellsCategoryChild in _spellsCategory.Children )
            {
                if ( Equals( spellsCategoryChild.Hotkey, ShortcutKeys.Default ) )
                {
                    continue;
                }

                JObject entry = new JObject
                {
                    { "Name", spellsCategoryChild.Name },
                    { "Keys", spellsCategoryChild.Hotkey.ToJObject() },
                    { "PassToUO", spellsCategoryChild.PassToUO }
                };

                spellsArray.Add( entry );
            }

            hotkeys.Add( "Spells", spellsArray );

            json?.Add( "Hotkeys", hotkeys );
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

                HotkeyCommandAttribute attr = hkc.GetType().GetCustomAttribute<HotkeyCommandAttribute>();

                if ( attr == null || string.IsNullOrEmpty( attr.Category ) )
                {
                    children.Add( hkc );
                }
                else
                {
                    string categoryName = Strings.ResourceManager.GetString( attr.Category );

                    HotkeyEntry category = Items.FirstOrDefault( hke => hke.Name == categoryName && hke.IsCategory );

                    if ( category != null )
                    {
                        if ( category.Children == null )
                        {
                            category.Children = new ObservableCollectionEx<HotkeySettable>();
                        }

                        category.Children.Add( hkc );
                    }
                    else
                    {
                        category = new HotkeyEntry
                        {
                            Name = categoryName,
                            IsCategory = true,
                            Children = new ObservableCollectionEx<HotkeySettable>()
                        };

                        category.Children.Add( hkc );
                        Items.AddSorted( category );
                        _serializeCategories.Add( category );
                    }
                }
            }

            _commandsCategory.Children = children;

            _hotkeyManager.Items.AddSorted( _commandsCategory );
            _serializeCategories.Add( _commandsCategory );

            JToken hotkeys = json?["Hotkeys"];

            if ( hotkeys?["Commands"] != null )
            {
                foreach ( JToken token in hotkeys["Commands"] )
                {
                    JToken type = token["Type"];

                    foreach ( HotkeyEntry category in _serializeCategories )
                    {
                        HotkeySettable entry =
                            category.Children.FirstOrDefault(
                                o => o.GetType().FullName == type.ToObject<string>() );

                        if ( entry == null )
                        {
                            continue;
                        }

                        JToken keys = token["Keys"];

                        entry.Hotkey = new ShortcutKeys( keys["Modifier"].ToObject<Key>(),
                            keys["Keys"].ToObject<Key>() );
                        entry.PassToUO = token["PassToUO"]?.ToObject<bool>() ?? true;
                    }
                }
            }

            _spellsCategory = new HotkeyEntry { IsCategory = true, Name = Strings.Spells };

            SpellManager spellManager = SpellManager.GetInstance();

            SpellData[] spells = spellManager.GetSpellData();

            children = new ObservableCollectionEx<HotkeySettable>();

            foreach ( SpellData spell in spells )
            {
                HotkeyCommand hkc = new HotkeyCommand
                {
                    Name = spell.Name,
                    Action = hks => spellManager.CastSpell( spell.ID ),
                    Hotkey = ShortcutKeys.Default,
                    PassToUO = true
                };

                children.Add( hkc );
            }

            _spellsCategory.Children = children;

            _hotkeyManager.Items.AddSorted( _spellsCategory );

            JToken spellsObj = hotkeys?["Spells"];

            if ( spellsObj != null )
            {
                foreach ( JToken token in spellsObj )
                {
                    JToken name = token["Name"];

                    HotkeySettable entry =
                        _spellsCategory.Children.FirstOrDefault( s => s.Name == name.ToObject<string>() );

                    if ( entry == null )
                    {
                        continue;
                    }

                    JToken keys = token["Keys"];

                    entry.Hotkey = new ShortcutKeys( keys["Modifier"].ToObject<Key>(), keys["Keys"].ToObject<Key>() );
                    entry.PassToUO = token["PassToUO"]?.ToObject<bool>() ?? true;
                }
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