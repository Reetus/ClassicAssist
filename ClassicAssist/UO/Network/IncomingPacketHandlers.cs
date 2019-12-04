using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Skills;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UO.Network
{
    public static class IncomingPacketHandlers
    {
        public delegate void dContainerContents( int serial, ItemCollection container );

        public delegate void dJournalEntryAdded( JournalEntry je );

        public delegate void dMobileIncoming( Mobile mobile, ItemCollection equipment );

        public delegate void dMobileUpdated( Mobile mobile );

        public delegate void dSkillList( SkillInfo[] skills );

        public delegate void dSkillUpdated( int id, float value, float baseValue, LockStatus lockStatus,
            float skillCap );

        private static PacketHandler[] _handlers;
        private static PacketHandler[] _extendedHandlers;

        public static event dJournalEntryAdded JournalEntryAddedEvent;

        public static event dSkillUpdated SkillUpdatedEvent;
        public static event dSkillList SkillsListEvent;

        public static event dMobileUpdated MobileUpdatedEvent;
        public static event dMobileIncoming MobileIncomingEvent;

        public static event dContainerContents ContainerContentsEvent;

        public static void Initialize()
        {
            _handlers = new PacketHandler[0x100];
            _extendedHandlers = new PacketHandler[0x100];

            Register( 0x11, 0, OnMobileStatus );
            Register( 0x1B, 37, OnInitializePlayer );
            Register( 0x1C, 0, OnASCIIText );
            Register( 0x1D, 5, OnItemDeleted );
            Register( 0x20, 19, OnMobileUpdated );
            Register( 0x21, 8, OnMoveRejected );
            Register( 0x22, 3, OnMoveAccepted );
            Register( 0x25, 21, OnItemAddedToContainer );
            Register( 0x2E, 15, OnItemEquipped );
            Register( 0x3A, 0, OnSkillsList );
            Register( 0x3C, 0, OnContainerContents );
            Register( 0x6C, 19, OnTarget );
            Register( 0x77, 17, OnMobileMoving );
            Register( 0x78, 0, OnMobileIncoming );
            Register( 0x98, 0, OnMobileName );
            Register( 0xA1, 9, OnMobileHits );
            Register( 0xA2, 9, OnMobileMana );
            Register( 0xA3, 9, OnMobileStamina );
            Register( 0xAE, 0, OnUnicodeText );
            Register( 0xB9, 5, OnSupportedFeatures );
            Register( 0xBF, 0, OnExtendedCommand );
            Register( 0xD6, 0, OnProperties );
            Register( 0xDD, 0, OnCompressedGump );
            Register( 0xF3, 26, OnSAWorldItem );

            RegisterExtended( 0x08, 0, OnMapChange );
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
            int flags = reader.ReadByte();

            Engine.TargetType = (TargetType) type;
            Engine.TargetSerial = tid;
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
            if ( Engine.Player != null )
            {
                Engine.Player.Map = (Map) reader.ReadByte();
            }
        }

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

            Mobile mobile = serial == Engine.Player.Serial ? Engine.Player : Engine.GetOrCreateMobile( mobileSerial );

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
            _handlers[packetId] = new PacketHandler( packetId, length, onReceive );
        }

        internal static PacketHandler GetHandler( int packetId )
        {
            return _handlers[packetId];
        }

        private static PacketHandler GetExtendedHandler( int packetId )
        {
            return _handlers[packetId];
        }
    }
}