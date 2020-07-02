// ReSharper disable once RedundantUsingDirective

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using ClassicAssist.Data;
using ClassicAssist.Data.Abilities;
using ClassicAssist.Data.Commands;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Data.Macros;
using ClassicAssist.Data.Scavenger;
using ClassicAssist.Data.Targeting;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
using ClassicAssist.UI.Views;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Gumps;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using CUO_API;
using Exceptionless;
using Octokit;

[assembly: InternalsVisibleTo( "ClassicAssist.Tests" )]

// ReSharper disable once CheckNamespace
namespace Assistant
{
    public static partial class Engine
    {
        public delegate void dConnected();

        public delegate void dDisconnected();

        public delegate void dPlayerInitialized( PlayerMobile player );

        public delegate void dSendRecvPacket( byte[] data, int length );

        public delegate void dUpdateWindowTitle();

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
        private static readonly PacketFilter _outgoingPacketPreFilter = new PacketFilter();
        private static readonly PacketFilter _outgoingPacketPostFilter = new PacketFilter();
        private static OnHotkey _onHotkeyPressed;
        private static RequestMove _requestMove;

        private static readonly int[] _sequenceList = new int[256];
        private static OnMouse _onMouse;

        private static readonly DateTime[] _lastMouseAction = new DateTime[(int) MouseOptions.None];
        private static readonly object _clientSendLock = new object();
        private static DateTime _nextPacketRecvTime;

        private static readonly TimeSpan PACKET_RECV_DELAY = TimeSpan.FromMilliseconds( 5 );
        private static readonly object _serverSendLock = new object();

        private static readonly TimeSpan PACKET_SEND_DELAY = TimeSpan.FromMilliseconds( 5 );
        private static DateTime _nextPacketSendTime;

        public static Assembly ClassicAssembly { get; set; }

        public static string ClientPath { get; set; }
        public static Version ClientVersion { get; set; }
        public static bool Connected { get; set; }
        public static ShardEntry CurrentShard { get; set; }
        public static Dispatcher Dispatcher { get; set; }
        public static FeatureFlags Features { get; set; }
        public static GumpCollection Gumps { get; set; } = new GumpCollection();
        public static ItemCollection Items { get; set; } = new ItemCollection( 0 );
        public static CircularBuffer<JournalEntry> Journal { get; set; } = new CircularBuffer<JournalEntry>( 1024 );

        public static DateTime LastActionPacket { get; set; }
        public static int LastPromptID { get; set; }
        public static int LastPromptSerial { get; set; }
        public static TargetQueue<object> LastTargetQueue { get; set; } = new TargetQueue<object>();
        public static MenuCollection Menus { get; set; } = new MenuCollection();
        public static MobileCollection Mobiles { get; set; } = new MobileCollection( Items );
        public static PacketWaitEntries PacketWaitEntries { get; set; }
        public static PlayerMobile Player { get; set; }
        public static QuestPointerList QuestPointers { get; set; } = new QuestPointerList();
        public static RehueList RehueList { get; set; } = new RehueList();
        public static List<ShardEntry> Shards { get; set; }
        public static string StartupPath { get; set; }
        public static bool TargetExists { get; set; }
        public static TargetFlags TargetFlags { get; set; }
        public static int TargetSerial { get; set; }
        public static TargetType TargetType { get; set; }
        public static bool WaitingForTarget { get; set; }
        internal static ConcurrentDictionary<uint, int> GumpList { get; set; } = new ConcurrentDictionary<uint, int>();

        public static event dUpdateWindowTitle UpdateWindowTitleEvent;

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
                _window.Show();
                Dispatcher.Run();
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
            _onMouse = OnMouse;
            WindowHandle = plugin->HWND;

