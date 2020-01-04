using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Abilities;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Data.Skills;
using ClassicAssist.Data.Vendors;
using ClassicAssist.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using ClassicAssist.UO.Objects.Gumps;

namespace ClassicAssist.UO.Network
{
    public static class IncomingPacketHandlers
    {
        public delegate void dBufficonEnabledDisabled( int type, bool enabled );

        public delegate void dContainerContents( int serial, ItemCollection container );

        public delegate void dGump( int gumpId, int serial, Gump gump );

        public delegate void dJournalEntryAdded( JournalEntry je );

        public delegate void dMapChanged( Map newMap );

        public delegate void dMobileIncoming( Mobile mobile, ItemCollection equipment );

        public delegate void dMobileUpdated( Mobile mobile );

        public delegate void dSkillList( SkillInfo[] skills );

        public delegate void dSkillUpdated( int id, float value, float baseValue, LockStatus lockStatus,
            float skillCap );

        public delegate void dToggleSpecialMove( int spellID, bool enabled );

        public delegate void dVendorBuyDisplay( int serial, ShopListEntry[] entries );

        public delegate void dVendorSellDisplay( int serial, SellListEntry[] entries );

        private static PacketHandler[] _handlers;
        private static PacketHandler[] _extendedHandlers;

        public static event dVendorSellDisplay VendorSellDisplayEvent;
        public static event dJournalEntryAdded JournalEntryAddedEvent;
        public static event dSkillUpdated SkillUpdatedEvent;
        public static event dSkillList SkillsListEvent;
        public static event dMobileUpdated MobileUpdatedEvent;
        public static event dMobileIncoming MobileIncomingEvent;
        public static event dContainerContents ContainerContentsEvent;
        public static event dGump GumpEvent;
        public static event dBufficonEnabledDisabled BufficonEnabledDisabledEvent;

        public static void Initialize()
        {
            _handlers = new PacketHandler[0x100];
            _extendedHandlers = new PacketHandler[0x100];

            Register( 0x11, 0, OnMobileStatus );
            Register( 0x17, 0, OnHealthbarColour );
            Register( 0x1B, 37, OnInitializePlayer );
            Register( 0x1C, 0, OnASCIIText );
            Register( 0x1D, 5, OnItemDeleted );
            Register( 0x20, 19, OnMobileUpdated );
            Register( 0x21, 8, OnMoveRejected );
            Register( 0x22, 3, OnMoveAccepted );
            Register( 0x24, 9, OnContainerDisplay );
            Register( 0x25, 21, OnItemAddedToContainer );
            Register( 0x2E, 15, OnItemEquipped );
            Register( 0x3A, 0, OnSkillsList );
            Register( 0x3C, 0, OnContainerContents );
            Register( 0x6C, 19, OnTarget );
            Register( 0x74, 0, OnShopList );
            Register( 0x77, 17, OnMobileMoving );
            Register( 0x78, 0, OnMobileIncoming );
            Register( 0x98, 0, OnMobileName );
            Register( 0x9E, 0, OnShopSell );
            Register( 0xA1, 9, OnMobileHits );
            Register( 0xA2, 9, OnMobileMana );
            Register( 0xA3, 9, OnMobileStamina );
            Register( 0xAE, 0, OnUnicodeText );
            Register( 0xB9, 5, OnSupportedFeatures );
            Register( 0xBF, 0, OnExtendedCommand );
            Register( 0xC1, 0, OnLocalizedText );
            Register( 0xD6, 0, OnProperties );
            Register( 0xDD, 0, OnCompressedGump );
            Register( 0xDF, 0, OnBuffAndAttributes );
            Register( 0xF3, 26, OnSAWorldItem );

            RegisterExtended( 0x04, 0, OnCloseGump );
            RegisterExtended( 0x06, 0, OnPartyCommand );
            RegisterExtended( 0x08, 0, OnMapChange );
            RegisterExtended( 0x21, 0, OnClearWeaponAbility );
            RegisterExtended( 0x25, 0, OnToggleSpecialMoves );
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
                Commands.SystemMessage( Strings.Current_Ability_Cleared );
            }

            manager.Enabled = AbilityType.None;
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

            if ( type < 0x3ea )
            {
                return;
            }

            bool enabled = count > 0;

            BufficonEnabledDisabledEvent?.Invoke( type, enabled );
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
                Arguments = reader.ReadUnicodeString( (int) reader.Size - 50 ).Split( '\t' )
            };

            journalEntry.Text = Cliloc.GetLocalString( journalEntry.Cliloc, journalEntry.Arguments );

