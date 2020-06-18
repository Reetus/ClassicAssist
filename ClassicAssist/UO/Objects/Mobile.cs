using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Assistant;
using ClassicAssist.Data.Vendors;
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

        public Item Backpack => Engine.Items.GetItem( GetLayer( Layer.Backpack ) );
        public ItemCollection Equipment { get; set; }

        public HealthbarColour HealthbarColour { get; set; }

        public int Hits { get; set; }
        public int HitsMax { get; set; }

        public bool IsDead
        {
            get =>
                ID == 0x0192 || ID == 0x0193 || ID >= 0x025F && ID <= 0x0260 || ID == 0x2B6 || ID == 0x02B7 || _isDead;
            set => _isDead = value;
        }

        public bool IsFrozen => Status.HasFlag( MobileStatus.Frozen );
        public bool IsMounted => Mount != null;

        public bool IsPoisoned
        {
            get
            {
                if ( Engine.ClientVersion != null && Engine.ClientVersion < new Version( 7, 0, 0, 0 ) )
                {
                    return Status.HasFlag( MobileStatus.Flying ) || HealthbarColour.HasFlag( HealthbarColour.Green );
                }

                return HealthbarColour.HasFlag( HealthbarColour.Green );
            }
        }

        public bool IsRenamable { get; set; }
        public bool IsYellowHits => HealthbarColour.HasFlag( HealthbarColour.Yellow );

        public int Mana { get; set; }
        public int ManaMax { get; set; }
        public Item Mount => Engine.Items.GetItem( GetLayer( Layer.Mount ) );
        public Notoriety Notoriety { get; set; }
        public ShopListEntry[] ShopBuy { get; set; }
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

        internal virtual void SetLayer( Layer layer, int serial )
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

        protected override void ToString( StringBuilder sb )
        {
            base.ToString( sb );
            sb.Append( $"Flags: {Status}\n" );
            sb.Append( $"Notoriety: {Notoriety}\n" );
        }
    }
}