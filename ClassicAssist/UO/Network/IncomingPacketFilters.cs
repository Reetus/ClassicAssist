using System;
using System.Collections.Generic;
using ClassicAssist.Data;
using ClassicAssist.Data.Filters;
using ClassicAssist.UO.Data;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.UO.Network
{
    public static class IncomingPacketFilters
    {
        private static readonly Dictionary<byte, Func<byte[], int, bool>> _filters =
            new Dictionary<byte, Func<byte[], int, bool>>();

        public static void Initialize()
        {
            Register( 0xC1, OnLocalizedMessage );
            Register( 0xCC, OnLocalizedMessageAffix );
        }

        private static bool OnLocalizedMessage( byte[] packet, int length )
        {
            PacketReader reader = new PacketReader( packet, length, false );

            JournalEntry journalEntry = new JournalEntry
            {
                Serial = reader.ReadInt32(),
                ID = reader.ReadInt16(),
                SpeechType = (JournalSpeech) reader.ReadByte(),
                SpeechHue = reader.ReadInt16(),
                SpeechFont = reader.ReadInt16(),
                Cliloc = reader.ReadInt32(),
                Name = reader.ReadString( 30 ),
                Arguments = reader.ReadUnicodeString( (int) reader.Size - 50 ).Split( '\t' )
            };

            journalEntry.Text = Cliloc.GetLocalString( journalEntry.Cliloc, journalEntry.Arguments );

            bool block = RepeatedMessagesFilter.CheckMessage( journalEntry );

            return block || ClilocFilter.CheckMessage( journalEntry );
        }

        private static bool OnLocalizedMessageAffix( byte[] packet, int length )
        {
            PacketReader reader = new PacketReader( packet, length, false );

            int serial = reader.ReadInt32();
            int graphic = reader.ReadInt16();
            JournalSpeech messageType = (JournalSpeech) reader.ReadByte();
            int hue = reader.ReadInt16();
            int font = reader.ReadInt16();
            int cliloc = reader.ReadInt32();
            MessageAffixType affixType = (MessageAffixType) reader.ReadByte();
            string name = reader.ReadString( 30 );
            string affix = reader.ReadString();
            string[] arguments = reader.ReadUnicodeString()
                .Split( new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries );

            JournalEntry journalEntry = new JournalEntry
            {
                Serial = serial,
                ID = graphic,
                SpeechType = messageType,
                SpeechHue = hue,
                SpeechFont = font,
                Cliloc = cliloc,
                Name = name,
                Arguments = arguments
            };

            string text = Cliloc.GetLocalString( journalEntry.Cliloc, journalEntry.Arguments );

            if ( affixType.HasFlag( MessageAffixType.Prepend ) )
            {
                journalEntry.Text = $"{affix}{text}";
            }
            else if ( affixType.HasFlag( MessageAffixType.Append ) )
            {
                journalEntry.Text = $"{text}{affix}";
            }

            bool block = RepeatedMessagesFilter.CheckMessage( journalEntry );

            return block || ClilocFilter.CheckMessageAffix( journalEntry, affixType, affix );
        }

        private static void Register( byte packetId, Func<byte[], int, bool> action )
        {
            if ( !_filters.ContainsKey( packetId ) )
            {
                _filters.Add( packetId, action );
            }
        }

        public static bool CheckPacket( byte[] data, int length )
        {
            if ( _filters.ContainsKey( data[0] ) &&
                 _filters.TryGetValue( data[0], out Func<byte[], int, bool> action ) )
            {
                return action.Invoke( data, length );
            }

            return false;
        }
    }
}