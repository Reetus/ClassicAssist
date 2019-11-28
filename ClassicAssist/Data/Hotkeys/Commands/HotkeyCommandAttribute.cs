using System;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    public class HotkeyCommandAttribute : Attribute
    {
        public string Name { get; set; }
    }
}