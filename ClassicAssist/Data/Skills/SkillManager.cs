using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Misc;

namespace ClassicAssist.Data.Skills
{
    public class SkillManager : SetPropertyNotifyChanged
    {
        private static readonly object _lock = new object();
        private static SkillManager _instance;
        private ObservableCollectionEx<SkillEntry> _items = new ObservableCollectionEx<SkillEntry>();

        private SkillManager()
        {
        }

        public ObservableCollectionEx<SkillEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public static SkillManager GetInstance()
        {
            // ReSharper disable once InvertIf
            if ( _instance == null )
            {
                lock ( _lock )
                {
                    if ( _instance != null )
                    {
                        return _instance;
                    }

                    _instance = new SkillManager();
                    return _instance;
                }
            }

            return _instance;
        }
    }
}