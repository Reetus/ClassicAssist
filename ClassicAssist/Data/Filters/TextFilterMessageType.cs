#region License

// Copyright (C) 2022 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.ComponentModel;
using ClassicAssist.UI.Misc;

namespace ClassicAssist.Data.Filters
{
    [Flags]
    [TypeConverter( typeof( EnumDescriptionTypeConverter ) )]
    public enum TextFilterMessageType
    {
        None = 0b0,
        Say = 0b1,
        System = 0b10,
        Emote = 0b100,
        Label = 0b1000,
        Whisper = 0b10000,
        Yell = 0b100000,
        Spell = 0b1000000,
        Guild = 0b10000000,
        Alliance = 0b100000000,
        GM = 0b1000000000,
        Party = 0b10000000000,

        [Description( "Global Chat" )]
        GlobalChat = 0b100000000000,
        All = 0b111111111111
    }
}