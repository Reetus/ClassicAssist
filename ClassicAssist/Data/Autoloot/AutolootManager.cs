#region License

// Copyright (C) 2020 Reetus
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
using System.Collections.Generic;

namespace ClassicAssist.Data.Autoloot
{
    public class AutolootManager
    {
        private static AutolootManager _instance;
        private static readonly object _lock = new object();
        public Action<int, bool> CheckContainer { get; set; }

        public Func<List<AutolootEntry>> GetEntries { get; set; } = () => new List<AutolootEntry>();
        public Func<bool> IsRunning { get; set; }

        public static AutolootManager GetInstance()
        {
            // ReSharper disable once InvertIf
            if ( _instance == null )
            {
                lock ( _lock )
                {
                    if ( _instance != null )
                    {
                        return _instance;
                    }

                    _instance = new AutolootManager();
                    return _instance;
                }
            }

            return _instance;
        }
    }
}