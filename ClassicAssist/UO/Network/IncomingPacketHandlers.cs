using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Abilities;
using ClassicAssist.Data.Chat;
using ClassicAssist.Data.Counters;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Data.Skills;
using ClassicAssist.Data.Targeting;
using ClassicAssist.Data.Vendors;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using ClassicAssist.UO.Objects.Gumps;
using Sentry;

namespace ClassicAssist.UO.Network
{
    public static class IncomingPacketHandlers
    {
        public delegate void dBufficonEnabledDisabled( int type, bool enabled, int duration );

        public delegate void dContainerContents( int serial, ItemCollection container );

        public delegate void dCorpseContainerDisplay( int serial, bool force );

        public delegate void dGump( uint gumpId, int serial, Gump gump );

        public delegate void dJournalEntryAdded( JournalEntry je );

        public delegate void dMapChanged( Map newMap );

        public delegate void dMenuAdded( int gumpId, Menu menu );

        public delegate void dMobileIncoming( Mobile mobile, ItemCollection equipment );

        public delegate void dMobileUpdated( Mobile mobile );

        public delegate void dNewWorldItem( Item item );

        public delegate void dSkillList( SkillInfo[] skills );

        public delegate void dSkillUpdated( int id, float value, float baseValue, LockStatus lockStatus,
            float skillCap );

        public delegate void dToggleSpecialMove( int spellID, bool enabled );

        public delegate void dVendorBuyDisplay( int serial, ShopListEntry[] entries );

        public delegate void dVendorSellDisplay( int serial, SellListEntry[] entries );

        private const int HUE_RED = 36;

        private static PacketHandler[] _handlers;
        private static PacketHandler[] _extendedHandlers;

        public static ConcurrentDictionary<int, Property[]> PropertyCache { get; set; } =
            new ConcurrentDictionary<int, Property[]>();

        public static event dVendorSellDisplay VendorSellDisplayEvent;
        public static event dJournalEntryAdded JournalEntryAddedEvent;
        public static event dSkillUpdated SkillUpdatedEvent;
        public static event dSkillList SkillsListEvent;
        public static event dMobileUpdated MobileUpdatedEvent;
        public static event dMobileIncoming MobileIncomingEvent;

        public static event dNewWorldItem NewWorldItemEvent;
        public static event dContainerContents ContainerContentsEvent;
        public static event dGump GumpEvent;
        public static event dBufficonEnabledDisabled BufficonEnabledDisabledEvent;
        public static event dCorpseContainerDisplay CorpseContainerDisplayEvent;

        public static event dMenuAdded MenuAddedEvent;

        public static void Initialize()
        {
            _handlers = new PacketHandler[0x100];
            _extendedHandlers = new PacketHandler[0x10000]; // Sphere id > 255

            Register( 0x11, 0, OnMobileStatus );
            Register( 0x1A, 0, OnWorldItem );
            Register( 0x17, 0, OnHealthbarColour );
            Register( 0x1B, 37, OnInitializePlayer );
            Register( 0x1C, 0, OnASCIIText );
            Register( 0x1D, 5, OnItemDeleted );
            Register( 0x20, 19, OnMobileUpdated );
            Register( 0x21, 8, OnMoveRejected );
            Register( 0x22, 3, OnMoveAccepted );
            Register( 0x24, 9, OnContainerDisplay );
            Register( 0x25, 21, OnItemAddedToContainer );
            Register( 0x27, 2, OnResetHolding );
            Register( 0x28, 5, OnResetHolding );
            Register( 0x29, 1, OnResetHolding );
            Register( 0x2E, 15, OnItemEquipped );
            Register( 0x31, 0, OnUO3DPetWindow );
            Register( 0x3A, 0, OnSkillsList );
            Register( 0x3C, 0, OnContainerContents );
            Register( 0x6C, 19, OnTarget );
            Register( 0x72, 5, OnWarMode );
            Register( 0x74, 0, OnShopList );
            Register( 0x77, 17, OnMobileMoving );
            Register( 0x7C, 0, OnDisplayItemListMenu );
            Register( 0x78, 0, OnMobileIncoming );
            Register( 0x98, 0, OnMobileName );
            Register( 0x99, 30, OnTarget );
            Register( 0x9E, 0, OnShopSell );
            Register( 0xA1, 9, OnMobileHits );
            Register( 0xA2, 9, OnMobileMana );
            Register( 0xA3, 9, OnMobileStamina );
            Register( 0xA8, 0, OnShardList );
            Register( 0xA9, 0, OnCharacterList );
            Register( 0xAE, 0, OnUnicodeText );
            Register( 0xB0, 0, OnGenericGump );
            Register( 0xB2, 0, OnChatMessage );
            Register( 0xB9, 5, OnSupportedFeatures );
            Register( 0xBF, 0, OnExtendedCommand );
            Register( 0xC1, 0, OnLocalizedText );
            Register( 0xC2, 0, OnUnicodePrompt );
            Register( 0xCC, 0, OnLocalizedTextAffix );
            Register( 0xD6, 0, OnProperties );
            Register( 0xDD, 0, OnCompressedGump );
            Register( 0xDF, 0, OnBuffAndAttributes );
            Register( 0xE5, 0, OnDisplayWaypoint );
            Register( 0xE6, 0, OnHideWaypoint );
            Register( 0xF3, 26, OnSAWorldItem );

            RegisterExtended( 0x04, 0, OnCloseGump );
            RegisterExtended( 0x06, 0, OnPartyCommand );
            RegisterExtended( 0x08, 0, OnMapChange );
            RegisterExtended( 0x10, 0, OnDisplayEquipmentInfo );
            RegisterExtended( 0x19, 0, OnMiscellaneousStatus );
            RegisterExtended( 0x21, 0, OnClearWeaponAbility );
            RegisterExtended( 0x25, 0, OnToggleSpecialMoves );
        }

        private static void OnGenericGump( PacketReader reader )
        {
            int senderSerial = reader.ReadInt32();
            uint gumpId = reader.ReadUInt32();

            Engine.GumpList.AddOrUpdate( gumpId, senderSerial, ( k, v ) => senderSerial );

            int x = reader.ReadInt32();
            int y = reader.ReadInt32();

            int layoutLength = reader.ReadUInt16();

            string layout = reader.ReadString( layoutLength );

            if ( string.IsNullOrEmpty( layout ) )
            {
                return;
            }

            int linesCount = reader.ReadUInt16();

            string[] text = new string[linesCount];

            for ( int i = 0; i < linesCount; i++ )
            {
                int length = reader.ReadUInt16();

                text[i] = reader.ReadUnicodeStringBE( length );
            }

            try
            {
                Gump gump = GumpParser.Parse( senderSerial, gumpId, x, y, layout, text );
                Engine.Gumps.Add( gump );

                GumpEvent?.Invoke( gumpId, senderSerial, gump );
            }
            catch ( Exception e )
            {
                SentrySdk.CaptureException( e, scope =>
                {
                    scope.SetExtra( "Serial", senderSerial );
                    scope.SetExtra( "GumpID", gumpId );
                    scope.SetExtra( "Layout", layout );
                    scope.SetExtra( "Text", text );
                    scope.SetExtra( "Packet", reader.GetData() );
                    scope.SetExtra( "Player", Engine.Player.ToString() );
                    scope.SetExtra( "WorldItemCount", Engine.Items.Count() );
                    scope.SetExtra( "WorldMobileCount", Engine.Mobiles.Count() );
                } );
            }
        }

