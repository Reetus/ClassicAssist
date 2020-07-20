using System;
using ClassicAssist.Misc;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Data.Hotkeys
{
    public enum MouseOptions
    {
        LeftButton,
        MiddleButton,
        RightButton,
        XButton1,
        XButton2,
        MouseWheelDown,
        MouseWheelUp,
        None
    }

    public class ShortcutKeys
    {
        public ShortcutKeys()
        {
        }

        public ShortcutKeys( Key modifier, Key key )
        {
            Modifier = modifier;
            Key = key;
        }

        public ShortcutKeys( JToken token )
        {
            if ( token == null )
            {
                return;
            }

            Key = token["Keys"]?.ToObject<Key>() ?? Key.None;
            Modifier = token["Modifier"]?.ToObject<Key>() ?? Key.None;
            Mouse = token["Mouse"]?.ToObject<MouseOptions>() ?? MouseOptions.None;
        }

        public static ShortcutKeys Default => new ShortcutKeys( Key.None, Key.None ) { Mouse = MouseOptions.None };
        public Key Key { get; set; } = Key.None;
        public Key Modifier { get; set; } = Key.None;
        public MouseOptions Mouse { get; set; } = MouseOptions.None;

        public Keys[] ToArray()
        {
            //TODO
            //Keys modifier = (Keys) KeyInterop.VirtualKeyFromKey( Modifier );
            //Keys key = (Keys) KeyInterop.VirtualKeyFromKey( Key );

            //if ( modifier == Keys.LControlKey || modifier == Keys.RControlKey )
            //{
            //    modifier = Keys.ControlKey;
            //}

            //return modifier == Keys.None ? new[] { key } : new[] { modifier, key };
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            if ( Mouse != MouseOptions.None )
            {
                return Modifier != Key.None ? $"{Modifier} + {Mouse}" : Mouse.ToString();
            }

            return Modifier != Key.None ? $"{Modifier} + {Key}" : Key.ToString();
        }

        public override bool Equals( object obj )
        {
            return obj is ShortcutKeys keys && Key == keys.Key && Modifier == keys.Modifier && Mouse == keys.Mouse;
        }

        public override int GetHashCode()
        {
            int hashCode = 572187996;
            hashCode = hashCode * -1521134295 + Key.GetHashCode();
            hashCode = hashCode * -1521134295 + Modifier.GetHashCode();
            hashCode = hashCode * -1521134295 + Mouse.GetHashCode();
            return hashCode;
        }

        public JObject ToJObject()
        {
            JObject keys = new JObject
            {
                { "Keys", (int) Key }, { "Modifier", (int) Modifier }, { "Mouse", (int) Mouse }
            };

            return keys;
        }
    }
}