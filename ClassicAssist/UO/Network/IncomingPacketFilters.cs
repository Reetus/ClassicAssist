using System;
using System.Collections.Generic;
using System.IO;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Filters;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.UO.Network
{
    public static class IncomingPacketFilters
    {
        private static readonly Dictionary<byte, Func<byte[], int, bool>> _filters =
            new Dictionary<byte, Func<byte[], int, bool>>();

        public static void Initialize()
        {
            Register( 0x1C, OnASCIIMessage );
            Register( 0x20, OnMobileUpdate );
            Register( 0x77, OnMobileMoving );
            Register( 0x78, OnMobileIncoming );
            Register( 0xC1, OnLocalizedMessage );
            Register( 0xCC, OnLocalizedMessageAffix );
            Register( 0xF3, OnSAWorldItem );
        }

        private static bool OnASCIIMessage( byte[] packet, int length )
        {
            PacketReader reader = new PacketReader( packet, length, false );

            JournalEntry journalEntry = new JournalEntry
            {
                Serial = reader.ReadInt32(),
                ID = reader.ReadInt16(),
                SpeechType = (JournalSpeech) reader.ReadByte(),
                SpeechHue = reader.ReadInt16(),
                SpeechFont = reader.ReadInt16(),
                Name = reader.ReadString( 30 ),
                Text = reader.ReadString()
            };

            return RepeatedMessagesFilter.CheckMessage( journalEntry );
        }

        private static bool OnMobileUpdate( byte[] packet, int length )
        {
            PacketReader reader = new PacketReader( packet, length, true );
            int serial = reader.ReadInt32();
            int id = reader.ReadInt16();
            reader.ReadByte(); // BYTE 0x00;
            int hue = reader.ReadUInt16();
            int status = reader.ReadByte();
            int x = reader.ReadInt16();
            int y = reader.ReadInt16();
            reader.ReadInt16(); // WORD 0x00;
            int direction = reader.ReadByte() & 0x07;
            int z = reader.ReadSByte();

            Mobile mobile = new Mobile( serial )
            {
                ID = id,
                Hue = hue,
                Status = (MobileStatus) status,
                X = x,
                Y = y,
                Direction = (Direction) direction,
                Z = z
            };

            if ( !Engine.RehueList.CheckMobileUpdate( mobile ) )
            {
                return false;
            }

            // Will need to send to handler ourselves because we won't see packet again
            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x20 );
            handler?.OnReceive( new PacketReader( packet, length, true ) );
            return true;
        }

        private static bool OnSAWorldItem( byte[] packet, int length )
        {
            if ( !Engine.RehueList.CheckSAWorldItem( ref packet, ref length ) )
            {
                return false;
            }

            // Will need to send to handler ourselves because we won't see packet again
            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0xF3 );
            handler?.OnReceive( new PacketReader( packet, length, true ) );
            return true;
        }

        private static bool OnMobileMoving( byte[] packet, int length )
        {
            PacketReader reader = new PacketReader( packet, length, true );
            int serial = reader.ReadInt32();
            int id = reader.ReadInt16();
            int x = reader.ReadInt16();
            int y = reader.ReadInt16();
            int z = reader.ReadSByte();
            int direction = reader.ReadByte() & 0x07;
            int hue = reader.ReadUInt16();
            int status = reader.ReadByte();
            int notoriety = reader.ReadByte();

            Mobile mobile = new Mobile( serial )
            {
                ID = id,
                X = x,
                Y = y,
                Z = z,
                Direction = (Direction) direction,
                Hue = hue,
                Status = (MobileStatus) status,
                Notoriety = (Notoriety) notoriety
            };

            if ( !Engine.RehueList.CheckMobileMoving( mobile ) )
            {
                return false;
            }

            // Will need to send to handler ourselves because we won't see packet again
            reader.Seek( 1, SeekOrigin.Begin );
            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x77 );
            handler?.OnReceive( reader );
            return true;
        }

        private static bool OnMobileIncoming( byte[] packet, int length )
        {
            PacketReader reader = new PacketReader( packet, length, false );

            int serial = reader.ReadInt32();
            ItemCollection container = new ItemCollection( serial );

            Mobile mobile = serial == Engine.Player?.Serial ? Engine.Player : Engine.GetOrCreateMobile( serial );

            mobile.ID = reader.ReadInt16();
            mobile.X = reader.ReadInt16();
            mobile.Y = reader.ReadInt16();
            mobile.Z = reader.ReadSByte();
            mobile.Direction = (Direction) ( reader.ReadByte() & 0x07 );
            mobile.Hue = reader.ReadUInt16();
            mobile.Status = (MobileStatus) reader.ReadByte();
            mobile.Notoriety = (Notoriety) reader.ReadByte();

            bool useNewIncoming = Engine.ClientVersion == null || Engine.ClientVersion >= new Version( 7, 0, 33, 1 );

            for ( ;; )
            {
                int itemSerial = reader.ReadInt32();

                if ( itemSerial == 0 )
                {
                    break;
                }

                Item item = Engine.GetOrCreateItem( itemSerial );
                item.Owner = serial;
                item.ID = reader.ReadUInt16();
                item.Layer = (Layer) reader.ReadByte();

                if ( useNewIncoming )
                {
                    item.Hue = reader.ReadUInt16();
                }
                else
                {
                    if ( ( item.ID & 0x8000 ) != 0 )
                    {
                        item.ID ^= 0x8000;
                        item.Hue = reader.ReadUInt16();
                    }
                }

                container.Add( item );
            }

            mobile.Equipment.Clear();
            mobile.Equipment.Add( container.GetItems() );

            foreach ( Item item in container.GetItems() )
            {
                mobile.SetLayer( item.Layer, item.Serial );
            }

            if ( !Engine.RehueList.CheckMobileIncoming( mobile, container ) )
            {
                return false;
            }

            // Will need to send to handler ourselves because we won't see packet again
            reader.Seek( 3, SeekOrigin.Begin );
            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x78 );
            handler?.OnReceive( reader );
            return true;
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