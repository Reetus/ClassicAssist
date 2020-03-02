using Assistant;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Network.Packets;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class MsgCommands
    {
        private const int DEFAULT_SPEAK_HUE = 34;

        [CommandsDisplay( Category = "Messages", Description = "Speaks the given message, Optional hue",
            InsertText = "Msg(\"hi\")" )]
        public static void Msg( string message, int hue = DEFAULT_SPEAK_HUE )
        {
            UOC.Speak( message, hue );
        }

        [CommandsDisplay( Category = "Messages", Description = "Yells the given message",
            InsertText = "YellMsg(\"hi\")" )]
        public static void YellMsg( string message )
        {
            UOC.Speak( message, DEFAULT_SPEAK_HUE, JournalSpeech.Yell );
        }

        [CommandsDisplay( Category = "Messages", Description = "Whispers the given message",
            InsertText = "WhisperMsg(\"hi\")" )]
        public static void WhisperMsg( string message )
        {
            UOC.Speak( message, DEFAULT_SPEAK_HUE, JournalSpeech.Whisper );
        }

        [CommandsDisplay( Category = "Messages", Description = "Emotes the given message",
            InsertText = "EmoteMsg(\"hi\")" )]
        public static void EmoteMsg( string message )
        {
            UOC.Speak( message, DEFAULT_SPEAK_HUE, JournalSpeech.Emote );
        }

        [CommandsDisplay( Category = "Messages", Description = "Sends given message to guild chat.",
            InsertText = "GuildMsg(\"alert\")" )]
        public static void GuildMsg( string message )
        {
            UOC.Speak( message, DEFAULT_SPEAK_HUE, JournalSpeech.Guild );
        }

        [CommandsDisplay( Category = "Messages", Description = "Sends given message to alliance chat.",
            InsertText = "AllyMsg(\"alert\")" )]
        public static void AllyMsg( string message )
        {
            UOC.Speak( message, DEFAULT_SPEAK_HUE, JournalSpeech.Alliance );
        }

        [CommandsDisplay( Category = "Messages", Description = "Sends given message to party chat.",
            InsertText = "PartyMsg(\"alert\")" )]
        public static void PartyMsg( string message )
        {
            UOC.PartyMessage( message );
        }

        [CommandsDisplay( Category = "Messages", Description = "Displays overhead message above given mobile / item.",
            InsertText = "HeadMsg(\"hi\", \"backpack\")" )]
        public static void HeadMsg( string message, object obj = null, int hue = DEFAULT_SPEAK_HUE )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            UOC.OverheadMessage( message, hue, serial );
        }

        [CommandsDisplay( Category = "Messages", Description = "Sends the specified message as a prompt response",
            InsertText = "PromptMsg(\"hello\")" )]
        public static void PromptMsg( string message )
        {
            Engine.SendPacketToServer( new UnicodePromptResponse( Engine.LastPromptSerial, Engine.LastPromptID,
                message ) );
        }

        [CommandsDisplay( Category = "Messages",
            Description = "Wait the specified timeout for a prompt packet to be received",
            InsertText = "WaitForPrompt(5000)" )]
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

        [CommandsDisplay( Category = "Messages", Description = "Cancels the current prompt.",
            InsertText = "CancelPrompt()" )]
        public static void CancelPrompt()
        {
            Engine.SendPacketToServer( new UnicodePromptCancel( Engine.LastPromptSerial, Engine.LastPromptID ) );
        }
    }
}