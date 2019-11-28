using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Input;
using ClassicAssist.Data;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Misc;
using ClassicAssist.UI.Views;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using CUO_API;

[assembly: InternalsVisibleTo( "ClassicAssist.Tests" )]

// ReSharper disable once CheckNamespace
namespace Assistant
{
    public static class Engine
    {
        public delegate void dConnected();

        public delegate void dDisconnected();

        public delegate void dPlayerInitialized( PlayerMobile player );

        public delegate void dSendRecvPacket( byte[] data, int length );

        private const int MAX_DISTANCE = 32;

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
        private static ThreadQueue<Packet> _outgoingQueue;
        private static MainWindow _window;
        private static Thread _mainThread;
        private static OnClientClose _onClientClosing;
        private static readonly PacketFilter _incomingPacketFilter = new PacketFilter();
        private static readonly PacketFilter _outgoingPacketFilter = new PacketFilter();
        private static OnHotkey _onHotkeyPressed;

        //private static readonly object _actionDelayLock = new object();
        public static string ClientPath { get; set; }
        public static Version ClientVersion { get; set; }
        public static bool Connected { get; set; }
        public static FeatureFlags Features { get; set; }
        public static ItemCollection Items { get; set; } = new ItemCollection( 0 );
        public static MobileCollection Mobiles { get; set; } = new MobileCollection( Items );

        //public static DateTime NextActionTime { get; set; }
        public static PlayerMobile Player { get; set; }
        public static string StartupPath { get; set; }
        public static int TargetSerial { get; set; }
        public static TargetType TargetType { get; set; }
        public static WaitEntries WaitEntries { get; set; }
        internal static ConcurrentDictionary<int, int> GumpList { get; set; } = new ConcurrentDictionary<int, int>();

        internal static event dSendRecvPacket InternalPacketSentEvent;
        internal static event dSendRecvPacket InternalPacketReceivedEvent;

        public static event dSendRecvPacket PacketReceivedEvent;
        public static event dSendRecvPacket PacketSentEvent;
        public static event dSendRecvPacket SentPacketFilteredEvent;
        public static event dSendRecvPacket ReceivedPacketFilteredEvent;
        public static event dConnected ConnectedEvent;
        public static event dDisconnected DisconnectedEvent;
        public static event dPlayerInitialized PlayerInitializedEvent;

        public static unsafe void Install( PluginHeader* plugin )
        {
            Initialize();

            InitializePlugin( plugin );

            _mainThread = new Thread( () =>
            {
                _window = new MainWindow();
                _window.ShowDialog();
            } ) { IsBackground = true };

            _mainThread.SetApartmentState( ApartmentState.STA );
            _mainThread.Start();
        }

        internal static unsafe void InitializePlugin( PluginHeader* plugin )
        {
            _onConnected = OnConnected;
            _onDisconnected = OnDisconnected;
            _onReceive = OnPacketReceive;
            _onSend = OnPacketSend;
            _onPlayerPositionChanged = OnPlayerPositionChanged;
            _onClientClosing = OnClientClosing;
            _onHotkeyPressed = OnHotkeyPressed;

            plugin->OnConnected = Marshal.GetFunctionPointerForDelegate( _onConnected );
            plugin->OnDisconnected = Marshal.GetFunctionPointerForDelegate( _onDisconnected );
            plugin->OnRecv = Marshal.GetFunctionPointerForDelegate( _onReceive );
            plugin->OnSend = Marshal.GetFunctionPointerForDelegate( _onSend );
            plugin->OnPlayerPositionChanged = Marshal.GetFunctionPointerForDelegate( _onPlayerPositionChanged );
            plugin->OnClientClosing = Marshal.GetFunctionPointerForDelegate( _onClientClosing );
            plugin->OnHotkeyPressed = Marshal.GetFunctionPointerForDelegate( _onHotkeyPressed );

            _getPacketLength = Marshal.GetDelegateForFunctionPointer<OnGetPacketLength>( plugin->GetPacketLength );
            _getUOFilePath = Marshal.GetDelegateForFunctionPointer<OnGetUOFilePath>( plugin->GetUOFilePath );
            _sendToClient = Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv>( plugin->Recv );
            _sendToServer = Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv>( plugin->Send );

            ClientPath = _getUOFilePath();

            if ( !Path.IsPathRooted( ClientPath ) )
            {
                ClientPath = Path.GetFullPath( ClientPath );
            }

            Art.Initialize( ClientPath );
            Hues.Initialize( ClientPath );
            Cliloc.Initialize( ClientPath );
            Skills.Initialize( ClientPath );
            TileData.Initialize( ClientPath );
        }

        private static bool OnHotkeyPressed( int key, int mod, bool pressed )
        {
            Key keys = SDLKeys.SDLKeyToKeys( key );

            bool pass = HotkeyManager.GetInstance().OnHotkeyPressed( keys );

            return !pass;
        }

        private static void OnClientClosing()
        {
            Options.Save( StartupPath );
        }

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
            return Items.GetItem( serial ) ?? new Item( serial, containerSerial );
        }

        public static Mobile GetOrCreateMobile( int serial )
        {
            if ( Player?.Serial == serial )
            {
                return Player;
            }

            return Mobiles.GetMobile( serial, out Mobile mobile ) ? mobile : new Mobile( serial );
        }

