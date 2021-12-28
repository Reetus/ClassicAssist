using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Assistant;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class JournalCommands
    {
        [CommandsDisplay( Category = nameof( Strings.Journal ) )]
        public static bool InJournal( string text, string author = "", int hue = -1, int timeout = -1 )
        {
            bool match;

            if ( timeout != -1 )
            {
                WaitForJournal( text, timeout, author, hue );
            }

            if ( author.ToLower().Equals( "system" ) )
            {
                match = Engine.Journal.GetBuffer().Any( je => je.Text.ToLower().Contains( text.ToLower() ) &&
                                                            ( je.SpeechType == JournalSpeech.System || je.Name == "System" ) &&
                                                            ( hue == -1 || je.SpeechHue == hue ) );
            }
            else
            {
                if ( !string.IsNullOrEmpty( author ) )
                {
                    int serial = AliasCommands.ResolveSerial( author );

                    if ( serial > 0 )
                    {
                        Entity entity = Engine.Items.GetItem( serial ) ?? (Entity) Engine.Mobiles.GetMobile( serial );
                        author = entity?.Name ?? author;
                    }
                }

                match = Engine.Journal.GetBuffer().Any( je => je.Text.ToLower().Contains( text.ToLower() ) &&
                                                            ( string.IsNullOrEmpty( author ) || je.Name.Equals( author.Trim(), StringComparison.CurrentCultureIgnoreCase ) ) &&
                                                            ( hue == -1 || je.SpeechHue == hue ) );
            }

            return match;
        }

        [CommandsDisplay( Category = nameof( Strings.Journal ) )]
        public static void ClearJournal()
        {
            Engine.Journal.Clear();
        }

        [CommandsDisplay( Category = nameof( Strings.Journal ) )]
        public static bool WaitForJournal( string text, int timeout, string author = "", int hue = -1 )
        {
            AutoResetEvent are = new AutoResetEvent( false );

            void OnIncomingPacketHandlersOnJournalEntryAddedEvent( JournalEntry je )
            {
                bool match;

                if ( author.ToLower().Equals( "system" ) )
                {
                    match = je.Text.ToLower().Contains( text.ToLower() ) &&
                          ( je.SpeechType == JournalSpeech.System || je.Name == "System" ) &&
                          ( hue == -1 || je.SpeechHue == hue );
                }
                else
                {
                    if ( !string.IsNullOrEmpty( author ) )
                    {
                        int serial = AliasCommands.ResolveSerial( author );

                        if ( serial > 0 )
                        {
                            Entity entity = Engine.Items.GetItem( serial ) ?? (Entity) Engine.Mobiles.GetMobile( serial );
                            author = entity?.Name ?? author;
                        }
                    }

                    match = je.Text.ToLower().Contains( text.ToLower() ) &&
                          ( string.IsNullOrEmpty( author ) || je.Name.Equals( author.Trim(), StringComparison.CurrentCultureIgnoreCase ) ) &&
                          ( hue == -1 || je.SpeechHue == hue );
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

        // ReSharper disable once ExplicitCallerInfoArgument
        [CommandsDisplay( "WaitForJournalArray", Category = nameof( Strings.Journal ) )]
        public static (int?, string) WaitForJournal( IEnumerable<string> entries, int timeout, string author = "" )
        {
            int? index = null;
            string text = null;

            string[] wanted = entries.ToArray();

            AutoResetEvent are = new AutoResetEvent( false );

            void OnIncomingPacketHandlersOnJournalEntryAddedEvent( JournalEntry je )
            {
                if ( author.ToLower().Equals( "system" ) )
                {
                    var entry = wanted.Select( ( txt, idx ) => new { Text = txt, Index = idx } ).FirstOrDefault( e => je.Text.ToLower().Contains( e.Text.ToLower() ) &&
                                                                                                                    ( je.SpeechType == JournalSpeech.System || je.Name == "System" ) );

                    if ( entry != null )
                    {
                        index = entry.Index;
                        text = entry.Text;
                    }
                }
                else
                {
                    var entry = wanted.Select( ( txt, idx ) => new { Text = txt, Index = idx } )
                        .FirstOrDefault( e => je.Text.ToLower().Contains( e.Text.ToLower() ) &&
                                              ( string.IsNullOrEmpty( author ) || je.Name.Equals( author.Trim(), StringComparison.CurrentCultureIgnoreCase ) ) );


                    if ( entry != null )
                    {
                        index = entry.Index;
                        text = entry.Text;
                    }
                }

                if ( text != null )
                {
                    are.Set();
                }
            }

            IncomingPacketHandlers.JournalEntryAddedEvent += OnIncomingPacketHandlersOnJournalEntryAddedEvent;

            try
            {
                are.WaitOne( timeout );
            }
            finally
            {
                IncomingPacketHandlers.JournalEntryAddedEvent -= OnIncomingPacketHandlersOnJournalEntryAddedEvent;
            }

            return ( index, text );
        }
    }
}