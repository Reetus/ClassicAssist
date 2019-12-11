using System;
using ClassicAssist.Resources;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    public class HotkeyCommand : HotkeySettable
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

            Action = hs => Execute();
        }

        public virtual void Execute()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}