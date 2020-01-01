using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Filters;
using ClassicAssist.Misc;
using ClassicAssist.UI.Misc;
using ClassicAssist.UI.Views;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.UI.ViewModels
{
    public class GeneralControlViewModel : BaseViewModel, ISettingProvider
    {
        private static ICommand _saveProfileCommand;
        private ICommand _changeProfileCommand;
        private ICommand _newProfileCommand;
        private Options _options;
        private ObservableCollection<string> _profiles = new ObservableCollection<string>();
        private string _selectedProfile = Options.CurrentOptions.Name;

        public ICommand SaveProfileCommand =
            _saveProfileCommand ?? ( _saveProfileCommand = new RelayCommand( SaveProfile, o => true ) );

        public GeneralControlViewModel()
        {
            Type[] filterTypes = { typeof( WeatherFilter ), typeof( SeasonFilter ), typeof( LightLevelFilter ) };

            foreach ( Type type in filterTypes )
            {
                FilterEntry fe = (FilterEntry) Activator.CreateInstance( type );
                Filters.Add( fe );
            }

            RefreshProfiles();
        }

        public ICommand ChangeProfileCommand =>
            _changeProfileCommand ?? ( _changeProfileCommand = new RelayCommand( ChangeProfile, o => true ) );

        public ObservableCollectionEx<FilterEntry> Filters { get; set; } = new ObservableCollectionEx<FilterEntry>();

        [OptionsBinding( Property = "LightLevel" )]
        public int LightLevelListen
        {
            get => throw new NotImplementedException();
            set
            {
                FilterEntry filter = Filters.FirstOrDefault( f => f is LightLevelFilter );

                if ( filter == null || !filter.Enabled )
                {
                    return;
                }

                byte[] packet = { 0x4F, (byte) value };

                Engine.SendPacketToClient( packet, packet.Length );
            }
        }

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

        public string SelectedProfile
        {
            get => _selectedProfile;
            set => SetProperty( ref _selectedProfile, value );
        }

        public void Serialize( JObject json )
        {
            JObject obj = new JObject
            {
                ["AlwaysOnTop"] = Options.CurrentOptions.AlwaysOnTop,
                ["LightLevel"] = Options.CurrentOptions.LightLevel,
                ["ActionDelay"] = Options.CurrentOptions.ActionDelay,
                ["ActionDelayMS"] = Options.CurrentOptions.ActionDelayMS,
                ["UpdateGumpVersion"] = Options.CurrentOptions.UpdateGumpVersion?.ToString() ?? "0.0.0.0",
                ["Debug"] = Options.CurrentOptions.Debug
            };

            json?.Add( "General", obj );
        }

        public void Deserialize( JObject json, Options options )
        {
            Options = options;

            if ( json?["General"] == null )
            {
                return;
            }

            JToken general = json["General"];

            Options.LightLevel = general["LightLevel"]?.ToObject<int>() ?? 100;
            Options.ActionDelay = general["ActionDelay"]?.ToObject<bool>() ?? false;
            Options.ActionDelayMS = general["ActionDelayMS"]?.ToObject<int>() ?? 900;
            Options.AlwaysOnTop = general["AlwaysOnTop"]?.ToObject<bool>() ?? false;
            Options.UpdateGumpVersion = general["UpdateGumpVersion"]?.ToObject<Version>() ?? new Version();
            Options.Debug = general["Debug"]?.ToObject<bool>() ?? false;
        }

        private static void SaveProfile( object obj )
        {
            Options.Save( Options.CurrentOptions );
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

        private static void ChangeProfile( object obj )
        {
            if ( !( obj is string profileName ) )
            {
                return;
            }

            LoadProfile( profileName );
        }

        private static void LoadProfile( string profile )
        {
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
            }

            await Task.CompletedTask;
        }
    }
}