        private static void Initialize()
        {
            StartupPath = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );

            if ( StartupPath == null )
            {
                throw new InvalidOperationException();
            }

            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            WaitEntries = new WaitEntries();

            _incomingQueue = new ThreadQueue<Packet>( ProcessIncomingQueue );
            _outgoingQueue = new ThreadQueue<Packet>( ProcessOutgoingQueue );

            IncomingPacketHandlers.Initialize();
            OutgoingPacketHandlers.Initialize();
        }

        private static void ProcessIncomingQueue( Packet packet )
        {
            PacketReceivedEvent?.Invoke( packet.GetPacket(), packet.GetLength() );

            PacketHandler handler = IncomingPacketHandlers.GetHandler( packet.GetPacketID() );

            int length = _getPacketLength( packet.GetPacketID() );

            handler?.OnReceive?.Invoke( new PacketReader( packet.GetPacket(), packet.GetLength(), length > 0 ) );
        }

        private static void ProcessOutgoingQueue( Packet packet )
        {
            PacketSentEvent?.Invoke( packet.GetPacket(), packet.GetLength() );

            PacketHandler handler = OutgoingPacketHandlers.GetHandler( packet.GetPacketID() );

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

                if ( !File.Exists( fullPath ) )
                {
                    continue;
                }

                Assembly assembly = Assembly.LoadFrom( fullPath );
                return assembly;
            }

            return null;
        }

        public static void SetPlayer( PlayerMobile mobile )
        {
            Player = mobile;

            PlayerInitializedEvent?.Invoke( mobile );

            mobile.MobileStatusUpdated += ( status, newStatus ) =>
            {
                if ( !Options.CurrentOptions.UseDeathScreenWhilstHidden )
                {
                    return;
                }

                if ( newStatus.HasFlag( MobileStatus.Hidden ) )
                {
                    SendPacketToClient( new MobileDeadIncoming( mobile ) );
                }
            };
        }

        public static void SendPacketToServer( byte[] packet, int length )
        {
            InternalPacketSentEvent?.Invoke( packet, length );

            _sendToServer?.Invoke( ref packet, ref length );
        }

        public static void SendPacketToClient( byte[] packet, int length )
        {
            InternalPacketReceivedEvent?.Invoke( packet, length );

            _sendToClient?.Invoke( ref packet, ref length );
        }

        public static void SendPacketToClient( PacketWriter packet )
        {
            byte[] data = packet.ToArray();

            SendPacketToClient( data, data.Length );
        }

        public static void SendPacketToClient( Packets packet )
        {
            byte[] data = packet.ToArray();

            SendPacketToClient( data, data.Length );
        }

        public static void SendPacketToServer( PacketWriter packet )
        {
            byte[] data = packet.ToArray();

            SendPacketToServer( data, data.Length );
        }

        public static void SendPacketToServer( Packets packet )
        {
            byte[] data = packet.ToArray();

            SendPacketToServer( data, data.Length );
        }

        #region ClassicUO Events

        private static bool OnPacketSend( ref byte[] data, ref int length )
        {
            if ( _outgoingPacketFilter.MatchFilterAll( data, out PacketFilterInfo[] pfis ) > 0 )
            {
                foreach ( PacketFilterInfo pfi in pfis )
                {
                    pfi.Action?.Invoke( data, pfi );
                }

                SentPacketFilteredEvent?.Invoke( data, data.Length );

                return false;
            }

            _outgoingQueue.Enqueue( new Packet( data, length ) );

            WaitEntries.CheckWait( data, PacketDirection.Outgoing );

            return true;
        }

        private static bool OnPacketReceive( ref byte[] data, ref int length )
        {
            if ( _incomingPacketFilter.MatchFilterAll( data, out PacketFilterInfo[] pfis ) > 0 )
            {
                foreach ( PacketFilterInfo pfi in pfis )
                {
                    pfi.Action?.Invoke( data, pfi );
                }

                ReceivedPacketFilteredEvent?.Invoke( data, data.Length );

                return false;
            }

            _incomingQueue.Enqueue( new Packet( data, length ) );

            WaitEntries.CheckWait( data, PacketDirection.Incoming );

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

            Items.Clear();
            Mobiles.Clear();
            Player = null;

            DisconnectedEvent?.Invoke();
        }

        #endregion

        #region Filters

        public static void AddSendFilter( PacketFilterInfo pfi )
        {
            _outgoingPacketFilter.Add( pfi );
        }

        public static void AddReceiveFilter( PacketFilterInfo pfi )
        {
            _incomingPacketFilter.Add( pfi );
        }

        public static void RemoveReceiveFilter( PacketFilterInfo pfi )
        {
            _incomingPacketFilter.Remove( pfi );
        }

        public static void RemoveSendFilter( PacketFilterInfo pfi )
        {
            _outgoingPacketFilter.Remove( pfi );
        }

        public static void ClearSendFilter()
        {
            _outgoingPacketFilter?.Clear();
        }

        public static void ClearReceiveFilter()
        {
            _incomingPacketFilter?.Clear();
        }

        #endregion
    }
}