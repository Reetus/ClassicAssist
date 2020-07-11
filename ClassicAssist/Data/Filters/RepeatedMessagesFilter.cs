using System;
using System.Collections.Generic;
using System.Linq;
using ClassicAssist.UI.Views.Filters;
using ClassicAssist.UO.Data;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Data.Filters
{
    [FilterOptions( Name = "Repeated Messages", DefaultEnabled = false )]
    public class RepeatedMessagesFilter : FilterEntry, IConfigurableFilter
    {
        public static MessageFilterOptions FilterOptions { get; set; } = new MessageFilterOptions();

        public static bool IsEnabled { get; set; }
        private static List<RepeatedMessageEntry> RepeatedMessageEntries { get; } = new List<RepeatedMessageEntry>();

        public void Configure()
        {
            RepeatedMessagesFilterConfigureWindow window = new RepeatedMessagesFilterConfigureWindow( FilterOptions );
            window.ShowDialog();
        }

        public void Deserialize( JToken token )
        {
            if ( token == null )
            {
                return;
            }

            try
            {
                FilterOptions = new MessageFilterOptions
                {
                    SendToJournal = token["SendToJournal"]?.ToObject<bool>() ?? false,
                    MessageLimit = token["MessageLimit"]?.ToObject<int>() ?? 5,
                    TimeLimit = token["TimeLimit"]?.ToObject<int>() ?? 5,
                    BlockedTime = token["BlockedTime"]?.ToObject<int>() ?? 5
                };
            }
            catch ( Exception )
            {
                FilterOptions = new MessageFilterOptions();
            }
        }

        public JObject Serialize()
        {
            return new JObject
            {
                { "SendToJournal", FilterOptions.SendToJournal },
                { "MessageLimit", FilterOptions.MessageLimit },
                { "TimeLimit", FilterOptions.TimeLimit },
                { "BlockedTime", FilterOptions.BlockedTime }
            };
        }

        public void ResetOptions()
        {
            FilterOptions = new MessageFilterOptions();
        }

        protected override void OnChanged( bool enabled )
        {
            IsEnabled = enabled;
        }

        public static bool CheckMessage( JournalEntry journalEntry )
        {
            if ( !IsEnabled )
            {
                return false;
            }

            if ( journalEntry.SpeechType != JournalSpeech.System &&
                 ( journalEntry.Name != "System" || journalEntry.Serial != -1 ) )
            {
                return false;
            }

            if ( FilterOptions.MessageLimit == 0 )
            {
                return true;
            }

            RepeatedMessageEntry entry = RepeatedMessageEntries.FirstOrDefault( e => e.Message == journalEntry.Text );

            if ( entry != null && entry.Blocked && entry.Expires < DateTime.Now )
            {
                RepeatedMessageEntries.Remove( entry );
            }

            if ( entry != null && entry.Blocked && entry.Expires > DateTime.Now )
            {
                return true;
            }

            if ( RepeatedMessageEntries.All( e => e.Message != journalEntry.Text ) )
            {
                RepeatedMessageEntries.Add( new RepeatedMessageEntry
                {
                    FirstReceived = DateTime.Now,
                    LastReceived = DateTime.Now,
                    Count = 1,
                    Message = journalEntry.Text
                } );

                return false;
            }

            entry = RepeatedMessageEntries.FirstOrDefault( e => e.Message == journalEntry.Text );

            if ( entry == null )
            {
                return false;
            }

            if ( entry.LastReceived < DateTime.Now - TimeSpan.FromSeconds( FilterOptions.TimeLimit ) )
            {
                RepeatedMessageEntries.Remove( entry );
                return false;
            }

            if ( entry.Count < FilterOptions.MessageLimit )
            {
                entry.Count++;
                entry.LastReceived = DateTime.Now;

                return false;
            }

            if ( Options.CurrentOptions.Debug )
            {
                UO.Commands.SystemMessage( $"Filtering message: {journalEntry.Text}" );
            }

            entry.Blocked = true;
            entry.Expires = DateTime.Now + TimeSpan.FromSeconds( FilterOptions.BlockedTime );

            return true;
        }

        internal class RepeatedMessageEntry
        {
            public bool Blocked { get; set; }
            public int Count { get; set; }
            public DateTime Expires { get; set; } = DateTime.Now;
            public DateTime FirstReceived { get; set; }
            public DateTime LastReceived { get; set; }
            public string Message { get; set; }
        }

        public class MessageFilterOptions
        {
            public int BlockedTime { get; set; } = 5;
            public int MessageLimit { get; set; } = 5;
            public bool SendToJournal { get; set; }
            public int TimeLimit { get; set; } = 5;
        }
    }
}