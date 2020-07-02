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
using ClassicAssist.Browser.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.Data.Filters
{
    public abstract class DynamicFilterEntry : FilterEntry
    {
        protected DynamicFilterEntry()
        {
            Filters.Add( this );
        }

        public static List<DynamicFilterEntry> Filters { get; set; } = new List<DynamicFilterEntry>();

        public virtual bool CheckPacket( byte[] packet, int length, PacketDirection direction )
        {
            throw new NotImplementedException();
        }
    }
}