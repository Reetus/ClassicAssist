using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Filters;
using ClassicAssist.Data.Macros;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Views;
using ClassicAssist.UI.Views.General;
using ClassicAssist.UO;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.UI.ViewModels
{
    public class GeneralControlViewModel : BaseViewModel, ISettingProvider
    {
        private static ICommand _saveProfileCommand;
        private ICommand _autologinConfigureCommand;
        private ICommand _backupSettingsCommand;
        private ICommand _changeProfileCommand;
        private ICommand _configureFilterCommand;
        private bool _isLinkedProfile;
        private ICommand _linkUnlinkProfileCommand;
        private ICommand _newProfileCommand;
        private Options _options;
        private ObservableCollection<string> _profiles = new ObservableCollection<string>();
        private ICommand _removeSavedPasswordCommand;
        private Dictionary<string, string> _savedPasswords = new Dictionary<string, string>();
        private string _selectedProfile = Options.CurrentOptions.Name;

        public GeneralControlViewModel()
        {
            Type[] filterTypes =
            {
                typeof( WeatherFilter ), typeof( SeasonFilter ), typeof( LightLevelFilter ),
                typeof( RepeatedMessagesFilter ), typeof( ClilocFilter ), typeof( SoundFilter ),
                typeof( ItemIDFilter )
            };

            foreach ( Type type in filterTypes )
            {
                FilterEntry fe = (FilterEntry) Activator.CreateInstance( type );
                Filters.Add( fe );
            }

            RefreshProfiles();

            AssistantOptions.ProfileChangedEvent += OnProfileChangedEvent;
            AssistantOptions.SavedPasswordsChanged += OnSavedPasswordsChangedEvent;

            OnSavedPasswordsChangedEvent( this, EventArgs.Empty );
        }

        public ICommand AutologinConfigureCommand =>
            _autologinConfigureCommand ?? ( _autologinConfigureCommand = new RelayCommand( AutologinConfigure ) );

        public ICommand BackupSettingsCommand =>
            _backupSettingsCommand ?? ( _backupSettingsCommand = new RelayCommand( BackupSettings, o => true ) );

        public ICommand ChangeProfileCommand =>
            _changeProfileCommand ?? ( _changeProfileCommand = new RelayCommand( ChangeProfile, o => true ) );

        public ICommand ConfigureFilterCommand =>
            _configureFilterCommand ?? ( _configureFilterCommand = new RelayCommand( ConfigureFilter ) );

        public ObservableCollectionEx<FilterEntry> Filters { get; set; } = new ObservableCollectionEx<FilterEntry>();

        public bool IsLinkedProfile
        {
            get => _isLinkedProfile;
            set => SetProperty( ref _isLinkedProfile, value );
        }

        public ICommand LinkUnlinkProfileCommand =>
            _linkUnlinkProfileCommand ?? ( _linkUnlinkProfileCommand = new RelayCommand( LinkUnlinkProfile ) );

        public ICommand NewProfileCommand =>
            _newProfileCommand ?? ( _newProfileCommand = new RelayCommandAsync( NewProfile, o => true ) );

        public Options Options
        {
            get => _options;
            set => SetProperty( ref _options, value );
        }

        public ObservableCollection<string> Profiles
        {
            get => _profiles;
            set => SetProperty( ref _profiles, value );
        }

        public ICommand RemoveSavedPasswordCommand =>
            _removeSavedPasswordCommand ?? ( _removeSavedPasswordCommand =
                new RelayCommand( RemoveSavedPassword, o => AssistantOptions.SavePasswords ) );

        public Dictionary<string, string> SavedPasswords
        {
            get => _savedPasswords;
            set => SetProperty( ref _savedPasswords, value );
        }

        public bool SavePasswords
        {
            get => AssistantOptions.SavePasswords;
            set => AssistantOptions.SavePasswords = value;
        }

        public bool SavePasswordsOnlyBlank
        {
            get => AssistantOptions.SavePasswordsOnlyBlank;
            set => AssistantOptions.SavePasswordsOnlyBlank = value;
        }

        public ICommand SaveProfileCommand =>
            _saveProfileCommand ?? ( _saveProfileCommand = new RelayCommand( SaveProfile ) );

        public string SelectedProfile
        {
            get => _selectedProfile;
            set => SetProperty( ref _selectedProfile, value );
        }

        public void Serialize( JObject json, bool global = false )
        {
            JObject obj = new JObject
            {
                ["AlwaysOnTop"] = Options.CurrentOptions.AlwaysOnTop,
                ["LightLevel"] = Options.CurrentOptions.LightLevel,
                ["ActionDelay"] = Options.CurrentOptions.ActionDelay,
                ["ActionDelayMS"] = Options.CurrentOptions.ActionDelayMS,
                ["DragDelay"] = Options.CurrentOptions.DragDelay,
                ["DragDelayMS"] = Options.CurrentOptions.DragDelayMS,
                ["Debug"] = Options.CurrentOptions.Debug,
                ["SysTray"] = Options.CurrentOptions.SysTray,
                ["SlowHandlerThreshold"] = Options.CurrentOptions.SlowHandlerThreshold,
                ["Autologin"] = Options.CurrentOptions.Autologin,
                ["AutologinUsername"] = Options.CurrentOptions.AutologinUsername,
                ["AutologinPassword"] = Crypter.Encrypt( Options.CurrentOptions.AutologinPassword ),
                ["AutologinServerIndex"] = Options.CurrentOptions.AutologinServerIndex,
                ["AutologinCharacterIndex"] = Options.CurrentOptions.AutologinCharacterIndex,
                ["AutologinConnectDelay"] = Options.CurrentOptions.AutologinConnectDelay,
                ["AutologinReconnectDelay"] = Options.CurrentOptions.AutologinReconnectDelay
            };

            JArray filtersArray = new JArray();

            foreach ( FilterEntry filterEntry in Filters )
            {
                JObject filterObj = new JObject
                {
                    { "Name", filterEntry.GetType().ToString() }, { "Enabled", filterEntry.Enabled }
                };

                if ( filterEntry is IConfigurableFilter configurableFilter )
                {
                    JObject options = configurableFilter.Serialize();
                    filterObj.Add( "Options", options );
                }

                filtersArray.Add( filterObj );
            }

            obj.Add( "Filters", filtersArray );

            json?.Add( "General", obj );
        }

        public void Deserialize( JObject json, Options options, bool global = false )
        {
            Options = options;

            // Reset current filters to default value
            foreach ( FilterEntry filterEntry in Filters )
            {
                FilterOptionsAttribute a =
                    (FilterOptionsAttribute) Attribute.GetCustomAttribute( filterEntry.GetType(),
                        typeof( FilterOptionsAttribute ) );

                if ( a == null )
                {
                    continue;
                }

                filterEntry.Enabled = a.DefaultEnabled;

                if ( filterEntry is IConfigurableFilter configurableFilter )
                {
                    configurableFilter.ResetOptions();
                }
            }

            if ( json?["General"] == null )
            {
                return;
            }

            JToken general = json["General"];

            Options.LightLevel = general["LightLevel"]?.ToObject<int>() ?? 100;
            Options.ActionDelay = general["ActionDelay"]?.ToObject<bool>() ?? true;
            Options.ActionDelayMS = general["ActionDelayMS"]?.ToObject<int>() ?? 1000;
            Options.DragDelay = general["DragDelay"]?.ToObject<bool>() ?? false;
            Options.DragDelayMS = general["DragDelayMS"]?.ToObject<int>() ?? 450;
            Options.AlwaysOnTop = general["AlwaysOnTop"]?.ToObject<bool>() ?? false;
            Options.Debug = general["Debug"]?.ToObject<bool>() ?? false;
            Options.SysTray = general["SysTray"]?.ToObject<bool>() ?? false;
            Options.SlowHandlerThreshold = general["SlowHandlerThreshold"]?.ToObject<int>() ?? 250;
            Options.Autologin = general["Autologin"]?.ToObject<bool>() ?? false;
            Options.AutologinUsername = general["AutologinUsername"]?.ToObject<string>();
            Options.AutologinPassword = Crypter.Decrypt( general["AutologinPassword"]?.ToObject<string>() );
            Options.AutologinServerIndex = general["AutologinServerIndex"]?.ToObject<int>() ?? 0;
            Options.AutologinCharacterIndex = general["AutologinCharacterIndex"]?.ToObject<int>() ?? 0;
            Options.AutologinConnectDelay =
                general["AutologinConnectDelay"]?.ToObject<TimeSpan>() ?? TimeSpan.FromSeconds( 5 );
            Options.AutologinReconnectDelay =
                general["AutologinReconnectDelay"]?.ToObject<TimeSpan>() ?? TimeSpan.FromSeconds( 10 );

            if ( general["Filters"] == null )
            {
                return;
            }

            foreach ( JToken token in general["Filters"] )
            {
                string filterName = token["Name"]?.ToObject<string>() ?? string.Empty;
                bool enabled = token["Enabled"]?.ToObject<bool>() ?? false;

                FilterEntry filter = Filters.FirstOrDefault( f => f.GetType().ToString().Equals( filterName ) );

                if ( filter != null )
                {
                    filter.Enabled = enabled;
                }

                if ( filter is IConfigurableFilter configurableFilter && token["Options"] != null )
                {
                    configurableFilter.Deserialize( token["Options"] );
                }
            }
        }

        private static void AutologinConfigure( object obj )
        {
            AutologinConfigureWindow window = new AutologinConfigureWindow();

            window.ShowDialog();
        }

        private static void BackupSettings( object obj )
        {
            if ( Engine.StartupDispatcher != null )
            {
                Engine.StartupDispatcher.Invoke( () =>
                {
                    BackupSettingsWindow window = new BackupSettingsWindow();
                    window.ShowDialog();
                } );
            }
            else
            {
                BackupSettingsWindow window = new BackupSettingsWindow();
                window.ShowDialog();
            }
        }

        private void OnSavedPasswordsChangedEvent( object sender, EventArgs e )
        {
            Dictionary<string, string> newList =
                AssistantOptions.SavedPasswords.ToDictionary( kvp => kvp.Key, kvp => kvp.Value );

            SavedPasswords = newList;
        }

        private void RemoveSavedPassword( object obj )
        {
            if ( !( obj is KeyValuePair<string, string> kvp ) )
            {
                return;
            }

            SavedPasswords.Remove( kvp.Key );
            AssistantOptions.SavedPasswords.Remove( kvp.Key );
            AssistantOptions.OnPasswordsChanged();
        }

        private static void ConfigureFilter( object obj )
        {
            if ( !( obj is IConfigurableFilter configurableFilter ) )
            {
                return;
            }

            configurableFilter.Configure();
        }

        private void OnProfileChangedEvent( string profile )
        {
            _dispatcher.Invoke( () =>
            {
                Options = Options.CurrentOptions;

                if ( Engine.Player != null )
                {
                    IsLinkedProfile = AssistantOptions.GetLinkedProfile( Engine.Player.Serial ) == profile;
                }

                SelectedProfile = profile;
            } );
        }

        private static void SaveProfile( object obj )
        {
            Options.Save( Options.CurrentOptions );
            Commands.SystemMessage( Strings.Profile_saved___ );
        }

        private void RefreshProfiles()
        {
            string[] profiles = Options.GetProfiles();

            if ( profiles == null )
            {
                return;
            }

            Profiles.Clear();

            foreach ( string profile in profiles )
            {
                Profiles.Add( Path.GetFileName( profile ) );
            }
        }

        private void ChangeProfile( object obj )
        {
            if ( !( obj is string profileName ) )
            {
                return;
            }

            MacroManager.GetInstance().StopAll();
            LoadProfile( profileName );
            Engine.UpdateWindowTitle();
        }

        private void LinkUnlinkProfile( object obj )
        {
            if ( Engine.Player == null )
            {
                return;
            }

            if ( AssistantOptions.GetLinkedProfile( Engine.Player.Serial ) == Options.CurrentOptions.Name )
            {
                AssistantOptions.RemoveLinkedProfile( Engine.Player.Serial );
                IsLinkedProfile = false;
            }
            else
            {
                AssistantOptions.SetLinkedProfile( Engine.Player.Serial, Options.CurrentOptions.Name );
                IsLinkedProfile = true;
            }
        }

        private void LoadProfile( string profile )
        {
            foreach ( FilterEntry filterEntry in Filters )
            {
                filterEntry?.Action( false );
            }

            Options.ClearOptions();
            Options.CurrentOptions = new Options();
            Options.Load( profile, Options.CurrentOptions );
        }

        private async Task NewProfile( object arg )
        {
            NewProfileWindow window = new NewProfileWindow();

            window.ShowDialog();

            if ( window.DataContext is NewProfileViewModel vm && !string.IsNullOrEmpty( vm.FileName ) )
            {
                RefreshProfiles();

                SelectedProfile = vm.FileName;

                ChangeProfile( vm.FileName );
            }

            await Task.CompletedTask;
        }
    }
}