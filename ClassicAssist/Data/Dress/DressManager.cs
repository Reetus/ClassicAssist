using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.Data.Dress
{
    public class DressManager : SetPropertyNotifyChanged
    {
        private static DressManager _instance;
        private static readonly object _instanceLock = new object();

        private readonly Layer[] _validLayers =
        {
            Layer.Arms, Layer.Bracelet, Layer.Cloak, Layer.Earrings, Layer.Gloves, Layer.Helm, Layer.InnerLegs,
            Layer.InnerTorso, Layer.MiddleTorso, Layer.Neck, Layer.OneHanded, Layer.OuterLegs, Layer.OuterTorso,
            Layer.Pants, Layer.Ring, Layer.Shirt, Layer.Shoes, Layer.Talisman, Layer.TwoHanded, Layer.Waist
        };

        private CancellationTokenSource _cancellationTokenSource;

        private DressAgentEntry _temporaryDress;
        private bool _useUo3DPackets;

        private DressManager()
        {
        }

        public Action<string> InvokeByName { get; set; }

        public bool IsDressing { get; set; }

        public ObservableCollectionEx<DressAgentEntry> Items { get; set; }

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

        public async Task UndressAll()
        {
            try
            {
                IsDressing = true;

                _cancellationTokenSource = new CancellationTokenSource();

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

                    UO.Commands.UO3DUnequipItems( layers );
                }
                else
                {
                    int[] items = player.GetEquippedItems().Where( i => IsValidLayer( i.Layer ) )
                        .Select( i => i.Serial ).ToArray();

                    foreach ( int item in items )
                    {
                        await ActionPacketQueue.EnqueueDragDrop( item, 1, backpack, QueuePriority.Medium, cancellationToken: _cancellationTokenSource.Token );
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
                if ( IsDressing )
                {
                    UO.Commands.SystemMessage( Strings.Dress_already_in_progress___, (int) SystemMessageHues.Red );
                    return;
                }

                IsDressing = true;
                _cancellationTokenSource = new CancellationTokenSource();

                await dae.Dress( _cancellationTokenSource.Token, moveConflictingItems );
            }
            finally
            {
                IsDressing = false;
            }
        }

        public async Task Undress( DressAgentEntry dae )
        {
            try
            {
                if ( IsDressing )
                {
                    UO.Commands.SystemMessage( Strings.Dress_already_in_progress___, (int) SystemMessageHues.Red );
                    return;
                }

                IsDressing = true;
                _cancellationTokenSource = new CancellationTokenSource();

                await dae.Undress( _cancellationTokenSource.Token );
            }
            finally
            {
                IsDressing = false;
            }
        }

        public void Stop()
        {
            if ( IsDressing && !( _cancellationTokenSource?.IsCancellationRequested ?? false ) )
            {
                _cancellationTokenSource?.Cancel();
            }
        }
    }
}