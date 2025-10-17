// ReSharper disable once RedundantUsingDirective

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
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
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Data.Misc;
using ClassicAssist.Data.Scavenger;
using ClassicAssist.Data.Targeting;
using ClassicAssist.Misc;
using ClassicAssist.Shared;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UI.Views;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Gumps;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using CUO_API;
using Newtonsoft.Json;
using Sentry;
using static ClassicAssist.Misc.SDLKeys;
using MacroManager = ClassicAssist.Data.Macros.MacroManager;

[assembly: InternalsVisibleTo( "ClassicAssist.Tests" )]
[assembly: InternalsVisibleTo( "ClassicAssist.UITest" )]

// ReSharper disable once CheckNamespace
namespace Assistant
{
    public static partial class Engine
    {
        public delegate void dClientClosing();

        public delegate void dConnected();

        public delegate void dDisconnected();

        public delegate void dFocusChanged( bool focus );

        public delegate void dHotkeyPressed( int key, int mod, Key keys, ModKey modKey );

        public delegate void dPlayerInitialized( PlayerMobile player );

        public delegate void dSendRecvPacket( byte[] data, int length );

        public delegate void dSlowHandler( PacketDirection direction, string handlerName, TimeSpan elapsed );

        public delegate void dUpdateWindowTitle();

        private const int MAX_DISTANCE = 32;

        private static OnConnected _onConnected;
        private static OnDisconnected _onDisconnected;
        private static OnPacketSendRecv _onReceive;
        private static OnPacketSendRecv _onSend;
        private static OnTick _onTick;
        private static OnGetUOFilePath _getUOFilePath;
        private static OnPacketSendRecv _sendToClient;
        private static OnPacketSendRecv _sendToServer;
        private static OnGetPacketLength _getPacketLength;
        private static OnUpdatePlayerPosition _onPlayerPositionChanged;
        private static OnSetTitle _setTitle;
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
        private static readonly Dictionary<Key, DateTime> _lastKeyAction = new Dictionary<Key, DateTime>();
        private static readonly object _clientSendLock = new object();
        private static DateTime _nextPacketRecvTime;

        private static readonly TimeSpan PACKET_RECV_DELAY = TimeSpan.FromMilliseconds( 5 );
        private static readonly object _serverSendLock = new object();

        private static readonly TimeSpan PACKET_SEND_DELAY = TimeSpan.FromMilliseconds( 5 );
        private static DateTime _nextPacketSendTime;
        public static int LastSpellID;
        private static Stopwatch _incomingStopwatch;
        private static Stopwatch _outgoingStopwatch;
        public static int LastSkillID;
        private static OnFocusGained _onFocusGained;
        private static OnFocusLost _onFocusLost;
        private static bool _clientHasFocus;
        public static CharacterListFlags CharacterListFlags { get; set; }

        public static Assembly ClassicAssembly { get; set; }

        public static string ClientPath { get; set; }
        public static Version ClientVersion { get; set; }
        public static bool Connected { get; set; }

        public static string CUOPath { get; set; }
        public static ShardEntry CurrentShard { get; set; }
        public static Dispatcher Dispatcher { get; set; }
        public static FeatureFlags Features { get; set; }
        public static GumpCollection Gumps { get; set; } = new GumpCollection();
        public static ItemCollection Items { get; set; } = new ItemCollection( 0 );
        public static CircularBuffer<JournalEntry> Journal { get; set; } = new CircularBuffer<JournalEntry>( 1024 );

        public static int KeyboardLayoutId { get; set; }

        public static DateTime LastActionPacket { get; set; }
        public static int LastPromptID { get; set; }
        public static int LastPromptSerial { get; set; }
        public static DateTime LastSkillTime { get; set; }

        public static TargetQueue<TargetQueueObject> LastTargetQueue { get; set; } = new TargetQueue<TargetQueueObject>();

