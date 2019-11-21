namespace ClassicAssist.UO.Objects
{
    public class Item : Entity
    {
        public Item( int serial ) : base( serial )
        {
        }

        public Item(int serial, int containerSerial) : base(serial)
        {
            Owner = containerSerial;
        }

        public int Owner { get; set; }
        public ItemCollection Container { get; set; }
        public bool IsContainer => Container != null;
        public int ArtDataID { get; set; }
        public int Count { get; set; }
        public int Flags { get; set; }
        public int Light { get; set; }
        public int Grid { get; set; }
    }
}