        private static void OnMiscellaneousStatus( PacketReader reader )
        {
            int command = reader.ReadByte();
            int serial = reader.ReadInt32();

            switch ( command )
            {
                case 0x00:
                {
                    // Old Bonded Status
                    bool dead = reader.ReadBoolean();

                    Mobile mobile = Engine.Mobiles.GetMobile( serial );

                    if ( mobile == null )
                    {
                        return;
                    }

                    mobile.IsDead = dead;

                    break;
                }
                case 0x05:
                {
                    // Lifted from CUO, packet is used for both bonded status and updating statues
                    int pos = (int) reader.Index;
                    byte zero = reader.ReadByte();
                    byte type2 = reader.ReadByte();

                    if ( type2 == 0xFF )
                    {
                        byte status = reader.ReadByte();
                        ushort animation = reader.ReadUInt16();
                        ushort frame = reader.ReadUInt16();

                        if ( status == 0 && animation == 0 && frame == 0 )
                        {
                            reader.Seek( pos, SeekOrigin.Begin );
                            goto case 0;
                        }
                    }

                    break;
                }
            }
        }

        private static void OnCharacterList( PacketReader reader )
        {
            int characters = reader.ReadByte();

            reader.Seek( characters * 60, SeekOrigin.Current );

            int cities = reader.ReadByte();

            bool isNew = Engine.ClientVersion >= new Version( 7, 0, 13, 0 );

            for ( int i = 0; i < cities; i++ )
            {
                int index = reader.ReadByte();
                string cityName = reader.ReadString( isNew ? 32 : 31 );
                string buildingName = reader.ReadString( isNew ? 32 : 31 );

                if ( !isNew )
                {
                    continue;
                }

                int x = reader.ReadInt32();
                int y = reader.ReadInt32();
                int z = reader.ReadInt32();
                int map = reader.ReadInt32();
                int cliloc = reader.ReadInt32();
                reader.ReadInt32();
            }

            Engine.CharacterListFlags = (CharacterListFlags) reader.ReadInt32();

            Engine.TooltipsEnabled =
                Engine.CharacterListFlags.HasFlag( CharacterListFlags.PaladinNecromancerClassTooltips );
        }

        private static void OnUO3DPetWindow( PacketReader reader )
        {
            int serial = reader.ReadInt32();
            int count = reader.ReadByte();

            List<int> pets = new List<int>();

            for ( int i = 0; i < count; i++ )
            {
                int petSerial = reader.ReadInt32();
                reader.ReadByte();
                pets.Add( petSerial );
            }

            Mobile mobile = Engine.Mobiles.GetMobile( serial );

            if ( mobile != null )
            {
                mobile.Pets = pets.ToArray();
            }
        }

        private static void OnHideWaypoint( PacketReader reader )
        {
            int serial = reader.ReadInt32();

            QuestPointer pointer = Engine.QuestPointers.FirstOrDefault( p => p.Serial == serial );

            if ( pointer != null )
            {
                Engine.QuestPointers.Remove( pointer );
            }
        }

        private static void OnDisplayWaypoint( PacketReader reader )
        {
            if ( !Options.CurrentOptions.ShowResurrectionWaypoints )
            {
                return;
            }

            int serial = reader.ReadInt32();
            int x = reader.ReadInt16();
            int y = reader.ReadInt16();
            int z = reader.ReadSByte();
            Map map = (Map) reader.ReadByte();
            int type = reader.ReadInt16();
            int ignoreObjectSerial = reader.ReadInt16();
            int clilocDescription = reader.ReadInt32();
            string clilocArguments = reader.ReadUnicodeStringLE();
            string cliloc = Cliloc.GetLocalString( clilocDescription, new[] { clilocArguments } );

            switch ( type )
            {
                case 0x01:
                {
                    Engine.QuestPointers.Add( new QuestPointer
                    {
                        Serial = serial, Type = QuestPointerType.Corpse, X = x, Y = y
                    } );

                    break;
                }
                case 0x06:
                {
                    if ( Engine.Player != null )
                    {
                        int distance = Math.Max( Math.Abs( x - Engine.Player.X ), Math.Abs( y - Engine.Player.Y ) );
                        Direction direction = UOMath.MapDirection( Engine.Player.X, Engine.Player.Y, x, y );

                        if ( distance < 250 )
                        {
                            Commands.SystemMessage( string.Format(
                                Strings.Resurrection_Point___0____Direction___1____Distance___2_, cliloc,
                                direction.ToString(), distance ) );
                        }
                    }

                    Engine.QuestPointers.Add( new QuestPointer
                    {
                        Serial = serial, Type = QuestPointerType.Resurrection, X = x, Y = y
                    } );

                    break;
                }
            }
        }

        private static void OnDisplayItemListMenu( PacketReader reader )
        {
            int serial = reader.ReadInt32();
            int gumpId = reader.ReadInt16();
            string title = reader.ReadString( reader.ReadByte() );

            int numLines = reader.ReadByte();

            List<MenuEntry> entries = new List<MenuEntry>();

            for ( int i = 0; i < numLines; i++ )
            {
                int id = reader.ReadInt16();
                int hue = reader.ReadInt16();
                string line = reader.ReadString( reader.ReadByte() );

                entries.Add( new MenuEntry { Index = i + 1, ID = id, Hue = hue, Title = line } );
            }

            Menu menu = new Menu { Serial = serial, ID = gumpId, Entries = entries.ToArray(), Title = title };

            Engine.Menus.Add( menu );

            MenuAddedEvent?.Invoke( gumpId, menu );
        }

