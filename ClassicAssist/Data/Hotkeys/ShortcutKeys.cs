using System.Windows.Forms;
using System.Windows.Input;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Data.Hotkeys
{
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

        public static ShortcutKeys Default => new ShortcutKeys( Key.None, Key.None );
        public Key Key { get; set; } = Key.None;
        public Key Modifier { get; set; } = Key.None;

        public Keys[] ToArray()
        {
            Keys modifier = (Keys) KeyInterop.VirtualKeyFromKey( Modifier );
            Keys key = (Keys) KeyInterop.VirtualKeyFromKey( Key );

            if ( modifier == Keys.LControlKey || modifier == Keys.RControlKey )
            {
                modifier = Keys.ControlKey;
            }

            return modifier == Keys.None ? new[] { key } : new[] { modifier, key };
        }

        public override string ToString()
        {
            return Modifier != Key.None ? $"{Modifier} + {Key}" : Key.ToString();
        }

        public override bool Equals( object obj )
        {
            return obj is ShortcutKeys keys &&
                   Key == keys.Key &&
                   Modifier == keys.Modifier;
        }

        public override int GetHashCode()
        {
            int hashCode = 572187996;
            hashCode = hashCode * -1521134295 + Key.GetHashCode();
            hashCode = hashCode * -1521134295 + Modifier.GetHashCode();
            return hashCode;
        }

        public JObject ToJObject()
        {
            JObject keys = new JObject { { "Keys", (int) Key }, { "Modifier", (int) Modifier } };

            return keys;
        }
    }
}