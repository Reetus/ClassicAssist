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

using Assistant;
using ClassicAssist.UO.Network.Packets;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Name = "Use Last Skill" )]
    public class UseLastSkill : HotkeyCommand
    {
        public override void Execute()
        {
            Engine.SendPacketToServer( new UseSkill( Engine.LastSkillID ) );
        }
    }
}