        private static void OnWorldItem( PacketReader reader )
        {
            byte[] packet = reader.GetData();
            Item item;
            uint serial = (uint) ( ( packet[3] << 24 ) | ( packet[4] << 16 ) | ( packet[5] << 8 ) | packet[6] );
            int offset = 9;

            if ( ( serial & 0x80000000 ) != 0 )
            {
                serial ^= 0x80000000;
                item = Engine.GetOrCreateItem( (int) serial );
                item.Count = ( packet[offset] << 8 ) | packet[offset + 1];
                offset += 2;
            }
            else
            {
                item = Engine.GetOrCreateItem( (int) serial );
            }

            int id = ( packet[7] << 8 ) | packet[8];

            if ( ( id & 0x4000 ) != 0 )
            {
                id ^= 0x4000;
                item.ArtDataID = 2;
            }

            if ( ( id & 0x8000 ) != 0 )
            {
                id ^= 0x8000;
                id += packet[offset]; // stack id
                offset++;
            }

            item.ID = id;
            int x = ( packet[offset] << 8 ) | packet[offset + 1];
            int y = ( packet[offset + 2] << 8 ) | packet[offset + 3];
            offset += 4;

            if ( ( x & 0x8000 ) != 0 )
            {
                x ^= 0x8000;
                item.Direction = (Direction) packet[offset];
                offset++;
            }

            item.X = x;
            item.Z = (sbyte) packet[offset];
            offset++;

            if ( ( y & 0x8000 ) != 0 )
            {
                y ^= 0x8000;
                item.Hue = ( packet[offset] << 8 ) | packet[offset + 1];
                offset += 2;
            }

            if ( ( y & 0x4000 ) != 0 )
            {
                y ^= 0x4000;
                item.Flags = packet[offset]; // ???
            }

            item.Y = y;
            item.Owner = 0;

            Engine.Items.Add( item );

            NewWorldItemEvent?.Invoke( item );
        }

        private static void OnResetHolding( PacketReader reader )
        {
            Engine.Player.Holding = 0;
            Engine.Player.HoldingAmount = 0;
            CountersManager.GetInstance().RecountAll?.Invoke();
        }

        private static void OnChatMessage( PacketReader reader )
        {
            ChatManager.GetInstance().OnChatPacket( reader );
        }

        private static void OnShardList( PacketReader reader )
        {
            reader.ReadByte();
            int count = reader.ReadInt16();

            List<ShardEntry> shards = new List<ShardEntry>();

            for ( int i = 0; i < count; i++ )
            {
                int index = reader.ReadInt16();
                string name = reader.ReadStringSafe( 32 );
                int full = reader.ReadByte();
                int timezone = reader.ReadSByte();
                int ip = reader.ReadInt32();

                shards.Add( new ShardEntry
                {
                    Index = index,
                    IP = ip,
                    Name = name,
                    PercentFull = full,
                    Timezone = timezone
                } );
            }

            Engine.Shards = shards;
        }

        private static void OnDisplayEquipmentInfo( PacketReader reader )
        {
            int serial = reader.ReadInt32();
            int cliloc = reader.ReadInt32();

            uint next = reader.ReadUInt32();

            List<Property> properties =
                new List<Property> { new Property { Cliloc = cliloc, Text = Cliloc.GetProperty( cliloc ) } };

            string journalOutput = Cliloc.GetProperty( cliloc );

            do
            {
                string attrName;

                int charges;
                uint number;

                switch ( next )
                {
                    case 0xFFFFFFFF:
                        break;
                    case 0xFFFFFFFD:
                    {
                        int nameLen = reader.ReadUInt16();
                        string craftedBy = reader.ReadString( nameLen );

                        properties.Add( new Property { Cliloc = -3, Text = craftedBy } );

                        journalOutput += $" {craftedBy}";

                        break;
                    }
                    case 0xFFFFFFFC:
                    {
                        number = reader.ReadUInt32();
                        charges = reader.ReadUInt16();

                        attrName = Cliloc.GetProperty( (int) number );

                        properties.Add( new Property
                        {
                            Cliloc = (int) number, Text = attrName, Arguments = new[] { charges.ToString() }
                        } );

                        journalOutput += $" {attrName}";

                        break;
                    }
                    default:
                    {
                        number = next;
                        charges = reader.ReadUInt16();

                        attrName = Cliloc.GetProperty( (int) number );

                        properties.Add( new Property
                        {
                            Cliloc = (int) number, Text = attrName, Arguments = new[] { charges.ToString() }
                        } );

                        journalOutput += $" {attrName}";

                        break;
                    }
                }

                if ( next == 0xFFFFFFFF )
                {
                    break;
                }

                next = reader.ReadUInt32();
            }
            while ( next != 0xFFFFFFFF );

            Engine.Journal.Write( new JournalEntry
            {
                Serial = -1, SpeechType = JournalSpeech.Label, Name = "System", Text = journalOutput
            } );

            Item item = Engine.Items.GetItem( serial );

            if ( item != null )
            {
                item.Properties = properties.ToArray();
            }
        }

        private static void OnUnicodePrompt( PacketReader reader )
        {
            int senderSerial = reader.ReadInt32();
            int promptId = reader.ReadInt32();

            Engine.LastPromptSerial = senderSerial;
            Engine.LastPromptID = promptId;
        }

        private static void OnShopSell( PacketReader reader )
        {
            int vendorSerial = reader.ReadInt32();
            int itemCount = reader.ReadInt16();

            List<SellListEntry> shopList = new List<SellListEntry>();

            for ( int i = 0; i < itemCount; i++ )
            {
                int serial = reader.ReadInt32();
                int id = reader.ReadInt16();
                int hue = reader.ReadInt16();
                int amount = reader.ReadInt16();
                int price = reader.ReadInt16();
                string name = reader.ReadString( reader.ReadInt16() );

                shopList.Add( new SellListEntry
                {
                    Serial = serial,
                    ID = id,
                    Hue = hue,
                    Amount = amount,
                    Price = price,
                    Name = name
                } );
            }

            if ( shopList.Count > 0 )
            {
                VendorSellDisplayEvent?.Invoke( vendorSerial, shopList.ToArray() );
            }
        }

        public static event dVendorBuyDisplay VendorBuyDisplayEvent;

        private static void OnContainerDisplay( PacketReader reader )
        {
            int serial = reader.ReadInt32();
            int gumpId = reader.ReadUInt16();
            int ctype = reader.ReadUInt16();

            // TODO write test for container => autoloot
            if ( ( reader.Size == 7 /* old client without ctype */ || ctype != 0 ) && gumpId == 0x09 )
            {
                Task.Run( () => CorpseContainerDisplayEvent?.Invoke( serial, false ) );
            }

            if ( ctype != 0 )
            {
                return;
            }

            Mobile entity = Engine.Mobiles.GetMobile( serial );

            if ( entity?.ShopBuy != null )
            {
                VendorBuyDisplayEvent?.Invoke( serial, entity.ShopBuy );
            }
        }

