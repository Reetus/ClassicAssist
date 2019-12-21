using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Filters;
using ClassicAssist.Misc;
using ClassicAssist.UI.Misc;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.UI.ViewModels
{
    public class GeneralControlViewModel : BaseViewModel, ISettingProvider
    {
        private Options _options;
        private ObservableCollection<string> _profiles = new ObservableCollection<string>();

        public GeneralControlViewModel()
        {
            Type[] filterTypes = { typeof( WeatherFilter ), typeof( SeasonFilter ), typeof( LightLevelFilter ) };

            foreach ( Type type in filterTypes )
            {
                FilterEntry fe = (FilterEntry) Activator.CreateInstance( type );
                Filters.Add( fe );
            }
        }

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

            string[] profiles = Options.GetProfiles();

            if ( profiles == null )
            {
                return;
            }

            foreach ( string profile in profiles )
            {
                Profiles.Add( Path.GetFileName( profile ) );
            }
        }
    }
}