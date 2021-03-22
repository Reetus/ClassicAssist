using System;

namespace ClassicAssist.UO.Data
{
    [Flags]
    public enum MessageAffixType
    {
        Append,
        Prepend,
        System
    }

    [Flags]
    public enum HealthbarColour
    {
        None = 0x00,
        Green = 0x01,
        Yellow = 0x02,
        Red = 0x04
    }

    public enum TargetFlags : byte
    {
        None,
        Harmful,
        Beneficial,
        Cancel
    }

    public enum Virtues
    {
        None = '0',
        Honor,
        Sacrafice,
        Valor,
        Compassion,
        Honesty,
        Humility,
        Justice,
        Spirituality
    }

    public enum MobileRace : byte
    {
        Unknown,
        Human,
        Elf,
        Gargoyle
    }

    public enum JournalSpeech : byte
    {
        Say,
        System,
        Emote,
        Unknown1,
        Unknown2,
        Unknown3,
        Label,
        Focus,
        Whisper,
        Yell,
        Spell,
        Unknown4,
        Unknown5,
        Guild,
        Alliance,
        GM
    }

    public enum MobileQueryType : byte
    {
        StatsRequest = 4,
        SkillsRequest = 5
    }

    public enum TargetType : byte
    {
        Object,
        Tile
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

    [Flags]
    public enum CharacterListFlags
    {
        Unknown = 0x01,
        OverwriteConfigurationButton = 0x02,
        OneCharacterSlot = 0x04,
        EnableContextMenus = 0x08,
        LimitCharacterSlots = 0x10,
        PaladinNecromancerClassTooltips = 0x20,
        SixthCharacterSlot = 0x40,
        SamuraiNinjaClasses = 0x80,
        ElvenRace = 0x100,
        Unknown2 = 0x200,
        UO3DClientType = 0x400,
        Unknown3 = 0x800,
        SeventhCharacterSlot = 0x1000,
        Unknown4 = 0x2000,
        NewMovementSystem = 0x4000,
        NewFeluccaAreas = 0x8000
    }
}