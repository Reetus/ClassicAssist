using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassicAssist.Annotations;
using ClassicAssist.UO.Data;

namespace ClassicAssist.Data.Skills
{
    public class SkillEntry : INotifyPropertyChanged
    {
        private float _base;
        private float _cap;
        private double _delta;
        private LockStatus _lockStatus;
        private Skill _skill;
        private float _value;

        public float Base
        {
            get => _base;
            set
            {
                _base = value;
                OnPropertyChanged();
            }
        }

        public float Cap
        {
            get => _cap;
            set
            {
                _cap = value;
                OnPropertyChanged();
            }
        }

        public double Delta
        {
            get => _delta;
            set
            {
                _delta = value;
                OnPropertyChanged();
            }
        }

        public LockStatus LockStatus
        {
            get => _lockStatus;
            set
            {
                _lockStatus = value;
                OnPropertyChanged();
            }
        }

        public Skill Skill
        {
            get => _skill;
            set
            {
                _skill = value;
                OnPropertyChanged();
            }
        }

        public float Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}