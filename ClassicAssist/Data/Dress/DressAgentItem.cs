using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassicAssist.Annotations;
using ClassicAssist.UO.Data;

namespace ClassicAssist.Data.Dress
{
    public class DressAgentItem : INotifyPropertyChanged
    {
        private Layer _layer;
        private int _serial;
        public Layer Layer
        {
            get => _layer;
            set => SetProperty(ref _layer, value);
        }
        public int Serial
        {
            get => _serial;
            set => SetProperty(ref _serial, value);
        }

        // ReSharper disable once RedundantAssignment
        public void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            field = value;
            OnPropertyChanged(propertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            return $"{Layer}: 0x{Serial:x8}";
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}