        public static MenuCollection Menus { get; set; } = new MenuCollection();
        public static MobileCollection Mobiles { get; set; } = new MobileCollection( Items );
        public static PacketWaitEntries PacketWaitEntries { get; set; }
        public static PlayerMobile Player { get; set; }
        public static QuestPointerList QuestPointers { get; set; } = new QuestPointerList();
        public static RehueList RehueList { get; set; } = new RehueList();
        public static List<ShardEntry> Shards { get; set; }

        public static Dispatcher StartupDispatcher { get; set; }
        public static string StartupPath { get; set; }
        public static bool TargetExists { get; set; }
        public static TargetFlags TargetFlags { get; set; }
        public static int TargetSerial { get; set; }
        public static TargetType TargetType { get; set; }

        public static Queue<Action> TickWorkQueue { get; set; } = new Queue<Action>();
        public static bool TooltipsEnabled { get; set; }
        public static bool WaitingForTarget { get; set; }
        internal static ConcurrentDictionary<uint, int> GumpList { get; set; } = new ConcurrentDictionary<uint, int>();

        public static event dHotkeyPressed HotkeyPressedEvent;

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
                SplashWindow splashWindow = new SplashWindow();
                splashWindow.Show();

                _window = new MainWindow();
                _window.Show();

                splashWindow.Close();

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
            _onTick = OnTick;
            _onFocusGained = () => OnFocusChanged( true );
            _onFocusLost = () => OnFocusChanged( false );
            WindowHandle = plugin->HWND;

            plugin->OnConnected = Marshal.GetFunctionPointerForDelegate( _onConnected );
            plugin->OnDisconnected = Marshal.GetFunctionPointerForDelegate( _onDisconnected );
            plugin->OnRecv = Marshal.GetFunctionPointerForDelegate( _onReceive );
            plugin->OnSend = Marshal.GetFunctionPointerForDelegate( _onSend );
            plugin->OnPlayerPositionChanged = Marshal.GetFunctionPointerForDelegate( _onPlayerPositionChanged );
            plugin->OnClientClosing = Marshal.GetFunctionPointerForDelegate( _onClientClosing );
            plugin->OnHotkeyPressed = Marshal.GetFunctionPointerForDelegate( _onHotkeyPressed );
            plugin->OnMouse = Marshal.GetFunctionPointerForDelegate( _onMouse );
            plugin->Tick = Marshal.GetFunctionPointerForDelegate( _onTick );
            plugin->OnFocusGained = Marshal.GetFunctionPointerForDelegate( _onFocusGained );
            plugin->OnFocusLost = Marshal.GetFunctionPointerForDelegate( _onFocusLost );

            _getPacketLength = Marshal.GetDelegateForFunctionPointer<OnGetPacketLength>( plugin->GetPacketLength );
            _getUOFilePath = Marshal.GetDelegateForFunctionPointer<OnGetUOFilePath>( plugin->GetUOFilePath );
            _sendToClient = Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv>( plugin->Recv );
            _sendToServer = Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv>( plugin->Send );
            _requestMove = Marshal.GetDelegateForFunctionPointer<RequestMove>( plugin->RequestMove );
            _setTitle = Marshal.GetDelegateForFunctionPointer<OnSetTitle>( plugin->SetTitle );

            ClientPath = _getUOFilePath();
            ClientVersion = new Version( (byte) ( plugin->ClientVersion >> 24 ), (byte) ( plugin->ClientVersion >> 16 ), (byte) ( plugin->ClientVersion >> 8 ),
                (byte) plugin->ClientVersion );

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

            ClassicAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault( a => a.FullName.StartsWith( "ClassicUO," ) );

            if ( ClassicAssembly != null )
            {
                CUOPath = Path.GetDirectoryName( ClassicAssembly.Location );
            }

            InitializeExtensions();
        }

        private static void OnFocusChanged( bool focus )
        {
            if ( focus == _clientHasFocus )
            {
                return;
            }

            _clientHasFocus = focus;

            FocusChangedEvent?.Invoke( focus );
        }

        public static event dFocusChanged FocusChangedEvent;

