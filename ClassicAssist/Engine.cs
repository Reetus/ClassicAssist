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
using System.Windows.Threading;
using ClassicAssist;
using ClassicAssist.Data;
using ClassicAssist.Misc;
using ClassicAssist.UI.Misc;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UI.Views;
using ClassicAssist.UO.Data;
using CUO_API;
using Newtonsoft.Json.Linq;
using SEngine = ClassicAssist.Shared.Engine;

// ReSharper disable once CheckNamespace
namespace Assistant
{
    public static unsafe class Engine
    {
        private static PluginHeader* _plugin;
        private static Thread _mainThread;
        private static MainWindow _window;

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

            _mainThread = new Thread( () =>
            {
                SEngine.Dispatcher = new WPFDispatcher( Dispatcher.CurrentDispatcher );
                SEngine.UIInvoker = new WPFUIInvoker( Dispatcher.CurrentDispatcher );
                _window = new MainWindow();
                _window.Show();

                Dispatcher.Run();
            } ) { IsBackground = true };

            _mainThread.SetApartmentState( ApartmentState.STA );
            _mainThread.Start();
        }

        private static void Initialize( PluginHeader* plugin )
        {
            SEngine.Install( plugin, new WPFMessageBoxProvider() );

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