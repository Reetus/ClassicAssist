using System.Collections.ObjectModel;
using System.Linq;
using Assistant;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UI.ViewModels.Filters;
using ClassicAssist.UI.Views.Filters;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.Packets;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Data.Filters
{
    [FilterOptions( Name = "Cliloc Filter", DefaultEnabled = false )]
    public class ClilocFilter : FilterEntry, IConfigurableFilter
    {
        public static ObservableCollection<FilterClilocEntry> Filters { get; set; } = new ObservableCollection<FilterClilocEntry>();

        public static bool IsEnabled { get; set; }

        public void Configure()
        {
            ClilocFilterConfigureWindow window = new ClilocFilterConfigureWindow { Topmost = Options.CurrentOptions.AlwaysOnTop };

            window.ShowDialog();
        }

        public void Deserialize( JToken token )
        {
            if ( token?["Filters"] == null )
            {
                return;
            }

            foreach ( JToken filterToken in token["Filters"] )
            {
                FilterClilocEntry entry = new FilterClilocEntry
                {
                    Cliloc = filterToken["Key"]?.ToObject<int>() ?? -1,
                    Replacement = filterToken["Value"]?.ToObject<string>(),
                    Hue = filterToken["Hue"]?.ToObject<int>() ?? -1,
                    ShowOverhead = filterToken["ShowOverhead"]?.ToObject<bool>() ?? false
                };

                if ( Filters.All( e => e.Cliloc != entry.Cliloc ) )
                {
                    Filters.Add( entry );
                }
            }
        }

        public JObject Serialize()
        {
            JArray itemsArray = new JArray();

            foreach ( FilterClilocEntry filter in Filters )
            {
                itemsArray.Add( new JObject { { "Key", filter.Cliloc }, { "Value", filter.Replacement }, { "Hue", filter.Hue }, { "ShowOverhead", filter.ShowOverhead } } );
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

            if ( Filters.All( f => f.Cliloc != journalEntry.Cliloc ) )
            {
                return false;
            }

            FilterClilocEntry match = Filters.FirstOrDefault( f => f.Cliloc == journalEntry.Cliloc );

            if ( match != null )
            {
                int serial = journalEntry.Serial;
                int id = journalEntry.ID;

                if ( serial == -1 && match.ShowOverhead )
                {
                    serial = Engine.Player.Serial;
                    id = Engine.Player.ID;
                }

                Engine.SendPacketToClient( new UnicodeText(serial, id, journalEntry.SpeechType, match.Hue == -1 ? journalEntry.SpeechHue : match.Hue,
                    journalEntry.SpeechFont, Strings.UO_LOCALE, journalEntry.Name, match.Replacement ) );

                return true;
            }

            return false;
        }

        public static bool CheckMessageAffix( JournalEntry journalEntry, MessageAffixType affixType, string affix )
        {
            if ( !IsEnabled || journalEntry.Cliloc == 0 )
            {
                return false;
            }

            if ( Filters.All( f => f.Cliloc != journalEntry.Cliloc ) )
            {
                return false;
            }

            FilterClilocEntry match = Filters.FirstOrDefault( f => f.Cliloc == journalEntry.Cliloc );

            if ( match == null )
            {
                return false;
            }

            string text = affixType.HasFlag( MessageAffixType.Prepend ) ? $"{affix}{match.Replacement}" : $"{match.Replacement}{affix}";

            Engine.SendPacketToClient( new UnicodeText( journalEntry.Serial, journalEntry.ID, JournalSpeech.Say, match.Hue == -1 ? journalEntry.SpeechHue : match.Hue,
                journalEntry.SpeechFont, Strings.UO_LOCALE, journalEntry.Name, text ) );

            return true;
        }
    }
}