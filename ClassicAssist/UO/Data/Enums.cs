using System;

namespace ClassicAssist.UO.Data
{
    public enum TargetType : byte
    {
        Object,
        Tile
    }

    public enum Skill : byte
    {
        Alchemy,
        Anatomy,
        Animal_Lore,
        Item_Identification,
        Arms_Lore,
        Parrying,
        Begging,
        Blacksmithy,
        Bowcraft_Fletching,
        Peacemaking,
        Camping,
        Carpentry,
        Cartography,
        Cooking,
        Detecting_Hidden,
        Discordance,
        Evaluating_Intelligence,
        Healing,
        Fishing,
        Forensic_Evaluation,
        Herding,
        Hiding,
        Provocation,
        Inscription,
        Lockpicking,
        Magery,
        Resisting_Spells,
        Tactics,
        Snooping,
        Musicianship,
        Poisoning,
        Archery,
        Spirit_Speak,
        Stealing,
        Tailoring,
        Animal_Taming,
        Taste_Identification,
        Tinkering,
        Tracking,
        Veterinary,
        Swordsmanship,
        Mace_Fighting,
        Fencing,
        Wrestling,
        Lumberjacking,
        Mining,
        Meditation,
        Stealth,
        Remove_Trap,
        Necromancy,
        Focus,
        Chivalry,
        Bushido,
        Ninjitsu,
        Spellweaving,
        Mysticism,
        Imbuing,
        Throwing
    }

    [Flags]
    public enum FeatureFlags
    {
        None = 0x00000000,
        T2A = 0x00000001,
        UOR = 0x00000002,
        UOTD = 0x00000004,
        LBR = 0x00000008,
        AOS = 0x00000010,
        SixthCharacterSlot = 0x00000020,
        SE = 0x00000040,
        ML = 0x00000080,
        EigthAge = 0x00000100,
        NinthAge = 0x00000200, /* Crystal/Shadow Custom House Tiles */
        TenthAge = 0x00000400,
        IncreasedStorage = 0x00000800, /* Increased Housing/Bank Storage */
        SeventhCharacterSlot = 0x00001000,
        RoleplayFaces = 0x00002000,
        TrialAccount = 0x00004000,
        LiveAccount = 0x00008000,
        SA = 0x00010000,
        HS = 0x00020000,
        Gothic = 0x00040000,
        Rustic = 0x00080000,
        Jungle = 0x00100000,
        Shadowguard = 0x00200000,
        TOL = 0x00400000,
        EJ = 0x00800000
    }

    public enum Notoriety : byte
    {
        Invalid,
        Innocent,
        Ally,
        Attackable,
        Criminal,
        Enemy,
        Murderer,
        Invulnerable,
        Unknown
    }

    [Flags]
    public enum MobileStatus : byte
    {
        None = 0x00,
        Frozen = 0x01,
        Female = 0x02,
        Flying = 0x04,
        Invulnerable = 0x08,
        IgnoreMobiles = 0x10,
        Unknown = 0x20,
        WarMode = 0x40,
        Hidden = 0x80
    }
}