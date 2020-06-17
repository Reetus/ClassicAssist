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

using System.Linq;
using Assistant;
using ClassicAssist.UO.Network.Packets;

namespace ClassicAssist.UO.Objects
{
    public class Menu
    {
        public int Serial { get; set; }
        public int ID { get; set; }
        public string Title { get; set; }
        public MenuEntry[] Lines { get; set; }

        public void OnResponse( int buttonId )
        {
            int id = 0;
            int hue = 0;

            if ( buttonId > 0 )
            {
                id = Lines.FirstOrDefault( i => i.Index == buttonId )?.ID ?? 0;
                hue = Lines.FirstOrDefault( i => i.Index == buttonId )?.Hue ?? 0;
            }

            Engine.SendPacketToServer( new MenuButtonClick( ID, Serial, id, hue ) );
        }
    }

    public class MenuEntry
    {
        public int Index { get; set; }
        public int ID { get; set; }
        public int Hue { get; set; }
        public string Title { get; set; }
    }
}