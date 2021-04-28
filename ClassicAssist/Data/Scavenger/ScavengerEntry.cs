using ClassicAssist.Shared.UI;

namespace ClassicAssist.Data.Scavenger
{
    public class ScavengerEntry : SetPropertyNotifyChanged
    {
        private bool _enabled;
        private int _graphic;
        private int _hue;
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
    }
}