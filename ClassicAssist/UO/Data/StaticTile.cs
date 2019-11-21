namespace ClassicAssist.UO.Data
{
    public struct StaticTile
    {
        public TileFlags Flags { get; set; }
        public byte Weight { get; set; }
        public byte Quality { get; set; }
        public byte Quantity { get; set; }
        public ushort ID { get; set; }
        public string Name { get; set; }
    }
}