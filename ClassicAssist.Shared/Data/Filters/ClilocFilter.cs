using System.Collections.Generic;
using System.Linq;
using ClassicAssist.Shared;
using ClassicAssist.Resources;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.Packets;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Data.Filters
{
    [FilterOptions( Name = "Cliloc Filter", DefaultEnabled = false )]
    public class ClilocFilter : FilterEntry, IConfigurableFilter
    {
        public static Dictionary<int, string> Filters { get; set; } = new Dictionary<int, string>();

        public static bool IsEnabled { get; set; }

        public void Configure()
        {
            //TODO
            //ClilocFilterConfigureWindow window =
            //    new ClilocFilterConfigureWindow { Topmost = Options.CurrentOptions.AlwaysOnTop };

            //window.ShowDialog();
        }

        public void Deserialize( JToken token )
        {
            if ( token?["Filters"] == null )
            {
                return;
            }

            foreach ( JToken filterToken in token["Filters"] )
            {
                int key = filterToken["Key"].ToObject<int>();
                string val = filterToken["Value"]?.ToObject<string>();

                if ( !Filters.ContainsKey( key ) )
                {
                    Filters.Add( key, val );
                }
            }
        }

        public JObject Serialize()
        {
            JArray itemsArray = new JArray();

            foreach ( KeyValuePair<int, string> kvp in Filters )
            {
                itemsArray.Add( new JObject { { "Key", kvp.Key }, { "Value", kvp.Value } } );
            }

            return new JObject { { "Filters", itemsArray } };
        }

        public void ResetOptions()
        {
            Filters.Clear();
        }

        protected override void OnChanged( bool enabled )
        {
            IsEnabled = enabled;
        }

        public static bool CheckMessage( JournalEntry journalEntry )
        {
            if ( !IsEnabled || journalEntry.Cliloc == 0 )
            {
                return false;
            }

            if ( Filters.All( f => f.Key != journalEntry.Cliloc ) )
            {
                return false;
            }

            KeyValuePair<int, string> match = Filters.FirstOrDefault( f => f.Key == journalEntry.Cliloc );

            Engine.SendPacketToClient( new UnicodeText( journalEntry.Serial, journalEntry.ID, journalEntry.SpeechType,
                journalEntry.SpeechHue, journalEntry.SpeechFont, Strings.UO_LOCALE, journalEntry.Name, match.Value ) );

            return true;
        }

        public static bool CheckMessageAffix( JournalEntry journalEntry, MessageAffixType affixType, string affix )
        {
            if ( !IsEnabled || journalEntry.Cliloc == 0 )
            {
                return false;
            }

            if ( Filters.All( f => f.Key != journalEntry.Cliloc ) )
            {
                return false;
            }

            KeyValuePair<int, string> match = Filters.FirstOrDefault( f => f.Key == journalEntry.Cliloc );

            string text = affixType.HasFlag( MessageAffixType.Prepend )
                ? $"{affix}{match.Value}"
                : $"{match.Value}{affix}";

            Engine.SendPacketToClient( new UnicodeText( journalEntry.Serial, journalEntry.ID, JournalSpeech.Say,
                journalEntry.SpeechHue, journalEntry.SpeechFont, Strings.UO_LOCALE, journalEntry.Name, text ) );

            return true;
        }
    }
}