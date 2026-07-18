#region License

// Copyright (C) 2026 Reetus
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

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Network.Packets;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class MsgCommands
    {
        // Eligible outgoing chat hue range (per POL / ClassicUO): 2 .. 0x03B2 (946).
        private const int MIN_SPEAK_HUE = 2;
        private const int MAX_SPEAK_HUE = 0x03B2;

        // Sentinel default: resolve the hue from the player's serial at call time.
        private const int DERIVED_SPEAK_HUE = -1;

        /// <summary>
        ///     Deterministically derives a valid speech hue from the player's serial so that every character
        ///     speaks in a stable, per-character colour instead of a single shared hue (which is a bot tell),
        ///     mimicking the per-profile speech hue a real client would have. The same serial always maps to
        ///     the same hue; different serials are well distributed across the eligible chat hue range.
        /// </summary>
        private static int GetSpeakHue()
        {
            int serial = Engine.Player?.Serial ?? 0;

            // Murmur3-style finalizer to scramble sequential serials into well-spread hues.
            uint h = (uint) serial;
            h ^= h >> 16;
            h *= 0x7feb352d;
            h ^= h >> 15;
            h *= 0x846ca68b;
            h ^= h >> 16;

            const uint range = MAX_SPEAK_HUE - MIN_SPEAK_HUE + 1;

            return MIN_SPEAK_HUE + (int) ( h % range );
        }

        private static int ResolveSpeakHue( int hue )
        {
            return hue == DERIVED_SPEAK_HUE ? GetSpeakHue() : hue;
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ), Parameters = new[] { nameof( ParameterType.String ), nameof( ParameterType.Hue ) } )]
        public static void Msg( string message, int hue = DERIVED_SPEAK_HUE )
        {
            UOC.Speak( message, ResolveSpeakHue( hue ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ), Parameters = new[] { nameof( ParameterType.String ) } )]
        public static void YellMsg( string message )
        {
            UOC.Speak( message, GetSpeakHue(), JournalSpeech.Yell );
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ), Parameters = new[] { nameof( ParameterType.String ) } )]
        public static void WhisperMsg( string message )
        {
            UOC.Speak( message, GetSpeakHue(), JournalSpeech.Whisper );
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ), Parameters = new[] { nameof( ParameterType.String ) } )]
        public static void EmoteMsg( string message )
        {
            UOC.Speak( message, GetSpeakHue(), JournalSpeech.Emote );
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ), Parameters = new[] { nameof( ParameterType.String ) } )]
        public static void GuildMsg( string message )
        {
            UOC.Speak( message, GetSpeakHue(), JournalSpeech.Guild );
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ), Parameters = new[] { nameof( ParameterType.String ) } )]
        public static void AllyMsg( string message )
        {
            UOC.Speak( message, GetSpeakHue(), JournalSpeech.Alliance );
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ), Parameters = new[] { nameof( ParameterType.String ) } )]
        public static void PartyMsg( string message )
        {
            UOC.PartyMessage( message );
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ),
            Parameters = new[] { nameof( ParameterType.String ), nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.Hue ) } )]
        public static void HeadMsg( string message, object obj = null, int hue = DERIVED_SPEAK_HUE )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            UOC.OverheadMessage( message, ResolveSpeakHue( hue ), serial );
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ), Parameters = new[] { nameof( ParameterType.String ) } )]
        public static void PromptMsg( string message )
        {
            BasePacket packet = Engine.LastPromptType == 0
                ? (BasePacket) new AsciiPromptResponse( Engine.LastPromptSerial, Engine.LastPromptID, message )
                : new UnicodePromptResponse( Engine.LastPromptSerial, Engine.LastPromptID, message );

            Engine.SendPacketToServer( packet );
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ), Parameters = new[] { nameof( ParameterType.String ), nameof( ParameterType.Timeout ) } )]
        public static (bool Result, string Message) GetText( string prompt, int timeout = 30000 )
        {
            int id = new Random().Next();

            UOC.SystemMessage( prompt );
            Engine.SendPacketToClient( new UnicodePromptRequest( id ) );

            AutoResetEvent are = new AutoResetEvent( false );

            string message = string.Empty;

            Engine.AddSendPreFilter( new PacketFilterInfo( 0xC2,
                new[] { PacketFilterConditions.IntAtPositionCondition( id, 3 ), PacketFilterConditions.IntAtPositionCondition( id, 7 ) }, ( bytes, info ) =>
                {
                    PacketReader pr = new PacketReader( bytes, bytes.Length, false );
                    pr.Seek( 16, SeekOrigin.Current );
                    message = pr.ReadUnicodeStringLE();
                    are.Set();
                } ) );

            bool result = are.WaitOne( timeout );

            if ( !result )
            {
                Engine.SendPacketToClient( new UnicodePromptCancel( id, id ) );
            }

            return ( result, message );
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ), Parameters = new[] { nameof( ParameterType.Timeout ) } )]
        public static bool WaitForPrompt( int timeout )
        {
            PacketFilterInfo[] pfis = {
                new PacketFilterInfo( 0xC2 ),
                new PacketFilterInfo( 0x9A )
            };

            PacketWaitEntry[] packetWaitEntries = pfis.Select( pfi => Engine.PacketWaitEntries.Add( pfi, PacketDirection.Incoming, true ) ).ToArray();

            Task timeoutTask = Task.Delay( timeout );

            Task[] tasks = packetWaitEntries.Select( pwe => pwe.Lock.ToTask() ).Append( timeoutTask ).ToArray();

            try
            {
                int index = Task.WaitAny( tasks );

                return index != tasks.Length -1;
            }
            finally
            {
                foreach ( PacketWaitEntry pwe in packetWaitEntries )
                {
                    Engine.PacketWaitEntries.Remove( pwe );
                }
            }
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ) )]
        public static void CancelPrompt()
        {
            BasePacket packet = Engine.LastPromptType == 0
                ? (BasePacket) new AsciiPromptCancel( Engine.LastPromptSerial, Engine.LastPromptID )
                : new UnicodePromptCancel( Engine.LastPromptSerial, Engine.LastPromptID );

            Engine.SendPacketToServer( packet );
        }

        [CommandsDisplay( Category = nameof( Strings.Messages ), Parameters = new[] { nameof( ParameterType.String ) } )]
        public static void ChatMsg( string message )
        {
            UOC.ChatMsg( message );
        }
    }
}