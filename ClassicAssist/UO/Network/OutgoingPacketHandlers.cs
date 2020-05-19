using System;
using System.Linq;
using Assistant;
using ClassicAssist.Data.Abilities;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Data.Targeting;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using ClassicAssist.UO.Objects.Gumps;

namespace ClassicAssist.UO.Network
{
    public static class OutgoingPacketHandlers
    {
        public delegate void dGump( uint gumpId, int serial, Gump gump );

        public delegate void dTargetSentEvent( TargetType targetType, int senderSerial, int flags, int serial, int x,
            int y, int z, int id );

        private static PacketHandler[] _handlers;
        private static PacketHandler[] _extendedHandlers;
        public static event dTargetSentEvent TargetSentEvent;
        public static event dGump GumpEvent;

        public static void Initialize()
        {
            _handlers = new PacketHandler[0x100];
            _extendedHandlers = new PacketHandler[0x100];

            Register( 0x02, 7, OnMoveRequested );
            Register( 0x06, 5, OnUseRequest );
            Register( 0x07, 7, OnLiftRequest );
            Register( 0x08, 15, OnDropRequest );
            Register( 0x13, 10, OnEquipRequest );
            Register( 0x6C, 19, OnTargetSent );
            Register( 0xA0, 3, OnPlayServer );
            Register( 0xB1, 0, OnGumpButtonPressed );
            Register( 0xBD, 0, OnClientVersion );
            Register( 0xD7, 0, OnEncodedCommand );
            Register( 0xEF, 31, OnNewClientVersion );
        }

        private static void OnPlayServer( PacketReader reader )
        {
            int index = reader.ReadInt16();

            Engine.CurrentShard = Engine.Shards.FirstOrDefault( i => i.Index == index );
        }

        private static void OnClientVersion( PacketReader reader )
        {
            string version = reader.ReadString();

            Engine.ClientVersion = Version.Parse( version );
        }

        private static void OnEquipRequest( PacketReader reader )
        {
            AbilitiesManager manager = AbilitiesManager.GetInstance();
            manager.ResendGump( manager.Enabled );
        }

        private static void OnLiftRequest( PacketReader reader )
        {
            Engine.LastActionPacket = DateTime.Now;
        }

        private static void OnDropRequest( PacketReader reader )
        {
            Engine.LastActionPacket = DateTime.Now;
        }

        private static void OnEncodedCommand( PacketReader reader )
        {
            int serial = reader.ReadInt32();

            int command = reader.ReadInt16();

            switch ( command )
            {
                case 0x19:
                {
                    reader.ReadByte();

                    int abilityIndex = reader.ReadInt32();

                    AbilitiesManager.GetInstance().CheckAbility( abilityIndex );

                    break;
                }
            }
        }

        private static void OnUseRequest( PacketReader reader )
        {
            Engine.LastActionPacket = DateTime.Now;

            int serial = reader.ReadInt32();

            if ( Engine.Player != null )
            {
                Engine.Player.LastObjectSerial = serial;
            }
        }

        private static void OnMoveRequested( PacketReader reader )
        {
            Direction direction = (Direction) ( reader.ReadByte() & 0x07 );
            int sequence = reader.ReadByte();

            Engine.SetSequence( sequence, direction );
        }

        private static void OnGumpButtonPressed( PacketReader reader )
        {
            int senderSerial = reader.ReadInt32();
            uint gumpId = reader.ReadUInt32();
            int buttonId = reader.ReadInt32();

            Engine.GumpList.TryRemove( gumpId, out _ );

            if ( Engine.Gumps.GetGump( gumpId, out Gump gump ) )
            {
                GumpEvent?.Invoke( gumpId, senderSerial, gump );
            }

            Engine.Gumps.Remove( gumpId, buttonId );
        }

        private static void OnTargetSent( PacketReader reader )
        {
            if ( Engine.Player == null )
            {
                return;
            }

            TargetType targetType = (TargetType) reader.ReadByte();

            Engine.Player.LastTargetType = targetType;

            int senderSerial = reader.ReadInt32(); // sender serial
            int flags = reader.ReadByte();
            int serial = reader.ReadInt32();
            int x = reader.ReadInt16();
            int y = reader.ReadInt16();
            int z = reader.ReadInt16();
            int id = reader.ReadInt16();

            if ( targetType == TargetType.Object && flags != 0x03 && serial != 0 )
            {
                Engine.Player.LastTargetSerial = serial;
                Engine.Player.LastTargetType = targetType;

                switch ( (TargetFlags) flags )
                {
                    case TargetFlags.Harmful when Engine.Mobiles.GetMobile( serial, out Mobile enemyMobile ) &&
                                                  AliasCommands.GetAlias( "enemy" ) != serial:
                        TargetManager.GetInstance().SetEnemy( enemyMobile );
                        break;
                    case TargetFlags.Beneficial when Engine.Mobiles.GetMobile( serial, out Mobile friendMobile ) &&
                                                     MobileCommands.InFriendList( serial ) &&
                                                     AliasCommands.GetAlias( "friend" ) != serial:
                        TargetManager.GetInstance().SetFriend( friendMobile );
                        break;
                }
            }

            Engine.TargetExists = false;

            TargetSentEvent?.Invoke( targetType, senderSerial, flags, serial, x, y, z, id );
        }

        private static void OnNewClientVersion( PacketReader reader )
        {
            reader.ReadInt32();
            Engine.ClientVersion = new Version( reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(),
                reader.ReadInt32() );
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