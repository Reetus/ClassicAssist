using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Data.Hotkeys.Commands;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Dress
{
    public class DressAgentEntry : HotkeyCommand
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

        public void AddOrReplaceDressItem( int itemSerial, Layer itemLayer, int id )
        {
            List<DressAgentItem> list = Items.ToList();

            DressAgentItem existingItem = Items.FirstOrDefault( i => i.Layer == itemLayer );

            if ( existingItem != null )
            {
                list.Remove( existingItem );
            }

            list.Add( new DressAgentItem { Serial = itemSerial, Layer = itemLayer } );

            Items = list;
        }

        public async Task Dress( bool moveConflicting = true )
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

                    int attempts;

                    if ( currentInLayer != 0 && moveConflicting && container != -1 )
                    {
                        attempts = 0;

                        do
                        {
                            await UOC.DragDropAsync( currentInLayer, 1, container );
                            await Task.Delay( Options.CurrentOptions.ActionDelayMS );
                        }
                        while ( Engine.Player?.GetLayer( dai.Layer ) != 0 && attempts++ < 10 );

                        Engine.Player?.SetLayer( dai.Layer, 0 );

                        currentInLayer = 0;
                    }

                    if ( currentInLayer != 0 )
                    {
                        continue;
                    }

                    attempts = 0;

                    do
                    {
                        switch ( dai.Type )
                        {
                            case DressAgentItemType.Serial:
                                UOC.EquipItem( item, dai.Layer );
                                break;
                            case DressAgentItemType.ID:
                                UOC.EquipType( dai.ID, dai.Layer );
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        await Task.Delay( Options.CurrentOptions.ActionDelayMS );
                    }
                    while ( Engine.Player?.GetLayer( dai.Layer ) == 0 && attempts++ < 10 );
                }
            } );
        }

        public void Undress()
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

            IEnumerable<Item> itemsToUnequip =
                Engine.Player.GetEquippedItems().Where( i => serials.Contains( i.Serial ) );

            foreach ( Item item in itemsToUnequip )
            {
                UOC.DragDropAsync( item.Serial, 1, container ).Wait();
                Engine.Player.SetLayer( item.Layer, 0 );
                Thread.Sleep( Options.CurrentOptions.ActionDelayMS );
            }
        }
    }
}