        private static void OnTick()
        {
            try
            {
                while ( TickWorkQueue.Count > 0 )
                {
                    Action action = TickWorkQueue.Dequeue();

                    action?.Invoke();
                }
            }
            catch ( Exception e )
            {
                SentrySdk.CaptureException( e );
                Commands.SystemMessage( e.Message );
            }
        }

        private static void InitializeExtensions()
        {
            IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes().Where( t => typeof( IExtension ).IsAssignableFrom( t ) && t.IsClass );

            foreach ( Type type in types )
            {
                try
                {
                    IExtension instance = (IExtension) Activator.CreateInstance( type );
                    instance?.Initialize();
                }
                catch ( Exception e )
                {
                    Console.WriteLine( e.ToString() );
                }
            }
        }

        private static void OnMouse( int button, int wheel )
        {
            MouseOptions mouse = MouseOptions.None;

            if ( button > 0 )
            {
                mouse = MouseButtonToMouseOptions( button );
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

            Dispatcher.Invoke( () =>
            {
                HotkeyManager.GetInstance().OnMouseAction( mouse );
            } );
        }

        private static bool OnHotkeyPressed( int key, int mod, bool pressed )
        {
            Key keys = SDLKeyToKeys( key );

            if ( !pressed )
            {
                return true;
            }

            bool noexecute = false;

            if ( Options.CurrentOptions.LimitHotkeyTrigger )
            {
                if ( _lastKeyAction.TryGetValue( keys, out DateTime lastAction ) )
                {
                    TimeSpan diff = DateTime.Now - lastAction;

                    if ( diff < TimeSpan.FromMilliseconds( Options.CurrentOptions.LimitHotkeyTriggerMS ) )
                    {
                        noexecute = true;
                    }
                }
            }

            if ( !noexecute )
            {
                HotkeyPressedEvent?.Invoke( key, mod, keys, IntToModKey( mod ) );
            }

            ( bool found, bool pass ) = HotkeyManager.GetInstance().OnHotkeyPressed( keys, IntToModKey( mod ), noexecute );

            if ( found && !noexecute )
            {
                _lastKeyAction[keys] = DateTime.Now;
            }

            return !pass;
        }

        public static event dClientClosing ClientClosing;

        private static void OnClientClosing()
        {
            ClientClosing?.Invoke();
            Options.Save( Options.CurrentOptions );
            AssistantOptions.Save();
            SentrySdk.Close();
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

            Task.Run( () => { ScavengerManager.GetInstance().CheckArea?.Invoke(); } ).ConfigureAwait( false );
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

            IncomingQueue = new ThreadQueue<Packet>( ProcessIncomingQueue );
            OutgoingQueue = new ThreadQueue<Packet>( ProcessOutgoingQueue );

            IncomingPacketHandlers.Initialize();
            OutgoingPacketHandlers.Initialize();

            IncomingPacketFilters.Initialize();
            OutgoingPacketFilters.Initialize();

            CommandsManager.Initialize();

            StartupDispatcher = Dispatcher.CurrentDispatcher;

            AssistantOptions.Load();
        }

        private static void ProcessIncomingQueue( Packet packet )
        {
            if ( _incomingStopwatch == null )
            {
                _incomingStopwatch = new Stopwatch();
            }

            _incomingStopwatch.Reset();
            _incomingStopwatch.Start();

            string handlerName = "None";

            try
            {
                PacketReceivedEvent?.Invoke( packet.GetPacket(), packet.GetLength() );

                PacketHandler handler = IncomingPacketHandlers.GetHandler( packet.GetPacketID() );

                int length = _getPacketLength( packet.GetPacketID() );

                if ( handler != null )
                {
                    handlerName = handler.OnReceive.Method.Name;
                }

                handler?.OnReceive?.Invoke( new PacketReader( packet.GetPacket(), packet.GetLength(), length > 0 ) );

                PacketWaitEntries?.CheckWait( packet.GetPacket(), PacketDirection.Incoming );
            }
            catch ( Exception e )
            {
                SentrySdk.CaptureException( e, scope =>
                {
                    scope.SetExtra( "Packet", packet.GetPacket() );
                    scope.SetExtra( "Player", Player.ToString() );
                    scope.SetExtra( "WorldItemCount", Items.Count() );
                    scope.SetExtra( "WorldMobileCount", Mobiles.Count() );
                } );
            }

            _incomingStopwatch.Stop();

            if ( _incomingStopwatch.ElapsedMilliseconds >= Options.CurrentOptions.SlowHandlerThreshold )
            {
                SlowHandlerEvent?.Invoke( PacketDirection.Incoming, handlerName, _incomingStopwatch.Elapsed );
            }
        }

        public static event dSlowHandler SlowHandlerEvent;

        private static void ProcessOutgoingQueue( Packet packet )
        {
            if ( _outgoingStopwatch == null )
            {
                _outgoingStopwatch = new Stopwatch();
            }

            _outgoingStopwatch.Reset();
            _outgoingStopwatch.Start();

            string handlerName = "None";

            try
            {
                PacketSentEvent?.Invoke( packet.GetPacket(), packet.GetLength() );

                PacketHandler handler = OutgoingPacketHandlers.GetHandler( packet.GetPacketID() );

                if ( handler != null )
                {
                    handlerName = handler.OnReceive.Method.Name;
                }

                int length = _getPacketLength( packet.GetPacketID() );

                handler?.OnReceive?.Invoke( new PacketReader( packet.GetPacket(), packet.GetLength(), length > 0 ) );

                PacketWaitEntries?.CheckWait( packet.GetPacket(), PacketDirection.Outgoing );
            }
            catch ( Exception e )
            {
                SentrySdk.CaptureException( e, scope =>
                {
                    scope.SetExtra( "Packet", packet.GetPacket() );
                    scope.SetExtra( "Player", Player.ToString() );
                    scope.SetExtra( "WorldItemCount", Items.Count() );
                    scope.SetExtra( "WorldMobileCount", Mobiles.Count() );
                } );
            }

            _outgoingStopwatch.Stop();

            if ( _outgoingStopwatch.ElapsedMilliseconds >= Options.CurrentOptions.SlowHandlerThreshold )
            {
                SlowHandlerEvent?.Invoke( PacketDirection.Outgoing, handlerName, _outgoingStopwatch.Elapsed );
            }
        }

        private static Assembly OnAssemblyResolve( object sender, ResolveEventArgs args )
        {
            string assemblyname = new AssemblyName( args.Name ).Name;

            string[] searchPaths = { StartupPath, RuntimeEnvironment.GetRuntimeDirectory() };

            if ( AssistantOptions.Assemblies?.Length > 0 )
            {
                searchPaths = searchPaths.Concat( GetAdditionalAssemblyPaths() ).ToArray();
            }

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

        private static string[] GetAdditionalAssemblyPaths()
        {
            return AssistantOptions.Assemblies == null
                ? Array.Empty<string>()
                : ( from assembly in AssistantOptions.Assemblies select Path.GetDirectoryName( assembly ) ).Distinct().ToArray();
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
                    SendPacketToClient(
                        new MobileUpdate( mobile.Serial, mobile.ID == 0x191 ? 0x193 : 0x192, mobile.Hue, newStatus, mobile.X, mobile.Y, mobile.Z, mobile.Direction ) );
                }
            };

            CheckGitHubVersion().ConfigureAwait( false );

            AbilitiesManager.GetInstance().Enabled = AbilityType.None;
            AbilitiesManager.GetInstance().ResendGump( AbilityType.None );

            Task.Run( async () =>
            {
                await Task.Delay( 3000 );

                if ( Connected && Player?.Backpack != null && Player?.Backpack?.Container == null )
                {
                    ObjectCommands.UseObject( Player?.Backpack );
                }

                MacroManager.GetInstance().Autostart();
            } );
        }

