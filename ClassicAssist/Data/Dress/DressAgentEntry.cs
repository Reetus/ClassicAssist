using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Data.Abilities;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Misc;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Dress
{
    /*
     * Future tip to self, this is HotkeyEntry so it doesn't try to add it to hotkey commands
     */
    public class DressAgentEntry : HotkeyEntry
    {
        private List<DressAgentItem> _items;
        private int _undressContainer;

        public List<DressAgentItem> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public int UndressContainer
        {
            get => _undressContainer;
            set => SetProperty( ref _undressContainer, value );
        }

        public override string ToString()
        {
            return Name;
        }

        public void AddOrReplaceDressItem( Item item )
        {
            List<DressAgentItem> list = Items.ToList();

            DressAgentItem existingItem = Items.FirstOrDefault( i => i.Layer == item.Layer );

            if ( existingItem != null )
            {
                list.Remove( existingItem );
            }

            list.Add( new DressAgentItem
            {
                Serial = item.Serial, Layer = item.Layer, Type = DressAgentItemType.Serial, ID = item.ID
            } );

            Items = list;
        }

        public async Task Dress( CancellationToken cancellationToken, bool moveConflicting = true )
        {
            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                return;
            }

            int container = DressManager.GetInstance().GetUndressContainer( UndressContainer );

            if ( container == 0 || container == -1 )
            {
                return;
            }

            if ( DressManager.GetInstance().UseUO3DPackets )
            {
                //TODO DressAgentItemType.ID

                if ( moveConflicting )
                {
                    List<int> conflictingLayers = Engine.Player.GetEquippedItems()
                        .Where( i => Items.Any( ii => i.Layer == ii.Layer && i.Serial != ii.Serial ) )
                        .Select( l => (int) l.Layer ).ToList();

                    IEnumerable<DressAgentItem> handLayers =
                        Items.Where( i => i.Layer == Layer.OneHanded || i.Layer == Layer.TwoHanded );

                    foreach ( DressAgentItem handLayer in handLayers )
                    {
                        ( int _, Layer layer ) = GetConflictingHand( handLayer.Layer );

                        if ( layer != Layer.Invalid )
                        {
                            conflictingLayers.Add( (int) layer );
                        }
                    }

                    if ( conflictingLayers.Count > 0 )
                    {
                        UOC.UO3DUnequipItems( conflictingLayers.ToArray() );
                        Thread.Sleep( Options.CurrentOptions.ActionDelayMS );
                    }
                }

                int[] dressItems = Items.Select( i => i.Serial ).ToArray();

                UOC.UO3DEquipItems( dressItems );
            }
            else
            {
                try
                {
                    await Task.Run( async () =>
                    {
                        foreach ( DressAgentItem dai in Items )
                        {
                            if ( cancellationToken.IsCancellationRequested )
                            {
                                return;
                            }

                            Item item = Engine.Items.GetItem( dai.Serial );

                            if ( item == null && dai.Type == DressAgentItemType.Serial )
                            {
                                continue;
                            }

                            ( int otherHand, Layer _ ) = GetConflictingHand( dai.Layer );

                            if ( otherHand != 0 && moveConflicting )
                            {
                                await ActionPacketQueue.EnqueueDragDrop( otherHand, 1, container, QueuePriority.Medium,
                                    cancellationToken: cancellationToken );
                            }

                            int currentInLayer = Engine.Player?.GetLayer( dai.Layer ) ?? 0;

                            if ( currentInLayer == item?.Serial )
                            {
                                continue;
                            }

                            if ( currentInLayer != 0 && moveConflicting && container != -1 )
                            {
                                await ActionPacketQueue.EnqueueDragDrop( currentInLayer, 1, container,
                                    QueuePriority.Medium, cancellationToken: cancellationToken );

                                currentInLayer = 0;
                            }

                            if ( currentInLayer != 0 )
                            {
                                continue;
                            }

                            switch ( dai.Type )
                            {
                                case DressAgentItemType.Serial:
                                    await UOC.EquipItem( item, dai.Layer );
                                    break;
                                case DressAgentItemType.ID:
                                    await UOC.EquipType( dai.ID, dai.Layer );
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }, cancellationToken );
                }
                catch ( TaskCanceledException )
                {
                    // ignored
                }
            }
        }

        private static (int, Layer) GetConflictingHand( Layer layer )
        {
            int otherHand;

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch ( layer )
            {
                case Layer.OneHanded:
                    otherHand = Engine.Player?.GetLayer( Layer.TwoHanded ) ?? 0;

                    WeaponData wd = AbilitiesManager.GetInstance()
                        .GetWeaponData( Engine.Items.GetItem( otherHand )?.ID ?? 0 );

                    if ( wd == null || !wd.Twohanded )
                    {
                        otherHand = 0;
                    }

                    return ( otherHand, Layer.TwoHanded );
                case Layer.TwoHanded:
                    otherHand = Engine.Player?.GetLayer( Layer.OneHanded ) ?? 0;

                    return ( otherHand, Layer.OneHanded );
            }

            return ( 0, Layer.Invalid );
        }

        public async Task Undress( CancellationToken cancellationToken )
        {
            if ( Engine.Player == null )
            {
                return;
            }

            int container = DressManager.GetInstance().GetUndressContainer( UndressContainer );

            if ( container == 0 || container == -1 )
            {
                return;
            }

            IEnumerable<int> serials = Items.Select( dai => dai.Serial );

            if ( DressManager.GetInstance().UseUO3DPackets )
            {
                int[] layers = Engine.Player.GetEquippedItems().Where( i => serials.Contains( i.Serial ) )
                    .Select( i => (int) i.Layer ).ToArray();

                UOC.UO3DUnequipItems( layers );
            }
            else
            {
                IEnumerable<Item> itemsToUnequip =
                    Engine.Player.GetEquippedItems().Where( i => serials.Contains( i.Serial ) );

                foreach ( Item item in itemsToUnequip )
                {
                    await ActionPacketQueue.EnqueueDragDrop( item.Serial, 1, container, QueuePriority.Medium,
                        cancellationToken: cancellationToken );
                }
            }
        }
    }
}