            plugin->OnConnected = Marshal.GetFunctionPointerForDelegate( _onConnected );
            plugin->OnDisconnected = Marshal.GetFunctionPointerForDelegate( _onDisconnected );
            plugin->OnRecv = Marshal.GetFunctionPointerForDelegate( _onReceive );
            plugin->OnSend = Marshal.GetFunctionPointerForDelegate( _onSend );
            plugin->OnPlayerPositionChanged = Marshal.GetFunctionPointerForDelegate( _onPlayerPositionChanged );
            plugin->OnClientClosing = Marshal.GetFunctionPointerForDelegate( _onClientClosing );
            plugin->OnHotkeyPressed = Marshal.GetFunctionPointerForDelegate( _onHotkeyPressed );
            plugin->OnMouse = Marshal.GetFunctionPointerForDelegate( _onMouse );

            _getPacketLength = Marshal.GetDelegateForFunctionPointer<OnGetPacketLength>( plugin->GetPacketLength );
            _getUOFilePath = Marshal.GetDelegateForFunctionPointer<OnGetUOFilePath>( plugin->GetUOFilePath );
            _sendToClient = Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv>( plugin->Recv );
            _sendToServer = Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv>( plugin->Send );
            _requestMove = Marshal.GetDelegateForFunctionPointer<RequestMove>( plugin->RequestMove );

            ClientPath = _getUOFilePath();
            ClientVersion = new Version( (byte) ( plugin->ClientVersion >> 24 ), (byte) ( plugin->ClientVersion >> 16 ),
                (byte) ( plugin->ClientVersion >> 8 ), (byte) plugin->ClientVersion );

            if ( !Path.IsPathRooted( ClientPath ) )
            {
                ClientPath = Path.GetFullPath( ClientPath );
            }

            Art.Initialize( ClientPath );
            Hues.Initialize( ClientPath );
            Cliloc.Initialize( ClientPath );
            Skills.Initialize( ClientPath );
            Speech.Initialize( ClientPath );
            TileData.Initialize( ClientPath );
            Statics.Initialize( ClientPath );
            MapInfo.Initialize( ClientPath );

            ClassicAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault( a => a.FullName.StartsWith( "ClassicUO," ) );
        }

        private static void OnMouse( int button, int wheel )
        {
            MouseOptions mouse = MouseOptions.None;

            if ( button > 0 )
            {
                mouse = SDLKeys.MouseButtonToMouseOptions( button );
            }

            if ( wheel != 0 )
            {
                mouse = wheel < 0 ? MouseOptions.MouseWheelDown : MouseOptions.MouseWheelUp;

                if ( Options.CurrentOptions.LimitMouseWheelTrigger )
                {
                    TimeSpan diff = DateTime.Now - _lastMouseAction[(int) mouse];

                    if ( diff < TimeSpan.FromMilliseconds( Options.CurrentOptions.LimitMouseWheelTriggerMS ) )
                    {
                        return;
                    }
                }

                _lastMouseAction[(int) mouse] = DateTime.Now;
            }

            HotkeyManager.GetInstance().OnMouseAction( mouse );
        }

        private static bool OnHotkeyPressed( int key, int mod, bool pressed )
        {
            Key keys = SDLKeys.SDLKeyToKeys( key );

            bool pass = HotkeyManager.GetInstance().OnHotkeyPressed( keys );

            return !pass;
        }

        private static void OnClientClosing()
        {
            Options.Save( Options.CurrentOptions );
            AssistantOptions.Save();
            ExceptionlessClient.Default.SubmitSessionEnd( AssistantOptions.UserId );
        }

        private static void OnPlayerPositionChanged( int x, int y, int z )
        {
            if ( Player != null )
            {
                Player.X = x;
                Player.Y = y;
                Player.Z = z;
            }

            Items.RemoveByDistance( MAX_DISTANCE, x, y );
            Mobiles.RemoveByDistance( MAX_DISTANCE, x, y );
            ScavengerManager.GetInstance().CheckArea?.Invoke();
        }

        public static Item GetOrCreateItem( int serial, int containerSerial = -1 )
        {
            Item item = Items.GetItem( serial );

            if ( item != null )
            {
                return item;
            }

            item = new Item( serial, containerSerial );

            if ( IncomingPacketHandlers.PropertyCache.TryGetValue( serial, out Property[] properties ) )
            {
                item.Properties = properties;
            }

            return item;
        }

