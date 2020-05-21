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

using System.Threading;
using System.Windows.Threading;
using Assistant;
using ClassicAssist.UI.Views;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Name = "Show Chat Window" )]
    public class ShowChatWindowCommand : HotkeyCommand
    {
        public override void Execute()
        {
            Engine.Dispatcher.Invoke( () =>
            {
                Thread t = new Thread( () =>
                {
                    ChatWindow window = new ChatWindow();

                    window.Show();
                    Dispatcher.Run();
                } ) { IsBackground = true };

                t.SetApartmentState( ApartmentState.STA );
                t.Start();
            } );
        }
    }
}