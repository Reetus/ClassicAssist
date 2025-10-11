#region License

// Copyright (C) 2025 Reetus
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

namespace ClassicAssist.Data.Autoloot
{
    [Flags]
    public enum AutolootAllowedOperators
    {
        All = 0x00000000,
        Equal = 0x00000001,
        NotEqual = 0x00000002,
        GreaterThan = 0x00000004,
        LessThan = 0x00000008,
        NotPresent = 0x00000010,
    }
}