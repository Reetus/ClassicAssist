using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Dress
{
    public class DressAgentEntry : HotkeySettable
    {
        private IEnumerable<DressAgentItem> _items;
        private int _undressContainer;

        public IEnumerable<DressAgentItem> Items
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

        public void AddOrReplaceDressItem( int itemSerial, Layer itemLayer )
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

            await Task.Run( async () =>
            {
                foreach ( DressAgentItem dai in Items )
                {
                    Item item = Engine.Items.GetItem( dai.Serial );

                    if ( item == null )
                    {
                        continue;
                    }

                    int currentInLayer = Engine.Player?.GetLayer( dai.Layer ) ?? 0;

                    if ( currentInLayer == item.Serial )
                    {
                        continue;
                    }

                    if ( currentInLayer != 0 && moveConflicting && container != -1 )
                    {
                        Engine.Player?.SetLayer( dai.Layer, 0 );

                        await UOC.DragDropAsync( currentInLayer, 1, container );
                        await Task.Delay( Options.CurrentOptions.ActionDelayMS );
                        currentInLayer = 0;
                    }

                    if ( currentInLayer != 0 )
                    {
                        continue;
                    }

                    UOC.EquipItem( item, dai.Layer );

                    await Task.Delay( Options.CurrentOptions.ActionDelayMS );
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

            IEnumerable<int> serials = Items.Select( dai => dai.Serial );

            IEnumerable<Item> itemsToUnequip =
                Engine.Player.GetEquippedItems().Where( i => serials.Contains( i.Serial ) );

            if ( container == -1 )
            {
                return;
            }

            foreach ( Item item in itemsToUnequip )
            {
                UOC.DragDropAsync( item.Serial, 1, container ).Wait();
                Engine.Player.SetLayer( item.Layer, 0 );
                Thread.Sleep( Options.CurrentOptions.ActionDelayMS );
            }
        }
    }
}