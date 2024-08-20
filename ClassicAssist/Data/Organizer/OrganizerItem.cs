using ClassicAssist.Shared.UI;

namespace ClassicAssist.Data.Organizer
{
    public class OrganizerItem : SetPropertyNotifyChanged
    {
        private int _amount;
        private int? _destinationContainer;
        private int _hue;
        private int _id;
        private string _item;
        private int? _sourceContainer;

        public int Amount
        {
            get => _amount;
            set => SetProperty( ref _amount, value );
        }

        public int? DestinationContainer
        {
            get => _destinationContainer;
            set => SetProperty( ref _destinationContainer, value );
        }

        public int Hue
        {
            get => _hue;
            set => SetProperty( ref _hue, value );
        }

        public int ID
        {
            get => _id;
            set => SetProperty( ref _id, value );
        }

        public string Item
        {
            get => _item;
            set => SetProperty( ref _item, value );
        }

        public int? SourceContainer
        {
            get => _sourceContainer;
            set => SetProperty( ref _sourceContainer, value );
        }
    }
}