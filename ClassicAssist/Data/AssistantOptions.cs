using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Assistant;
using ClassicAssist.UI.Misc;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Data
{
    public static class AssistantOptions
    {
        public delegate void dProfileChanged( string profile );

        private const string DEFAULT_BACKUP_PATH = "Backup";

        private static readonly Dictionary<int, string> _linkedProfiles = new Dictionary<int, string>();
        public static string[] Assemblies { get; set; }
        public static bool AutoBackupProfiles { get; set; }
        public static int AutoBackupProfilesDays { get; set; }
        public static string AutoBackupProfilesDirectory { get; set; }
        public static DateTime AutoBackupProfilesLast { get; set; }
        public static Language LanguageOverride { get; set; }
        public static string LastProfile { get; set; }
        public static Dictionary<string, string> SavedPasswords { get; set; } = new Dictionary<string, string>();
        public static bool SavePasswords { get; set; }
        public static bool SavePasswordsOnlyBlank { get; set; }
        public static Version UpdateGumpVersion { get; set; }
        public static string UserId { get; set; }
        public static double WindowHeight { get; set; }

        public static double WindowWidth { get; set; }

        public static event EventHandler SavedPasswordsChanged;
        public static event EventHandler OptionsLoaded;

        public static void Save()
        {
            JObject json = new JObject
            {
                { "LanguageOverride", LanguageOverride.ToString() },
                { "LastProfile", LastProfile },
                { "UpdateGumpVersion", UpdateGumpVersion?.ToString() ?? "0.0.0.0" },
                { "AutoBackupProfiles", AutoBackupProfiles },
                { "AutoBackupProfilesDays", AutoBackupProfilesDays },
                { "AutoBackupProfilesDirectory", AutoBackupProfilesDirectory },
                { "AutoBackupProfilesLast", AutoBackupProfilesLast },
                { "SavePasswords", SavePasswords },
                { "SavePasswordsOnlyBlank", SavePasswordsOnlyBlank },
                { "UserId", UserId },
#if !DEVELOP
                { "WindowWidth", WindowWidth },
                { "WindowHeight", WindowHeight },
#endif
            };

            JArray linkedProfilesArray = new JArray();

            foreach ( JObject linkedObj in _linkedProfiles.Select( profile =>
                new JObject { { "Serial", profile.Key }, { "Profile", profile.Value } } ) )
            {
                linkedProfilesArray.Add( linkedObj );
            }

            json.Add( "Profiles", linkedProfilesArray );

            JArray savedPasswordsArray = new JArray();

            foreach ( KeyValuePair<string, string> kvp in SavedPasswords )
            {
                savedPasswordsArray.Add( new JObject
                {
                    { "Username", kvp.Key }, { "Password", Crypter.Encrypt( kvp.Value ) }
                } );
            }

            json.Add( "SavedPasswords", savedPasswordsArray );

            JArray assembliesArray = new JArray();

            foreach ( string assembly in Assemblies )
            {
                assembliesArray.Add( assembly );
            }

            json.Add( "Assemblies", assembliesArray );

            File.WriteAllText( Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Assistant.json" ),
                json.ToString( Formatting.Indented ) );
        }

        public static void Load()
        {
            Engine.PlayerInitializedEvent += PlayerInitialized;

            string configPath = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Assistant.json" );

            if ( !File.Exists( configPath ) )
            {
                LastProfile = Options.DEFAULT_SETTINGS_FILENAME;
                return;
            }

            JObject json = JObject.Parse( File.ReadAllText( configPath ) );

            LanguageOverride = json?["LanguageOverride"]?.ToObject<Language>() ?? Language.Default;
            LastProfile = json?["LastProfile"]?.ToObject<string>() ?? Options.DEFAULT_SETTINGS_FILENAME;
            UpdateGumpVersion = json?["UpdateGumpVersion"]?.ToObject<Version>() ?? new Version();
            AutoBackupProfiles = json?["AutoBackupProfiles"]?.ToObject<bool>() ?? true;
            AutoBackupProfilesDays = json?["AutoBackupProfilesDays"]?.ToObject<int>() ?? 7;
            AutoBackupProfilesDirectory =
                json?["AutoBackupProfilesDirectory"]?.ToObject<string>() ?? DEFAULT_BACKUP_PATH;
            AutoBackupProfilesLast = json?["AutoBackupProfilesLast"]?.ToObject<DateTime>() ?? default;
            SavePasswords = json?["SavePasswords"]?.ToObject<bool>() ?? false;
            SavePasswordsOnlyBlank = json?["SavePasswordsOnlyBlank"]?.ToObject<bool>() ?? false;
            UserId = json?["UserId"]?.ToObject<string>() ?? Guid.NewGuid().ToString();
            WindowWidth = json?["WindowWidth"]?.ToObject<int>() ?? 625;
            WindowHeight = json?["WindowHeight"]?.ToObject<int>() ?? 500;
            Assemblies = json?["Assemblies"]?.ToObject<string[]>() ?? new string[0];

            if ( json?["Profiles"] != null )
            {
                foreach ( JToken token in json["Profiles"] )
                {
                    _linkedProfiles.Add( token["Serial"].ToObject<int>(), token["Profile"].ToObject<string>() );
                }
            }

            if ( json?["SavedPasswords"] != null )
            {
                foreach ( JToken token in json["SavedPasswords"] )
                {
                    SavedPasswords.Add( token["Username"].ToObject<string>(),
                        Crypter.Decrypt( token["Password"].ToObject<string>() ) );
                }

                OnPasswordsChanged();
            }

            SetLanguage( LanguageOverride );

            if ( DateTime.Now - AutoBackupProfilesLast >= TimeSpan.FromDays( AutoBackupProfilesDays ) )
            {
                BackupProfiles();
            }

            foreach ( string assembly in Assemblies )
            {
                try
                {
                    Assembly.LoadFile( assembly );
                }
                catch ( Exception )
                {
                    // ignored
                }
            }

            OptionsLoaded?.Invoke( null, EventArgs.Empty );
        }

        public static void OnPasswordsChanged()
        {
            SavedPasswordsChanged?.Invoke( null, new PropertyChangedEventArgs( nameof( SavedPasswords ) ) );
        }

        private static void BackupProfiles()
        {
            string profileDirectory = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Profiles" );

            if ( !Directory.Exists( profileDirectory ) )
            {
                return;
            }

            IEnumerable<string> files = Directory.EnumerateFiles( profileDirectory ).ToList();

            if ( !files.Any() )
            {
                return;
            }

            string outputPath = AutoBackupProfilesDirectory;

            if ( string.IsNullOrEmpty( outputPath ) )
            {
                outputPath = DEFAULT_BACKUP_PATH;
            }

            bool rooted = Path.IsPathRooted( AutoBackupProfilesDirectory );

            if ( !rooted )
            {
                outputPath = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory,
                    AutoBackupProfilesDirectory );
            }

            if ( !Directory.Exists( outputPath ) )
            {
                Directory.CreateDirectory( outputPath );
            }

            try
            {
                foreach ( string file in files )
                {
                    string outputFile = Path.Combine( outputPath,
                        Path.GetFileName( file ) ?? throw new InvalidOperationException() );
                    File.Copy( file, outputFile, true );
                }
            }
            catch ( Exception )
            {
                // We tried
            }

            AutoBackupProfilesLast = DateTime.Now;
        }

        public static event dProfileChanged ProfileChangedEvent;

        private static void PlayerInitialized( PlayerMobile player )
        {
            if ( !_linkedProfiles.ContainsKey( player.Serial ) )
            {
                return;
            }

            string profile = _linkedProfiles[player.Serial];

            Engine.Dispatcher.Invoke( () =>
            {
                Options.Save( Options.CurrentOptions );
                Options.CurrentOptions = new Options { Name = profile };
                Options.Load( profile, Options.CurrentOptions );
                ProfileChangedEvent?.Invoke( profile );
            } );
        }

        public static void SetLanguage( Language language )
        {
            CultureInfo locale = CultureInfo.CurrentCulture;

            switch ( language )
            {
                case Language.English:
                    locale = new CultureInfo( "en-US" );
                    break;
                case Language.Korean:
                    locale = new CultureInfo( "ko-KR" );
                    break;
                case Language.Chinese:
                    locale = new CultureInfo( "zh" );
                    break;
                case Language.Italian:
                    locale = new CultureInfo( "it-IT" );
                    break;
                case Language.Polish:
                    locale = new CultureInfo( "pl-PL" );
                    break;
                case Language.Default:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            CultureInfo.DefaultThreadCurrentUICulture = locale;
            CultureInfo.DefaultThreadCurrentUICulture = locale;
            CultureInfo.CurrentCulture = locale;
            CultureInfo.CurrentUICulture = locale;
        }

        public static void SetLinkedProfile( int serial, string profile )
        {
            if ( _linkedProfiles.ContainsKey( serial ) )
            {
                _linkedProfiles.Remove( serial );
            }

            _linkedProfiles.Add( serial, profile );
        }

        public static string GetLinkedProfile( int serial )
        {
            return _linkedProfiles.ContainsKey( serial ) ? _linkedProfiles[serial] : null;
        }

        public static void RemoveLinkedProfile( int serial )
        {
            if ( _linkedProfiles.ContainsKey( serial ) )
            {
                _linkedProfiles.Remove( serial );
            }
        }

        public static void OnWindowLoaded()
        {
            if ( LastProfile == null )
            {
                LastProfile = Options.DEFAULT_SETTINGS_FILENAME;
            }

            Options.Load( LastProfile, Options.CurrentOptions );
            ProfileChangedEvent?.Invoke( LastProfile );
        }
    }
}