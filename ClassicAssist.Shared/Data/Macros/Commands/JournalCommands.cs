using System;
using System.Linq;
using System.Threading;
using ClassicAssist.Shared;
using ClassicAssist.Resources;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class JournalCommands
    {
        [CommandsDisplay( Category = nameof( Strings.Journal ) )]
        public static bool InJournal( string text, string author = "", int hue = -1 )
        {
            bool match;

            if ( author.ToLower().Equals( "system" ) )
            {
                match = Engine.Journal.GetBuffer().Any( je =>
                    je.Text.ToLower().Contains( text.ToLower() ) &&
                    ( je.SpeechType == JournalSpeech.System || je.Name == "System" ) &&
                    ( hue == -1 || je.SpeechHue == hue ) );
            }
            else
            {
                match = Engine.Journal.GetBuffer().Any( je =>
                    je.Text.ToLower().Contains( text.ToLower() ) &&
                    ( string.IsNullOrEmpty( author ) || string.Equals( je.Name, author,
                          StringComparison.CurrentCultureIgnoreCase ) ) && ( hue == -1 || je.SpeechHue == hue ) );
            }

            return match;
        }

        [CommandsDisplay( Category = nameof( Strings.Journal ) )]
        public static void ClearJournal()
        {
            Engine.Journal.Clear();
        }

        [CommandsDisplay( Category = nameof( Strings.Journal ) )]
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