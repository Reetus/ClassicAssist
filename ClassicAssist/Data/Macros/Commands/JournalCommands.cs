using System;
using System.Linq;
using System.Threading;
using Assistant;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class JournalCommands
    {
        [CommandsDisplay( Category = "Journal", Description = "Check for a text in journal, optional source name.",
            InsertText = "if InJournal(\"town guards\", \"system\"):" )]
        public static bool InJournal( string text, string author = "" )
        {
            bool match;

            if ( author.ToLower().Equals( "system" ) )
            {
                match = Engine.Journal.GetBuffer().Any( je =>
                    je.Text.ToLower().Contains( text.ToLower() ) &&
                    ( je.SpeechType == JournalSpeech.System || je.Name == "System" ) );
            }
            else
            {
                match = Engine.Journal.GetBuffer()
                    .Any( je => je.Text.ToLower().Contains( text.ToLower() ) &&
                                ( string.IsNullOrEmpty( author ) || string.Equals( je.Name, author,
                                      StringComparison.CurrentCultureIgnoreCase ) ) );
            }

            return match;
        }

        [CommandsDisplay( Category = "Journal", Description = "Clear all journal texts.",
            InsertText = "ClearJournal()" )]
        public static void ClearJournal()
        {
            Engine.Journal.Clear();
        }

        [CommandsDisplay( Category = "Journal", Description = "Wait the given timeout for the journal text to appear.",
            InsertText = "if WaitForJournal(\"town guards\", 5000, \"system\"):" )]
        public static bool WaitForJournal( string text, int timeout, string author = "" )
        {
            AutoResetEvent are = new AutoResetEvent( false );

            void OnIncomingPacketHandlersOnJournalEntryAddedEvent( JournalEntry je )
            {
                bool match;

                if ( author.ToLower().Equals( "system" ) )
                {
                    match = je.Text.ToLower().Contains( text.ToLower() ) &&
                            ( je.SpeechType == JournalSpeech.System || je.Name == "System" );
                }
                else
                {
                    match = je.Text.ToLower().Contains( text.ToLower() ) &&
                            ( string.IsNullOrEmpty( author ) || string.Equals( je.Name, author,
                                  StringComparison.CurrentCultureIgnoreCase ) );
                }

                if ( match )
                {
                    are.Set();
                }
            }

            IncomingPacketHandlers.JournalEntryAddedEvent += OnIncomingPacketHandlersOnJournalEntryAddedEvent;

            bool result;

            try
            {
                result = are.WaitOne( timeout );
            }
            finally
            {
                IncomingPacketHandlers.JournalEntryAddedEvent -= OnIncomingPacketHandlersOnJournalEntryAddedEvent;
            }

            return result;
        }
    }
}