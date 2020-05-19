using System;
using System.Collections.Generic;
using System.Linq;
using ClassicAssist.UO.Data;

namespace ClassicAssist.Data.Filters
{
    [FilterOptions( Name = "Repeated Messages", DefaultEnabled = false )]
    public class RepeatedMessagesFilter : FilterEntry
    {
        private const int MESSAGE_LIMIT = 5;
        private static readonly TimeSpan RESET_DELAY = TimeSpan.FromSeconds( 5 );

        public static bool IsEnabled { get; set; }

        private static List<RepeatedMessageEntry> RepeatedMessageEntries { get; } = new List<RepeatedMessageEntry>();

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
    }
}