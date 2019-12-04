namespace ClassicAssist.Data.Hotkeys
{
    public class HotkeyChangedEventArgs
    {
        public HotkeyChangedEventArgs( ShortcutKeys oldValue, ShortcutKeys newValue )
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public ShortcutKeys NewValue { get; set; }
        public ShortcutKeys OldValue { get; set; }
    }
}