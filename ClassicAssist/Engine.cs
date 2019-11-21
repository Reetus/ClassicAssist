using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Logging.Serilog;
using ClassicAssist;
using ClassicAssist.Misc;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UI.Views;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Objects;
using CUO_API;

// ReSharper disable once CheckNamespace
namespace Assistant
{
    public static class Engine
    {
        public delegate void dConnected();

        public delegate void dDisconnected();

        private static OnConnected _onConnected;
        private static OnDisconnected _onDisconnected;
        private static OnPacketSendRecv _onReceive;
        private static OnPacketSendRecv _onSend;
        private static OnGetUOFilePath _getUOFilePath;
        private static OnPacketSendRecv _sendToClient;
        private static OnPacketSendRecv _sendToServer;
        private static OnGetPacketLength _getPacketLength;
        private static ThreadQueue<Packet> _incomingQueue;
        private static OnUpdatePlayerPosition _onPlayerPositionChanged;

        public static ItemCollection Items { get; set; } = new ItemCollection( 0 );
        public static MobileCollection Mobiles { get; set; } = new MobileCollection( Items );

        public static string StartupPath { get; set; }
        public static string ClientPath { get; set; }
        public static bool Connected { get; set; }

        public static PlayerMobile Player { get; private set; }

        public static event dConnected ConnectedEvent;
        public static event dDisconnected DisconnectedEvent;

        public static unsafe void Install( PluginHeader* plugin )
        {
            Initialize();

            while ( !Debugger.IsAttached )
            {
                Thread.Sleep( 100 );
            }

            InitializePlugin( plugin );

            Task.Run( () => { BuildAvaloniaApp().Start( AppMain, new string[] { } ); } );
        }

        private static unsafe void InitializePlugin( PluginHeader* plugin )
        {
            _onConnected = OnConnected;
            _onDisconnected = OnDisconnected;
            _onReceive = OnPacketReceive;
            _onSend = OnPacketSend;
            _onPlayerPositionChanged = OnPlayerPositionChanged;

            plugin->OnConnected = Marshal.GetFunctionPointerForDelegate( _onConnected );
            plugin->OnDisconnected = Marshal.GetFunctionPointerForDelegate( _onDisconnected );
            plugin->OnRecv = Marshal.GetFunctionPointerForDelegate( _onReceive );
            plugin->OnSend = Marshal.GetFunctionPointerForDelegate( _onSend );
            plugin->OnPlayerPositionChanged = Marshal.GetFunctionPointerForDelegate( _onPlayerPositionChanged );

            _getPacketLength = Utility.GetDelegateForFunctionPointer<OnGetPacketLength>( plugin->GetPacketLength );
            _getUOFilePath = Utility.GetDelegateForFunctionPointer<OnGetUOFilePath>( plugin->GetUOFilePath );
            _sendToClient = Utility.GetDelegateForFunctionPointer<OnPacketSendRecv>( plugin->Recv );
            _sendToServer = Utility.GetDelegateForFunctionPointer<OnPacketSendRecv>( plugin->Send );

            ClientPath = _getUOFilePath();
        }

        private const int MAX_DISTANCE = 32;

        private static void OnPlayerPositionChanged( int x, int y, int z )
        {
            Player.X = x;
            Player.Y = y;
            Player.Z = z;

            Items.RemoveByDistance( MAX_DISTANCE, x, y );
            Mobiles.RemoveByDistance( MAX_DISTANCE, x, y );
        }

        public static Item GetOrCreateItem( int serial, int containerSerial = -1 )
        {
            return Items.GetItem( serial ) != null ? Items.GetItem( serial ) : new Item( serial, containerSerial );
        }

        private static void Initialize()
        {
            StartupPath = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );

            if ( StartupPath == null )
            {
                throw new InvalidOperationException();
            }

            string[] assembles =
            {
                Path.Combine( StartupPath, "System.Runtime.CompilerServices.Unsafe.dll" ),
                Path.Combine( StartupPath, "ReactiveUI.dll" )
            };

            foreach ( string assembly in assembles )
            {
                Assembly.LoadFile( assembly );
            }

            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            _incomingQueue = new ThreadQueue<Packet>( ProcessIncomingQueue );

            IncomingPacketHandlers.Initialize();
        }

        private static void ProcessIncomingQueue( Packet packet )
        {
            PacketHandler handler = IncomingPacketHandlers.GetHandler( packet.GetPacketID() );

            int length = _getPacketLength( packet.GetPacketID() );

            handler?.OnReceive?.Invoke( new PacketReader( packet.GetPacket(), packet.GetLength(), length > 0 ) );
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

                if ( File.Exists( fullPath ) )
                {
                    Assembly assembly = Assembly.LoadFrom( fullPath );
                    return assembly;
                }
            }

            return null;
        }

        public static void SetPlayer( PlayerMobile mobile )
        {
            Player = mobile;
        }

        #region UI Code

        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>().UsePlatformDetect().LogToDebug().UseReactiveUI();
        }

        private static void AppMain( Application app, string[] args )
        {
            MainWindow window = new MainWindow { DataContext = new MainWindowViewModel() };

            app.Run( window );
        }

        #endregion

        #region ClassicUO Events

        private static bool OnPacketSend( ref byte[] data, ref int length )
        {
            return true;
        }

        private static bool OnPacketReceive( ref byte[] data, ref int length )
        {
            _incomingQueue.Enqueue( new Packet( data, length ) );

            return true;
        }

        private static void OnConnected()
        {
            Connected = true;

            ConnectedEvent?.Invoke();
        }

        private static void OnDisconnected()
        {
            Connected = false;

            DisconnectedEvent?.Invoke();
        }

        #endregion
    }
}