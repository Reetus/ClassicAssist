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
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Resources;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Category = "Commands", Name = "Take Snapshot" )]
    public class SnapshotCommand : HotkeyCommand
    {
        public override void Execute()
        {
            DateTime now = DateTime.Now;
            string fileName =
                $"ClassicAssist-{now.Year}-{now.Month}-{now.Day}-{now.Hour}-{now.Minute}-{now.Second}.png";
            bool result = MainCommands.Snapshot( 0, false, fileName );

            if ( result )
            {
                UOC.SystemMessage( string.Format( Strings.Snapshot_Saved___0_, fileName ) );
            }
        }
    }
}