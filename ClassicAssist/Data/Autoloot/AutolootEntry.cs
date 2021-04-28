using System.Collections.ObjectModel;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.Data.Autoloot
{
    public class AutolootEntry : SetPropertyNotifyChanged
    {
        private bool _autoloot = true;

        private ObservableCollection<AutolootConstraintEntry> _constraints =
            new ObservableCollection<AutolootConstraintEntry>();

        private bool _enabled = true;
        private int _id;
        private string _name;
        private AutolootPriority _priority = AutolootPriority.Normal;
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

        public AutolootPriority Priority
        {
            get => _priority;
            set => SetProperty( ref _priority, value );
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

        public override string ToString()
        {
            return $"{Name} - 0x{ID:x}";
        }
    }
}