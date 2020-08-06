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

// ReSharper disable once CheckNamespace
using System.Threading.Tasks;

namespace ClassicAssist.Misc
{
    public enum WindowType
    {
        WPF,
        Avalonia
    }

    public struct SharedWindow
    {
        public object Window;
        public WindowType Type;
        public string Caption;
        public string Text;
        public MessageBoxButtons Buttons;
        public MessageBoxImage? Icon;
    }

    public enum MessageBoxButtons
    {
        OK,
        YesNo
    }

    public enum MessageBoxResult
    {
        OK,
        Yes,
        No,
        Cancel
    }

    public enum MessageBoxImage
    {
        Information,
        Error,
        Warning
    }

    public interface IMessageBoxProvider
    {
        Task<MessageBoxResult> Show( string text, string caption = null,
            MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxImage? icon = null );
    }
}