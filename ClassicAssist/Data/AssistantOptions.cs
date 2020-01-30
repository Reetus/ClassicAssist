using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

        private static readonly Dictionary<int, string> _linkedProfiles = new Dictionary<int, string>();
        public static Language LanguageOverride { get; set; }
        public static string LastProfile { get; set; }
        public static Version UpdateGumpVersion { get; set; }

        public static void Save()
        {
            JObject json = new JObject
            {
                { "LanguageOverride", LanguageOverride.ToString() },
                { "LastProfile", LastProfile },
                { "UpdateGumpVersion", UpdateGumpVersion?.ToString() ?? "0.0.0.0" }
            };

            JArray linkedProfilesArray = new JArray();

            foreach ( JObject linkedObj in _linkedProfiles.Select( profile =>
                new JObject { { "Serial", profile.Key }, { "Profile", profile.Value } } ) )
            {
                linkedProfilesArray.Add( linkedObj );
            }

            json.Add( "Profiles", linkedProfilesArray );

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

            if ( json?["Profiles"] != null )
            {
                foreach ( JToken token in json["Profiles"] )
                {
                    _linkedProfiles.Add( token["Serial"].ToObject<int>(), token["Profile"].ToObject<string>() );
                }
            }

            SetLanguage( LanguageOverride );
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