            if ( journalEntry.SpeechType == JournalSpeech.Label )
            {
                if ( Engine.Player?.LastTargetSerial == journalEntry.Serial )
                {
                    MsgCommands.HeadMsg( "[Last Target]", journalEntry.Serial );
                }

                if ( Engine.Player?.EnemyTargetSerial == journalEntry.Serial )
                {
                    MsgCommands.HeadMsg( "[Enemy]", journalEntry.Serial );
                }

                if ( Engine.Player?.FriendTargetSerial == journalEntry.Serial )
                {
                    MsgCommands.HeadMsg( "[Friend]", journalEntry.Serial );
                }
            }

            Engine.Journal.Write( journalEntry );
            JournalEntryAddedEvent?.Invoke( journalEntry );
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
            }
        }

        private static void OnHealthbarColour( PacketReader reader )
        {
            int serial = reader.ReadInt32();
            reader.ReadInt16(); // 0x01;

            int status = reader.ReadInt16();
            int flags = reader.ReadByte();

            HealthbarColour healthbar = HealthbarColour.None;

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

            Mobile mobile = Engine.Mobiles.GetMobile( serial );

            if ( mobile == null )
            {
                return;
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

        private static void OnCloseGump( PacketReader reader )
        {
            int gumpID = reader.ReadInt32();
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

            Engine.Journal.Write( journalEntry );
            JournalEntryAddedEvent?.Invoke( journalEntry );
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

            Engine.Journal.Write( journalEntry );
            JournalEntryAddedEvent?.Invoke( journalEntry );
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
            int gumpId = reader.ReadInt32();

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

            string layout = Encoding.ASCII.GetString( decompressedBuffer );

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

            Gump gump = GumpParser.Parse( senderSerial, gumpId, x, y, layout, text );
            Engine.Gumps.Add( gump );

            GumpEvent?.Invoke( gumpId, senderSerial, gump );
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

            MobileUpdatedEvent?.Invoke( mobile );
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

            if ( !Options.CurrentOptions.QueueLastTarget || (TargetType) type != TargetType.Object ||
                 Engine.LastTargetQueue.Count == 0 )
            {
                return;
            }

            int serial = Engine.LastTargetQueue.Dequeue();

            if ( serial == 0 )
            {
                return;
            }

            TargetCommands.Target( serial, Options.CurrentOptions.RangeCheckLastTarget );

            Engine.TargetExists = false;
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
            /*byte type = */
            reader.ReadByte();
            int id = reader.ReadInt16();
            int value = reader.ReadInt16();
            int baseValue = reader.ReadInt16();
            LockStatus lockStatus = (LockStatus) reader.ReadByte();
            int skillCap = reader.ReadInt16();

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
                    skillCap = reader.ReadInt16();

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
            int command = reader.ReadInt16();

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
            int grid = reader.ReadByte();
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
                Engine.RehueList.CheckRehue( item );
            }

            //rehueList
        }

        private static void OnItemDeleted( PacketReader reader )
        {
            int serial = reader.ReadInt32();

            Mobile mobile = Engine.Mobiles
                .FirstOrDefault( m => m.GetEquippedItems().Any( i => i.Serial == serial ) );

            if ( mobile != null )
            {
                Item item = Engine.Items.GetItem( serial );

                if ( item != null )
                {
                    mobile.SetLayer( item.Layer, 0 );
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

            Mobile mobile = Engine.GetOrCreateMobile( mobileSerial );

            mobile.SetLayer( item.Layer, item.Serial );
            mobile.Equipment.Add( item );
            Engine.Items.Add( item );
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

            if ( Engine.Player?.Serial == serial )
            {
                Engine.Player.Name = name;
                Engine.Player.Properties = list.ToArray();
            }
            else if ( UOMath.IsMobile( serial ) )
            {
                Mobile mobile = Engine.GetOrCreateMobile( serial );

                mobile.Name = name;
                mobile.Properties = list.ToArray();
            }
            else
            {
                Item item = Engine.GetOrCreateItem( serial );

                item.Name = name;
                item.Properties = list.ToArray();
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

            Mobile mobile = serial == Engine.Player.Serial ? Engine.Player : Engine.GetOrCreateMobile( serial );
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

            bool useNewIncoming = Engine.ClientVersion >= new Version( 7, 0, 33, 1 );

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

            MobileIncomingEvent?.Invoke( mobile, container );
        }

        private static void OnContainerContents( PacketReader reader )
        {
            if ( reader.Size == 5 )
            {
                return;
            }

            bool oldStyle = false;

            int count = reader.ReadInt16();

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

            Item containerItem = Engine.Items.GetItem( container.Serial );

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

            ContainerContentsEvent?.Invoke( container.Serial, container );

            Engine.RehueList.CheckRehue( container );
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
    }
}