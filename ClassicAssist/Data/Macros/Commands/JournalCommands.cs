using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Assistant;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class JournalCommands
    {
        private const string DEFAULT_JOURNAL_BUFFER = "main";
        private const string SYSTEM_MESSAGE_AUTHOR = "system";

        [CommandsDisplay( Category = nameof( Strings.Journal ),
            Parameters = new[]
            {
                nameof( ParameterType.String ), nameof( ParameterType.String ), nameof( ParameterType.Hue ), nameof( ParameterType.Timeout ), nameof( ParameterType.String )
            } )]
        public static bool InJournal( string text, string author = "", int hue = -1, int timeout = -1, string buffer = DEFAULT_JOURNAL_BUFFER )
        {
            bool match;

            if ( timeout != -1 )
            {
                WaitForJournal( text, timeout, author, hue );
            }

            if ( string.Equals( author, SYSTEM_MESSAGE_AUTHOR, StringComparison.CurrentCultureIgnoreCase ))
            {
                match = Engine.Journal.FindAny( je => je.Text.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) >= 0 &&
                                                            ( je.SpeechType == JournalSpeech.System || string.Equals( je.Name, SYSTEM_MESSAGE_AUTHOR, StringComparison.CurrentCultureIgnoreCase ) ) &&
                                                            ( hue == -1 || je.SpeechHue == hue ), buffer );
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

                match = Engine.Journal.FindAny( je => je.Text.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) >= 0 &&
                                                            ( string.IsNullOrEmpty( author ) || string.Equals( je.Name, author.Trim(), StringComparison.CurrentCultureIgnoreCase ) ) &&
                                                            ( hue == -1 || je.SpeechHue == hue ), buffer );
            }

            return match;
        }

        [CommandsDisplay( Category = nameof( Strings.Journal ),
            Parameters = new[]
            {
                nameof( ParameterType.String )
            } )]
        public static void ClearJournal( string buffer = DEFAULT_JOURNAL_BUFFER )
        {
            Engine.Journal.Clear( buffer );
        }

        [CommandsDisplay( Category = nameof( Strings.Journal ),
            Parameters = new[]
            {
                nameof( ParameterType.String ), nameof( ParameterType.Timeout ), nameof( ParameterType.String ), nameof( ParameterType.Hue )
            } )]
        public static bool WaitForJournal( string text, int timeout, string author = "", int hue = -1 )
        {
            AutoResetEvent are = new AutoResetEvent( false );

            void OnIncomingPacketHandlersOnJournalEntryAddedEvent( JournalEntry je )
            {
                bool match;

                if ( string.Equals( author, SYSTEM_MESSAGE_AUTHOR, StringComparison.CurrentCultureIgnoreCase ) )
                {
                    match = je.Text.IndexOf( text, StringComparison.CurrentCultureIgnoreCase ) >= 0 &&
                          ( je.SpeechType == JournalSpeech.System || string.Equals( je.Name, SYSTEM_MESSAGE_AUTHOR, StringComparison.CurrentCultureIgnoreCase ) ) &&
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

                    match = je.Text.IndexOf( text, StringComparison.CurrentCultureIgnoreCase ) >= 0 &&
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
        [CommandsDisplay( "WaitForJournalArray", Category = nameof( Strings.Journal ),
            Parameters = new[]
            {
                nameof( ParameterType.StringArray ), nameof( ParameterType.Timeout ), nameof( ParameterType.String )
            } )]
        public static (int?, string) WaitForJournal( IEnumerable<string> entries, int timeout, string author = "" )
        {
            int? index = null;
            string text = null;

            string[] wanted = entries.ToArray();

            AutoResetEvent are = new AutoResetEvent( false );

            void OnIncomingPacketHandlersOnJournalEntryAddedEvent( JournalEntry je )
            {
                if ( string.Equals(author, SYSTEM_MESSAGE_AUTHOR, StringComparison.CurrentCultureIgnoreCase) )
                {
                    var entry = wanted.Select( ( txt, idx ) => new { Text = txt, Index = idx } ).FirstOrDefault( e => je.Text.IndexOf( e.Text, StringComparison.CurrentCultureIgnoreCase ) >= 0 &&
                                                                                                                    ( je.SpeechType == JournalSpeech.System || string.Equals(je.Name, SYSTEM_MESSAGE_AUTHOR, StringComparison.CurrentCultureIgnoreCase) ) );

                    if ( entry != null )
                    {
                        index = entry.Index;
                        text = entry.Text;
                    }
                }
                else
                {
                    var entry = wanted.Select( ( txt, idx ) => new { Text = txt, Index = idx } )
                        .FirstOrDefault( e => je.Text.IndexOf(e.Text, StringComparison.CurrentCultureIgnoreCase) >= 0 &&
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