        private static void OnClearWeaponAbility( PacketReader reader )
        {
            AbilitiesManager manager = AbilitiesManager.GetInstance();

            if ( manager.Enabled != AbilityType.None )
            {
                Commands.SystemMessage( Strings.Current_Ability_Cleared, (int) SystemMessageHues.Red );
            }

            manager.Enabled = AbilityType.None;
            manager.ResendGump( AbilityType.None );
        }

        private static void OnShopList( PacketReader reader )
        {
            int serial = reader.ReadInt32();
            int count = reader.ReadByte();

            List<ShopListEntry> shopList = new List<ShopListEntry>();

            for ( int i = 0; i < count; i++ )
            {
                int price = reader.ReadInt32();
                string name = reader.ReadString( reader.ReadByte() );

                shopList.Add( new ShopListEntry { Name = name, Price = price, VendorSerial = serial } );
            }

            Mobile mobile = Engine.Mobiles.SelectEntity( m => m.Equipment.GetItems().Any( i => i.Serial == serial ) );

            if ( mobile == null )
            {
                return;
            }

            Item containerItem = Engine.Items.GetItem( serial );

            if ( containerItem?.Container == null )
            {
                return;
            }

            List<Item> containerItems = new List<Item>( containerItem.Container.GetItems() );
            containerItems.Sort( new XYComparer() );

            for ( int i = 0; i < shopList.Count; i++ )
            {
                shopList[i].VendorSerial = mobile.Serial;

                if ( shopList[i] != null )
                {
                    shopList[i].Item = containerItems[i];
                    shopList[i].Amount = containerItems[i].Count;
                }
            }

            mobile.ShopBuy = shopList.ToArray();
        }

        public static event dToggleSpecialMove ToggleSpecialMoveEvent;

        private static void OnToggleSpecialMoves( PacketReader reader )
        {
            int spellID = reader.ReadInt16();
            bool enabled = reader.ReadByte() == 1;

            ToggleSpecialMoveEvent?.Invoke( spellID, enabled );
        }

        private static void OnBuffAndAttributes( PacketReader reader )
        {
            reader.ReadInt32(); // serial
            int type = reader.ReadUInt16();
            int count = reader.ReadInt16();

            if ( type < 0x3e9 )
            {
                return;
            }

            bool enabled = count > 0;
            int duration = 0;

            if ( count == 1 )
            {
                reader.Seek( 12, SeekOrigin.Current );
                duration = reader.ReadInt16();
            }

            BufficonEnabledDisabledEvent?.Invoke( type, enabled, duration );
        }

        private static void OnLocalizedText( PacketReader reader )
        {
            JournalEntry journalEntry = new JournalEntry
            {
                Serial = reader.ReadInt32(),
                ID = reader.ReadInt16(),
                SpeechType = (JournalSpeech) reader.ReadByte(),
                SpeechHue = reader.ReadInt16(),
                SpeechFont = reader.ReadInt16(),
                Cliloc = reader.ReadInt32(),
                Name = reader.ReadString( 30 ),
                Arguments = reader.ReadUnicodeString( (int) reader.Size - 50 )
                    .Split( new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries )
            };

            journalEntry.Text = Cliloc.GetLocalString( journalEntry.Cliloc, journalEntry.Arguments );

            if ( journalEntry.SpeechType == JournalSpeech.Label )
            {
                if ( Engine.Player?.LastTargetSerial == journalEntry.Serial )
                {
                    MsgCommands.HeadMsg( Options.CurrentOptions.LastTargetMessage, journalEntry.Serial );
                }

                if ( Engine.Player?.EnemyTargetSerial == journalEntry.Serial )
                {
                    MsgCommands.HeadMsg( Options.CurrentOptions.EnemyTargetMessage, journalEntry.Serial );
                }

                if ( Engine.Player?.FriendTargetSerial == journalEntry.Serial )
                {
                    MsgCommands.HeadMsg( Options.CurrentOptions.FriendTargetMessage, journalEntry.Serial );
                }
            }

            AddToJournal( journalEntry );
        }

        private static void OnLocalizedTextAffix( PacketReader reader )
        {
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

            AddToJournal( journalEntry );
        }

        private static void OnPartyCommand( PacketReader reader )
        {
            int command = reader.ReadByte();

            switch ( command )
            {
                case 1:
                {
                    int count = reader.ReadByte();

                    List<int> partyMembers = new List<int>();

                    for ( int i = 0; i < count; i++ )
                    {
                        int serial = reader.ReadInt32();
                        partyMembers.Add( serial );
                    }

                    if ( Engine.Player == null )
                    {
                        return;
                    }

                    Engine.Player.Party = partyMembers.ToArray();

                    break;
                }

                case 2:
                {
                    int count = reader.ReadByte();

                    reader.ReadInt32(); // removed member serial

                    List<int> partyMembers = new List<int>();

                    for ( int i = 0; i < count; i++ )
                    {
                        int serial = reader.ReadInt32();
                        partyMembers.Add( serial );
                    }

                    if ( Engine.Player == null )
                    {
                        return;
                    }

                    Engine.Player.Party = partyMembers.ToArray();

                    break;
                }

                case 4:
                {
                    int serial = reader.ReadInt32();
                    string message = reader.ReadUnicodeString();

                    JournalEntry journalEntry = new JournalEntry
                    {
                        Serial = serial,
                        Name = Engine.Mobiles.GetMobile( serial )?.Name ?? "Unknown",
                        SpeechType = JournalSpeech.Whisper,
                        Text = message
                    };

                    AddToJournal( journalEntry );

                    break;
                }

                case 7:
                {
                    int leaderSerial = reader.ReadInt32();

                    if ( Options.CurrentOptions.AutoAcceptPartyInvite )
                    {
                        if ( !Options.CurrentOptions.AutoAcceptPartyOnlyFromFriends ||
                             Options.CurrentOptions.AutoAcceptPartyOnlyFromFriends &&
                             MobileCommands.InFriendList( leaderSerial ) )
                        {
                            Engine.SendPacketToServer( new AcceptPartyInvitation( leaderSerial ) );
                        }
                    }

                    break;
                }
            }
        }

