using System;
using System.Threading.Tasks;
using ClassicAssist.Resources;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    public class HotkeyCommand : HotkeySettable, IComparable<HotkeyCommand>
    {
        public HotkeyCommand()
        {
            HotkeyCommandAttribute a =
                (HotkeyCommandAttribute) Attribute.GetCustomAttribute( GetType(), typeof( HotkeyCommandAttribute ) );

            if ( a != null )
            {
                // if the attribute exists, the Name must be localizable.
                string resourceName = Strings.ResourceManager.GetString(
                    a.Name ?? throw new ArgumentNullException( $"No localizable string for {a.Name}" ) );

                Name = string.IsNullOrEmpty( resourceName ) ? a.Name : resourceName;
            }

            Action = hs => Task.Run( Execute );
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