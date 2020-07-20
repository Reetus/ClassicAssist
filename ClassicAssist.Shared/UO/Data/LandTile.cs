namespace ClassicAssist.UO.Data
{
    public struct LandTile
    {
        public TileFlags Flags { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public sbyte Z { get; set; }
    }
}