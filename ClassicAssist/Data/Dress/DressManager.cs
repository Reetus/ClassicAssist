using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Annotations;
using ClassicAssist.UI.Misc;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

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

        public async Task UndressAll()
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

                int[] items = player.GetEquippedItems().Where( i => IsValidLayer( i.Layer ) ).Select( i => i.Serial )
                    .ToArray();

                foreach ( int item in items )
                {
                    Item itemObj = Engine.Items.GetItem( item );

                    await UOC.DragDropAsync( item, 1, backpack );
                    Engine.Player.SetLayer( itemObj?.Layer ?? Layer.Invalid, 0 );
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
    }
}