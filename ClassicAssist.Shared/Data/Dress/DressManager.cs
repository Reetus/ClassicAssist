using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ClassicAssist.Annotations;
using ClassicAssist.Misc;
using ClassicAssist.Shared;
using ClassicAssist.UI.Misc;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.Data.Dress
{
    public class DressManager : INotifyPropertyChanged
    {
        private static DressManager _instance;
        private static readonly object _instanceLock = new object();

        private readonly Layer[] _validLayers =
        {
            Layer.Arms, Layer.Bracelet, Layer.Cloak, Layer.Earrings, Layer.Gloves, Layer.Helm, Layer.InnerLegs,
            Layer.InnerTorso, Layer.MiddleTorso, Layer.Neck, Layer.OneHanded, Layer.OuterLegs, Layer.OuterTorso,
            Layer.Pants, Layer.Ring, Layer.Shirt, Layer.Shoes, Layer.Talisman, Layer.TwoHanded, Layer.Waist
        };

        private ObservableCollectionEx<DressAgentEntry> _items = new ObservableCollectionEx<DressAgentEntry>();
        private DressAgentEntry _temporaryDress;
        private bool _useUo3DPackets;

        private DressManager()
        {
            Items.CollectionChanged += OnCollectionChanged;
        }

        public bool IsDressing { get; set; }

        public ObservableCollectionEx<DressAgentEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public DressAgentEntry TemporaryDress
        {
            get => _temporaryDress;
            set => SetProperty( ref _temporaryDress, value );
        }

        public bool UseUO3DPackets
        {
            get => _useUo3DPackets;
            set => SetProperty( ref _useUo3DPackets, value );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            OnPropertyChanged( nameof( Items ) );
        }

        public static DressManager GetInstance()
        {
            // ReSharper disable once InvertIf
            if ( _instance == null )
            {
                lock ( _instanceLock )
                {
                    if ( _instance == null )
                    {
                        _instance = new DressManager();
                    }
                }
            }

            return _instance;
        }

        // ReSharper disable once RedundantAssignment
        public void SetProperty<T>( ref T obj, T value, [CallerMemberName] string propertyName = "" )
        {
            obj = value;
            OnPropertyChanged( propertyName );
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        public async Task UndressAll( CancellationToken cancellationToken = default )
        {
            try
            {
                IsDressing = true;

                PlayerMobile player = Engine.Player;

                if ( player == null )
                {
                    return;
                }

                int backpack = player.Backpack.Serial;

                if ( backpack == 0 || backpack == -1 )
                {
                    return;
                }

                if ( _useUo3DPackets )
                {
                    int[] layers = player.GetEquippedItems().Where( i => IsValidLayer( i.Layer ) )
                        .Select( i => (int) i.Layer ).ToArray();

                    Shared.UO.Commands.UO3DUnequipItems( layers );
                }
                else
                {
                    int[] items = player.GetEquippedItems().Where( i => IsValidLayer( i.Layer ) )
                        .Select( i => i.Serial ).ToArray();

                    foreach ( int item in items )
                    {
                        if ( cancellationToken.IsCancellationRequested )
                        {
                            return;
                        }

                        await ActionPacketQueue.EnqueueDragDrop( item, 1, backpack, QueuePriority.Medium );
                    }
                }
            }
            finally
            {
                IsDressing = false;
            }
        }

        public bool IsValidLayer( Layer layer )
        {
            return _validLayers.Any( l => l == layer );
        }

        public int GetUndressContainer( int undressContainer )
        {
            int container = undressContainer;

            if ( container != 0 && ( Engine.Items.GetItem( container )?.Distance ?? byte.MaxValue ) > 3 ||
                 container == 0 )
            {
                container = Engine.Player?.Backpack?.Serial ?? 0;
            }

            return container;
        }

        public void ImportItems( DressAgentEntry dae )
        {
            PlayerMobile player = Engine.Player;

            List<DressAgentItem> items = player.GetEquippedItems().Where( i => IsValidLayer( i.Layer ) ).Select( i =>
                    new DressAgentItem
                        {
                            Serial = i.Serial, Layer = i.Layer, ID = i.ID, Type = DressAgentItemType.Serial
                        } )
                .ToList();

            dae.Items = items;
        }

        public async Task DressAllItems( DressAgentEntry dae, bool moveConflictingItems )
        {
            try
            {
                IsDressing = true;

                await dae.Dress( moveConflictingItems );
            }
            finally
            {
                IsDressing = false;
            }
        }
    }
}