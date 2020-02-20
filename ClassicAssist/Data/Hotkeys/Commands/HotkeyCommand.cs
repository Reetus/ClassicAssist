using System;
using System.Threading.Tasks;
using ClassicAssist.Resources;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    public class HotkeyCommand : HotkeyEntry, IComparable<HotkeyCommand>
    {
        private string _tooltip;

        public HotkeyCommand()
        {
            HotkeyCommandAttribute a =
                (HotkeyCommandAttribute) Attribute.GetCustomAttribute( GetType(), typeof( HotkeyCommandAttribute ) );

            if ( a != null )
            {
                // if the attribute exists, the Name must be localizable.
                string hotkeyName = Strings.ResourceManager.GetString(
                    a.Name ?? throw new ArgumentNullException( $"No localizable string for {a.Name}" ) );

                string tooltipName = Strings.ResourceManager.GetString(
                    a.Tooltip ?? throw new ArgumentNullException( $"No localizable string for {a.Tooltip}" ) );

                base.Name = string.IsNullOrEmpty( hotkeyName ) ? a.Name : hotkeyName;
                Tooltip = string.IsNullOrEmpty( tooltipName ) ? a.Tooltip : tooltipName;
            }

            Action = hs => Task.Run( Execute );
        }

        public string Tooltip
        {
            get => _tooltip;
            set => SetProperty( ref _tooltip, value );
        }

        public int CompareTo( HotkeyCommand other )
        {
            return string.Compare( Name, other.Name, StringComparison.Ordinal );
        }

        public virtual void Execute()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals( object obj )
        {
            if ( !( obj is HotkeyCommand hkc ) )
            {
                return false;
            }

            return Name == hkc.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}