        private static void OnHealthbarColour( PacketReader reader )
        {
            int serial = reader.ReadInt32();
            int count = reader.ReadInt16();

            Mobile mobile = Engine.Mobiles.GetMobile( serial );

            if ( mobile == null )
            {
                return;
            }

            HealthbarColour healthbar = HealthbarColour.None;

            if ( Engine.ClientVersion < new Version( 7, 0, 0, 0 ) )
            {
                for ( int i = 0; i < count; i++ )
                {
                    int type = reader.ReadInt16();
                    bool enabled = reader.ReadBoolean();

                    switch ( type )
                    {
                        case 1 when enabled:
                            healthbar |= HealthbarColour.Green;
                            break;
                        case 1:
                            healthbar &= ~HealthbarColour.Green;
                            break;
                        case 2 when enabled:
                            healthbar |= HealthbarColour.Yellow;
                            break;
                        case 2:
                            healthbar &= ~HealthbarColour.Yellow;
                            break;
                        case 3 when enabled:
                            healthbar |= HealthbarColour.Red;
                            break;
                        case 3:
                            healthbar &= ~HealthbarColour.Red;
                            break;
                    }
                }

                mobile.HealthbarColour = healthbar;
            }
            else
            {
                int status = reader.ReadInt16();
                int flags = reader.ReadByte();

                switch ( status )
                {
                    case 0x01:
                        healthbar = HealthbarColour.Green;

                        break;

                    case 0x02:
                        healthbar = HealthbarColour.Yellow;

                        break;

                    case 0x03:
                        healthbar = HealthbarColour.Red;

                        break;
                }

                if ( flags >= 1 )
                {
                    mobile.HealthbarColour |= healthbar;
                }
                else
                {
                    mobile.HealthbarColour &= ~healthbar;
                }
            }
        }

        private static void OnCloseGump( PacketReader reader )
        {
            uint gumpID = reader.ReadUInt32();
            int buttonID = reader.ReadInt32();

            Engine.GumpList.TryRemove( gumpID, out int _ );
            Engine.Gumps.Remove( gumpID, buttonID );
        }

        private static void OnASCIIText( PacketReader reader )
        {
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

            AddToJournal( journalEntry );
        }

        private static void OnUnicodeText( PacketReader reader )
        {
            JournalEntry journalEntry = new JournalEntry
            {
                Serial = reader.ReadInt32(),
                ID = reader.ReadInt16(),
                SpeechType = (JournalSpeech) reader.ReadByte(),
                SpeechHue = reader.ReadInt16(),
                SpeechFont = reader.ReadInt16(),
                SpeechLanguage = reader.ReadString( 4 ),
                Name = reader.ReadString( 30 ),
                Text = reader.ReadUnicodeString()
            };

            AddToJournal( journalEntry );
        }

        private static void OnMoveAccepted( PacketReader reader )
        {
            Direction direction = Engine.GetSequence( reader.ReadByte() );

            if ( Engine.Player != null )
            {
                Engine.Player.Direction = direction;
            }
        }

        private static void OnMoveRejected( PacketReader reader )
        {
            int sequence = reader.ReadByte();
            int x = reader.ReadInt16();
            int y = reader.ReadInt16();
            Direction direction = (Direction) reader.ReadByte();
            int z = reader.ReadSByte();

            if ( Engine.Player != null )
            {
                Engine.Player.Direction = direction;
            }
        }

        private static void OnCompressedGump( PacketReader reader )
        {
            int senderSerial = reader.ReadInt32();
            uint gumpId = reader.ReadUInt32();

            Engine.GumpList.AddOrUpdate( gumpId, senderSerial, ( k, v ) => senderSerial );

            int x = reader.ReadInt32();
            int y = reader.ReadInt32();
            int compressedLength = reader.ReadInt32();
            int decompressedLength = reader.ReadInt32() + 1;

            if ( compressedLength <= 4 )
            {
                return;
            }

            compressedLength -= 4;

            byte[] decompressedBuffer = new byte[decompressedLength];

            byte[] compressedBuffer = reader.ReadByteArray( compressedLength );

            bool success = Compression.Uncompress( ref decompressedBuffer, ref decompressedLength, compressedBuffer,
                compressedLength );

            if ( !success )
            {
                return;
            }

            string layout = Encoding.ASCII.GetString( decompressedBuffer ).TrimEnd( '\0' );

            if ( string.IsNullOrEmpty( layout ) )
            {
                return;
            }

            int linesCount = reader.ReadInt32();

            compressedLength = reader.ReadInt32();
            decompressedLength = reader.ReadInt32() + 1;

            string[] text = new string[linesCount];

            if ( compressedLength > 4 )
            {
                compressedLength -= 4;

                decompressedBuffer = new byte[decompressedLength];

                compressedBuffer = reader.ReadByteArray( compressedLength );

                success = Compression.Uncompress( ref decompressedBuffer, ref decompressedLength, compressedBuffer,
                    compressedLength );

                if ( !success )
                {
                    return;
                }

                int offset = 0;

                for ( int i = 0; i < linesCount; i++ )
                {
                    int length = ( ( decompressedBuffer[offset] << 8 ) | decompressedBuffer[offset + 1] ) * 2;
                    offset += 2;
                    text[i] = Encoding.BigEndianUnicode.GetString( decompressedBuffer, offset, length );
                    offset += length;
                }
            }

            try
            {
                Gump gump = GumpParser.Parse( senderSerial, gumpId, x, y, layout, text );
                Engine.Gumps.Add( gump );

                GumpEvent?.Invoke( gumpId, senderSerial, gump );
            }
            catch ( Exception e )
            {
                SentrySdk.CaptureException( e, scope =>
                {
                    scope.SetExtra( "Serial", senderSerial );
                    scope.SetExtra( "GumpID", gumpId );
                    scope.SetExtra( "Layout", layout );
                    scope.SetExtra( "Text", text );
                    scope.SetExtra( "Packet", reader.GetData() );
                    scope.SetExtra( "Player", Engine.Player.ToString() );
                    scope.SetExtra( "WorldItemCount", Engine.Items.Count() );
                    scope.SetExtra( "WorldMobileCount", Engine.Mobiles.Count() );
                } );
            }
        }

        private static void OnMobileName( PacketReader reader )
        {
            int serial = reader.ReadInt32();
            string name = reader.ReadString( 30 );

            Mobile mobile = Engine.GetOrCreateMobile( serial );

            mobile.Name = name;
        }

        private static void OnMobileMoving( PacketReader reader )
        {
            int serial = reader.ReadInt32();
            int id = reader.ReadInt16();
            int x = reader.ReadInt16();
            int y = reader.ReadInt16();
            int z = reader.ReadSByte();
            int direction = reader.ReadByte() & 0x07;
            int hue = reader.ReadUInt16();
            int status = reader.ReadByte();
            int notoriety = reader.ReadByte();

            Mobile mobile = Engine.GetOrCreateMobile( serial );
            mobile.ID = id;
            mobile.X = x;
            mobile.Y = y;
            mobile.Z = z;
            mobile.Direction = (Direction) direction;
            mobile.Hue = hue;
            mobile.Status = (MobileStatus) status;
            mobile.Notoriety = (Notoriety) notoriety;

            Engine.Mobiles.Add( mobile );
        }

