using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using ClassicAssist.Data;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Data.Hotkeys.Commands;
using ClassicAssist.Data.Spells;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.UI.ViewModels
{
    public class HotkeysTabViewModel : BaseViewModel, IGlobalSettingProvider
    {
        private const string GLOBAL_HOTKEYS_FILENAME = "Hotkeys.json";
        private readonly HotkeyManager _hotkeyManager;
        private readonly List<HotkeyCommand> _serializeCategories = new List<HotkeyCommand>();
        private ICommand _clearHotkeyCommand;
        private ICommand _createMacroButtonCommand;
        private ICommand _executeCommand;
        private ObservableCollection<HotkeyCommand> _filterItems;
        private string _filterText;
        private HotkeyCommand _masteriesCategory;
        private HotkeyEntry _selectedItem;
        private HotkeyCommand _spellsCategory;

        public HotkeysTabViewModel()
        {
            _hotkeyManager = HotkeyManager.GetInstance();
            _hotkeyManager.ClearAllHotkeys = ClearAllHotkeys;

            Items.CollectionChanged += ( s, ea ) => UpdateFilteredItems();
        }

        public ICommand ClearHotkeyCommand =>
            _clearHotkeyCommand ?? ( _clearHotkeyCommand = new RelayCommand( ClearHotkey, o => SelectedItem != null ) );

        public ICommand CreateMacroButtonCommand =>
            _createMacroButtonCommand ?? ( _createMacroButtonCommand =
                new RelayCommand( CreateMacroButton, o => SelectedItem != null ) );

        public ICommand ExecuteCommand =>
            _executeCommand ?? ( _executeCommand = new RelayCommand( ExecuteHotkey, o => SelectedItem != null ) );

        public ObservableCollection<HotkeyCommand> FilterItems
        {
            get => _filterItems;
            set => SetProperty( ref _filterItems, value );
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                SetProperty( ref _filterText, value );
                UpdateFilteredItems();
            }
        }

        public ShortcutKeys Hotkey
        {
            get => SelectedItem?.Hotkey;
            set => CheckOverwriteHotkey( SelectedItem, value );
        }

        public ObservableCollectionEx<HotkeyCommand> Items
        {
            get => _hotkeyManager.Items;
            set => _hotkeyManager.Items = value;
        }

        public bool Searching { get; set; }

        public HotkeyEntry SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty( ref _selectedItem, value );
                OnPropertyChanged( nameof( Hotkey ) );
            }
        }

        public string GetGlobalFilename()
        {
            return GLOBAL_HOTKEYS_FILENAME;
        }

        public void Serialize( JObject json, bool global = false )
        {
            JObject hotkeys = new JObject();

            JArray commandsArray = SerializeCommands( e => e.IsGlobal == global );

            hotkeys.Add( "Commands", commandsArray );

            JArray spellsArray = SerializeSpells( e => e.IsGlobal == global );

            hotkeys.Add( "Spells", spellsArray );

            JArray masteryArray = SerializeMasteries( e => e.IsGlobal == global );

            hotkeys.Add( "Masteries", masteryArray );

            json?.Add( "Hotkeys", hotkeys );
        }

        public void Deserialize( JObject json, Options options, bool global = false )
        {
            if ( !global )
            {
                IEnumerable<Type> hotkeyCommands = Assembly.GetExecutingAssembly().GetTypes()
                    .Where( i => i.IsSubclassOf( typeof( HotkeyCommand ) ) );

                foreach ( Type hotkeyCommand in hotkeyCommands )
                {
                    HotkeyCommand hkc = (HotkeyCommand) Activator.CreateInstance( hotkeyCommand );

                    HotkeyCommandAttribute attr = hkc.GetType().GetCustomAttribute<HotkeyCommandAttribute>();

                    string categoryName = Strings.Commands;

                    if ( attr != null && !string.IsNullOrEmpty( attr.Category ) )
                    {
                        categoryName = Strings.ResourceManager.GetString( attr.Category );
                    }

                    HotkeyCommand category = Items.FirstOrDefault( hke => hke.Name == categoryName && hke.IsCategory );

                    if ( category != null )
                    {
                        if ( category.Children == null )
                        {
                            category.Children = new ObservableCollectionEx<HotkeyEntry>();
                        }

                        if ( category.Children.Contains( hkc ) )
                        {
                            category.Children.Remove( hkc );
                        }

                        category.Children.Add( hkc );
                    }
                    else
                    {
                        category = new HotkeyCommand
                        {
                            Name = categoryName,
                            IsCategory = true,
                            Children = new ObservableCollectionEx<HotkeyEntry> { hkc }
                        };

                        _hotkeyManager.AddCategory( category );
                        _serializeCategories.Add( category );
                    }
                }
            }

            JToken hotkeys = json?["Hotkeys"];

            if ( hotkeys?["Commands"] != null )
            {
                foreach ( JToken token in hotkeys["Commands"] )
                {
                    JToken type = token["Type"];

                    foreach ( HotkeyCommand category in _serializeCategories )
                    {
                        HotkeyEntry entry =
                            category.Children.FirstOrDefault( o => o.GetType().FullName == type.ToObject<string>() );

                        if ( entry == null )
                        {
                            continue;
                        }

                        JToken keys = token["Keys"];

                        entry.Hotkey = new ShortcutKeys( keys );
                        entry.PassToUO = token["PassToUO"]?.ToObject<bool>() ?? true;
                        entry.Disableable = token["Disableable"]?.ToObject<bool>() ?? entry.Disableable;
                        entry.IsGlobal = global;
                    }
                }
            }

            if ( _spellsCategory != null && !global )
            {
                _hotkeyManager.Items.Remove( _spellsCategory );
            }

            SpellManager spellManager = SpellManager.GetInstance();

            if ( !global )
            {
                _spellsCategory = new HotkeyCommand { Name = Strings.Spells, IsCategory = true };

                SpellData[] spells = spellManager.GetSpellData();

                ObservableCollectionEx<HotkeyEntry> children = new ObservableCollectionEx<HotkeyEntry>();

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

                _hotkeyManager.AddCategory( _spellsCategory );
            }

            JToken spellsObj = hotkeys?["Spells"];

            if ( spellsObj != null )
            {
                foreach ( JToken token in spellsObj )
                {
                    JToken name = token["Name"];

                    HotkeyEntry entry =
                        _spellsCategory.Children.FirstOrDefault( s => s.Name == name.ToObject<string>() );

                    if ( entry == null )
                    {
                        continue;
                    }

                    entry.Hotkey = new ShortcutKeys( token["Keys"] );
                    entry.PassToUO = token["PassToUO"]?.ToObject<bool>() ?? true;
                    entry.IsGlobal = global;
                }
            }

            if ( _masteriesCategory != null && !global )
            {
                _hotkeyManager.Items.Remove( _masteriesCategory );
            }

            if ( !global )
            {
                _masteriesCategory = new HotkeyCommand { Name = Strings.Masteries, IsCategory = true };

                SpellData[] masteries = spellManager.GetMasteryData();

                ObservableCollectionEx<HotkeyEntry> masteryChildren = new ObservableCollectionEx<HotkeyEntry>();

                foreach ( SpellData mastery in masteries )
                {
                    HotkeyCommand hkc = new HotkeyCommand
                    {
                        Name = mastery.Name,
                        Action = hks => spellManager.CastSpell( mastery.ID ),
                        Hotkey = ShortcutKeys.Default,
                        PassToUO = true
                    };

                    masteryChildren.Add( hkc );
                }

                _masteriesCategory.Children = masteryChildren;

                _hotkeyManager.AddCategory( _masteriesCategory );
            }

            JToken masteryObj = hotkeys?["Masteries"];

            if ( masteryObj != null )
            {
                foreach ( JToken token in masteryObj )
                {
                    JToken name = token["Name"];

                    HotkeyEntry entry =
                        _masteriesCategory.Children.FirstOrDefault( s => s.Name == name.ToObject<string>() );

                    if ( entry == null )
                    {
                        continue;
                    }

                    entry.Hotkey = new ShortcutKeys( token["Keys"] );
                    entry.PassToUO = token["PassToUO"]?.ToObject<bool>() ?? true;
                    entry.IsGlobal = global;
                }
            }
        }

        private void CreateMacroButton( object obj )
        {
            if ( !( SelectedItem is HotkeyEntry hotkeyEntry ) )
            {
                return;
            }

            Data.ClassicUO.Macros.CreateMacroButton( hotkeyEntry );
        }

        private JArray SerializeMasteries( Func<HotkeyEntry, bool> predicate )
        {
            JArray masteryArray = new JArray();

            foreach ( HotkeyEntry masteriesCategoryChild in _masteriesCategory.Children.Where( predicate ) )
            {
                if ( Equals( masteriesCategoryChild.Hotkey, ShortcutKeys.Default ) )
                {
                    continue;
                }

                JObject entry = new JObject
                {
                    { "Name", masteriesCategoryChild.Name },
                    { "Keys", masteriesCategoryChild.Hotkey.ToJObject() },
                    { "PassToUO", masteriesCategoryChild.PassToUO }
                };

                masteryArray.Add( entry );
            }

            return masteryArray;
        }

        private JArray SerializeSpells( Func<HotkeyEntry, bool> predicate )
        {
            JArray spellsArray = new JArray();

            foreach ( HotkeyEntry spellsCategoryChild in _spellsCategory.Children.Where( predicate ) )
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

            return spellsArray;
        }

        private JArray SerializeCommands( Func<HotkeyEntry, bool> predicate )
        {
            JArray commandsArray = new JArray();

            foreach ( JObject entry in from category in _serializeCategories
                     from categoryChild in category.Children.Where( predicate )
                     where !Equals( categoryChild.Hotkey, ShortcutKeys.Default )
                     select new JObject
                     {
                         { "Type", categoryChild.GetType().FullName },
                         { "Keys", categoryChild.Hotkey.ToJObject() },
                         { "PassToUO", categoryChild.PassToUO },
                         { "Disableable", categoryChild.Disableable }
                     } )
            {
                commandsArray.Add( entry );
            }

            return commandsArray;
        }

        private void UpdateFilteredItems()
        {
            _dispatcher.Invoke( () =>
            {
                if ( string.IsNullOrEmpty( FilterText ) )
                {
                    FilterItems = Items;
                    return;
                }

                if ( Searching )
                {
                    return;
                }

                Searching = true;

                bool Predicate( HotkeyEntry hke )
                {
                    return hke.Name.ToLower().Contains( FilterText.ToLower() );
                }

                FilterItems = new ObservableCollectionEx<HotkeyCommand>( Items.Where( e => e.Children.Any( Predicate ) )
                    .Select( e =>
                    {
                        HotkeyCommand hkc = new HotkeyCommand
                        {
                            Tooltip = e.Tooltip,
                            Name = e.Name,
                            Action = e.Action,
                            Disableable = e.Disableable,
                            Hotkey = e.Hotkey,
                            IsCategory = e.IsCategory,
                            PassToUO = e.PassToUO,
                            IsExpanded = true
                        };

                        IEnumerable<HotkeyEntry> children = e.Children.Where( Predicate ).ToList();

                        foreach ( HotkeyEntry child in children )
                        {
                            child.PropertyChanged += ( s, ea ) => UpdateFilteredItems();
                        }

                        hkc.Children = new ObservableCollectionEx<HotkeyEntry>( children );

                        return hkc;
                    } ) );

                Searching = false;
            } );
        }

        private void CheckOverwriteHotkey( HotkeyEntry selectedItem, ShortcutKeys hotkey )
        {
            HotkeyEntry conflict = null;

            foreach ( HotkeyCommand hotkeyEntry in Items )
            {
                foreach ( HotkeyEntry entry in hotkeyEntry.Children )
                {
                    if ( entry.Hotkey.Equals( hotkey ) )
                    {
                        conflict = entry;
                    }
                }
            }

            if ( conflict != null && !ReferenceEquals( selectedItem, conflict ) )
            {
                MessageBoxResult result =
                    MessageBox.Show( string.Format( Strings.Overwrite_existing_hotkey___0____, conflict ),
                        Strings.Warning, MessageBoxButton.YesNo );

                if ( result == MessageBoxResult.No )
                {
                    OnPropertyChanged( nameof( Hotkey ) );
                    return;
                }
            }

            SelectedItem.Hotkey = hotkey;
            OnPropertyChanged( nameof( Hotkey ) );
        }

        private void ClearAllHotkeys()
        {
            foreach ( HotkeyEntry entryChild in _serializeCategories.SelectMany( hotkeyEntry => hotkeyEntry.Children ) )
            {
                entryChild.Hotkey = ShortcutKeys.Default;
            }

            foreach ( HotkeyEntry spellEntry in _spellsCategory.Children )
            {
                spellEntry.Hotkey = ShortcutKeys.Default;
            }
        }

        private static void ClearHotkey( object obj )
        {
            if ( obj is HotkeyEntry cmd )
            {
                cmd.Hotkey = ShortcutKeys.Default;
            }
        }

        private static void ExecuteHotkey( object obj )
        {
            if ( obj is HotkeyEntry cmd )
            {
                cmd.Action( cmd );
            }
        }
    }
}