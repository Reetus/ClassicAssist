namespace ClassicAssist.UO.Objects
{
    public abstract class Entity
    {
        private volatile int _serial;

        protected Entity( int serial )
        {
            _serial = serial;
        }

        public int Serial => _serial;
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int ID { get; set; }
        public Direction Direction { get; set; }
        public int Hue { get; set; }
    }
}