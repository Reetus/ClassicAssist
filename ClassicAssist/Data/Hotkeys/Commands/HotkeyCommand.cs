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

            string resourceName = Strings.ResourceManager.GetString( a?.Name ?? "" );

            Name = string.IsNullOrEmpty( resourceName ) ? a?.Name : resourceName;

            Action = hs => Execute();
        }

        public virtual bool Disableable { get; set; } = true;

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