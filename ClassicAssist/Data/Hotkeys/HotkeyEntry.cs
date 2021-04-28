using System;
using System.Windows.Media;
using ClassicAssist.Misc;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Misc;
using Newtonsoft.Json;

namespace ClassicAssist.Data.Hotkeys
{
    public abstract class HotkeyEntry : SetPropertyNotifyChanged, IComparable<HotkeyEntry>
    {
        public delegate void HotkeyChangedEventHandler( object sender, HotkeyChangedEventArgs e );

        private readonly object _childrenLock = new object();

        private ObservableCollectionEx<HotkeyEntry> _children = new ObservableCollectionEx<HotkeyEntry>();

        private ShortcutKeys _hotkey = new ShortcutKeys();
        private bool _isCategory;

        private string _name;
        private bool _passToUo = true;

        [JsonIgnore]
        public Action<HotkeyEntry> Action { get; set; }

        [JsonIgnore]
        public ObservableCollectionEx<HotkeyEntry> Children
        {
            get
            {
                lock ( _childrenLock )
                {
                    return _children;
                }
            }
            set
            {
                lock ( _childrenLock )
                {
                    SetProperty( ref _children, value );
                }
            }
        }

        public virtual bool Disableable { get; set; } = true;

        public ShortcutKeys Hotkey
        {
            get => _hotkey;
            set
            {
                if ( !Equals( value, ShortcutKeys.Default ) )
                {
                    HotkeyManager manager = HotkeyManager.GetInstance();
                    manager.ClearPreviousHotkey( value );
                }

                SetProperty( ref _hotkey, value );
                HotkeyChanged?.Invoke( this, new HotkeyChangedEventArgs( _hotkey, value ) );
            }
        }

        [JsonIgnore]
        public ImageSource Image =>
            Equals( Hotkey, ShortcutKeys.Default )
                ? Properties.Resources.red_circle.ToImageSource()
                : Properties.Resources.green_circle.ToImageSource();

        public bool IsCategory
        {
            get => _isCategory;
            set => SetProperty( ref _isCategory, value );
        }

        public virtual string Name
        {
            get => _name;
            set => SetProperty( ref _name, value );
        }

        public bool PassToUO
        {
            get => _passToUo;
            set => SetProperty( ref _passToUo, value );
        }

        public int CompareTo( HotkeyEntry other )
        {
            if ( ReferenceEquals( this, other ) )
            {
                return 0;
            }

            if ( ReferenceEquals( null, other ) )
            {
                return 1;
            }

            int isCategoryComparison = _isCategory.CompareTo( other._isCategory );

            if ( isCategoryComparison != 0 )
            {
                return isCategoryComparison;
            }

            return string.Compare( _name, other._name, StringComparison.Ordinal );
        }

        public event HotkeyChangedEventHandler HotkeyChanged;

        public override string ToString()
        {
            return Name;
        }
    }
}