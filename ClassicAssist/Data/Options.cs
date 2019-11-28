using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using ClassicAssist.Annotations;
using ClassicAssist.Misc;
using ClassicAssist.UI.ViewModels;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Data
{
    public class Options : INotifyPropertyChanged
    {
        private const string DEFAULT_SETTINGS_FILENAME = "settings.json";
        private static string _profilePath;
        private bool _actionDelay;
        private int _actionDelayMs;
        private bool _alwaysOnTop;
        private int _lightLevel;
        private string _name;
        private bool _useDeathScreenWhilstHidden;

        public bool ActionDelay
        {
            get => _actionDelay;
            set => SetProperty( ref _actionDelay, value );
        }

        public int ActionDelayMS
        {
            get => _actionDelayMs;
            set => SetProperty( ref _actionDelayMs, value );
        }

        public bool AlwaysOnTop
        {
            get => _alwaysOnTop;
            set => SetProperty( ref _alwaysOnTop, value );
        }

        public static Options CurrentOptions { get; set; } = new Options();

        public int LightLevel
        {
            get => _lightLevel;
            set => SetProperty( ref _lightLevel, value );
        }

        public string Name
        {
            get => _name;
            set => SetProperty( ref _name, value );
        }

        public bool UseDeathScreenWhilstHidden
        {
            get => _useDeathScreenWhilstHidden;
            set => SetProperty( ref _useDeathScreenWhilstHidden, value );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static void Save( string startupPath )
        {
            BaseViewModel[] instances = BaseViewModel.Instances;

            JObject obj = new JObject { { "Name", DEFAULT_SETTINGS_FILENAME } };

            EnsureProfilePath( startupPath );

            foreach ( BaseViewModel instance in instances )
            {
                if ( instance is ISettingProvider settingProvider )
                {
                    settingProvider.Serialize( obj );
                }
            }

            File.WriteAllText( Path.Combine( _profilePath, DEFAULT_SETTINGS_FILENAME ), obj.ToString() );
        }

        private static void EnsureProfilePath( string startupPath )
        {
            _profilePath = Path.Combine( startupPath, "Profiles" );

            if ( !Directory.Exists( _profilePath ) )
            {
                Directory.CreateDirectory( _profilePath );
            }
        }

        public static void Load( string startupPath, Options options )
        {
            BaseViewModel[] instances = BaseViewModel.Instances;

            EnsureProfilePath( startupPath );

            JObject json = new JObject();

            if ( File.Exists( Path.Combine( _profilePath, DEFAULT_SETTINGS_FILENAME ) ) )
            {
                json = JObject.Parse( File.ReadAllText( Path.Combine( _profilePath, DEFAULT_SETTINGS_FILENAME ) ) );
            }

            CurrentOptions.Name = json["Name"]?.ToObject<string>() ?? DEFAULT_SETTINGS_FILENAME;

            foreach ( BaseViewModel instance in instances )
            {
                if ( instance is ISettingProvider settingProvider )
                {
                    settingProvider.Deserialize( json, options );
                }
            }
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

        public static string[] GetProfiles()
        {
            return Directory.EnumerateFiles( _profilePath, "*.json" ).ToArray();
        }
    }
}