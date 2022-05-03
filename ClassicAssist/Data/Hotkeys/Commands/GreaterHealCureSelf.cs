#region License

// Copyright (C) 2022 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System;
using ClassicAssist.Data.Macros.Commands;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Name = "Greater Heal / Cure Self", Category = "Healing" )]
    public class GreaterHealCureSelf : HotkeyCommand
    {
        public override bool Configurable => true;

        [HotkeyConfiguration( BaseType = typeof( Enum ), Type = typeof( CureType ), Name = "Cure Type" )]
        public CureType CureType { get; set; }

        public override void Execute()
        {
            if ( MobileCommands.Poisoned( "self" ) )
            {
                SpellCommands.Cast( CureType == CureType.ArchCure ? "Arch Cure" : "Cure", "self" );
            }
            else
            {
                SpellCommands.Cast( "Greater Heal", "self" );
            }
        }
    }
}