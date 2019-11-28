namespace ClassicAssist.UO.Data
{
    public enum Layer : byte
    {
        Invalid = 0,
        OneHanded = 1,
        TwoHanded = 2,
        Shoes = 3,
        Pants = 4,
        Shirt = 5,
        Helm = 6,
        Gloves = 7,
        Ring = 8,
        Talisman = 9,
        Neck = 10, // 0x0A
        Hair = 11, // 0x0B
        Waist = 12, // 0x0C
        InnerTorso = 13, // 0x0D
        Bracelet = 14, // 0x0E
        Unused_xF = 15, // 0x0F
        FacialHair = 16, // 0x10
        MiddleTorso = 17, // 0x11
        Earrings = 18, // 0x12
        Arms = 19, // 0x13
        Cloak = 20, // 0x14
        Backpack = 21, // 0x15
        OuterTorso = 22, // 0x16
        OuterLegs = 23, // 0x17
        InnerLegs = 24, // 0x18
        Mount = 25, // 0x19
        ShopBuy = 26, // 0x1A
        ShopResale = 27, // 0x1B
        ShopSell = 28, // 0x1C
        Bank = 29, // 0x1D
        LastValid = 29, // 0x1D
    }
}