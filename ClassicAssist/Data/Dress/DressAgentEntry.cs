using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
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

        public async Task Dress( bool moveConflicting = true )
        {
            if ( DressManager.GetInstance().IsDressing )
            {
                UOC.SystemMessage( Strings.Dress_already_in_progress___ );
                return;
            }

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
                    int[] conflictingLayers = Engine.Player.GetEquippedItems()
                        .Where( i => Items.Any( ii => i.Layer == ii.Layer && i.Serial != ii.Serial ) )
                        .Select( l => (int) l.Layer ).ToArray();

                    if ( conflictingLayers.Length > 0 )
                    {
                        UOC.UO3DUnequipItems( conflictingLayers );
                        Thread.Sleep( Options.CurrentOptions.ActionDelayMS );
                    }
                }

                int[] dressItems = Items.Select( i => i.Serial ).ToArray();

                UOC.UO3DEquipItems( dressItems );
            }
            else
            {
                await Task.Run( async () =>
                {
                    foreach ( DressAgentItem dai in Items )
                    {
                        Item item = Engine.Items.GetItem( dai.Serial );

                        if ( item == null && dai.Type == DressAgentItemType.Serial )
                        {
                            continue;
                        }

                        int currentInLayer = Engine.Player?.GetLayer( dai.Layer ) ?? 0;

                        if ( currentInLayer == item?.Serial )
                        {
                            continue;
                        }

                        if ( currentInLayer != 0 && moveConflicting && container != -1 )
                        {
                            await ActionPacketQueue.EnqueueDragDrop( currentInLayer, 1, container,
                                QueuePriority.Medium );

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
                } );
            }
        }

        public async Task Undress()
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
                    await ActionPacketQueue.EnqueueDragDrop( item.Serial, 1, container, QueuePriority.Medium );
                }
            }
        }
    }
}