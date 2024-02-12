using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Assistant;
using ClassicAssist.Data.Backup;
using ClassicAssist.UI.Misc;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UI.Views;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Data
{
    public static class AssistantOptions
    {
        public delegate void dProfileChanged( string profile );

        private static readonly Dictionary<int, string> _linkedProfiles = new Dictionary<int, string>();
        public static string[] Assemblies { get; set; }
        public static BackupOptions BackupOptions { get; set; }
        public static JObject DebugWindowOptions { get; set; }
        public static string GlobalDirectory { get; set; } = ".";
        public static Language LanguageOverride { get; set; }
        public static string LastProfile { get; set; }
        public static string ProfileDirectory { get; set; } = "Profiles";
        public static Dictionary<string, string> SavedPasswords { get; set; } = new Dictionary<string, string>();
        public static bool SavePasswords { get; set; }
        public static bool SavePasswordsOnlyBlank { get; set; }
        public static string SessionId { get; set; }
        public static string UpdateGumpVersion { get; set; }
        public static bool UseCUOClilocLanguage { get; set; }
        public static string UserId { get; set; }
        public static double WindowHeight { get; set; }

        public static double WindowWidth { get; set; }

        public static event EventHandler SavedPasswordsChanged;
        public static event EventHandler OptionsLoaded;

        public static string GetProfilePath()
        {
            string path = Path.IsPathRooted( ProfileDirectory ) ? ProfileDirectory : Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, ProfileDirectory );

            if ( !Directory.Exists( path ) )
            {
                Directory.CreateDirectory( path );
            }

            return path;
        }

        public static string GetGlobalPath()
        {
            string path = Path.IsPathRooted( GlobalDirectory ) ? GlobalDirectory : Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, GlobalDirectory );

            if ( !Directory.Exists( path ) )
            {
                Directory.CreateDirectory( path );
            }

            return path;
        }

        public static void Save()
        {
            if ( BackupOptions != null && BackupOptions.Enabled &&
                 DateTime.Now - BackupOptions.LastBackup >= TimeSpan.FromDays( BackupOptions.Days ) )
            {
                BackupProfiles();
            }

            JObject json = new JObject
            {
                { "LanguageOverride", LanguageOverride.ToString() },
                { "LastProfile", LastProfile },
                { "UpdateGumpVersion", UpdateGumpVersion ?? "0.0.0.0" },
                { "SavePasswords", SavePasswords },
                { "SavePasswordsOnlyBlank", SavePasswordsOnlyBlank },
                { "UserId", UserId },
                { "UseCUOClilocLanguage", UseCUOClilocLanguage },
                { "ProfileDirectory", ProfileDirectory },
                { "GlobalDirectory", GlobalDirectory },
                { "DebugWindowOptions", DebugWindowOptions },
#if !DEVELOP
                { "WindowWidth", WindowWidth },
                { "WindowHeight", WindowHeight },
#endif
            };

            BackupOptions?.Serialize( json );

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

            foreach ( string assembly in Assemblies ?? Array.Empty<string>() )
            {
                assembliesArray.Add( assembly );
            }

            json.Add( "Assemblies", assembliesArray );

            File.WriteAllText( Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Assistant.json" ),
                json.ToString( Formatting.Indented ) );
        }

        private static bool IsTesting()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Any( assem => assem.FullName.ToLowerInvariant().Contains( "testplatform" ) );
        }

        public static void Load()
        {
            Engine.PlayerInitializedEvent += PlayerInitialized;

            string configPath = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Assistant.json" );

            if ( !File.Exists( configPath ) )
            {
                LastProfile = Options.DEFAULT_SETTINGS_FILENAME;
                UserId = Guid.NewGuid().ToString();
                SessionId = Guid.NewGuid().ToString();
                BackupOptions = new BackupOptions();
                return;
            }

            JObject json = JObject.Parse( File.ReadAllText( configPath ) );

            LanguageOverride = json["LanguageOverride"]?.ToObject<Language>() ?? Language.Default;
            LastProfile = json["LastProfile"]?.ToObject<string>() ?? Options.DEFAULT_SETTINGS_FILENAME;
            UpdateGumpVersion = json["UpdateGumpVersion"]?.ToObject<string>() ?? new Version().ToString();

            if ( json["Backup"] == null )
            {
                BackupOptions = new BackupOptions
                {
                    Enabled = !IsTesting() /*json["AutoBackupProfiles"]?.ToObject<bool>() ?? true*/,
                    Days = json["AutoBackupProfilesDays"]?.ToObject<int>() ?? 7,
                    LastBackup = json["AutoBackupProfilesLast"]?.ToObject<DateTime>() ?? default,
                    Provider = new LocalBackupProvider
                    {
                        BackupPath = json["AutoBackupProfilesDirectory"]?.ToObject<string>() ??
                                     BackupOptions.DefaultBackupPath
                    }
                };
            }
            else
            {
                BackupOptions = new BackupOptions();
                BackupOptions.Deserialize( json, Options.CurrentOptions );
            }

            SavePasswords = json["SavePasswords"]?.ToObject<bool>() ?? false;
            SavePasswordsOnlyBlank = json["SavePasswordsOnlyBlank"]?.ToObject<bool>() ?? false;
            UserId = json["UserId"]?.ToObject<string>() ?? Guid.NewGuid().ToString();
            WindowWidth = json["WindowWidth"]?.ToObject<int>() ?? 700;
            WindowHeight = json["WindowHeight"]?.ToObject<int>() ?? 570;
            UseCUOClilocLanguage = json["UseCUOClilocLanguage"]?.ToObject<bool>() ?? false;
            Assemblies = json["Assemblies"]?.ToObject<string[]>() ?? Array.Empty<string>();
            ProfileDirectory = json["ProfileDirectory"]?.ToObject<string>() ?? "Profiles";
            GlobalDirectory = json["GlobalDirectory"]?.ToObject<string>() ?? ".";
            SessionId = Guid.NewGuid().ToString();
            DebugWindowOptions = json["DebugWindowOptions"]?.ToObject<JObject>() ?? new JObject();

            if ( json["Profiles"] != null )
            {
                foreach ( JToken token in json["Profiles"] )
                {
                    _linkedProfiles.Add( token["Serial"]?.ToObject<int>() ?? 0, token["Profile"]?.ToObject<string>() );
                }
            }

            if ( json["SavedPasswords"] != null )
            {
                foreach ( JToken token in json["SavedPasswords"] )
                {
                    SavedPasswords.Add( token["Username"]?.ToObject<string>() ?? string.Empty,
                        Crypter.Decrypt( token["Password"]?.ToObject<string>() ) );
                }

                OnPasswordsChanged();
            }

            SetLanguage( LanguageOverride );

            foreach ( string assembly in Assemblies )
            {
                try
                {
                    Assembly asm = Assembly.LoadFile( assembly );

                    IEnumerable<MethodInfo> initializeMethods = asm.GetTypes()
                        .Where( e => e.IsClass && e.IsPublic && e.GetMethod( "Initialize",
                            BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null ) != null )
                        .Select( e => e.GetMethod( "Initialize", BindingFlags.Public | BindingFlags.Static, null,
                            Type.EmptyTypes, null ) );

                    foreach ( MethodInfo initializeMethod in initializeMethods )
                    {
                        initializeMethod?.Invoke( null, null );
                    }
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
            Engine.Dispatcher.Invoke( () =>
            {
                BackupWindowViewModel vm = new BackupWindowViewModel( BackupOptions );
                BackupWindow backupWindow = new BackupWindow { DataContext = vm };
                vm.CloseWindow = () => backupWindow.Close();
                backupWindow.ShowDialog();
            } );
        }

        public static event dProfileChanged ProfileChangingEvent;
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
                case Language.Czech:
                    locale = new CultureInfo( "cs-CZ" );
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

            Engine.Dispatcher.Invoke( () =>
            {
                Options.Load( LastProfile, Options.CurrentOptions );
            } );
        }

        public static void OnProfileChanging( string profile )
        {
            ProfileChangingEvent?.Invoke( profile );
        }

        public static void OnProfileChanged( string profile )
        {
            LastProfile = profile;
            ProfileChangedEvent?.Invoke( profile );
        }
    }
}