#region License

// Copyright (C) 2023 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using ClassicAssist.UI.Misc;
using System.ComponentModel;

namespace ClassicAssist.UI.ViewModels.Agents
{
    [TypeConverter( typeof( EnumDescriptionTypeConverter ) )]
    public enum SkillBonusSkills
    {
        Any = -1,
        Alchemy,
        Anatomy,
        [Description( "Animal Lore" )]
        AnimalLore,
        [Description( "Animal Taming" )]
        AnimalTaming,
        Archery,
        Blacksmithing,
        Bushido,
        Carpentry,
        Cartography,
        Chivalry,
        Cooking,
        Discordance,
        [Description( "Evaluating Intelligence" )]
        EvaluatingIntelligence,
        Fencing,
        Fletching,
        Focus,
        Glassblowing,
        Healing,
        Inscription,
        Lumberjacking,
        [Description( "Mace Fighting" )]
        MaceFighting,
        Magery,
        Masonry,
        Meditation,
        Mining,
        Musicianship,
        Mysticism,
        Necromancy,
        Ninjitsu,
        Parrying,
        Peacemaking,
        Provocation,
        [Description( "Resisting Spells" )]
        ResistingSpells,
        Snooping,
        [Description( "Spirit Speak" )]
        SpiritSpeak,
        Stealth,
        Stealing,
        Swordsmanship,
        Tactics,
        Tailoring,
        Throwing,
        Tinkering,
        Veterinary,
        Wrestling
    }
}