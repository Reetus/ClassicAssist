#region License

// Copyright (C) 2021 Reetus
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
using ClassicAssist.UI.Controls.DraggableTreeView;

namespace ClassicAssist.UI.ViewModels
{
    public class GroupsBeforeMacrosComparer : IComparer<IDraggable>
    {
        public int Compare( IDraggable x, IDraggable y )
        {
            if ( x is IDraggableGroup group && y is IDraggableGroup group2 )
            {
                return string.Compare( group.Name, group2.Name, StringComparison.Ordinal );
            }

            if ( x is IDraggableGroup )
            {
                return -1;
            }

            if ( y is IDraggableGroup )
            {
                return 1;
            }

            if ( x == null || y == null )
            {
                return 1;
            }

            return string.Compare( x.Name, y.Name, StringComparison.Ordinal );
        }
    }
}