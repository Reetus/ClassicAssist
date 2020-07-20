using System;

namespace ClassicAssist.Data.Regions
{
    [Flags]
    public enum RegionAttributes
    {
        None = 0,
        Guarded = 1 << 1,
        Jail = 1 << 2,
        Wilderness = 1 << 3,
        Town = 1 << 4,
        Dungeon = 1 << 5,
        Special = 1 << 6,
        Default = 1 << 7
    }
}