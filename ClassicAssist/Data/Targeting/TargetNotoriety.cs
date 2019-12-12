using System;

namespace ClassicAssist.Data.Targeting
{
    [Flags]
    public enum TargetNotoriety
    {
        None = 0b0,
        Innocent = 0b1,
        Criminal = 0b10,
        Enemy = 0b100,
        Murderer = 0b1000,
        Friend = 0b10000,
        Gray = 0b100000,
        Any = 0b111111
    }
}