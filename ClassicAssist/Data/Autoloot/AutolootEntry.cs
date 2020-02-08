using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClassicAssist.Annotations;

namespace ClassicAssist.Data.Autoloot
{
    public class AutolootEntry : INotifyPropertyChanged
    {
        private bool _autoloot;

        private ObservableCollection<AutolootConstraintEntry>
            _constraints = new ObservableCollection<AutolootConstraintEntry>();

        private bool _enabled = true;
        private int _id;
        private string _name;
        private bool _rehue;
        private int _rehueHue = 1153;

        public bool Autoloot
        {
            get => _autoloot;
            set => SetProperty( ref _autoloot, value );
        }

        public ObservableCollection<AutolootConstraintEntry> Constraints
        {
            get => _constraints;
            set => SetProperty( ref _constraints, value );
        }

        public bool Enabled
        {
            get => _enabled;
            set => SetProperty( ref _enabled, value );
        }

        public int ID
        {
            get => _id;
            set => SetProperty( ref _id, value );
        }

        public string Name
        {
            get => _name;
            set => SetProperty( ref _name, value );
        }

        public bool Rehue
        {
            get => _rehue;
            set => SetProperty( ref _rehue, value );
        }

        public int RehueHue
        {
            get => _rehueHue;
            set => SetProperty( ref _rehueHue, value );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            return $"{Name} - 0x{ID:x}";
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        // ReSharper disable once RedundantAssignment
        public virtual void SetProperty<T>( ref T obj, T value, [CallerMemberName] string propertyName = "" )
        {
            obj = value;
            OnPropertyChanged( propertyName );
            CommandManager.InvalidateRequerySuggested();
        }
    }
}