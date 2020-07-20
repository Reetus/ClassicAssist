using System;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    public class HotkeyCommandAttribute : Attribute
    {
        public string Category { get; set; } = "";
        public string Name { get; set; }
        public string Tooltip { get; set; } = "";
    }
}