namespace ClassicAssist.Data.Friends
{
    public class FriendEntry
    {
        public string Name { get; set; }
        public int Serial { get; set; }

        public override string ToString()
        {
            return $"{Name}: 0x{Serial:x}";
        }

        public override bool Equals( object obj )
        {
            if ( !( obj is FriendEntry fe ) )
            {
                return false;
            }

            return Equals( fe );
        }

        protected bool Equals( FriendEntry other )
        {
            return Serial == other.Serial;
        }

        public override int GetHashCode()
        {
            return Serial.GetHashCode();
        }
    }
}