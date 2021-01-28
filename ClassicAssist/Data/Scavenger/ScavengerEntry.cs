using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassicAssist.Annotations;

namespace ClassicAssist.Data.Scavenger
{
    public class ScavengerEntry : INotifyPropertyChanged
    {
        private bool _enabled;
        private int _graphic;
        private int _hue;
        private int _flag;
        private string _name;
        private ScavengerPriority _priority = ScavengerPriority.Normal;

        public bool Enabled
        {
            get => _enabled;
            set => SetProperty( ref _enabled, value );
        }

        public int Graphic
        {
            get => _graphic;
            set => SetProperty( ref _graphic, value );
        }

        public int Hue
        {
            get => _hue;
            set => SetProperty( ref _hue, value );
        }

        public int Flag
        {
            get => _flag;
            set => SetProperty(ref _flag, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty( ref _name, value );
        }

        public ScavengerPriority Priority
        {
            get => _priority;
            set => SetProperty( ref _priority, value );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        // ReSharper disable once RedundantAssignment
        public void SetProperty<T>( ref T field, T value, [CallerMemberName] string propertyName = null )
        {
            field = value;
            OnPropertyChanged( propertyName );
        }
    }
}