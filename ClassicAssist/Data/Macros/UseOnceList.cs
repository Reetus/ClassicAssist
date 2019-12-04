using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assistant;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.Data.Macros
{
    internal class UseOnceList : IEnumerable<int>
    {
        private readonly List<int> _useOnce = new List<int>();

        public IEnumerator<int> GetEnumerator()
        {
            return _useOnce.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add( int serial )
        {
            _useOnce.Add( serial );
            Engine.RehueList.Add( serial, 766 );
        }

        public void Add( Item item )
        {
            Add( item.Serial );

            int backpack = Engine.Player?.Backpack?.Serial ?? -1;

            if ( item.IsDescendantOf( backpack ) )
            {
                Engine.RehueList.CheckRehue( item );
            }
        }

        public void Remove( Item item )
        {
            Engine.RehueList.Remove( item.Serial );

            Engine.SendPacketToClient( new ContainerContentUpdate( item.Serial, item.ID, item.Direction, item.Count,
                item.X, item.Y, item.Grid, item.Owner, item.Hue ) );

            _useOnce.Remove( item.Serial );
        }

        public bool Contains( int serial )
        {
            return _useOnce.Any( i => i == serial );
        }

        public void Clear()
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for ( int index = 0; index < _useOnce.Count; index++ )
            {
                int serial = _useOnce[index];

                if ( Engine.Items.GetItem( serial, out Item item ) )
                {
                    Remove( item );
                }
            }
        }
    }
}