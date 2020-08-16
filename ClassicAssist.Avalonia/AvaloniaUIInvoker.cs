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
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using ClassicAssist.Avalonia.Views;
using ClassicAssist.Data;
using ClassicAssist.Shared;
using Engine = Assistant.Engine;

namespace ClassicAssist.Avalonia
{
    public class AvaloniaUIInvoker : IUIInvoker
    {
        private readonly IClipboard _clipboard;
        private readonly Dispatcher _dispatcher;

        public AvaloniaUIInvoker( Dispatcher dispatcher )
        {
            _dispatcher = dispatcher;
            _clipboard = AvaloniaLocator.Current.GetService<IClipboard>();
        }

        public Task Invoke( string typeName, object[] ctorParam = null, Type dataContextType = null,
            object[] dataContextParam = null )
        {
            Type type = Assembly.GetExecutingAssembly().GetTypes()
                .FirstOrDefault( t => t.Name == typeName && t.IsSubclassOf( typeof( Window ) ) );

            if ( type == null )
            {
                Shared.Engine.MessageBoxProvider.Show( $"Cannot find type: {typeName}" );
                return Task.CompletedTask;
            }

            Window window = (Window) Activator.CreateInstance( type, ctorParam );

            if ( window == null )
            {
                throw new ArgumentNullException( $"Failed to create window of type: {typeName}" );
            }

            if ( dataContextType != null )
            {
                object dc = Activator.CreateInstance( dataContextType, dataContextParam );
                window.DataContext = dc;
            }

            return _dispatcher.InvokeAsync( () =>
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

        public Task InvokeDialog<T>( string typeName, object[] ctorParam = null, T dataContext = default ) where T: class
        {
            Type type = Assembly.GetExecutingAssembly().GetTypes()
                .FirstOrDefault( t => t.Name == typeName && t.IsSubclassOf( typeof( Window ) ) );

            if (type == null)
            {
                Shared.Engine.MessageBoxProvider.Show( $"Cannot find type: {typeName}" );
                return Task.CompletedTask;
            }

            Window window = (Window)Activator.CreateInstance( type, ctorParam );

            if (window == null)
            {
                throw new ArgumentNullException( $"Failed to create window of type: {typeName}" );
            }

            if ( dataContext != null )
            {
                window.DataContext = dataContext;
            }

            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            _dispatcher.InvokeAsync( async () =>
            {
                try
                {
                    await window.ShowDialog( Engine.MainWindow );
                    taskCompletionSource.TrySetResult( true );
                }
                catch (Exception e)
                {
                    Console.WriteLine( e.ToString() );
                }
            } );

            return taskCompletionSource.Task;
        }

        public async Task<int> GetHueAsync()
        {
            HuePickerWindow window = new HuePickerWindow { Topmost = Options.CurrentOptions.AlwaysOnTop };

            await window.ShowDialog( Engine.MainWindow );

            return window.SelectedHue;
        }

        public void SetClipboardText( string text )
        {
            _clipboard.SetTextAsync( text ).ConfigureAwait( false );
        }

        public string GetClipboardText()
        {
            return _clipboard.GetTextAsync().GetAwaiter().GetResult();
        }
    }
}