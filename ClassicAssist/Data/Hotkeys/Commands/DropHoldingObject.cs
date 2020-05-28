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

using Assistant;
using ClassicAssist.UO.Network.Packets;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Name = "Drop Holding Object" )]
    public class DropHoldingObject : HotkeyCommand
    {
        public override void Execute()
        {
            if ( Engine.Player != null && Engine.Player.Holding != 0 )
            {
                Engine.SendPacketToServer( new DropItem( Engine.Player.Holding, Engine.Player.Backpack.Serial, -1, -1,
                    0 ) );
            }
        }
    }
}