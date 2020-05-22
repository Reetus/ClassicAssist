using Assistant;
using ClassicAssist.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Network.Packets;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class MsgCommands
    {
        private const int DEFAULT_SPEAK_HUE = 34;

        [CommandsDisplay( Category = nameof( Strings.Messages ),
            Parameters = new[] { nameof( ParameterType.String ), nameof( ParameterType.Hue ) } )]
        public static void Msg( string message, int hue = DEFAULT_SPEAK_HUE )
        {
            UOC.Speak( message, hue );
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ),
            Parameters = new[] { nameof( ParameterType.String ) } )]
        public static void YellMsg( string message )
        {
            UOC.Speak( message, DEFAULT_SPEAK_HUE, JournalSpeech.Yell );
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ),
            Parameters = new[] { nameof( ParameterType.String ) } )]
        public static void WhisperMsg( string message )
        {
            UOC.Speak( message, DEFAULT_SPEAK_HUE, JournalSpeech.Whisper );
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ),
            Parameters = new[] { nameof( ParameterType.String ) } )]
        public static void EmoteMsg( string message )
        {
            UOC.Speak( message, DEFAULT_SPEAK_HUE, JournalSpeech.Emote );
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ),
            Parameters = new[] { nameof( ParameterType.String ) } )]
        public static void GuildMsg( string message )
        {
            UOC.Speak( message, DEFAULT_SPEAK_HUE, JournalSpeech.Guild );
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ),
            Parameters = new[] { nameof( ParameterType.String ) } )]
        public static void AllyMsg( string message )
        {
            UOC.Speak( message, DEFAULT_SPEAK_HUE, JournalSpeech.Alliance );
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ),
            Parameters = new[] { nameof( ParameterType.String ) } )]
        public static void PartyMsg( string message )
        {
            UOC.PartyMessage( message );
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ),
            Parameters = new[]
            {
                nameof( ParameterType.String ), nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.Hue )
            } )]
        public static void HeadMsg( string message, object obj = null, int hue = DEFAULT_SPEAK_HUE )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            UOC.OverheadMessage( message, hue, serial );
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ),
            Parameters = new[] { nameof( ParameterType.String ) } )]
        public static void PromptMsg( string message )
        {
            Engine.SendPacketToServer( new UnicodePromptResponse( Engine.LastPromptSerial, Engine.LastPromptID,
                message ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ),
            Parameters = new[] { nameof( ParameterType.Timeout ) } )]
        public static bool WaitForPrompt( int timeout )
        {
            PacketFilterInfo pfi = new PacketFilterInfo( 0xC2 );

            PacketWaitEntry packetWaitEntry = Engine.PacketWaitEntries.Add( pfi, PacketDirection.Incoming, true );

            try
            {
                bool result = packetWaitEntry.Lock.WaitOne( timeout );

                return result;
            }
            finally
            {
                Engine.PacketWaitEntries.Remove( packetWaitEntry );
            }
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ) )]
        public static void CancelPrompt()
        {
            Engine.SendPacketToServer( new UnicodePromptCancel( Engine.LastPromptSerial, Engine.LastPromptID ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ),
            Parameters = new[] { nameof( ParameterType.String ) } )]
        public static void ChatMsg( string message )
        {
            UOC.ChatMsg( message );
        }
    }
}