        private static void OnMobileUpdated( PacketReader reader )
        {
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

            Mobile mobile = Engine.GetOrCreateMobile( serial );
            mobile.ID = id;
            mobile.Hue = hue;
            mobile.Status = (MobileStatus) status;
            mobile.X = x;
            mobile.Y = y;
            mobile.Direction = (Direction) direction;
            mobile.Z = z;

            Engine.Mobiles.Add( mobile );

            MobileUpdatedEvent?.Invoke( mobile );
        }

        private static void OnWarMode( PacketReader reader )
        {
            if ( reader.ReadBoolean() )
            {
                Engine.Player.Status |= MobileStatus.WarMode;
            }
            else
            {
                Engine.Player.Status &= ~MobileStatus.WarMode;
            }

            MobileUpdatedEvent?.Invoke( Engine.Player );
        }

        private static void OnTarget( PacketReader reader )
        {
            byte type = reader.ReadByte();
            int tid = reader.ReadInt32();
            TargetFlags flags = (TargetFlags) reader.ReadByte();

            Engine.TargetType = (TargetType) type;
            Engine.TargetSerial = tid;
            Engine.TargetFlags = flags;
            Engine.TargetExists = flags != TargetFlags.Cancel;
            Engine.InternalTarget = false;

            if ( Engine.TargetExists )
            {
                Engine.WaitingForTarget = false;
            }

            if ( !Options.CurrentOptions.QueueLastTarget || Engine.LastTargetQueue.Count == 0 )
            {
                return;
            }

            if ( flags == TargetFlags.Cancel )
            {
                Engine.TargetExists = false;
            }
            else
            {
                TargetQueueObject obj = Engine.LastTargetQueue.Dequeue();

                if ( obj == null )
                {
                    return;
                }

                TargetCommands.Target( obj.Object, Options.CurrentOptions.RangeCheckLastTarget );
            }
        }

        private static void OnMobileStamina( PacketReader reader )
        {
            int serial = reader.ReadInt32();
            int staminaMax = reader.ReadInt16();
            int stamina = reader.ReadInt16();

            Mobile mobile = Engine.GetOrCreateMobile( serial );
            mobile.Stamina = stamina;
            mobile.StaminaMax = staminaMax;
        }

        private static void OnMobileMana( PacketReader reader )
        {
            int serial = reader.ReadInt32();
            int manaMax = reader.ReadInt16();
            int mana = reader.ReadInt16();

            Mobile mobile = Engine.GetOrCreateMobile( serial );
            mobile.Mana = mana;
            mobile.ManaMax = manaMax;
        }

        private static void OnMobileHits( PacketReader reader )
        {
            int serial = reader.ReadInt32();
            int hitsMax = reader.ReadInt16();
            int hits = reader.ReadInt16();

            Mobile mobile = Engine.GetOrCreateMobile( serial );
            mobile.Hits = hits;
            mobile.HitsMax = hitsMax;
        }

        private static void OnSupportedFeatures( PacketReader reader )
        {
            Engine.Features = (FeatureFlags) reader.ReadInt32();
        }

        private static void OnSkillsList( PacketReader reader )
        {
            byte type = reader.ReadByte();
            int id = reader.ReadInt16();
            int value = reader.ReadInt16();
            int baseValue = reader.ReadInt16();
            LockStatus lockStatus = (LockStatus) reader.ReadByte();
            bool haveCap = type != 0u && type <= 0x03 || type == 0xDF;
            int skillCap = haveCap ? reader.ReadInt16() : 1000;

            if ( reader.Size <= 13 )
            {
                SkillUpdatedEvent?.Invoke( id, (float) value / 10, (float) baseValue / 10, lockStatus,
                    (float) skillCap / 10 );
            }
            else
            {
                SkillInfo si = new SkillInfo
                {
                    Value = (float) value / 10,
                    BaseValue = (float) baseValue / 10,
                    LockStatus = lockStatus,
                    SkillCap = (float) skillCap / 10,
                    ID = id - 1
                };

                List<SkillInfo> skillInfoList = new List<SkillInfo>( 128 ) { si };

                for ( ;; )
                {
                    id = reader.ReadInt16();

                    if ( id == 0 )
                    {
                        break;
                    }

                    value = reader.ReadInt16();
                    baseValue = reader.ReadInt16();
                    lockStatus = (LockStatus) reader.ReadByte();
                    skillCap = haveCap ? reader.ReadInt16() : 1000;

                    si = new SkillInfo
                    {
                        Value = (float) value / 10,
                        BaseValue = (float) baseValue / 10,
                        LockStatus = lockStatus,
                        SkillCap = (float) skillCap / 10,
                        ID = id - 1
                    };

                    skillInfoList.Add( si );
                }

                SkillsListEvent?.Invoke( skillInfoList.ToArray() );
            }
        }

        private static void OnExtendedCommand( PacketReader reader )
        {
            int command = reader.ReadUInt16();

            PacketHandler handler = GetExtendedHandler( command );
            handler?.OnReceive( reader );
        }

        private static void OnMapChange( PacketReader reader )
        {
            Map map = (Map) reader.ReadByte();

            if ( Engine.Player != null )
            {
                Engine.Player.Map = map;
            }

            MapChangedEvent?.Invoke( map );
        }

        public static event dMapChanged MapChangedEvent;

        private static void OnItemAddedToContainer( PacketReader reader )
        {
            int serial = reader.ReadInt32();
            int id = reader.ReadUInt16();
            reader.ReadByte(); // offset
            int count = reader.ReadUInt16();
            int x = reader.ReadInt16();
            int y = reader.ReadInt16();
            int grid = Engine.ClientVersion == null || Engine.ClientVersion >= new Version( 6, 0, 1, 7 )
                ? reader.ReadByte()
                : 0;
            int containerSerial = reader.ReadInt32();
            int hue = reader.ReadUInt16();

            Item item = Engine.GetOrCreateItem( serial, containerSerial );
            item.ID = id;
            item.Count = count;
            item.X = x;
            item.Y = y;
            item.Grid = grid;
            item.Owner = containerSerial;
            item.Hue = hue;

            // Check mobile layer items if container is not a mobile
            if ( !UOMath.IsMobile( containerSerial ) )
            {
                Layer layer = Engine.Player?.GetAllLayers().Select( ( s, i ) => new { i, s } )
                    .Where( t => t.s == serial ).Select( t => (Layer) t.i ).FirstOrDefault() ?? Layer.Invalid;

                if ( layer != Layer.Invalid )
                {
                    Engine.Player?.SetLayer( layer, 0 );
                }
                else
                {
                    Mobile mobile = Engine.Mobiles.SelectEntity( m => m.GetAllLayers().Contains( serial ) );

                    layer = mobile?.GetAllLayers().Select( ( s, i ) => new { i, s } ).Where( t => t.s == serial )
                        .Select( t => (Layer) t.i ).FirstOrDefault() ?? Layer.Invalid;

                    if ( layer != Layer.Invalid )
                    {
                        mobile?.SetLayer( layer, 0 );
                    }
                }
            }

            if ( UOMath.IsMobile( containerSerial ) )
            {
                item.Owner = containerSerial;
                Engine.Items.Add( item );
            }
            else
            {
                Item container = Engine.GetOrCreateItem( containerSerial );

                item.Owner = container.Serial;

                if ( container.Container == null )
                {
                    container.Container = new ItemCollection( containerSerial );
                }

                container.Container.Add( item );
            }

            //rehueList

            int backpack = Engine.Player?.Backpack?.Serial ?? 0;

            if ( item.IsDescendantOf( backpack ) )
            {
                Engine.RehueList.CheckItem( item );
            }

            //rehueList
        }

