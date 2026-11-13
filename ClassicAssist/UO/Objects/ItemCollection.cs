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

        public ItemCollection( int serial ) : base( DefaultCapacity )
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
                OnCollectionChanged( true, true, new[] { entity } );
            }

            return added;
        }

        public override bool Add( Item[] entities )
        {
            bool added = base.Add( entities );

            if ( added )
            {
                OnCollectionChanged( true, true, entities );
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
                EntityList.TryGetValue( serial, out Item match );

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
                Remove( GetOwnedContents( entity ) );

                changed = base.Remove( entity );
            }

            if ( changed )
            {
                OnCollectionChanged( true, false, new[] { entity } );
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
                    Remove( GetOwnedContents( entity ) );
                }

                if ( base.Remove( entity ) )
                {
                    changed = true;
                }
            }

            if ( changed )
            {
                OnCollectionChanged( true, false, entities );
            }

            return changed;
        }

        /// <summary>
        ///     The contents to remove along with <paramref name="container" />: only what it still owns.
        ///     <para>
        ///         <see cref="Remove(int)" /> takes a serial out of every container that lists it, so an item the
        ///         container still listed after it had moved elsewhere was removed from where it really is. A
        ///         chest swept by distance took what had been moved from it out of the backpack.
        ///     </para>
        /// </summary>
        private static Item[] GetOwnedContents( Item container )
        {
            return container.Container.GetItems().Where( i => i.Owner == container.Serial ).ToArray();
        }

        public override bool Remove( int serial )
        {
            bool changed = false;

            Item item = GetItem( serial );

            if ( EntityList.ContainsKey( serial ) )
            {
                if ( item != null )
                {
                    item.Container = null;
                }

                if ( item != null && item.Owner != 0 )
                {
                    Item owner = Engine.Items.GetItem( item.Owner );

                    // Only purge the item from the owner's container when it's a different
                    // collection to this one. If it's the same collection, base.Remove below
                    // handles the removal and raises CollectionChanged - removing it here first
                    // would leave nothing for base.Remove to remove, swallowing the event.
                    if ( !ReferenceEquals( owner?.Container, this ) && owner?.Container?.GetItem( serial ) != null )
                    {
                        owner.Container.EntityList.TryRemove( serial, out _ );
                    }
                }

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
                OnCollectionChanged( true, false, new[] { item } );
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

        public override void RemoveByDistance( int maxDistance, int x, int y )
        {
            Item[] items = SelectEntities( i => i is Item item && item.Owner == 0 && UOMath.Distance( x, y, item.X, item.Y ) > maxDistance && i.ArtDataID != 2 );

            if ( items == null )
            {
                return;
            }

            bool changed = Remove( items );

            if ( changed )
            {
                OnCollectionChanged( false, items );
            }
        }

        public void ClearMultis()
        {
            Item[] items = SelectEntities( i => i is Item item && i.ArtDataID == 2 );

            if ( items == null )
            {
                return;
            }

            bool changed = Remove( items );

            if ( changed )
            {
                OnCollectionChanged( false, items );
            }
        }

        public override Item[] SelectEntities( Func<Item, bool> func )
        {
            List<Item> itemList = new List<Item>();
            HashSet<int> seen = new HashSet<int>();

            foreach ( Item i in EntityList.Values )
            {
                if ( func( i ) && seen.Add( i.Serial ) )
                {
                    itemList.Add( i );
                }

                if ( !i.IsContainer )
                {
                    continue;
                }

                Item[] result = i.Container.SelectEntities( func );

                if ( result == null )
                {
                    continue;
                }

                // An item can be reachable by more than one path (e.g. a container also held at the top level),
                // only return it once
                foreach ( Item child in result )
                {
                    if ( seen.Add( child.Serial ) )
                    {
                        itemList.Add( child );
                    }
                }
            }

            return itemList.Count > 0 ? itemList.ToArray() : null;
        }

        /// <summary>
        ///     Remove the entry for the given serial from this collection only, without clearing the item's
        ///     contents, touching its owner, or raising CollectionChanged. Used when an item that was held here
        ///     as a fallback is found to belong in another container.
        /// </summary>
        internal bool Detach( int serial )
        {
            return EntityList.TryRemove( serial, out _ );
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

        public void OnCollectionChanged( bool rippleUp, bool added, Item[] items )
        {
            OnCollectionChanged( added, items );

            // Ripple up through every ancestor container so viewers watching a parent
            // collection (e.g. the flattened ShowChildItems view) are notified of changes
            // to any descendant, regardless of nesting depth.

            if ( !rippleUp )
            {
                return;
            }

            if ( Serial == 0 || !Engine.Items.GetItem( Serial, out Item item ) || item.Owner == 0 )
            {
                return;
            }

            if ( Engine.Items.GetItem( item.Owner, out Item parent ) )
            {
                parent.Container?.OnCollectionChanged( true, added, items );
            }
        }

        public override string ToString()
        {
            return $"ItemCollection: Items: {EntityList.Count}, Total Items: {GetTotalItemCount()}";
        }
    }
}