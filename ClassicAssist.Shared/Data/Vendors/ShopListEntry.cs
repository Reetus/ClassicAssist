using ClassicAssist.UO.Objects;

namespace ClassicAssist.Data.Vendors
{
    public class ShopListEntry
    {
        public int Amount { get; set; }
        public Item Item { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public int VendorSerial { get; set; }
    }
}