using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Assistant;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Objects
{
    public class Mobile : Entity
    {
        public delegate void dMobileStatusUpdated( MobileStatus oldStatus, MobileStatus newStatus );

        private bool _isDead;

        internal int[] _layerArray = new int[(int) Layer.LastValid + 1];
        private MobileStatus _status;

        public Mobile( int serial ) : base( serial )
        {
            Equipment = new ItemCollection( serial );
        }

        public bool IsMounted => Mount != null;
        public Item Mount => Engine.Items.GetItem( GetLayer( Layer.Mount ) );
        public Item Backpack => Engine.Items.GetItem( GetLayer( Layer.Backpack ) );
        public ItemCollection Equipment { get; set; }

        public int Hits { get; set; }
        public int HitsMax { get; set; }

        public bool IsDead
        {
            get =>
                ID == 0x0192 ||
                ID == 0x0193 ||
                ID >= 0x025F && ID <= 0x0260 ||
                ID == 0x2B6 || ID == 0x02B7 || _isDead;
            set => _isDead = value;
        }

        public int Mana { get; set; }
        public int ManaMax { get; set; }
        public Notoriety Notoriety { get; set; }
        public int Stamina { get; set; }
        public int StaminaMax { get; set; }

        public MobileStatus Status
        {
            get => _status;
            set
            {
                MobileStatusUpdated?.Invoke( _status, value );

                _status = value;
            }
        }

        public event dMobileStatusUpdated MobileStatusUpdated;

        internal void SetLayer( Layer layer, int serial )
        {
            if ( (int) layer >= _layerArray.Length )
            {
                return;
            }

            Interlocked.Exchange( ref _layerArray[(int) layer], serial );
        }

        internal int[] GetAllLayers()
        {
            return _layerArray;
        }

        internal int GetLayer( Layer layer )
        {
            return (int) layer >= _layerArray.Length ? 0 : Thread.VolatileRead( ref _layerArray[(int) layer] );
        }

        public Item[] GetEquippedItems()
        {
            List<Item> itemList = new List<Item>();

            int[] layerArray = GetAllLayers();

            IEnumerable<int> layers = layerArray.Where( layer => layer != 0 );

            foreach ( int layer in layers )
            {
                if ( Engine.Items.GetItem( layer, out Item i ) && i.Layer != Layer.Invalid )
                {
                    itemList.Add( i );
                }
            }

            return itemList.ToArray();
        }
    }
}