using System.Collections.Generic;
using System.Linq;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.UO.Data;

namespace ClassicAssist.Data.Dress
{
    public class DressAgentEntry : HotkeySettable
    {
        private IEnumerable<DressAgentItem> _items;
        private int _undressContainer;

        public IEnumerable<DressAgentItem> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public int UndressContainer
        {
            get => _undressContainer;
            set => SetProperty(ref _undressContainer, value);
        }

        public override string ToString()
        {
            return Name;
        }

        public void AddOrReplaceDressItem(int itemSerial, Layer itemLayer)
        {
            List<DressAgentItem> list = Items.ToList();

            DressAgentItem existingItem = Items.FirstOrDefault(i => i.Layer == itemLayer);

            if (existingItem != null)
                list.Remove(existingItem);

            list.Add(new DressAgentItem() { Serial = itemSerial, Layer = itemLayer });

            Items = list;
        }
    }
}