        private static async Task CheckGitHubVersion()
        {
            try
            {
                UpdaterSettings updaterSettings = UpdaterSettings.Load( StartupPath ?? Environment.CurrentDirectory );

                ChangelogEntry latestRelease = await Updater.GetLatestRelease( updaterSettings.InstallPrereleases );

                string latestVersion = latestRelease.Version;
                string localVersion = VersionHelpers.GetProductVersion( Path.Combine( StartupPath ?? Environment.CurrentDirectory, "ClassicAssist.dll" ) ).ToString();

                if ( VersionHelpers.IsVersionNewer( localVersion, latestVersion ) && VersionHelpers.IsVersionNewer( AssistantOptions.UpdateGumpVersion, latestVersion ) )
                {
                    string commitMessage = await Updater.GetUpdateText( updaterSettings.InstallPrereleases );
                    string donationAmount = await GetDonationsSummary();
                    StringBuilder donationMessage = new StringBuilder();

                    if ( !string.IsNullOrEmpty( donationAmount ) )
                    {
                        donationMessage.AppendLine( string.Format( Strings.Current_month_donations, DateTime.Now.ToString( "MMMM" ), donationAmount ) );
                        donationMessage.AppendLine();
                        donationMessage.AppendLine( $"<A HREF=\"https://www.paypal.me/reeeetus\">{Strings.Donate_Now}</A>" );
                    }

                    StringBuilder message = new StringBuilder();
                    message.AppendLine( Strings.ProductName );
                    message.AppendLine( $"{Strings.New_version_available_} <A HREF=\"https://github.com/Reetus/ClassicAssist/releases/tag/{latestVersion}\">{latestVersion}</A>" );
                    message.AppendLine();

                    if ( !string.IsNullOrEmpty( donationAmount ) )
                    {
                        message.AppendLine( donationMessage.ToString() );
                    }

                    message.AppendLine( commitMessage );
                    message.AppendLine( $"<A HREF=\"https://github.com/Reetus/ClassicAssist/releases\">{Strings.See_More}</A>" );

                    UpdateMessageGump gump = new UpdateMessageGump( WindowHandle, message.ToString(), latestVersion );
                    gump.SendGump();
                }
            }
            catch ( Exception )
            {
                // Squash all
            }
        }

