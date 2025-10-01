namespace ClassicAssist.UO.Data
{
    public struct StaticTile
    {
        public TileFlags Flags { get; set; }
        public byte Weight { get; set; }
        public byte Layer { get; set; }
        public int Quantity { get; set; }
        public ushort ID { get; set; }
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int Hue { get; set; }
        public int Height { get; set; }
    }
}