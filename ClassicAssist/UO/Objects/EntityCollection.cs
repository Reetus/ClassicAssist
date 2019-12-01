using System;
using System.Collections.Concurrent;
using System.Linq;

namespace ClassicAssist.UO.Objects
{
    public abstract class EntityCollection<T> where T : Entity
    {
        public delegate void dCollectionChanged( int totalCount );

        protected ConcurrentDictionary<int, T> EntityList;

        protected EntityCollection() : this( short.MaxValue )
        {
        }

        protected EntityCollection( int capacity )
        {
            EntityList = new ConcurrentDictionary<int, T>( 16, capacity );
        }

        public T this[ int s ]
        {
            get => EntityList.ContainsKey( s ) ? EntityList[s] : null;
            set => EntityList[s] = value;
        }

        public virtual bool Add( T entity )
        {
            bool changed = EntityList.AddOrUpdate( entity.Serial, entity, ( k, v ) => entity ) != null;

            if ( changed )
            {
                OnCollectionChanged();
            }

            return changed;
        }

        public virtual bool Add( T[] entities )
        {
            bool changed = false;

            foreach ( T entity in entities )
            {
                if ( EntityList.AddOrUpdate( entity.Serial, entity, ( k, v ) => entity ) == null )
                {
                    continue;
                }

                changed = true;
            }

            if ( changed )
            {
                OnCollectionChanged();
            }

            return changed;
        }

        protected T[] GetEntities()
        {
            T[] entityArray = new T[EntityList.Values.Count];
            EntityList.Values.CopyTo( entityArray, 0 );
            return entityArray;
        }

        public virtual bool Remove( T entity )
        {
            return Remove( entity.Serial );
        }

        public virtual bool Remove( int serial )
        {
            EntityList.TryRemove( serial, out T val );

            if ( val != null )
            {
                OnCollectionChanged();
            }

            return val != null;
        }

        public virtual bool Remove( T[] entities )
        {
            bool changed = false;

            foreach ( T entity in entities )
            {
                if ( Remove( entity.Serial ) )
                {
                    changed = true;
                }
            }

            return changed;
        }

        internal virtual void Clear()
        {
            EntityList.Clear();

            OnCollectionChanged();
        }

        public event dCollectionChanged CollectionChanged;

        public virtual void OnCollectionChanged()
        {
            CollectionChanged?.Invoke( EntityList.Count );
        }

        public virtual void RemoveByDistance( int maxDistance, int x, int y )
        {
            T[] items = SelectEntities( i =>
                i is Item item && item.Owner == 0 && UOMath.Distance( x, y, item.X, item.Y ) > maxDistance );

            if ( items == null )
            {
                return;
            }

            bool changed = Remove( items );

            if ( changed )
            {
                OnCollectionChanged();
            }
        }

        public virtual T SelectEntity( Func<T, bool> func )
        {
            if ( func == null )
            {
                return null;
            }

            T entity = EntityList.Select( m => m.Value ).FirstOrDefault( func );

            return entity;
        }

        public virtual T[] SelectEntities( Func<T, bool> func )
        {
            return EntityList.Select( m => m.Value ).Where( func ).ToArray();
        }

        protected T GetEntity( int key )
        {
            EntityList.TryGetValue( key, out T val );

            return val;
        }
    }
}