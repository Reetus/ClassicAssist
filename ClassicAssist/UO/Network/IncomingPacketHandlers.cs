using Assistant;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UO.Network
{
    public static class IncomingPacketHandlers
    {
        private static PacketHandler[] _handlers;
        private static PacketHandler[] _extendedHandlers;

        public static void Initialize()
        {
            _handlers = new PacketHandler[0x100];
            _extendedHandlers = new PacketHandler[0x100];

            Register( 0x1B, 37, OnInitializePlayer );
            Register( 0x3C, 0, OnContainerContents );
            Register( 0xF3, 26, OnSAWorldItem );
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
                    container = new ItemCollection( containerSerial, count );
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

            int zero = reader.ReadInt32(); // DWORD 0
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