        public static Mobile GetOrCreateMobile( int serial )
        {
            if ( Player?.Serial == serial )
            {
                return Player;
            }

            if ( Mobiles.GetMobile( serial, out Mobile mobile ) )
            {
                return mobile;
            }

            mobile = new Mobile( serial );

            if ( IncomingPacketHandlers.PropertyCache.TryGetValue( serial, out Property[] properties ) )
            {
                mobile.Properties = properties;
            }

            return mobile;
        }

        private static void Initialize()
        {
            StartupPath = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );

            if ( StartupPath == null )
            {
                throw new InvalidOperationException();
            }

            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            PacketWaitEntries = new PacketWaitEntries();

            _incomingQueue = new ThreadQueue<Packet>( ProcessIncomingQueue );
            _outgoingQueue = new ThreadQueue<Packet>( ProcessOutgoingQueue );

            IncomingPacketHandlers.Initialize();
            OutgoingPacketHandlers.Initialize();

            IncomingPacketFilters.Initialize();
            OutgoingPacketFilters.Initialize();

            CommandsManager.Initialize();

            AssistantOptions.Load();
        }

        private static void ProcessIncomingQueue( Packet packet )
        {
            try
            {
                PacketReceivedEvent?.Invoke( packet.GetPacket(), packet.GetLength() );

                PacketHandler handler = IncomingPacketHandlers.GetHandler( packet.GetPacketID() );

                int length = _getPacketLength( packet.GetPacketID() );

                handler?.OnReceive?.Invoke( new PacketReader( packet.GetPacket(), packet.GetLength(), length > 0 ) );

                PacketWaitEntries?.CheckWait( packet.GetPacket(), PacketDirection.Incoming );
            }
            catch ( Exception e )
            {
                e.ToExceptionless().SetProperty( "Packet", packet.GetPacket() )
                    .SetProperty( "Player", Player.ToString() ).SetProperty( "WorldItemCount", Items.Count() )
                    .SetProperty( "WorldMobileCount", Mobiles.Count() ).Submit();
            }
        }

        private static void ProcessOutgoingQueue( Packet packet )
        {
            try
            {
                PacketSentEvent?.Invoke( packet.GetPacket(), packet.GetLength() );

                PacketHandler handler = OutgoingPacketHandlers.GetHandler( packet.GetPacketID() );

                int length = _getPacketLength( packet.GetPacketID() );

                handler?.OnReceive?.Invoke( new PacketReader( packet.GetPacket(), packet.GetLength(), length > 0 ) );

                PacketWaitEntries?.CheckWait( packet.GetPacket(), PacketDirection.Outgoing );
            }
            catch ( Exception e )
            {
                e.ToExceptionless().SetProperty( "Packet", packet.GetPacket() )
                    .SetProperty( "Player", Player.ToString() ).SetProperty( "WorldItemCount", Items.Count() )
                    .SetProperty( "WorldMobileCount", Mobiles.Count() ).Submit();
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
                    SendPacketToClient( new MobileUpdate( mobile.Serial, mobile.ID == 0x191 ? 0x193 : 0x192, mobile.Hue,
                        newStatus, mobile.X, mobile.Y, mobile.Z, mobile.Direction ) );
                }
            };

            Task.Run( async () =>
            {
                try
                {
                    GitHubClient client = new GitHubClient( new ProductHeaderValue( "ClassicAssist" ) );

                    IReadOnlyList<Release> releases =
                        await client.Repository.Release.GetAll( "Reetus", "ClassicAssist" );

                    Release latestRelease = releases.FirstOrDefault();

                    if ( latestRelease == null )
                    {
                        return;
                    }

                    Version latestVersion = Version.Parse( latestRelease.TagName );

                    if ( !Version.TryParse(
                        FileVersionInfo.GetVersionInfo( Path.Combine( StartupPath, "ClassicAssist.dll" ) )
                            .ProductVersion, out Version localVersion ) )
                    {
                        return;
                    }

                    if ( latestVersion > localVersion && AssistantOptions.UpdateGumpVersion < latestVersion )
                    {
                        IReadOnlyList<GitHubCommit> commits =
                            await client.Repository.Commit.GetAll( "Reetus", "ClassicAssist" );

                        IEnumerable<GitHubCommit> latestCommits =
                            commits.OrderByDescending( c => c.Commit.Author.Date ).Take( 15 );

                        StringBuilder commitMessage = new StringBuilder();

                        foreach ( GitHubCommit gitHubCommit in latestCommits )
                        {
                            commitMessage.AppendLine( $"{gitHubCommit.Commit.Author.Date.Date.ToShortDateString()}:" );
                            commitMessage.AppendLine();
                            commitMessage.AppendLine( gitHubCommit.Commit.Message );
                            commitMessage.AppendLine();
                        }

                        StringBuilder message = new StringBuilder();
                        message.AppendLine( Strings.ProductName );
                        message.AppendLine(
                            $"{Strings.New_version_available_} <A HREF=\"https://github.com/Reetus/ClassicAssist/releases/tag/{latestVersion}\">{latestVersion}</A>" );
                        message.AppendLine();
                        message.AppendLine( commitMessage.ToString() );
                        message.AppendLine(
                            $"<A HREF=\"https://github.com/Reetus/ClassicAssist/commits/master\">{Strings.See_More}</A>" );

                        UpdateMessageGump gump =
                            new UpdateMessageGump( WindowHandle, message.ToString(), latestVersion );
                        gump.SendGump();
                    }
                }
                catch ( Exception )
                {
                    // Squash all
                }
            } );

            AbilitiesManager.GetInstance().Enabled = AbilityType.None;
            AbilitiesManager.GetInstance().ResendGump( AbilityType.None );

            Task.Run( async () =>
            {
                await Task.Delay( 3000 );
                MacroManager.GetInstance().Autostart();
            } );
        }

        public static void SendPacketToServer( byte[] packet, int length )
        {
            lock ( _serverSendLock )
            {
                while ( DateTime.Now < _nextPacketSendTime )
                {
                    Thread.Sleep( 1 );
                }

                InternalPacketSentEvent?.Invoke( packet, length );

                PacketWaitEntries?.CheckWait( packet, PacketDirection.Outgoing, true );

                ( byte[] data, int dataLength ) = Utility.CopyBuffer( packet, length );

                _sendToServer?.Invoke( ref data, ref dataLength );

                _nextPacketSendTime = DateTime.Now + PACKET_SEND_DELAY;
            }
        }

        public static void SendPacketToClient( byte[] packet, int length )
        {
            try
            {
                lock ( _clientSendLock )
                {
                    while ( DateTime.Now < _nextPacketRecvTime )
                    {
                        Thread.Sleep( 1 );
                    }

                    InternalPacketReceivedEvent?.Invoke( packet, length );

                    _sendToClient?.Invoke( ref packet, ref length );

                    _nextPacketRecvTime = DateTime.Now + PACKET_RECV_DELAY;
                }
            }
            catch ( ThreadInterruptedException )
            {
                // Macro was interupted whilst we were waiting...
            }
        }

        public static void SendPacketToClient( PacketWriter packet )
        {
            byte[] data = packet.ToArray();

            SendPacketToClient( data, data.Length );
        }

        public static void SendPacketToClient( BasePacket basePacket )
        {
            if ( basePacket.Direction != PacketDirection.Any && basePacket.Direction != PacketDirection.Incoming )
            {
                throw new InvalidOperationException( "Send packet wrong direction." );
            }

            byte[] data = basePacket.ToArray();

            SendPacketToClient( data, data.Length );
        }

        public static void SendPacketToServer( PacketWriter packet )
        {
            byte[] data = packet.ToArray();

            SendPacketToServer( data, data.Length );
        }

        public static void SendPacketToServer( BasePacket basePacket )
        {
            if ( basePacket.Direction != PacketDirection.Any && basePacket.Direction != PacketDirection.Outgoing )
            {
                throw new InvalidOperationException( "Send packet wrong direction." );
            }

            byte[] data = basePacket.ToArray();

            if ( data == null )
            {
                return;
            }

            SendPacketToServer( data, data.Length );
        }

        public static bool Move( Direction direction, bool run )
        {
            return _requestMove?.Invoke( (int) direction, run ) ?? false;
        }

        public static void UpdateWindowTitle()
        {
            UpdateWindowTitleEvent?.Invoke();
        }

        public static void GetMapZ( int x, int y, out sbyte groundZ, out sbyte staticZ )
        {
            groundZ = staticZ = (sbyte) ( Player?.Z ?? 0 );

            if ( ClassicAssembly == null )
            {
                return;
            }

            PropertyInfo mapProperty = ClassicAssembly.GetType( "ClassicUO.Game.World" )?.GetProperty( "Map" );

            if ( mapProperty == null )
            {
                return;
            }

            object mapInstance = mapProperty.GetMethod.Invoke( mapProperty, null );

            MethodInfo getMapZMethod = mapInstance?.GetType().GetMethod( "GetMapZ" );

            if ( getMapZMethod == null )
            {
                return;
            }

            object[] parameters = { x, y, null, null };

            getMapZMethod.Invoke( mapInstance, parameters );

            groundZ = (sbyte) parameters[2];
            staticZ = (sbyte) parameters[3];
        }

        #region ClassicUO Events

        private static bool OnPacketSend( ref byte[] data, ref int length )
        {
            bool filter = false;

            if ( CommandsManager.IsSpeechPacket( data[0] ) )
            {
                filter = CommandsManager.CheckCommand( data, length );
            }

            if ( _outgoingPacketPreFilter.MatchFilterAll( data, out PacketFilterInfo[] pfis ) > 0 )
            {
                foreach ( PacketFilterInfo pfi in pfis )
                {
                    pfi.Action?.Invoke( data, pfi );
                }

                SentPacketFilteredEvent?.Invoke( data, data.Length );

                PacketWaitEntries.CheckWait( data, PacketDirection.Outgoing, true );

                return false;
            }

            if ( OutgoingPacketFilters.CheckPacket( ref data, ref length ) )
            {
                SentPacketFilteredEvent?.Invoke( data, data.Length );

                return false;
            }

            _outgoingQueue.Enqueue( new Packet( data, length ) );

            // ReSharper disable once InvertIf
            if ( _outgoingPacketPostFilter.MatchFilterAll( data, out PacketFilterInfo[] pfisPost ) > 0 )
            {
                foreach ( PacketFilterInfo pfi in pfisPost )
                {
                    pfi.Action?.Invoke( data, pfi );
                }

                SentPacketFilteredEvent?.Invoke( data, data.Length );

                PacketWaitEntries.CheckWait( data, PacketDirection.Outgoing, true );

                return false;
            }

            return !filter;
        }

        public static IntPtr WindowHandle { get; private set; }

        private static bool OnPacketReceive( ref byte[] data, ref int length )
        {
            if ( _incomingPacketFilter.MatchFilterAll( data, out PacketFilterInfo[] pfis ) > 0 )
            {
                foreach ( PacketFilterInfo pfi in pfis )
                {
                    pfi.Action?.Invoke( data, pfi );
                }

                ReceivedPacketFilteredEvent?.Invoke( data, length );

                PacketWaitEntries.CheckWait( data, PacketDirection.Incoming, true );

                return false;
            }

            if ( IncomingPacketFilters.CheckPacket( ref data, ref length ) )
            {
                ReceivedPacketFilteredEvent?.Invoke( data, length );

                return false;
            }

            _incomingQueue.Enqueue( new Packet( data, length ) );

            return true;
        }

        public static Direction GetSequence( int sequence )
        {
            return (Direction) Thread.VolatileRead( ref _sequenceList[sequence] );
        }

        public static void SetSequence( int sequence, Direction direction )
        {
            _sequenceList[sequence] = (int) direction;
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
    }
}