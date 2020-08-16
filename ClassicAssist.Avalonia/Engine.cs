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
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using ClassicAssist.Avalonia;
using ClassicAssist.Avalonia.Misc;
using ClassicAssist.Avalonia.Views;
using ClassicAssist.Data;
using ClassicAssist.Misc;
using ClassicAssist.Shared;
using ClassicAssist.UI.ViewModels;
using CUO_API;
using Newtonsoft.Json.Linq;
using SEngine = ClassicAssist.Shared.Engine;

// ReSharper disable once CheckNamespace
namespace Assistant
{
    public static unsafe class Engine
    {
        private static PluginHeader* _plugin;

        public static MainWindow MainWindow { get; internal set; }

        public static string StartupPath { get; set; }

        public static void Install( PluginHeader* plugin )
        {
            _plugin = plugin;

            StartupPath = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );

            if ( StartupPath == null )
            {
                throw new InvalidOperationException();
            }

            Assembly.LoadFile( Path.Combine( StartupPath, "ClassicAssist.Shared.dll" ) );

            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            Initialize( plugin );

            // The Avalonia APIs must be used in a separate method _after_ the
            // AssemblyResolve property has been set. Otherwise, the static
            // run-time type initializer for Engine.Install will fail to load 
            // the Avalonia.Controls.dll that are in the plugin's output folder.
            LoadUI();
        }

        private static void LoadUI()
        {
            if ( SEngine.GetPlatformType() == PlatformType.Unix )
            {
                // Launch UI in a thread on Linux
                Thread mainThread = new Thread( () =>
                {
                    SEngine.Dispatcher = new AvaloniaDispatcher( Dispatcher.UIThread );
                    SEngine.UIInvoker = new AvaloniaUIInvoker( Dispatcher.UIThread );
                    AppBuilder.Configure<App>().UsePlatformDetect().LogToDebug()
                        .StartWithClassicDesktopLifetime( null );
                } ) { IsBackground = true };

                mainThread.Start();
            }
            else
            {
                // Set up the Avalonia application without starting. This
                // initializes the Avalonia APIs.
                AppBuilder.Configure<App>().UsePlatformDetect().LogToDebug().UseReactiveUI().SetupWithoutStarting();

                // Avalonia is set up, so can create dispatcher.
                SEngine.Dispatcher = new AvaloniaDispatcher( Dispatcher.UIThread );
                SEngine.UIInvoker = new AvaloniaUIInvoker( Dispatcher.UIThread );

                // Showing a window is nonblocking; only `await window.ShowDialog(parent)` blocks
                MainWindow = new MainWindow();
                MainWindow.Show();
            }
        }

        private static void Initialize( PluginHeader* plugin )
        {
            SEngine.Install( plugin, new AvaloniaMessageBoxProvider() );

            Options.LoadEvent += OnOptionsLoad;
            Options.SaveEvent += OnOptionsSave;
        }

        private static void OnOptionsSave( JObject obj )
        {
            BaseViewModel[] instances = BaseViewModel.Instances;

            foreach ( BaseViewModel instance in instances )
            {
                if ( instance is ISettingProvider settingProvider )
                {
                    settingProvider.Serialize( obj );
                }
            }
        }

        private static void OnOptionsLoad( JObject json, Options options )
        {
            BaseViewModel[] instances = BaseViewModel.Instances;

            foreach ( BaseViewModel instance in instances )
            {
                if ( instance is ISettingProvider settingProvider )
                {
                    settingProvider.Deserialize( json, options );
                }
            }
        }

        private static Assembly OnAssemblyResolve( object sender, ResolveEventArgs args )
        {
            string assemblyname = new AssemblyName( args.Name ).Name;

            string[] searchPaths = { StartupPath, RuntimeEnvironment.GetRuntimeDirectory() };

            if ( assemblyname.Contains( "Colletions" ) )
            {
                assemblyname = "System.Collections";
            }

            foreach ( string searchPath in searchPaths )
            {
                string fullPath = Path.Combine( searchPath, assemblyname + ".dll" );

                string culture = new AssemblyName( args.Name ).CultureName;

                if ( !File.Exists( fullPath ) )
                {
                    string culturePath = Path.Combine( searchPath, culture, assemblyname + ".dll" );

                    if ( File.Exists( culturePath ) )
                    {
                        fullPath = culturePath;
                    }
                    else
                    {
                        continue;
                    }
                }

                Assembly assembly = Assembly.LoadFrom( fullPath );

                return assembly;
            }

            return null;
        }
    }
}