using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Data.Hotkeys.Commands;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Data.Skills;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
using ClassicAssist.UI.Misc;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using Newtonsoft.Json.Linq;
using Skill = ClassicAssist.Data.Skills.Skill;

namespace ClassicAssist.UI.ViewModels
{
    public class SkillsTabViewModel : BaseViewModel, ISettingProvider
    {
        private HotkeyEntry _hotkeyCategory;
        private ObservableCollectionEx<SkillEntry> _items = new ObservableCollectionEx<SkillEntry>();
        private SkillEntry[] _selectedItems;
        private SkillEntry _selectedSkillEntry;
        private ICommand _setAllSkillLocksCommand;
        private float _totalBase;

        public SkillsTabViewModel()
        {
            Items.CollectionChanged += ( sender, args ) => { UpdateTotalBase(); };

            IncomingPacketHandlers.SkillUpdatedEvent += OnSkillUpdatedEvent;
            IncomingPacketHandlers.SkillsListEvent += OnSkillsListEvent;

            if ( Engine.Player != null )
            {
                Commands.MobileQuery( Engine.Player.Serial, MobileQueryType.SkillsRequest );
            }

            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public ObservableCollectionEx<SkillEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public SkillEntry[] SelectedItems
        {
            get => _selectedItems;
            set => SetProperty( ref _selectedItems, value );
        }

        public SkillEntry SelectedSkillEntry
        {
            get => _selectedSkillEntry;
            set => SetProperty( ref _selectedSkillEntry, value );
        }

        public ICommand SetAllSkillLocksCommand =>
            _setAllSkillLocksCommand ?? ( _setAllSkillLocksCommand = new RelayCommand( SetAllSkillLocks, o => true ) );

        public float TotalBase
        {
            get => _totalBase;
            set => SetProperty( ref _totalBase, value );
        }

        public void Serialize( JObject json )
        {
            JArray skills = new JArray();

            if ( _hotkeyCategory?.Children == null )
            {
                return;
            }

            foreach ( HotkeySettable hks in _hotkeyCategory.Children )
            {
                skills.Add( new JObject
                {
                    { "Name", hks.Name }, { "Keys", hks.Hotkey.ToJObject() }, { "PassToUO", hks.PassToUO }
                } );
            }

            json.Add( "Skills", skills );
        }

        public void Deserialize( JObject json, Options options )
        {
            HotkeyManager hotkey = HotkeyManager.GetInstance();

            if ( Skills.GetSkills() == null )
            {
                return;
            }

            IOrderedEnumerable<SkillData> skills = Skills.GetSkills().Where( s => s.Invokable ).OrderBy( s => s.Name );

            ObservableCollectionEx<HotkeySettable> hotkeyEntries = new ObservableCollectionEx<HotkeySettable>();

            foreach ( SkillData skill in skills )
            {
                hotkeyEntries.Add( new HotkeyCommand
                {
                    Action = hks => SkillCommands.UseSkill( skill.Name ), Name = skill.Name
                } );
            }

            if ( json["Skills"] != null )
            {
                foreach ( HotkeySettable hke in hotkeyEntries )
                {
                    JToken token = json["Skills"].FirstOrDefault( jo => jo["Name"].ToObject<string>() == hke.Name );

                    if ( token != null )
                    {
                        hke.Hotkey = new ShortcutKeys( token["Keys"]["Modifier"].ToObject<Key>(),
                            token["Keys"]["Keys"].ToObject<Key>() );
                        hke.PassToUO = token["PassToUO"].ToObject<bool>();
                    }
                }
            }

            _hotkeyCategory = new HotkeyEntry { IsCategory = true, Name = Strings.Skills, Children = hotkeyEntries };

            hotkey.Items.AddSorted( _hotkeyCategory );
        }

        private void SetAllSkillLocks( object obj )
        {
            LockStatus lockStatus = (LockStatus) (int) obj;

            IEnumerable<SkillEntry> skillsToSet = Items.Where( i => i.LockStatus != lockStatus );

            foreach ( SkillEntry skillEntry in skillsToSet )
            {
                Commands.ChangeSkillLock( skillEntry, lockStatus );
            }

            Commands.MobileQuery( Engine.Player.Serial, MobileQueryType.SkillsRequest );
        }

        private void UpdateTotalBase()
        {
            TotalBase = Items.Sum( se => se.Base );
        }

        private void OnSkillsListEvent( SkillInfo[] skills )
        {
            _dispatcher.Invoke( () => { Items.Clear(); } );

            foreach ( SkillInfo si in skills )
            {
                Skill skill = new Skill { ID = si.ID, Name = Skills.GetSkillName( si.ID ) };

                SkillEntry se = new SkillEntry
                {
                    Skill = skill,
                    Value = si.Value,
                    Base = si.BaseValue,
                    Cap = si.SkillCap,
                    LockStatus = si.LockStatus
                };

                _dispatcher.Invoke( () => { Items.Add( se ); } );
            }
        }

        private void OnSkillUpdatedEvent( int skillID, float value, float baseValue, LockStatus lockStatus,
            float skillCap )
        {
            SkillEntry entry = _items.FirstOrDefault( se => se.Skill.ID == skillID );

            if ( entry == null )
            {
                return;
            }

            _dispatcher.Invoke( () =>
            {
                entry.Delta += baseValue - entry.Base;
                entry.Value = value;
                entry.Base = baseValue;
                entry.Cap = skillCap;
                entry.LockStatus = lockStatus;
            } );
        }
    }
}