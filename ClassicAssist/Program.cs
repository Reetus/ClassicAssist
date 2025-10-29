#region License

// Copyright (C) 2025 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System;
using System.IO.Pipes;
using System.Windows.Forms;
using Assistant;
using ClassicAssist.Shared;
using ClassicAssist.UI.Views;
using ClassicAssist.UO.Data;
using StreamJsonRpc;

namespace ClassicAssist
{
    public static class Program
    {
        private static MainWindow _window;

        [STAThread]
        public static void Main( string[] args )
        {
            if ( args == null || args.Length == 0 )
            {
                return;
            }

            string pipeName = args[0];

            NamedPipeClientStream clientStream = new NamedPipeClientStream( ".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous );
            clientStream.Connect();

            // Attach client RPC
            var pluginMethods = new Engine.PluginMethods();
            JsonRpc rpc = JsonRpc.Attach( clientStream, pluginMethods );
            IHostMethods host = rpc.Attach<IHostMethods>();

            Engine.Install( rpc, host, pluginMethods );
        }
    }
}