using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Assistant;
using ClassicAssist.Data.Abilities;
using ClassicAssist.Data.Counters;
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

        public delegate void dMenuClick( int serial, int gumpId, int index, int id, int hue );

        public delegate void dShardChanged( string name );

        public delegate void dTargetSentEvent( TargetType targetType, int senderSerial, int flags, int serial, int x,
            int y, int z, int id );

        private static PacketHandler[] _handlers;
        private static PacketHandler[] _extendedHandlers;
        public static event dTargetSentEvent TargetSentEvent;
        public static event dGump GumpEvent;
        public static event dShardChanged ShardChangedEvent;

        public static event dMenuClick MenuClickedEvent;

        public static void Initialize()
        {
            _handlers = new PacketHandler[0x100];
            _extendedHandlers = new PacketHandler[0x100];

            Register( 0x02, 7, OnMoveRequested );
            Register( 0x06, 5, OnUseRequest );
            Register( 0x07, 7, OnLiftRequest );
            Register( 0x08, 15, OnDropRequest );
            Register( 0x12, 0, OnUseSkillOrLegacySpell );
            Register( 0x13, 10, OnEquipRequest );
            Register( 0x6C, 19, OnTargetSent );
            Register( 0x6F, 0, OnSecureTrade );
            Register( 0x7D, 13, OnMenuResponse );
            Register( 0xA0, 3, OnPlayServer );
            Register( 0xB1, 0, OnGumpButtonPressed );
            Register( 0xBD, 0, OnClientVersion );
            Register( 0xBF, 0, OnExtendedCommand );
            Register( 0xD7, 0, OnEncodedCommand );
            Register( 0xEF, 31, OnNewClientVersion );
            RegisterExtended( 0x1C, 0, OnSpellCast );
        }

        private static void OnSecureTrade( PacketReader reader )
        {
            byte action = reader.ReadByte();
            int serial = reader.ReadInt32();
            int value1 = reader.ReadInt32();
            int value2 = reader.ReadInt32();

            TradeAction tradeAction = (TradeAction) action;

            if ( tradeAction != TradeAction.Gold )
            {
                return;
            }

            Engine.Trade.GoldLocal = value1;
            Engine.Trade.PlatinumLocal = value2;
        }

        private static void OnUseSkillOrLegacySpell( PacketReader reader )
        {
            int command = reader.ReadByte();

            if ( command == 0x24 )
            {
                OnUseSkill( reader );
            }
            else if (  command == 0x56 )
            {
                OnLegacySpellCast( reader );
            }
        }

        private static void OnLegacySpellCast( PacketReader reader )
        {
            if ( ReadIdAsString( reader, out int id ) )
            {
                Engine.LastSpellID = id;
            }
        }

        private static void OnUseSkill( PacketReader reader )
        {
            if ( ReadIdAsString( reader, out int id ) )
            {
                Engine.LastSkillID = id;
                Engine.LastSkillTime = DateTime.Now;
            }
        }

        private static bool ReadIdAsString( PacketReader reader, out int id )
        {
            Span<byte> span = new Span<byte>( reader.GetData(), (int) reader.Index,
                            (int) ( reader.Size - reader.Index ) );

            string skill = Encoding.ASCII.GetString( span.ToArray() );

            if ( !int.TryParse( skill.Substring( 0, skill.IndexOf( ' ' ) ), out id ) )
            {
                return false;
            }

            return true;
        }

        private static void OnSpellCast( PacketReader reader )
        {
            int type = reader.ReadInt16();

            if ( type == 0 )
            {
                reader.Seek( 4, SeekOrigin.Current );
            }

            Engine.LastSpellID = reader.ReadInt16();
        }

        private static void OnExtendedCommand( PacketReader reader )
        {
            int command = reader.ReadInt16();

            PacketHandler handler = GetExtendedHandler( command );
            handler?.OnReceive( reader );
        }

        private static void OnMenuResponse( PacketReader reader )
        {
            int serial = reader.ReadInt32();
            int gumpId = reader.ReadInt16();
            int index = reader.ReadInt16();
            int id = reader.ReadInt16();
            int hue = reader.ReadInt16();

            Engine.Menus.Remove( gumpId );

            MenuClickedEvent?.Invoke( serial, gumpId, index, id, hue );
        }

        private static void OnPlayServer( PacketReader reader )
        {
            int index = reader.ReadInt16();

            Engine.CurrentShard = Engine.Shards.FirstOrDefault( i => i.Index == index );
            ShardChangedEvent?.Invoke( Engine.CurrentShard?.Name );
        }

        private static void OnClientVersion( PacketReader reader )
        {
            string version = reader.ReadString();

            version = Regex.Replace( version, "[^0-9\\.]", "" );

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
            Engine.Player.Holding = reader.ReadInt32();
            Engine.Player.HoldingAmount = reader.ReadUInt16();
            CountersManager.GetInstance().RecountAll?.Invoke();
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
            int switchesCount = reader.ReadInt32();

            int[] switches = null;

            for ( int i = 0; i < switchesCount; i++ )
            {
                if ( switches == null )
                {
                    switches = new int[switchesCount];
                }

                switches[i] = reader.ReadInt32();
            }

            int textEntryCount = reader.ReadInt32();
            List<(int Key, string Value)> textEntries = new List<(int Key, string Value)>();

            for ( int i = 0; i < textEntryCount; i++ )
            {
                int id = reader.ReadInt16();
                int length = reader.ReadInt16();

                textEntries.Add( ( id, reader.ReadUnicodeStringBE( length ) ) );
            }

            Engine.GumpList.TryRemove( gumpId, out _ );

            if ( Engine.Gumps.GetGump( gumpId, out Gump gump ) )
            {
                GumpEvent?.Invoke( gumpId, senderSerial, gump );
            }

            Engine.Gumps.Remove( gumpId, buttonId, switches, textEntries );
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