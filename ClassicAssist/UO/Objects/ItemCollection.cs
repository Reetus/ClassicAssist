using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assistant;

namespace ClassicAssist.UO.Objects
{
    public sealed class ItemCollection : EntityCollection<Item>, IEnumerable<Item>
    {
        public delegate void dContainerContentsChanged( bool added, IEnumerable<Item> items );

        private const int DefaultCapacity = 125;
        public readonly int Serial;

        internal ItemCollection( int serial ) : base( DefaultCapacity )
        {
            Serial = serial;
        }

        public IEnumerator<Item> GetEnumerator()
        {
            return EntityList.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool FindItem( int id, out Item item )
        {
            item = SelectEntity( i => i.ID == id );

            if ( item != null )
            {
                return true;
            }

            item = null;

            return false;
        }

        public override bool Add( Item entity )
        {
            bool added = base.Add( entity );

            if ( added )
            {
                OnCollectionChanged( true );
            }

            return added;
        }

        public override bool Add( Item[] entities )
        {
            bool added = base.Add( entities );

            if ( added )
            {
                OnCollectionChanged( true );
            }

            return added;
        }

        public bool FindItems( int id, out Item[] items )
        {
            items = SelectEntities( i => i.ID == id );

            if ( items != null )
            {
                return true;
            }

            items = null;

            return false;
        }

        /// <summary>
        ///     Flatten items + subitems into a single array.
        /// </summary>
        /// <param name="items">Item array to flatten.</param>
        /// <returns>Array of flattened items.</returns>
        public static Item[] GetAllItems( Item[] items )
        {
            List<Item> output = new List<Item>();

            for ( int i = 0; i < items.Length; i++ )
            {
                output.Add( items[i] );

                if ( items[i].IsContainer )
                {
                    output.AddRange( GetAllItems( items[i].Container.GetItems() ) );
                }
            }

            return output.ToArray();
        }

        /// <summary>
        ///     Get item with specified serial.
        /// </summary>
        /// <param name="serial">Serial of item to retrieve.</param>
        /// <param name="item">Item (out).</param>
        /// <returns>Null if no match is found.</returns>
        public bool GetItem( int serial, out Item item )
        {
            try
            {
                Item match = EntityList.Values.FirstOrDefault( i => i.Serial == serial );

                if ( match != null )
                {
                    item = match;

                    return true;
                }

                IEnumerable<Item> containers = EntityList.Values.Where( i => i.IsContainer );

                foreach ( Item container in containers )
                {
                    if ( !container.Container.GetItem( serial, out Item containerMatch ) )
                    {
                        continue;
                    }

                    item = containerMatch;
                    return true;
                }
            }
            catch ( Exception )
            {
                // ignored
            }

            item = null;

            return false;
        }

        public Item GetItem( int serial )
        {
            GetItem( serial, out Item item );

            return item;
        }

        /// <summary>
        ///     Get all items contained within this container.
        /// </summary>
        public Item[] GetItems()
        {
            Item[] itemArray = new Item[EntityList.Values.Count];
            EntityList.Values.CopyTo( itemArray, 0 );

            return itemArray;
        }

        public int GetItemCount()
        {
            return EntityList.Count;
        }

        public int GetTotalItemCount()
        {
            int count = 0;

            foreach ( Item item in EntityList.Values )
            {
                count++;

                if ( item.IsContainer )
                {
                    count += item.Container.GetTotalItemCount();
                }
            }

            return count;
        }

        public static implicit operator int( ItemCollection i )
        {
            return i.Serial;
        }

        public override bool Remove( Item entity )
        {
            bool changed;

            if ( entity.Container == null )
            {
                changed = base.Remove( entity );
            }
            else
            {
                Remove( entity.Container.GetItems() );

                changed = base.Remove( entity );
            }

            if ( changed )
            {
                OnCollectionChanged( true );
            }

            return changed;
        }

        public override bool Remove( Item[] entities )
        {
            bool changed = false;

            if ( entities == null )
            {
                return false;
            }

            foreach ( Item entity in entities )
            {
                if ( entity == null )
                {
                    continue;
                }

                if ( entity.Container != null )
                {
                    Remove( entity.Container.GetItems() );
                }

                if ( base.Remove( entity ) )
                {
                    changed = true;
                }
            }

            return changed;
        }

        public override bool Remove( int serial )
        {
            bool changed = false;

            Item item = GetItem( serial );

            if ( EntityList.ContainsKey( serial ) )
            {
                changed = base.Remove( serial );
            }
            else
            {
                IEnumerable<Item> containers = EntityList.Values.Where( i => i.IsContainer );

                foreach ( Item container in containers )
                {
                    if ( container.Container?.GetItem( serial ) == null )
                    {
                        continue;
                    }

                    container.Container.Remove( serial );
                }
            }

            if ( changed && item != null )
            {
                OnCollectionChanged( true );
            }

            return changed;
        }

        internal void RemoveByOwner( int serial )
        {
            Item[] items = SelectEntities( i => i.Owner == serial );

            if ( items == null )
            {
                return;
            }

            Remove( items );
        }

        public override Item[] SelectEntities( Func<Item, bool> func )
        {
            List<Item> itemList = new List<Item>();
            IEnumerable<Item> ents = EntityList.Select( m => m.Value ).Where( m => func( m ) || m.IsContainer );

            foreach ( Item i in ents )
            {
                if ( func( i ) && !itemList.Contains( i ) )
                {
                    itemList.Add( i );
                }

                if ( !i.IsContainer )
                {
                    continue;
                }

                Item[] result = i.Container.SelectEntities( func );

                if ( result != null )
                {
                    itemList.AddRange( result );
                }
            }

            return itemList.Count > 0 ? itemList.ToArray() : null;
        }

        public override Item SelectEntity( Func<Item, bool> func )
        {
            Item item = base.SelectEntity( func );

            if ( item != null )
            {
                return item;
            }

            IEnumerable<Item> containers = EntityList.Select( i => i.Value ).Where( i => i.Container != null );

            foreach ( Item container in containers )
            {
                item = container.Container.SelectEntity( func );

                if ( item != null )
                {
                    return item;
                }
            }

            return null;
        }

        public void OnCollectionChanged( bool rippleDown )
        {
            base.OnCollectionChanged();

            // Ripple down

            if ( !rippleDown )
            {
                return;
            }

            if ( Serial == 0 || !Engine.Items.GetItem( Serial, out Item item ) || item.Owner == 0 )
            {
                return;
            }

            if ( Engine.Items.GetItem( item.Owner, out Item parent ) )
            {
                parent.Container?.OnCollectionChanged( false );
            }
        }

        public override string ToString()
        {
            return $"ItemCollection: Items: {EntityList.Count}, Total Items: {GetTotalItemCount()}";
        }
    }
}