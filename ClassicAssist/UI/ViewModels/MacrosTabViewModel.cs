using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Controls.DraggableTreeView;
using ClassicAssist.Data;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Data.Macros;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.ViewModels.Macros;
using ClassicAssist.UI.Views;
using ClassicAssist.UI.Views.Macros;
using ClassicAssist.UO;
using ICSharpCode.AvalonEdit.Document;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.UI.ViewModels
{
    public class MacrosTabViewModel : HotkeyEntryViewModel<MacroEntry>, ISettingProvider
    {
        private readonly MacroManager _manager;
        private int _caretPosition;
        private ICommand _clearHotkeyCommand;
        private ICommand _createMacroButtonCommand;
        private TextDocument _document;
        private ObservableCollection<IDraggable> _draggables = new ObservableCollection<IDraggable>();
        private ICommand _executeCommand;
        private ICommand _formatCodeCommand;
        private string _formatError;
        private ICommand _inspectObjectCommand;
        private bool _isPerformingAction;
        private bool _isRecording;
        private double _leftColumnWidth = 200;
        private ICommand _newGroupCommand;
        private RelayCommand _newMacroCommand;
        private ICommand _openModulesFolderCommand;
        private ICommand _recordCommand;
        private ICommand _removeGroupCommand;
        private RelayCommand _removeMacroCommand;
        private ICommand _removeMacroConfirmCommand;
        private ICommand _resetImportCacheCommand;
        private ICommand _saveMacroCommand;
        private MacroGroup _selectedGroup;
        private MacroEntry _selectedItem;
        private ICommand _shareMacroCommand;
        private ICommand _showActiveObjectsWindowCommand;
        private ICommand _showCommandsCommand;
        private ICommand _showMacrosWikiCommand;
        private ICommand _stopCommand;

        public MacrosTabViewModel() : base( Strings.Macros )
        {
            Engine.DisconnectedEvent += OnDisconnectedEvent;

            _manager = MacroManager.GetInstance();

            _manager.IsRecording = () => _isRecording;
            _manager.InsertDocument = str =>
            {
                _dispatcher.Invoke( () =>
                {
                    if ( SelectedItem != null )
                    {
                        SelectedItem.Macro += str ?? string.Empty;
                    }
                } );
            };
            _manager.NewMacro = NewMacro;
            _manager.Items = Items;
            Items.CollectionChanged += UpdateDraggables;
        }

        public int CaretPosition
        {
            get => _caretPosition;
            set => SetProperty( ref _caretPosition, value );
        }

        public ICommand ClearHotkeyCommand =>
            _clearHotkeyCommand ?? ( _clearHotkeyCommand = new RelayCommand( ClearHotkey, o => SelectedItem != null ) );

        public ICommand CreateMacroButtonCommand =>
            _createMacroButtonCommand ?? ( _createMacroButtonCommand =
                new RelayCommand( CreateMacroButton, o => Engine.Connected ) );

        public TextDocument Document
        {
            get => _document;
            set => SetProperty( ref _document, value );
        }

        public ObservableCollection<IDraggable> Draggables
        {
            get => _draggables;
            set => SetProperty( ref _draggables, value );
        }

        public ICommand ExecuteCommand =>
            _executeCommand ?? ( _executeCommand = new RelayCommandAsync( Execute, CanExecute ) );

        public ICommand FormatCodeCommand =>
            _formatCodeCommand ??
            ( _formatCodeCommand = new RelayCommandAsync( FormatCode, o => SelectedItem != null ) );

        public string FormatError
        {
            get => _formatError;
            set => SetProperty( ref _formatError, value );
        }

        public ShortcutKeys Hotkey
        {
            get => SelectedItem?.Hotkey;
            set => CheckOverwriteHotkey( SelectedItem, value );
        }

        public ICommand InspectObjectCommand =>
            _inspectObjectCommand ??
            ( _inspectObjectCommand = new RelayCommandAsync( InspectObject, o => Engine.Connected ) );

        public bool IsPerformingAction
        {
            get => _isPerformingAction;
            set => SetProperty( ref _isPerformingAction, value );
        }

        public bool IsRecording
        {
            get => _isRecording;
            set => SetProperty( ref _isRecording, value );
        }

        public double LeftColumnWidth
        {
            get => _leftColumnWidth;
            set => SetProperty( ref _leftColumnWidth, value );
        }

        public ICommand NewGroupCommand =>
            _newGroupCommand ?? ( _newGroupCommand = new RelayCommand( NewGroup, o => true ) );

        public RelayCommand NewMacroCommand =>
            _newMacroCommand ??
            ( _newMacroCommand = new RelayCommand( NewMacro, o => !SelectedItem?.IsRunning ?? true ) );

        public ICommand OpenModulesFolderCommand =>
            _openModulesFolderCommand ??
            ( _openModulesFolderCommand = new RelayCommand( OpenModulesFolder, o => true ) );

        public ICommand RecordCommand =>
            _recordCommand ?? ( _recordCommand = new RelayCommand( Record, o => SelectedItem != null ) );

        public string RecordLabel => IsRecording ? Strings.Stop : Strings.Record;

        public ICommand RemoveGroupCommand =>
            _removeGroupCommand ?? ( _removeGroupCommand = new RelayCommand( RemoveGroup, o => o is IDraggableGroup ) );

        public RelayCommand RemoveMacroCommand =>
            _removeMacroCommand ?? ( _removeMacroCommand =
                new RelayCommand( RemoveMacro, o => !SelectedItem?.IsRunning ?? SelectedItem != null ) );

        public ICommand RemoveMacroConfirmCommand =>
            _removeMacroConfirmCommand ?? ( _removeMacroConfirmCommand =
                new RelayCommand( RemoveMacroConfirm, o => SelectedItem != null ) );

        public ICommand ResetImportCacheCommand =>
            _resetImportCacheCommand ?? ( _resetImportCacheCommand = new RelayCommand( ResetImportCache,
                o => SelectedItem != null && !SelectedItem.IsRunning ) );

        public ICommand SaveMacroCommand =>
            _saveMacroCommand ?? ( _saveMacroCommand = new RelayCommand( SaveMacro, o => true ) );

        public MacroGroup SelectedGroup
        {
            get => _selectedGroup;
            set => SetProperty( ref _selectedGroup, value );
        }

        public MacroEntry SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty( ref _selectedItem, value );
                OnPropertyChanged( nameof( Hotkey ) );

                if ( Document != null )
                {
                    Document.UndoStack.SizeLimit = 0;
                }
            }
        }

        public ICommand ShareMacroCommand =>
            _shareMacroCommand ??
            ( _shareMacroCommand = new RelayCommandAsync( ShareMacro, o => SelectedItem != null ) );

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
            JArray groupArray = new JArray();

            foreach ( MacroEntry macroEntry in Items )
            {
                _dispatcher.Invoke( () => { macroEntry.Group = FindGroup( macroEntry ); } );
            }

            foreach ( IDraggableGroup draggableGroup in Draggables.Where( i => i is IDraggableGroup )
                .Cast<IDraggableGroup>() )
            {
                JObject entry = new JObject { { "Name", draggableGroup.Name } };

                groupArray.Add( entry );
            }

            IEnumerable<MacroEntry> globalMacros = Items.Where( e => e.Global ).ToList();

            if ( globalMacros.Any() )
            {
                string globalJson = JsonConvert.SerializeObject( globalMacros.Select( e => e.ToJObject() ), Formatting.Indented );

                File.WriteAllText( Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Macros.json" ),
                    globalJson );
            }

            foreach ( MacroEntry macroEntry in Items.Where( e => !e.Global ) )
            {
                macroArray.Add( macroEntry.ToJObject() );
            }

            macros.Add( "LeftColumnWidth", LeftColumnWidth );
            macros.Add( "Groups", groupArray );
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
            SelectedItem = null;
            SelectedGroup = null;

            Items.Clear();
            Draggables.Clear();

            JToken config = json?["Macros"];

