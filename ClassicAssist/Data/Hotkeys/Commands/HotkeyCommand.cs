using System;
using System.Threading.Tasks;
using ClassicAssist.Shared.Resources;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    public class HotkeyCommand : HotkeyEntry, IComparable<HotkeyCommand>
    {
        private bool _isExpanded;
        private string _tooltip;

        public HotkeyCommand()
        {
            HotkeyCommandAttribute a =
                (HotkeyCommandAttribute) Attribute.GetCustomAttribute( GetType(), typeof( HotkeyCommandAttribute ) );

            if ( a != null )
            {
                // if the attribute exists, the Name must be localizable.
                string hotkeyName = Strings.ResourceManager.GetString( a.Name );

                if ( string.IsNullOrEmpty( hotkeyName ) )
                {
                    throw new ArgumentNullException( $"No localizable string for {a.Name}" );
                }

                if ( !string.IsNullOrEmpty( a.Tooltip ) )
                {
                    string tooltipName = Strings.ResourceManager.GetString( a.Tooltip );

                    if ( string.IsNullOrEmpty( tooltipName ) )
                    {
                        throw new ArgumentNullException( $"No localizable string for {a.Name}" );
                    }

                    Tooltip = tooltipName;
                }

                base.Name = hotkeyName;
            }

            Action = hs => Task.Run( Execute );
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty( ref _isExpanded, value );
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