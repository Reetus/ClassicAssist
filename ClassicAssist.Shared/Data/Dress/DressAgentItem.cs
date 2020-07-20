using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassicAssist.Annotations;
using ClassicAssist.Resources;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO.Data;

namespace ClassicAssist.Data.Dress
{
    public class DressAgentItem : INotifyPropertyChanged
    {
        private int _id = -1;
        private Layer _layer;
        private string _name;
        private int _serial;
        private DressAgentItemType _type = DressAgentItemType.Serial;

        public DressAgentItem()
        {
            PropertyChanged += ( sender, args ) =>
            {
                // Set the name when the Type changes...
                if ( args.PropertyName == nameof( Type ) )
                {
                    Name = Type == DressAgentItemType.ID
                        ? $"{Layer}: {Strings.Type}: 0x{ID:x4}"
                        : $"{Layer}: 0x{Serial:x8}";
                }
            };
        }

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
            set => SetProperty( ref _type, value );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // ReSharper disable once RedundantAssignment
        public void SetProperty<T>( ref T field, T value, [CallerMemberName] string propertyName = null )
        {
            field = value;
            OnPropertyChanged( propertyName );
        }

        public override string ToString()
        {
            return Name;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}