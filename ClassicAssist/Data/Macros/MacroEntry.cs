using System;
using ClassicAssist.Data.Hotkeys;

namespace ClassicAssist.Data.Macros
{
    public class MacroEntry : HotkeySettable, IComparable<MacroEntry>
    {
        private bool _doNotAutoInterrupt;
        private bool _loop;
        private string _macro = string.Empty;

        public bool DoNotAutoInterrupt
        {
            get => _doNotAutoInterrupt;
            set => SetProperty( ref _doNotAutoInterrupt, value );
        }

        public bool Loop
        {
            get => _loop;
            set => SetProperty( ref _loop, value );
        }

        public string Macro
        {
            get => _macro;
            set => SetProperty( ref _macro, value );
        }

        public Action Stop { get; set; }

        public int CompareTo( MacroEntry other )
        {
            return string.Compare( Name, other.Name, StringComparison.OrdinalIgnoreCase );
        }

        public override string ToString()
        {
            return Name;
        }
    }
}