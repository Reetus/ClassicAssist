using System;
using System.Collections.Generic;
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

        public bool ActionDelay
        {
            get => Options.CurrentOptions.ActionDelay;
            set => Options.CurrentOptions.ActionDelay = value;
        }

        public int ActionDelayMS
        {
            get => Options.CurrentOptions.ActionDelayMS;
            set => Options.CurrentOptions.ActionDelayMS = value;
        }

        public bool AlwaysOnTop
        {
            get => Options.CurrentOptions.AlwaysOnTop;
            set => Options.CurrentOptions.AlwaysOnTop = value;
        }

        public ObservableCollectionEx<FilterEntry> Filters { get; set; } = new ObservableCollectionEx<FilterEntry>();

        public ObservableCollection<string> Profiles
        {
            get => _profiles;
            set => SetProperty(ref _profiles, value);
        }

        public int LightLevel
        {
            get => Options.CurrentOptions.LightLevel;
            set => Options.CurrentOptions.LightLevel = value;
        }

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

        public bool UseDeathScreenWhilstHidden
        {
            get => Options.CurrentOptions.UseDeathScreenWhilstHidden;
            set => Options.CurrentOptions.UseDeathScreenWhilstHidden = value;
        }

        public void Serialize( JObject json )
        {
            JObject obj = new JObject
            {
                ["AlwaysOnTop"] = Options.CurrentOptions.AlwaysOnTop,
                ["LightLevel"] = Options.CurrentOptions.LightLevel,
                ["ActionDelay"] = Options.CurrentOptions.ActionDelay,
                ["ActionDelayMS"] = Options.CurrentOptions.ActionDelayMS,
                ["UseDeathScreenWhilstHidden"] = Options.CurrentOptions.UseDeathScreenWhilstHidden
            };

            json.Add( "General", obj );
        }

        public void Deserialize( JObject json, Options options )
        {
            if ( json["General"] == null )
            {
                return;
            }

            JToken general = json["General"];

            SetOptionsNotify( nameof( LightLevel ), general["LightLevel"]?.ToObject<int>(), 100 );
            SetOptionsNotify( nameof( ActionDelay ), general["ActionDelay"]?.ToObject<bool>(), false );
            SetOptionsNotify( nameof( ActionDelayMS ), general["ActionDelayMS"]?.ToObject<int>(), 900 );
            SetOptionsNotify( nameof( UseDeathScreenWhilstHidden ),
                general["UseDeathScreenWhilstHidden"]?.ToObject<bool>(), false );
            bool topmost = general["AlwaysOnTop"]?.ToObject<bool>() ?? false;
            SetOptionsNotify( nameof( AlwaysOnTop ), topmost, false );

            string[] profiles = Options.GetProfiles();

            if (profiles == null)
                return;

            foreach (string profile in profiles)
            {
                Profiles.Add(Path.GetFileName(profile));
            }
        }
    }
}