        private static void OnItemDeleted( PacketReader reader )
        {
            int serial = reader.ReadInt32();

            Mobile mobile = Engine.Mobiles.FirstOrDefault( m => m.GetAllLayers().Contains( serial ) );

            if ( mobile != null )
            {
                Item item = Engine.Items.GetItem( serial );

                if ( item != null )
                {
                    mobile.SetLayer( item.Layer, 0 );
                    mobile.Equipment.Remove( serial );
                }
            }

            if ( UOMath.IsMobile( serial ) )
            {
                Engine.Mobiles.Remove( serial );
                Engine.Items.RemoveByOwner( serial );
            }
            else
            {
                Engine.Items.Remove( serial );
            }
        }

        private static void OnItemEquipped( PacketReader reader )
        {
            int serial = reader.ReadInt32();
            int id = reader.ReadInt16();
            reader.ReadByte(); // BYTE 0x00
            int layer = reader.ReadByte();
            int mobileSerial = reader.ReadInt32();
            int hue = reader.ReadUInt16();

            Item item = Engine.GetOrCreateItem( serial );

            item.Owner = mobileSerial;
            item.Layer = (Layer) layer;
            item.Hue = hue;
            item.ID = id;

            if ( item.Properties == null && PropertyCache.TryGetValue( serial, out Property[] properties ) )
            {
                item.Properties = properties;
            }

            Mobile mobile = Engine.GetOrCreateMobile( mobileSerial );

            Item existing = mobile.Equipment.FirstOrDefault( i => i.Layer == (Layer) layer );

            if ( existing != null && item.Layer != Layer.Backpack )
            {
                mobile.Equipment.Remove( existing );
            }

            mobile.Equipment.Add( item );
            Engine.Items.Add( item );

            mobile.SetLayer( item.Layer, item.Serial );
        }

        private static void OnProperties( PacketReader reader )
        {
            reader.Seek( 2, SeekOrigin.Current ); // word 1

            int serial = reader.ReadInt32();

            reader.Seek( 2, SeekOrigin.Current ); // word 0

            int hash = reader.ReadInt32();

            List<Property> list = new List<Property>();

            int cliloc;
            bool first = true;
            string name = "";

            while ( ( cliloc = reader.ReadInt32() ) != 0 )
            {
                Property property = new Property { Cliloc = cliloc };

                int length = reader.ReadInt16();

                property.Arguments = reader.ReadUnicodeString( length )
                    .Split( new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries );

                property.Text = Cliloc.GetLocalString( cliloc, property.Arguments );

                if ( first )
                {
                    name = property.Text;
                    first = false;
                }

                list.Add( property );
            }

            Property[] propertyArray = list.ToArray();

            PropertyCache.AddOrUpdate( serial, v => propertyArray, ( k, v ) => propertyArray );

            if ( Engine.Player?.Serial == serial )
            {
                Engine.Player.Name = name;
                Engine.Player.Properties = propertyArray;
                Engine.UpdateWindowTitle();
            }
            else if ( UOMath.IsMobile( serial ) )
            {
                Mobile mobile = Engine.GetOrCreateMobile( serial );

                mobile.Name = name;
                mobile.Properties = propertyArray;
            }
            else
            {
                Item item = Engine.GetOrCreateItem( serial );

                item.Name = name;
                item.Properties = propertyArray;
            }
        }

        private static void OnMobileStatus( PacketReader reader )
        {
            long length = reader.Size;

            int serial = reader.ReadInt32();
            string name = reader.ReadString( 30 );
            int hits = reader.ReadInt16();
            int hitsMax = reader.ReadInt16();
            bool allowNameChange = reader.ReadBoolean(); // Allow Name Change
            byte features = reader.ReadByte();

            Mobile mobile = serial == Engine.Player?.Serial ? Engine.Player : Engine.GetOrCreateMobile( serial );
            mobile.Name = name;
            mobile.Hits = hits;
            mobile.HitsMax = hitsMax;
            mobile.IsRenamable = allowNameChange;

            if ( features <= 0 )
            {
                return;
            }

            int sex = reader.ReadByte();

            if ( serial != Engine.Player?.Serial )
            {
                return;
            }

            PlayerMobile player = Engine.Player;

            player.Strength = reader.ReadInt16();
            player.Dex = reader.ReadInt16();
            player.Int = reader.ReadInt16();
            player.Stamina = reader.ReadInt16();
            player.StaminaMax = reader.ReadInt16();
            player.Mana = reader.ReadInt16();
            player.ManaMax = reader.ReadInt16();
            player.Gold = reader.ReadInt32();
            player.PhysicalResistance = reader.ReadInt16();
            player.Weight = reader.ReadInt16();
            player.WeightMax = 40 + (int) ( 3.5 * player.Strength );

            if ( features >= 5 )
            {
                player.WeightMax = reader.ReadInt16();
                player.Race = (MobileRace) reader.ReadByte();
            }

            if ( features >= 3 )
            {
                player.StatCap = reader.ReadInt16();
                player.Followers = reader.ReadByte();
                player.FollowersMax = reader.ReadByte();
            }

            if ( features >= 4 )
            {
                player.FireResistance = reader.ReadInt16();
                player.ColdResistance = reader.ReadInt16();
                player.PoisonResistance = reader.ReadInt16();
                player.EnergyResistance = reader.ReadInt16();
                player.Luck = reader.ReadInt16();
                player.Damage = reader.ReadInt16();
                player.DamageMax = reader.ReadInt16();
                player.TithingPoints = reader.ReadInt32();
            }

            // ReSharper disable once InvertIf
            if ( features >= 6 )
            {
                player.PhysicalResistanceMax = reader.ReadInt16();
                player.FireResistanceMax = reader.ReadInt16();
                player.ColdResistanceMax = reader.ReadInt16();
                player.PoisonResistanceMax = reader.ReadInt16();
                player.EnergyResistanceMax = reader.ReadInt16();
                player.DefenseChanceIncrease = reader.ReadInt16();
                player.DefenseChanceIncreaseMax = reader.ReadInt16();
                player.HitChanceIncrease = reader.ReadInt16();
                player.SwingSpeedIncrease = reader.ReadInt16();
                player.DamageIncrease = reader.ReadInt16();
                player.LowerReagentCost = reader.ReadInt16();
                player.SpellDamageIncrease = reader.ReadInt16();
                player.FasterCastRecovery = reader.ReadInt16();
                player.FasterCasting = reader.ReadInt16();
                player.LowerManaCost = reader.ReadInt16();
            }

            Engine.UpdateWindowTitle();
            Engine.SetTitle();
        }

