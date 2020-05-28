using System.Collections.Generic;
using Assistant;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UO.Data
{
    public class RehueList
    {
        private readonly Dictionary<int, int> _rehueList = new Dictionary<int, int>();

        public void Add( int serial, int hue )
        {
            if ( _rehueList.ContainsKey( serial ) )
            {
                _rehueList.Remove( serial );
            }

            _rehueList.Add( serial, hue );
        }

        public bool Remove( int serial )
        {
            return _rehueList.ContainsKey( serial ) && _rehueList.Remove( serial );
        }

        public bool Contains( int serial )
        {
            return _rehueList.ContainsKey( serial );
        }

        public void CheckItem( Item item )
        {
            if ( !_rehueList.TryGetValue( item.Serial, out int hueOverride ) )
            {
                return;
            }

            if ( item.Owner != 0 && !UOMath.IsMobile( item.Serial ) )
            {
                Engine.SendPacketToClient( new ContainerContentUpdate( item.Serial, item.ID, item.Direction, item.Count,
                    item.X, item.Y, item.Grid, item.Owner, hueOverride ) );
            }
            else
            {
                // TODO
                Commands.Resync();
            }
        }

        public bool CheckSAWorldItem( ref byte[] packet, ref int length )
        {
            int serial = ( packet[4] << 24 ) | ( packet[5] << 16 ) | ( packet[6] << 8 ) | packet[7];

            if ( !_rehueList.TryGetValue( serial, out int hueOverride ) )
            {
                return false;
            }

            Engine.SendPacketToClient( new SAWorldItem( packet, length, hueOverride ) );
            return true;
        }

        public bool CheckMobileIncoming( Mobile mobile, ItemCollection equipment )
        {
            if ( !_rehueList.TryGetValue( mobile.Serial, out int hueOverride ) )
            {
                return false;
            }

            Engine.SendPacketToClient( new MobileIncoming( mobile, equipment, hueOverride ) );
            return true;
        }

        public bool CheckMobileUpdate( Mobile mobile )
        {
            if ( !_rehueList.TryGetValue( mobile.Serial, out int hueOverride ) )
            {
                return false;
            }

            Engine.SendPacketToClient( new MobileUpdate( mobile.Serial, mobile.ID,
                hueOverride > 0 ? hueOverride : mobile.Hue, mobile.Status, mobile.X, mobile.Y, mobile.Z,
                mobile.Direction ) );
            return true;
        }

        public void CheckContainer( ItemCollection collection )
        {
            int backpack = Engine.Player?.Backpack?.Serial ?? 0;

            foreach ( Item item in collection.GetItems() )
            {
                if ( item.IsDescendantOf( backpack ) )
                {
                    CheckItem( item );
                }
            }
        }

        public bool CheckMobileMoving( Mobile mobile )
        {
            if ( !_rehueList.TryGetValue( mobile.Serial, out int hueOverride ) )
            {
                return false;
            }

            Engine.SendPacketToClient( new MobileMoving( mobile, hueOverride ) );
            return true;
        }
    }
}