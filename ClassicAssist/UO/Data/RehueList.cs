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

        public void CheckRehue( Item item )
        {
            if ( !_rehueList.TryGetValue( item.Serial, out int hueOverride ) )
            {
                return;
            }

            Engine.SendPacketToClient( new ContainerContentUpdate( item.Serial, item.ID, item.Direction, item.Count,
                item.X, item.Y, item.Grid, item.Owner, hueOverride ) );
        }

        public void CheckRehue( ItemCollection collection )
        {
            int backpack = Engine.Player?.Backpack?.Serial ?? 0;

            foreach ( Item item in collection.GetItems() )
            {
                if ( item.IsDescendantOf( backpack ) )
                {
                    CheckRehue( item );
                }
            }
        }
    }
}