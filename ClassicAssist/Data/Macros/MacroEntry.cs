using System;
using System.Linq;
using System.Windows;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Resources;

namespace ClassicAssist.Data.Macros
{
    public class MacroEntry : HotkeyEntry, IComparable<MacroEntry>
    {
        private bool _doNotAutoInterrupt;
        private bool _loop;
        private string _macro = string.Empty;
        private string _name;

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

        public override string Name
        {
            get => _name;
            set => SetName( _name, value );
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

        private void SetName( string name, string value )
        {
            MacroManager manager = MacroManager.GetInstance();

            bool exists = manager.Items.Any( m => m.Name == value && !ReferenceEquals( m, this ) );

            if ( exists && name == null )
            {
                SetName( null, $"{value}-" );
                return;
            }

            if ( exists )
            {
                MessageBox.Show( Strings.Macro_name_must_be_unique_, Strings.Error );
                return;
            }

            SetProperty( ref _name, value );
        }
    }
}