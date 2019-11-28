using Assistant;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Objects
{
    public class PlayerMobile : Mobile
    {
        public PlayerMobile( int serial ) : base( serial )
        {
        }

        public Map Map { get; set; }
        public TargetType LastTargetType { get; set; }
        public int LastTargetSerial { get; set; }
    }
}