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
using System.Threading.Tasks;
using Avalonia.Threading;

namespace ClassicAssist.Avalonia
{
    public class AvaloniaDispatcher : IDispatcher
    {
        private readonly Dispatcher _dispatcher;

        public AvaloniaDispatcher( Dispatcher dispatcher )
        {
            _dispatcher = dispatcher;
        }

        public void Invoke( Action action )
        {
            _dispatcher.Post( action );
        }

        public Task InvokeAsync( Action action )
        {
            return _dispatcher.InvokeAsync( action );
        }
    }
}