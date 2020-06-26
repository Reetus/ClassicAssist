using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Data.Macros;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
using ClassicAssist.UI.ViewModels.Macros;
using ClassicAssist.UI.Views;
using ClassicAssist.UI.Views.Macros;
using ClassicAssist.UO;
using ClassicAssist.UO.Objects;
using ICSharpCode.AvalonEdit.Document;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.UI.ViewModels
{
    public class MacrosTabViewModel : HotkeyEntryViewModel<MacroEntry>, ISettingProvider
    {
        private readonly MacroManager _manager;
        private int _caretPosition;
        private ICommand _clearHotkeyCommand;
        private TextDocument _document;
        private ICommand _executeCommand;
        private ICommand _inspectObjectCommand;
        private bool _isRecording;
        private RelayCommand _newMacroCommand;
        private ICommand _recordCommand;
        private RelayCommand _removeMacroCommand;
        private ICommand _removeMacroConfirmCommand;
        private ICommand _saveMacroCommand;
        private MacroEntry _selectedItem;
        private ICommand _showActiveObjectsWindowCommand;
        private ICommand _showCommandsCommand;
        private ICommand _showMacrosWikiCommand;
        private ICommand _stopCommand;

        public MacrosTabViewModel() : base( Strings.Macros )
        {
            Engine.DisconnectedEvent += OnDisconnectedEvent;

            _manager = MacroManager.GetInstance();

            _manager.IsRecording = () => _isRecording;
            _manager.InsertDocument = str => { _dispatcher.Invoke( () => { SelectedItem.Macro += str; } ); };
            _manager.NewMacro = NewMacro;
            _manager.Items = Items;
        }

        public int CaretPosition
        {
            get => _caretPosition;
            set => SetProperty( ref _caretPosition, value );
        }

        public ICommand ClearHotkeyCommand =>
            _clearHotkeyCommand ?? ( _clearHotkeyCommand = new RelayCommand( ClearHotkey, o => SelectedItem != null ) );

        public TextDocument Document
        {
            get => _document;
            set => SetProperty( ref _document, value );
        }

        public ICommand ExecuteCommand =>
            _executeCommand ?? ( _executeCommand = new RelayCommandAsync( Execute, CanExecute ) );

        public ShortcutKeys Hotkey
        {
            get => SelectedItem?.Hotkey;
            set => CheckOverwriteHotkey( SelectedItem, value );
        }

        public ICommand InspectObjectCommand =>
            _inspectObjectCommand ??
            ( _inspectObjectCommand = new RelayCommandAsync( InspectObject, o => Engine.Connected ) );

        public bool IsRecording
        {
            get => _isRecording;
            set => SetProperty( ref _isRecording, value );
        }

        public RelayCommand NewMacroCommand =>
            _newMacroCommand ??
            ( _newMacroCommand = new RelayCommand( NewMacro, o => !SelectedItem?.IsRunning ?? true ) );

        public ICommand RecordCommand =>
            _recordCommand ?? ( _recordCommand = new RelayCommand( Record, o => SelectedItem != null ) );

        public string RecordLabel => IsRecording ? Strings.Stop : Strings.Record;

        public RelayCommand RemoveMacroCommand =>
            _removeMacroCommand ?? ( _removeMacroCommand =
                new RelayCommand( RemoveMacro, o => !SelectedItem?.IsRunning ?? SelectedItem != null ) );

        public ICommand RemoveMacroConfirmCommand =>
            _removeMacroConfirmCommand ?? ( _removeMacroConfirmCommand =
                new RelayCommand( RemoveMacroConfirm, o => SelectedItem != null ) );

        public ICommand SaveMacroCommand =>
            _saveMacroCommand ?? ( _saveMacroCommand = new RelayCommand( SaveMacro, o => true ) );

        public MacroEntry SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty( ref _selectedItem, value );
                NotifyPropertyChanged( nameof( Hotkey ) );
            }
        }

        public ICommand ShowActiveObjectsWindowCommand =>
            _showActiveObjectsWindowCommand ?? ( _showActiveObjectsWindowCommand =
                new RelayCommand( ShowActiveObjectsWindow, o => true ) );

        public ICommand ShowCommandsCommand =>
            _showCommandsCommand ?? ( _showCommandsCommand = new RelayCommand( ShowCommands, o => true ) );

        public ICommand ShowMacrosWikiCommand =>
            _showMacrosWikiCommand ?? ( _showMacrosWikiCommand = new RelayCommand( ShowMacrosWiki, o => true ) );

        public ICommand StopCommand =>
            _stopCommand ?? ( _stopCommand = new RelayCommandAsync( Stop, o => SelectedItem?.IsRunning ?? false ) );

        public void Serialize( JObject json )
        {
            JObject macros = new JObject();

            JArray macroArray = new JArray();

            foreach ( MacroEntry macroEntry in Items )
            {
                JObject entry = new JObject
                {
                    { "Name", macroEntry.Name },
                    { "Loop", macroEntry.Loop },
                    { "DoNotAutoInterrupt", macroEntry.DoNotAutoInterrupt },
                    { "Macro", macroEntry.Macro },
                    { "PassToUO", macroEntry.PassToUO },
                    { "Keys", macroEntry.Hotkey.ToJObject() },
                    { "IsBackground", macroEntry.IsBackground },
                    { "IsAutostart", macroEntry.IsAutostart },
                    { "Disableable", macroEntry.Disableable }
                };

                JArray aliasesArray = new JArray();

                foreach ( JObject aliasObj in macroEntry.Aliases.Select( kvp =>
                    new JObject { { "Key", kvp.Key }, { "Value", kvp.Value } } ) )
                {
                    aliasesArray.Add( aliasObj );
                }

                entry.Add( "Aliases", aliasesArray );

                macroArray.Add( entry );
            }

            macros.Add( "Macros", macroArray );

            JArray aliasArray = new JArray();

            foreach ( JObject entry in AliasCommands.GetAllAliases()
                .Select( kvp => new JObject { { "Name", kvp.Key }, { "Value", kvp.Value } } ) )
            {
                aliasArray.Add( entry );
            }

            macros.Add( "Alias", aliasArray );

            json?.Add( "Macros", macros );
        }

        public void Deserialize( JObject json, Options options )
        {
            Items.Clear();

            JToken config = json?["Macros"];

            if ( config == null )
            {
                return;
            }

            if ( config["Macros"] != null )
            {
                foreach ( JToken token in config["Macros"] )
                {
                    MacroEntry entry = new MacroEntry
                    {
                        Name = GetJsonValue( token, "Name", string.Empty ),
                        Loop = GetJsonValue( token, "Loop", false ),
                        DoNotAutoInterrupt = GetJsonValue( token, "DoNotAutoInterrupt", false ),
                        Macro = GetJsonValue( token, "Macro", string.Empty ),
                        PassToUO = GetJsonValue( token, "PassToUO", true ),
                        Hotkey = new ShortcutKeys( token["Keys"] ),
                        IsBackground = GetJsonValue( token, "IsBackground", false ),
                        IsAutostart = GetJsonValue( token, "IsAutostart", false ),
                        Disableable = GetJsonValue( token, "Disableable", true )
                    };

                    if ( token["Aliases"] != null )
                    {
                        foreach ( JToken aliasToken in token["Aliases"] )
                        {
                            entry.Aliases.Add( aliasToken["Key"].ToObject<string>(),
                                aliasToken["Value"].ToObject<int>() );
                        }
                    }

                    entry.Action = async hks => await Execute( entry );

                    if ( Options.CurrentOptions.SortMacrosAlphabetical )
                    {
                        Items.AddSorted( entry );
                    }
                    else
                    {
                        Items.Add( entry );
                    }
                }
            }

            if ( config["Alias"] != null )
            {
                foreach ( JToken token in config["Alias"] )
                {
                    AliasCommands.SetAlias( token["Name"].ToObject<string>(), token["Value"].ToObject<int>() );
                }
            }
        }

        private void RemoveMacroConfirm( object obj )
        {
            if ( !( obj is MacroEntry entry ) )
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show( string.Format( Strings.Really_remove_macro___0___, entry.Name ),
                Strings.Warning, MessageBoxButton.YesNo, MessageBoxImage.Warning );

            if ( result == MessageBoxResult.No )
            {
                return;
            }

            RemoveMacro( entry );
        }

        private static void ShowMacrosWiki( object obj )
        {
            Process.Start( Strings.MACRO_WIKI_URL );
        }

        private bool CanExecute( object arg )
        {
            if ( !( arg is MacroEntry entry ) )
            {
                return false;
            }

            if ( entry.IsRunning )
            {
                return false;
            }

            return true;
        }

        private void CheckOverwriteHotkey( HotkeyEntry selectedItem, ShortcutKeys hotkey )
        {
            HotkeyEntry conflict = null;

            foreach ( HotkeyEntry hotkeyEntry in HotkeyManager.GetInstance().Items )
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
                    NotifyPropertyChanged( nameof( Hotkey ) );
                    return;
                }
            }

            SelectedItem.Hotkey = hotkey;
            NotifyPropertyChanged( nameof( Hotkey ) );
        }

        private static void SaveMacro( object obj )
        {
            //Saves whole profile, think of better way
            Options.Save( Options.CurrentOptions );
        }

        private async Task Execute( object obj )
        {
            if ( !( obj is MacroEntry entry ) )
            {
                return;
            }

            _manager.Execute( entry );

            await Task.CompletedTask;
        }

        private static void ShowActiveObjectsWindow( object obj )
        {
            ActiveObjectsWindow window = new ActiveObjectsWindow();
            window.Show();
        }

        private void OnDisconnectedEvent()
        {
            _manager.StopAll();
        }

        private static void ClearHotkey( object obj )
        {
            if ( !( obj is MacroEntry entry ) )
            {
                return;
            }

            entry.Hotkey = ShortcutKeys.Default;
        }

        private static async Task InspectObject( object arg )
        {
            int serial = await Commands.GetTargeSerialAsync( Strings.Target_object___ );

            if ( serial > 0 )
            {
                Entity entity = UOMath.IsMobile( serial )
                    ? (Entity) Engine.Mobiles.GetMobile( serial )
                    : Engine.Items.GetItem( serial );

                if ( entity == null )
                {
                    return;
                }

                ObjectInspectorWindow window =
                    new ObjectInspectorWindow { DataContext = new ObjectInspectorViewModel( entity ) };

                window.Show();
            }
        }

        private void NewMacro( object obj )
        {
            int count = Items.Count;

            MacroEntry macro = new MacroEntry { Name = $"Macro-{count + 1}", Macro = string.Empty };

            macro.Action = async hks => await Execute( macro );

            Items.Add( macro );
        }

        private void NewMacro( string name, string macroText )
        {
            MacroEntry macro = new MacroEntry() { Name = name, Macro = macroText};

            macro.Action = async hks => await Execute( macro );

            Items.Add( macro );

            SelectedItem = macro;
        }

        private void RemoveMacro( object obj )
        {
            if ( obj is MacroEntry entry )
            {
                Items.Remove( entry );
            }
        }

        private async Task Stop( object obj )
        {
            if ( !( obj is MacroEntry entry ) )
            {
                return;
            }

            entry.Stop();

            await Task.CompletedTask;
        }

        private void ShowCommands( object obj )
        {
            MacrosCommandWindow window = new MacrosCommandWindow { DataContext = new MacrosCommandViewModel( this ) };
            window.ShowDialog();
        }

        private void Record( object obj )
        {
            if ( IsRecording )
            {
                IsRecording = false;
                NotifyPropertyChanged( nameof( RecordLabel ) );
                return;
            }

            IsRecording = true;
            NotifyPropertyChanged( nameof( RecordLabel ) );
        }
    }
}