#if !DEVELOP
            LeftColumnWidth = config?["LeftColumnWidth"]?.ToObject<double>() ?? 200;
#endif

            if ( config?["Groups"] != null )
            {
                foreach ( JToken token in config["Groups"] )
                {
                    MacroGroup macroGroup = new MacroGroup { Name = GetJsonValue( token, "Name", "Group" ) };

                    if ( Draggables.Where( i => i is IDraggableGroup ).Any( e => e.Name == macroGroup.Name ) )
                    {
                        continue;
                    }

                    if ( Options.CurrentOptions.SortMacrosAlphabetical )
                    {
                        Draggables.AddSorted( macroGroup, new GroupsBeforeMacrosComparer() );
                    }
                    else
                    {
                        Draggables.Add( macroGroup );
                    }
                }
            }

            string globalPath = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Macros.json" );

            if ( File.Exists( globalPath ) )
            {
                JArray globalJson = JArray.Parse( File.ReadAllText( globalPath ) );

                foreach ( JToken token in globalJson )
                {
                    MacroEntry entry = new MacroEntry( token ) { Global = true };

                    entry.Action = async hks => await Execute( entry );
                    entry.Hotkey = new ShortcutKeys( token["Keys"] );
                    entry.Global = true;

                    if ( entry.Group != null && !Draggables.Any( e => e is IDraggableGroup && e.Name == entry.Group ) )
                    {
                        MacroGroup macroGroup = new MacroGroup { Name = entry.Group };

                        if ( Options.CurrentOptions.SortMacrosAlphabetical )
                        {
                            Draggables.AddSorted( macroGroup, new GroupsBeforeMacrosComparer() );
                        }
                        else
                        {
                            Draggables.Add( macroGroup );
                        }
                    }

                    if ( Options.CurrentOptions.SortMacrosAlphabetical )
                    {
                        Items.AddSorted( entry, new GroupsBeforeMacrosComparer() );
                    }
                    else
                    {
                        Items.Add( entry );
                    }
                }
            }

            if ( config?["Macros"] != null )
            {
                foreach ( JToken token in config["Macros"] )
                {
                    MacroEntry entry = new MacroEntry( token );

                    // Global macros take precedence for hotkey
                    ShortcutKeys hotkey = new ShortcutKeys( token["Keys"] );

                    if ( !Items.Any( e => e.Global && Equals( e.Hotkey, hotkey ) ) )
                    {
                        entry.Hotkey = hotkey;
                    }

                    entry.Action = async hks => await Execute( entry );
                    entry.Global = false;

                    if ( Options.CurrentOptions.SortMacrosAlphabetical )
                    {
                        Items.AddSorted( entry, new GroupsBeforeMacrosComparer() );
                    }
                    else
                    {
                        Items.Add( entry );
                    }
                }
            }

            if ( config?["Alias"] != null )
            {
                foreach ( JToken token in config["Alias"] )
                {
                    AliasCommands.SetAlias( token["Name"].ToObject<string>(), token["Value"].ToObject<int>() );
                }
            }

            string modulePath = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Modules" );

            if ( !Directory.Exists( modulePath ) )
            {
                Directory.CreateDirectory( modulePath );
            }

            SelectedItem = Items.LastOrDefault();
        }

        private string FindGroup( IDraggable macroEntry )
        {
            return ( from draggableGroup in Draggables.Where( i => i is IDraggableGroup ).Cast<IDraggableGroup>()
                where draggableGroup.Children.Contains( macroEntry )
                select draggableGroup.Name ).FirstOrDefault();
        }

        private void NewGroup( object obj )
        {
            int count = Draggables.Count( i => i is IDraggableGroup );

            if ( Options.CurrentOptions.SortMacrosAlphabetical )
            {
                Draggables.AddSorted( new MacroGroup { Name = $"Group-{count + 1}" },
                    new GroupsBeforeMacrosComparer() );
            }
            else
            {
                Draggables.Add( new MacroGroup { Name = $"Group-{count + 1}" } );
            }
        }

        private void UpdateDraggables( object sender, NotifyCollectionChangedEventArgs e )
        {
            if ( e.NewItems != null )
            {
                foreach ( object newItem in e.NewItems )
                {
                    if ( !( newItem is MacroEntry macroEntry ) )
                    {
                        continue;
                    }

                    if ( !string.IsNullOrEmpty( macroEntry.Group ) )
                    {
                        MacroGroup macroGroup =
                            (MacroGroup) Draggables.FirstOrDefault( i =>
                                i is MacroGroup && i.Name == macroEntry.Group );

                        if ( macroGroup == null )
                        {
                            if ( Options.CurrentOptions.SortMacrosAlphabetical )
                            {
                                Draggables.AddSorted( macroEntry, new GroupsBeforeMacrosComparer() );
                            }
                            else
                            {
                                Draggables.Add( macroEntry );
                            }

                            return;
                        }

                        if ( Options.CurrentOptions.SortMacrosAlphabetical )
                        {
                            macroGroup.Children.AddSorted( macroEntry );
                        }
                        else
                        {
                            macroGroup.Children.Add( macroEntry );
                        }

                        OnPropertyChanged( nameof( Draggables ) );
                    }
                    else
                    {
                        if ( Options.CurrentOptions.SortMacrosAlphabetical )
                        {
                            Draggables.AddSorted( macroEntry, new GroupsBeforeMacrosComparer() );
                        }
                        else
                        {
                            Draggables.Add( macroEntry );
                        }
                    }
                }
            }

            if ( e.OldItems == null )
            {
                return;
            }

            foreach ( object newItem in e.OldItems )
            {
                if ( !( newItem is MacroEntry macroEntry ) )
                {
                    continue;
                }

                if ( string.IsNullOrEmpty( macroEntry.Group ) )
                {
                    Draggables.Remove( macroEntry );
                }
                else
                {
                    MacroGroup macroGroup =
                        (MacroGroup) Draggables.FirstOrDefault( i => i is MacroGroup && i.Name == macroEntry.Group );

                    macroGroup?.Children.Remove( macroEntry );
                }
            }
        }

        private async Task ShareMacro( object arg )
        {
            if ( !( arg is MacroEntry macro ) )
            {
                return;
            }

            try
            {
                IsPerformingAction = true;
                HttpClient httpClient = new HttpClient();

                ShareMacroModel data = new ShareMacroModel { Content = macro.Macro };

                HttpResponseMessage response = await httpClient.PostAsync(
                    "https://classicassist.azurewebsites.net/api/macros/stage",
                    new StringContent( JsonConvert.SerializeObject( data ), Encoding.UTF8, "application/json" ) );

                string json = await response.Content.ReadAsStringAsync();

                if ( response.IsSuccessStatusCode )
                {
                    ShareMacroModel responseData = JsonConvert.DeserializeObject<ShareMacroModel>( json );

                    if ( !string.IsNullOrEmpty( responseData?.Uuid ) )
                    {
                        Process.Start( $"https://classicassist.azurewebsites.net/?id={responseData.Uuid}" );
                    }
                }
                else
                {
                    ShareErrorResponseModel errorObj = JsonConvert.DeserializeObject<ShareErrorResponseModel>( json );

                    if ( errorObj != null && !string.IsNullOrEmpty( errorObj.Message ) )
                    {
                        MessageBox.Show( $"Error sharing macro: {errorObj.Message}" );
                    }
                    else
                    {
                        MessageBox.Show( $"Unknown error sharing macro: {response.StatusCode}" );
                    }
                }
            }
            finally
            {
                IsPerformingAction = false;
            }
        }

        private static void ResetImportCache( object obj )
        {
            MacroInvoker.ResetImportCache();
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

        private static void CreateMacroButton( object obj )
        {
            if ( !( obj is MacroEntry macro ) )
            {
                return;
            }

            Data.ClassicUO.Macros.CreateMacroButton( macro );
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
                    OnPropertyChanged( nameof( Hotkey ) );
                    return;
                }
            }

            SelectedItem.Hotkey = hotkey;
            OnPropertyChanged( nameof( Hotkey ) );
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
            await Commands.InspectObjectAsync();
        }

        private void NewMacro( object obj )
        {
            int count = Items.Count;

            MacroEntry macro = new MacroEntry { Name = $"Macro-{count + 1}", Macro = string.Empty };

            macro.Action = async hks => await Execute( macro );

            Items.Add( macro );

            SelectedItem = macro;
        }

        private void NewMacro( string name, string macroText )
        {
            MacroEntry macro = new MacroEntry { Name = name, Macro = macroText };

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
                OnPropertyChanged( nameof( RecordLabel ) );
                return;
            }

            IsRecording = true;
            OnPropertyChanged( nameof( RecordLabel ) );
        }

        private void RemoveGroup( object obj )
        {
            if ( !( obj is IDraggableGroup group ) )
            {
                return;
            }

            foreach ( MacroEntry groupChild in group.Children.Where( i => i is MacroEntry ).Cast<MacroEntry>() )
            {
                if ( Options.CurrentOptions.SortMacrosAlphabetical )
                {
                    Draggables.AddSorted( groupChild, new GroupsBeforeMacrosComparer() );
                }
                else
                {
                    Draggables.Add( groupChild );
                }

                groupChild.Group = null;
            }

            Draggables.Remove( group );
        }

        private static void OpenModulesFolder( object obj )
        {
            Process.Start( "explorer.exe",
                Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Modules" ) );
        }

        private async Task FormatCode( object obj )
        {
            if ( !( obj is MacroEntry macroEntry ) )
            {
                return;
            }

            string code =
                $"from yapf.yapflib.yapf_api import FormatCode\r\nformatted_code, changed = FormatCode('{macroEntry.Macro.Replace( "\r\n", @"\n" ).Replace( "\n", @"\n" ).Replace( "'", @"\'" )}')";

            try
            {
                IsPerformingAction = true;
                FormatError = null;
                ScriptEngine engine = Python.CreateEngine();
                string modulePath = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Modules" );
                ICollection<string> searchPaths = engine.GetSearchPaths();

                if ( !searchPaths.Contains( modulePath ) )
                {
                    searchPaths.Add( modulePath );
                }

                searchPaths.Add( Engine.StartupPath );

                engine.SetSearchPaths( searchPaths );

                ScriptSource importSource = engine.CreateScriptSourceFromString( code );
                ScriptScope scope = engine.CreateScope();
                importSource.Compile();

                await Task.Run( () => importSource.Execute( scope ) );

                bool changed = scope.GetVariable<bool>( "changed" );

                if ( changed )
                {
                    macroEntry.Macro = scope.GetVariable<string>( "formatted_code" ).Replace( "\n", "\r\n" );
                }
            }
            catch ( Exception e )
            {
                if ( e is SyntaxErrorException syntaxError )
                {
                    FormatError = $"{Strings.Line_Number} {syntaxError.RawSpan.Start.Line}: {e.Message}";
                }
                else
                {
                    FormatError = e.Message;
                }
            }
            finally
            {
                IsPerformingAction = false;
            }
        }
    }

    internal class ShareMacroModel
    {
        public string Content { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string Uuid { get; set; }
    }

    internal class ShareErrorResponseModel
    {
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}