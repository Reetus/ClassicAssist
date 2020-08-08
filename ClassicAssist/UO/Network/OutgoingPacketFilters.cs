using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClassicAssist.Data;
using ClassicAssist.Data.Abilities;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.UO.Network
{
    public static class OutgoingPacketFilters
    {
        public delegate bool OnReceive( ref byte[] packet, ref int length );

        private static readonly Dictionary<byte, OnReceive> _filters = new Dictionary<byte, OnReceive>();

        public static void Initialize()
        {
            Register( 0x05, OnAttackRequested );
            Register( 0x06, OnUseRequest );
            Register( 0x80, OnAccountLoginRequest );
            Register( 0x91, OnGameServerLogin );
        }

        private static bool OnUseRequest( ref byte[] packet, ref int length )
        {
            int serial = (packet[1] << 24) | (packet[2] << 16) | (packet[3] << 8) | packet[4];

            if ( Options.CurrentOptions.CheckHandsPotions )
            {
                return AbilitiesManager.GetInstance().CheckHands( serial );
            }

            if ( Options.CurrentOptions.UseObjectQueue )
            {
                ActionPacketQueue.EnqueuePacket( new UseObject( serial ), QueuePriority.High );

                return true;
            }

            return false;
        }

        private static bool OnGameServerLogin( ref byte[] packet, ref int length )
        {
            PacketReader reader = new PacketReader( packet, length, true );

            _ = reader.ReadInt32();

            string username = reader.ReadString( 30 );
            string password = reader.ReadString( 30 );

            if ( !AssistantOptions.SavePasswords )
            {
                return false;
            }

            if ( !AssistantOptions.SavedPasswords.ContainsKey( username ) ||
                 AssistantOptions.SavePasswordsOnlyBlank && !string.IsNullOrEmpty( password ) )
            {
                return false;
            }

            string storedPassword = AssistantOptions.SavedPasswords[username];
            byte[] passwordBytes = Encoding.ASCII.GetBytes( storedPassword );
            byte[] buffer = new byte[30];

            Buffer.BlockCopy( passwordBytes, 0, buffer, 0, passwordBytes.Length );
            Buffer.BlockCopy( buffer, 0, packet, 35, buffer.Length );

            return false;
        }

        private static bool OnAccountLoginRequest( ref byte[] packet, ref int packetLength )
        {
            PacketReader reader = new PacketReader( packet, packetLength, true );

            string username = reader.ReadString( 30 );
            string password = reader.ReadString( 30 );

            if ( !AssistantOptions.SavePasswords )
            {
                return false;
            }

            if ( AssistantOptions.SavedPasswords.ContainsKey( username ) &&
                 ( !AssistantOptions.SavePasswordsOnlyBlank || string.IsNullOrEmpty( password ) ) )
            {
                string storedPassword = AssistantOptions.SavedPasswords[username];
                byte[] passwordBytes = Encoding.ASCII.GetBytes( storedPassword );
                byte[] buffer = new byte[30];

                Buffer.BlockCopy( passwordBytes, 0, buffer, 0, passwordBytes.Length );
                Buffer.BlockCopy( buffer, 0, packet, 31, buffer.Length );

                return false;
            }

            if ( string.IsNullOrEmpty( password ) )
            {
                return false;
            }

            if ( AssistantOptions.SavedPasswords.ContainsKey( username ) )
            {
                AssistantOptions.SavedPasswords.Remove( username );
            }

            AssistantOptions.SavedPasswords.Add( username, password );
            AssistantOptions.OnPasswordsChanged();

            return false;
        }

        private static bool OnAttackRequested( ref byte[] packet, ref int length )
        {
            int serial = ( packet[1] << 24 ) | ( packet[2] << 16 ) | ( packet[3] << 8 ) | packet[4];

            bool block = Options.CurrentOptions.PreventAttackingFriendsInWarMode &&
                         Options.CurrentOptions.Friends.Any( fe => fe.Serial == serial );

            if ( block )
            {
                UOC.SystemMessage( Strings.Attack_request_blocked___ );
            }

            return block;
        }

        private static void Register( byte packetId, OnReceive action )
        {
            if ( !_filters.ContainsKey( packetId ) )
            {
                _filters.Add( packetId, action );
            }
        }

        public static bool CheckPacket( ref byte[] data, ref int length )
        {
            if ( _filters.ContainsKey( data[0] ) && _filters.TryGetValue( data[0], out OnReceive onReceive ) )
            {
                return onReceive.Invoke( ref data, ref length );
            }

            return false;
        }
    }
}