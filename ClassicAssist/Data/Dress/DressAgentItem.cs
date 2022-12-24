using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UO.Data;

namespace ClassicAssist.Data.Dress
{
    public class DressAgentItem : SetPropertyNotifyChanged
    {
        private int _id = -1;
        private Layer _layer;
        private string _name;
        private int _serial;
        private DressAgentItemType _type = DressAgentItemType.Serial;

        public int ID
        {
            get => _id;
            set => SetProperty( ref _id, value );
        }

        public Layer Layer
        {
            get => _layer;
            set => SetProperty( ref _layer, value );
        }

        public string Name
        {
            get => _name;
            set => SetProperty( ref _name, value );
        }

        public int Serial
        {
            get => _serial;
            set => SetProperty( ref _serial, value );
        }

        public DressAgentItemType Type
        {
            get => _type;
            set => SetProperty( ref _type, value, false, SetName );
        }

        private void SetName( DressAgentItemType type )
        {
            Name = type == DressAgentItemType.ID ? $"{Layer}: {Strings.Type}: 0x{ID:x4}" : $"{Layer}: 0x{Serial:x8}";
        }

        public override string ToString()
        {
            return Name;
        }
    }
}