        private static async Task<string> GetDonationsSummary()
        {
            HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync( "https://classicassist.azurewebsites.net/api/donations/summary" );

            if ( !response.IsSuccessStatusCode )
            {
                return null;
            }

            try
            {
                string json = await response.Content.ReadAsStringAsync();

                dynamic obj = JsonConvert.DeserializeObject<dynamic>( json );

                return obj?.amount;
            }
            catch ( Exception e )
            {
                SentrySdk.CaptureException( e );
                return null;
            }
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

                if ( _getPacketLength != null )
                {
                    int expectedLength = _getPacketLength( packet[0] );

                    if ( expectedLength == -1 )
                    {
                        expectedLength = ( packet[1] << 8 ) | packet[2];
                    }

                    if ( length != expectedLength )
                    {
                        SentrySdk.CaptureMessage( $"Invalid packet length: {length} != {expectedLength}", scope =>
                        {
                            scope.SetExtra( "Packet", packet );
                            scope.SetExtra( "Length", length );
                            scope.SetExtra( "Direction", PacketDirection.Outgoing );
                            scope.SetExtra( "Expected Length", expectedLength );
                        } );

                        return;
                    }
                }

                ( byte[] data, int dataLength ) = Utility.CopyBuffer( packet, length );

                _sendToServer?.Invoke( ref data, ref dataLength );

                _nextPacketSendTime = DateTime.Now + PACKET_SEND_DELAY;
            }
        }

        public static void SendPacketToClient( byte[] packet, int length, bool delay = true )
        {
            try
            {
                lock ( _clientSendLock )
                {
                    if ( delay )
                    {
                        while ( DateTime.Now < _nextPacketRecvTime )
                        {
                            Thread.Sleep( 1 );
                        }
                    }

                    InternalPacketReceivedEvent?.Invoke( packet, length );

                    PacketWaitEntries?.CheckWait( packet, PacketDirection.Incoming, true );

                    if ( _getPacketLength != null )
                    {
                        int expectedLength = _getPacketLength( packet[0] );

                        if ( expectedLength == -1 )
                        {
                            expectedLength = ( packet[1] << 8 ) | packet[2];
                        }

                        if ( length != expectedLength )
                        {
                            SentrySdk.CaptureMessage( $"Invalid packet length: {length} != {expectedLength}", scope =>
                            {
                                scope.SetExtra( "Packet", packet );
                                scope.SetExtra( "Length", length );
                                scope.SetExtra( "Direction", PacketDirection.Incoming );
                                scope.SetExtra( "Expected Length", expectedLength );
                            } );

                            return;
                        }
                    }

                    ( byte[] data, int dataLength ) = Utility.CopyBuffer( packet, length );

                    _sendToClient?.Invoke( ref data, ref dataLength );

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

        public static void SendPacketToClient( BasePacket basePacket, bool delay = true )
        {
            if ( basePacket.Direction != PacketDirection.Any && basePacket.Direction != PacketDirection.Incoming )
            {
                throw new InvalidOperationException( "Send packet wrong direction." );
            }

            byte[] data = basePacket.ToArray();

            SendPacketToClient( data, data.Length, delay );
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

            basePacket.ThrottleBeforeSend();

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

        public static void SetTitle( string title = null )
        {
            if ( Options.CurrentOptions.SetUOTitle )
            {
                _setTitle?.Invoke( string.IsNullOrEmpty( title ) ? Player == null ? string.Empty : $"{Player.Name} ({CurrentShard?.Name})" : title );
            }
            else
            {
                _setTitle?.Invoke( string.Empty );
            }
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

        public static void LaunchUpdater()
        {
            string updaterPath = Path.Combine( StartupPath ?? Environment.CurrentDirectory, "ClassicAssist.Updater.exe" );

            string version = FileVersionInfo.GetVersionInfo( Assembly.GetExecutingAssembly().Location ).ProductVersion;

            if ( !File.Exists( updaterPath ) )
            {
                return;
            }

            ProcessStartInfo psi =
                new ProcessStartInfo( updaterPath, $"--path \"{StartupPath}\"" + ( version != null ? $" --version {version}" : "" ) )
                {
                    UseShellExecute = false, WorkingDirectory = StartupPath ?? Environment.CurrentDirectory
                };

            Process.Start( psi );
        }

        public static bool CheckOutgoingPreFilter( byte[] data )
        {
            if ( _outgoingPacketPreFilter.MatchFilterAll( data, out PacketFilterInfo[] pfis ) <= 0 )
            {
                return false;
            }

            foreach ( PacketFilterInfo pfi in pfis )
            {
                pfi.Action?.Invoke( data, pfi );
            }

            SentPacketFilteredEvent?.Invoke( data, data.Length );

            PacketWaitEntries.CheckWait( data, PacketDirection.Outgoing, true );

            return true;
        }

        #region ClassicUO Events

        private static bool OnPacketSend( ref byte[] data, ref int length )
        {
            if ( data.Length == 0 )
            {
                return false;
            }

            bool filter = false;

            if ( CommandsManager.IsSpeechPacket( data[0] ) )
            {
                filter = CommandsManager.CheckCommand( data, length );
            }

            if ( CheckOutgoingPreFilter( data ) )
            {
                return false;
            }

            if ( OutgoingPacketFilters.CheckPacket( ref data, ref length ) )
            {
                SentPacketFilteredEvent?.Invoke( data, data.Length );

                return false;
            }

            OutgoingQueue.Enqueue( new Packet( data, length ) );

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

        public static ThreadQueue<Packet> IncomingQueue { get; set; }

        public static ThreadQueue<Packet> OutgoingQueue { get; set; }
        public static bool InternalTarget { get; set; }
        public static int InternalTargetSerial { get; set; }
        public static Trade Trade { get; set; } = new Trade();

        private static bool OnPacketReceive( ref byte[] data, ref int length )
        {
            if ( data.Length == 0 )
            {
                return false;
            }

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

            IncomingQueue.Enqueue( new Packet( data, length ) );

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
