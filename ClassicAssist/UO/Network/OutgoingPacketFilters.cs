using System;
using System.Collections.Generic;
using System.Linq;
using ClassicAssist.Data;
using ClassicAssist.Resources;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.UO.Network
{
    public static class OutgoingPacketFilters
    {
        private static readonly Dictionary<byte, Func<byte[], int, bool>> _filters =
            new Dictionary<byte, Func<byte[], int, bool>>();

        public static void Initialize()
        {
            Register( 0x05, OnAttackRequested );
        }

        private static bool OnAttackRequested( byte[] packet, int length )
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