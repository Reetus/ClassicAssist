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

using System.Windows.Media;

namespace ClassicAssist.Data.Chat
{
    public class ChatMessage
    {
        public string Channel { get; set; }
        public SolidColorBrush Colour { get; set; }
        public string Text { get; set; }
        public string Username { get; set; }

        public override string ToString()
        {
            return $"{Username}: {Text}";
        }
    }
}