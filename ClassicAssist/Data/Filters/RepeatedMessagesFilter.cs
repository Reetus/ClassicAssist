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
        private const int MESSAGE_LIMIT = 5;
        private static readonly TimeSpan RESET_DELAY = TimeSpan.FromSeconds( 5 );
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

            FilterOptions =
                new MessageFilterOptions { SendToJournal = token["SendToJournal"]?.ToObject<bool>() ?? false };
        }

        public JObject Serialize()
        {
            return new JObject { { "SendToJournal", FilterOptions.SendToJournal } };
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

            RepeatedMessageEntry entry = RepeatedMessageEntries.FirstOrDefault( e => e.Message == journalEntry.Text );

            if ( entry == null )
            {
                return false;
            }

            if ( entry.LastReceived < DateTime.Now - RESET_DELAY )
            {
                RepeatedMessageEntries.Remove( entry );
                return false;
            }

            if ( entry.Count < MESSAGE_LIMIT )
            {
                entry.Count++;
                entry.LastReceived = DateTime.Now;

                return false;
            }

            if ( Options.CurrentOptions.Debug )
            {
                UO.Commands.SystemMessage( $"Filtering message: {journalEntry.Text}" );
            }

            return true;
        }

        internal class RepeatedMessageEntry
        {
            public int Count { get; set; }
            public DateTime FirstReceived { get; set; }
            public DateTime LastReceived { get; set; }
            public string Message { get; set; }
        }

        public class MessageFilterOptions
        {
            public bool SendToJournal { get; set; }
            //TODO Configurable Limit/Cooldown
        }
    }
}