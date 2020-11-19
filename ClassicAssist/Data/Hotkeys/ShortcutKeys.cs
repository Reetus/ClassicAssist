using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using static ClassicAssist.Misc.SDLKeys;

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

        public ShortcutKeys( ModKey modifier, Key key )
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
            JToken legacyModifier = token["Modifier"];

            if ( legacyModifier != null )
            {
                Key legacyModifierKey = legacyModifier.ToObject<Key>();

                switch ( legacyModifierKey )
                {
                    case Key.LeftCtrl:
                        Modifier = ModKey.LeftCtrl;
                        break;
                    case Key.RightCtrl:
                        Modifier = ModKey.RightCtrl;
                        break;
                    case Key.LeftAlt:
                        Modifier = ModKey.LeftAlt;
                        break;
                    case Key.RightAlt:
                        Modifier = ModKey.RightAlt;
                        break;
                    case Key.LeftShift:
                        Modifier = ModKey.LeftShift;
                        break;
                    case Key.RightShift:
                        Modifier = ModKey.RightShift;
                        break;
                    default:
                        Modifier = ModKey.None;
                        break;
                }
            }
            else
            {
                Modifier = token["SDLModifier"]?.ToObject<ModKey>() ?? ModKey.None;
            }

            Mouse = token["Mouse"]?.ToObject<MouseOptions>() ?? MouseOptions.None;
        }

        public static ShortcutKeys Default => new ShortcutKeys( ModKey.None, Key.None ) { Mouse = MouseOptions.None };
        public Key Key { get; set; } = Key.None;
        public ModKey Modifier { get; set; } = ModKey.None;
        public MouseOptions Mouse { get; set; } = MouseOptions.None;

        public Keys[] ToArray()
        {
            Keys key = (Keys) KeyInterop.VirtualKeyFromKey( Key );
            return Modifier.ToKeysList().Append( key ).ToArray();
        }

        public override string ToString()
        {
            string modifier = Modifier.ToString().Replace( ", ", "+ " );

            if ( Mouse != MouseOptions.None )
            {
                return Modifier != ModKey.None ? $"{modifier} + {Mouse}" : Mouse.ToString();
            }

            return Modifier != ModKey.None ? $"{modifier} + {Key}" : Key.ToString();
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
                { "Keys", (int) Key }, { "SDLModifier", (int) Modifier }, { "Mouse", (int) Mouse }
            };

            return keys;
        }
    }
}