        private static void OnMobileIncoming( PacketReader reader )
        {
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

            Engine.Items.Add( container.GetItems() );

            if ( !( mobile is PlayerMobile ) )
            {
                Engine.Mobiles.Add( mobile );
            }
            else
            {
                Task.Run( () =>
                {
                    AbilitiesManager manager = AbilitiesManager.GetInstance();
                    manager.ResendGump( manager.Enabled );
                } ).ConfigureAwait( false );
            }

            MobileIncomingEvent?.Invoke( mobile, container );
        }

        private static void OnContainerContents( PacketReader reader )
        {
            Item containerItem = null;
            int count = 0;

            try
            {
                if ( reader.Size == 5 )
                {
                    return;
                }

                bool oldStyle = false;

                count = reader.ReadInt16();

                if ( ( reader.Size - 5 ) / 20 != count )
                {
                    oldStyle = true;
                }

                ItemCollection container = null;

                for ( int i = 0; i < count; i++ )
                {
                    int serial = reader.ReadInt32();
                    int id = reader.ReadUInt16();
                    reader.ReadByte(); // Item ID Offset
                    int amount = reader.ReadUInt16();
                    int x = reader.ReadInt16();
                    int y = reader.ReadInt16();
                    int grid = 0;

                    if ( !oldStyle )
                    {
                        grid = reader.ReadByte();
                    }

                    int containerSerial = reader.ReadInt32();
                    int hue = reader.ReadUInt16();

                    if ( container == null )
                    {
                        container = new ItemCollection( containerSerial );
                    }

                    Item item = Engine.GetOrCreateItem( serial, containerSerial );

                    item.ID = id;
                    item.Count = amount;
                    item.Owner = containerSerial;
                    item.Hue = hue;
                    item.Grid = grid;
                    item.X = x;
                    item.Y = y;

                    container.Add( item );
                }

                if ( container == null )
                {
                    return;
                }

                containerItem = Engine.GetOrCreateItem( container.Serial );

                if ( containerItem.Container == null )
                {
                    containerItem.Container = container;
                }
                else
                {
                    containerItem.Container.Clear();
                    containerItem.Container.Add( container.GetItems() );
                }

                Engine.Items.Add( containerItem );

                Task.Run( () =>
                {
                    ContainerContentsEvent?.Invoke( container.Serial, container );

                    Engine.RehueList.CheckContainer( container );
                } ).ConfigureAwait( false );
            }
            catch ( Exception e )
            {
                SentrySdk.CaptureException( e, scope =>
                {
                    scope.SetExtra( "ContainerSerial", containerItem?.Serial );
                    scope.SetExtra( "Count", count );
                    scope.SetExtra( "Packet", reader.GetData() );
                    scope.SetExtra( "WorldItemCount", Engine.Items.Count() );
                    scope.SetExtra( "WorldMobileCount", Engine.Mobiles.Count() );
                } );
            }
        }

        private static void OnSAWorldItem( PacketReader reader )
        {
            reader.ReadInt16(); // WORD 0x01
            byte type = reader.ReadByte(); // Data Type (0x00 = use TileData, 0x01 = use BodyData, 0x02 = use MultiData)
            int serialf3 = reader.ReadInt32();
            //Log.Info( "P Adding 0x{0:x}", serialf3 );
            Item item = Engine.GetOrCreateItem( serialf3 );
            item.ArtDataID = type;
            item.ID = reader.ReadUInt16();
            item.Direction = (Direction) reader.ReadByte();
            item.Count = reader.ReadUInt16();
            reader.ReadInt16(); // Second Amount?
            item.X = reader.ReadInt16();
            item.Y = reader.ReadInt16();
            item.Z = reader.ReadSByte();
            item.Light = reader.ReadByte();
            item.Hue = reader.ReadUInt16();
            item.Flags = reader.ReadByte();
            item.Owner = 0;

            Engine.Items.Add( item );
            NewWorldItemEvent?.Invoke( item );
        }

        private static void OnInitializePlayer( PacketReader reader )
        {
            int serial = reader.ReadInt32();

            PlayerMobile mobile = new PlayerMobile( serial );

            // ClassicUO (or ServUO) seems to send other packets before InitializePlayer that can cause the mobile to already exist before we
            // know it's the player, if already in collection, copy and delete.

            if ( Engine.Mobiles.GetMobile( serial, out Mobile m ) )
            {
                mobile.Equipment = m.Equipment;
                Engine.Mobiles.Remove( m );
            }

            reader.ReadInt32(); // DWORD 0

            short id = reader.ReadInt16();
            short x = reader.ReadInt16();
            short y = reader.ReadInt16();
            short z = reader.ReadInt16();
            byte direction = reader.ReadByte();

            mobile.ID = id;
            mobile.X = x;
            mobile.Y = y;
            mobile.Z = z;
            mobile.Direction = (Direction) direction;

            Engine.SetPlayer( mobile );
        }

        private static void Register( int packetId, int length, OnPacketReceive onReceive )
        {
            _handlers[packetId] = new PacketHandler( packetId, length, onReceive );
        }

        private static void RegisterExtended( int packetId, int length, OnPacketReceive onReceive )
        {
            _extendedHandlers[packetId] = new PacketHandler( packetId, length, onReceive );
        }

        internal static PacketHandler GetHandler( int packetId )
        {
            return _handlers[packetId];
        }

        private static PacketHandler GetExtendedHandler( int packetId )
        {
            return _extendedHandlers[packetId];
        }

        public static void AddToJournal( JournalEntry entry )
        {
            Engine.Journal.Write( entry );
            JournalEntryAddedEvent?.Invoke( entry );
        }

        public static void CAASystemMessageToJournal( string text )
        {
            JournalEntry journalEntry = new JournalEntry
            {
                Serial = -1,
                ID = -1,
                SpeechType = JournalSpeech.System,
                SpeechHue = (int) SystemMessageHues.Normal,
                SpeechFont = 0x03,
                Name = "System",
                Text = text
            };

            AddToJournal( journalEntry );
        }
    }
}