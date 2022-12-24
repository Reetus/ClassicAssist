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

namespace ClassicAssist.Data.NameOverride
{
    public class NameOverrideEntry
    {
        public bool Enabled { get; set; } = true;
        public string Name { get; set; }
        public int Serial { get; set; }
        public string Notes { get; set; }
    }
}