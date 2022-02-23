#region License

// Copyright (C) 2021 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Assistant;
using ClassicAssist.UI.Views.Filters;
using ClassicAssist.UO.Data;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Data.Filters
{
    [FilterOptions( Name = "Text Filter", DefaultEnabled = false )]
    public class TextFilter : FilterEntry, IConfigurableFilter
    {
        public static ObservableCollection<TextFilterEntry> Filters { get; set; } =
            new ObservableCollection<TextFilterEntry>();

        public static bool IsEnabled { get; set; }

        public void Configure()
        {
            TextFilterConfigureWindow window = new TextFilterConfigureWindow();

            window.ShowDialog();
        }

        public void Deserialize( JToken token )
        {
            if ( token?["Filters"] == null )
            {
                return;
            }

            foreach ( JToken jToken in token["Filters"] )
            {
                TextFilterEntry filter = new TextFilterEntry
                {
                    Enabled = jToken[nameof( TextFilterEntry.Enabled )]?.ToObject<bool>() ?? false,
                    Match = jToken[nameof( TextFilterEntry.Match )]?.ToObject<string>() ?? string.Empty,
                    MatchType =
                        jToken[nameof( TextFilterEntry.MatchType )]?.ToObject<TextFilterMatchType>() ??
                        TextFilterMatchType.StartsWith,
                    MessageTypes =
                        jToken[nameof( TextFilterEntry.MessageTypes )]?.ToObject<TextFilterMessageType>() ??
                        TextFilterMessageType.All,
                    ExcludeSelf = jToken[nameof( TextFilterEntry.ExcludeSelf )]?.ToObject<bool>() ?? false
                };

                Filters.Add( filter );
            }
        }

        public JObject Serialize()
        {
            JArray itemsArray = new JArray();

            foreach ( TextFilterEntry entry in Filters )
            {
                JObject jObject = new JObject
                {
                    { nameof( TextFilterEntry.Enabled ), entry.Enabled },
                    { nameof( TextFilterEntry.Match ), entry.Match },
                    { nameof( TextFilterEntry.MatchType ), entry.MatchType.ToString() },
                    { nameof( TextFilterEntry.MessageTypes ), entry.MessageTypes.ToString() },
                    { nameof( TextFilterEntry.ExcludeSelf ), entry.ExcludeSelf }
                };

                itemsArray.Add( jObject );
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

        public static bool CheckMessage( JournalEntry journalEntry,
            TextFilterMessageType messageType = TextFilterMessageType.None )
        {
            if ( !IsEnabled )
            {
                return false;
            }

            if ( messageType == TextFilterMessageType.None )
            {
                messageType = FromJournalSpeech( journalEntry.SpeechType );
            }

            return Filters.Any( e =>
            {
                if ( !e.Enabled )
                {
                    return false;
                }

                if ( messageType == TextFilterMessageType.None || !e.MessageTypes.HasFlag( messageType ) )
                {
                    return false;
                }

                if ( e.ExcludeSelf && journalEntry.Serial == Engine.Player.Serial )
                {
                    return false;
                }

                bool found;

                switch ( e.MatchType )
                {
                    case TextFilterMatchType.Equals:
                        found = journalEntry.Text.Equals( e.Match );
                        break;
                    case TextFilterMatchType.StartsWith:
                        found = journalEntry.Text.StartsWith( e.Match );
                        break;
                    case TextFilterMatchType.Contains:
                        found = journalEntry.Text.Contains( e.Match );
                        break;
                    case TextFilterMatchType.EndsWith:
                        found = journalEntry.Text.EndsWith( e.Match );
                        break;
                    case TextFilterMatchType.Regex:
                        found = Regex.IsMatch( journalEntry.Text, e.Match );
                        break;
                    default:
                        found = false;
                        break;
                }

                return found;
            } );
        }

        private static TextFilterMessageType FromJournalSpeech( JournalSpeech journalEntrySpeechType )
        {
            switch ( journalEntrySpeechType )
            {
                case JournalSpeech.Say:
                    return TextFilterMessageType.Say;
                case JournalSpeech.System:
                    return TextFilterMessageType.System;
                case JournalSpeech.Emote:
                    return TextFilterMessageType.Emote;
                case JournalSpeech.Label:
                    return TextFilterMessageType.Label;
                case JournalSpeech.Whisper:
                    return TextFilterMessageType.Whisper;
                case JournalSpeech.Yell:
                    return TextFilterMessageType.Yell;
                case JournalSpeech.Spell:
                    return TextFilterMessageType.Spell;
                case JournalSpeech.Guild:
                    return TextFilterMessageType.Guild;
                case JournalSpeech.Alliance:
                    return TextFilterMessageType.Alliance;
                case JournalSpeech.GM:
                    return TextFilterMessageType.GM;
                case JournalSpeech.Focus:
                case JournalSpeech.Unknown1:
                case JournalSpeech.Unknown2:
                case JournalSpeech.Unknown3:
                case JournalSpeech.Unknown4:
                case JournalSpeech.Unknown5:
                default:
                    return TextFilterMessageType.None;
            }
        }
    }
}