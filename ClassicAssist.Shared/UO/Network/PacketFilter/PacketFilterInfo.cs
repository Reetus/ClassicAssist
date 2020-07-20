using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassicAssist.UO.Network.PacketFilter
{
    public class PacketFilterInfo
    {
        private PacketFilterCondition[] _conditions;

        public PacketFilterInfo( int packetId, Action<byte[], PacketFilterInfo> onMatch = null )
        {
            PacketID = packetId;
            _conditions = null;
            Action = onMatch;
        }

        public PacketFilterInfo( int packetid, PacketFilterCondition[] conditions,
            Action<byte[], PacketFilterInfo> onMatch = null )
        {
            PacketID = packetid;
            _conditions = conditions;
            Action = onMatch;
        }

        public Action<byte[], PacketFilterInfo> Action { get; set; }

        public int PacketID { get; set; }

        public override bool Equals( object obj )
        {
            if ( !( obj is PacketFilterInfo pfi ) )
            {
                return false;
            }

            // Disabled because won't remove filter as Action == null in Remove methods.
            //if ( Action != pfi.Action )
            //    return false;

            if ( GetConditions() == null && pfi.GetConditions() != null )
            {
                return false;
            }

            if ( pfi.GetConditions() == null && GetConditions() != null )
            {
                return false;
            }

            if ( GetConditions() == null && pfi.GetConditions() == null )
            {
                return PacketID == pfi.PacketID;
            }

            return PacketID == pfi.PacketID && GetConditions().SequenceEqual( pfi.GetConditions() );
        }

        public PacketFilterCondition[] GetConditions()
        {
            return _conditions;
        }

        public override int GetHashCode()
        {
            int hashCode = -1985156662;
            hashCode = hashCode * -1521134295 +
                       EqualityComparer<PacketFilterCondition[]>.Default.GetHashCode( _conditions );
            hashCode = hashCode * -1521134295 + PacketID.GetHashCode();
            hashCode = hashCode * -1521134295 +
                       EqualityComparer<Action<byte[], PacketFilterInfo>>.Default.GetHashCode( Action );

            return hashCode;
        }

        public void SetConditions( PacketFilterCondition[] value )
        {
            _conditions = value;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat( "PacketID: 0x{0:X}\r\n", PacketID );

            if ( _conditions == null )
            {
                sb.AppendLine( "No conditions." );
            }
            else
            {
                foreach ( PacketFilterCondition pfc in _conditions )
                {
                    sb.Append( pfc );
                }
            }

            return sb.ToString();
        }
    }
}