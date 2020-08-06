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
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Threading;
using ClassicAssist.Shared;

namespace ClassicAssist.Avalonia
{
    public class AvaloniaUIInvoker : IUIInvoker
    {
        private readonly Dispatcher _dispatcher;

        public AvaloniaUIInvoker( Dispatcher dispatcher )
        {
            _dispatcher = dispatcher;
        }

        public void Invoke( string typeName, object[] ctorParam = null, Type dataContextType = null,
            object[] dataContextParam = null )
        {
            Type type = Assembly.GetExecutingAssembly().GetTypes()
                .FirstOrDefault( t => t.Name == typeName && t.IsSubclassOf( typeof( Window ) ) );

            if ( type == null )
            {
                throw new ArgumentNullException( $"Cannot find type: ${typeName}" );
            }

            Window window = (Window) Activator.CreateInstance( type, ctorParam );

            if ( window == null )
            {
                throw new ArgumentNullException( $"Failed to create window of type: ${typeName}" );
            }

            if ( dataContextType != null )
            {
                object dc = Activator.CreateInstance( dataContextType, dataContextParam );
                window.DataContext = dc;
            }

            _dispatcher.InvokeAsync( () =>
            {
                try
                {
                    window.Show();
                }
                catch ( Exception e )
                {
                    Console.WriteLine( e.ToString() );
                }
            } );
        }
    }
}