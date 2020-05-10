using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Timers;
using System.Windows.Input;
using System.Windows.Threading;
using Assistant;
using ClassicAssist.Resources;
using ClassicAssist.UI.Views;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UI.ViewModels
{
    public class AboutControlViewModel : BaseViewModel
    {
        private ICommand _checkForUpdatesCommand;
        private bool _connected;
        private DateTime _connectedTime;
        private int _itemCount;
        private int _lastTargetSerial;
        private double _latency;
        private ICommand _launchHomepageCommand;
        private int _mobileCount;
        private Timer _pingTimer;
        private string _playerName;
        private int _playerSerial;
        private string _playerStatus;
        private ICommand _showItemsCommand;
        private Timer _timer;

        public AboutControlViewModel()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;

            Version = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            BuildDate = $"{GetBuildDateTime( assembly ).ToLongDateString()}";

            Engine.ConnectedEvent += OnConnectedEvent;
            Engine.DisconnectedEvent += OnDisconnectedEvent;
            Engine.PlayerInitializedEvent += PlayerInitializedEvent;
            Engine.Items.CollectionChanged += ItemsOnCollectionChanged;
            Engine.Mobiles.CollectionChanged += MobilesOnCollectionChanged;

            IncomingPacketHandlers.MobileUpdatedEvent += OnMobileUpdatedEvent;
        }

        public string BuildDate { get; set; }

        public ICommand CheckForUpdatesCommand =>
            _checkForUpdatesCommand ?? ( _checkForUpdatesCommand = new RelayCommand( CheckForUpdates, o => true ) );

        public bool Connected
        {
            get => _connected;
            set => SetProperty( ref _connected, value );
        }

        public DateTime ConnectedTime
        {
            get => _connectedTime;
            set => SetProperty( ref _connectedTime, value );
        }

        public int ItemCount
        {
            get => _itemCount;
            set => SetProperty( ref _itemCount, value );
        }

        public int LastTargetSerial
        {
            get => _lastTargetSerial;
            set => SetProperty( ref _lastTargetSerial, value );
        }

        public double Latency
        {
            get => _latency;
            set => SetProperty( ref _latency, value );
        }

        public ICommand LaunchHomepageCommand =>
            _launchHomepageCommand ?? ( _launchHomepageCommand = new RelayCommand( LaunchHomepage, o => true ) );

        public int MobileCount
        {
            get => _mobileCount;
            set => SetProperty( ref _mobileCount, value );
        }

        public string PlayerName
        {
            get => _playerName;
            set => SetProperty( ref _playerName, value );
        }

        public int PlayerSerial
        {
            get => _playerSerial;
            set => SetProperty( ref _playerSerial, value );
        }

        public string PlayerStatus
        {
            get => _playerStatus;
            set => SetProperty( ref _playerStatus, value );
        }

        public string Product { get; } = Strings.ProductName;

        public ICommand ShowItemsCommand =>
            _showItemsCommand ?? ( _showItemsCommand = new RelayCommand( ShowItems, o => Connected ) );

        public string Version { get; set; }

        private void LastTargetChangedEvent( int serial )
        {
            LastTargetSerial = serial;
        }

        private void OnMobileUpdatedEvent( Mobile mobile )
        {
            if ( mobile.Serial == Engine.Player?.Serial )
            {
                PlayerInitializedEvent( Engine.Player );
            }
        }

        private static void LaunchHomepage( object obj )
        {
            Process.Start( "https://github.com/Reetus/ClassicAssist" );
        }

        private void MobilesOnCollectionChanged( int totalcount )
        {
            MobileCount = totalcount;
        }

        private void ItemsOnCollectionChanged( int totalcount )
        {
            ItemCount = Engine.Items.GetTotalItemCount();
        }

        private static void ShowItems( object obj )
        {
            Item[] e = ItemCollection.GetAllItems( Engine.Items.GetItems() );

            Dispatcher.CurrentDispatcher.Invoke( () =>
            {
                EntityCollectionViewer window = new EntityCollectionViewer
                {
                    DataContext = new EntityCollectionViewerViewModel( new ItemCollection( 0 ) { e } )
                };

                window.Show();
            } );
        }

        private void PlayerInitializedEvent( PlayerMobile player )
        {
            PlayerSerial = player.Serial;
            PlayerName = player.Name;
            PlayerStatus = player.Status.ToString();
            player.LastTargetChangedEvent += LastTargetChangedEvent;
            player.MobileStatusUpdated += OnMobileStatusUpdated;
        }

        private void OnMobileStatusUpdated( MobileStatus oldstatus, MobileStatus newstatus )
        {
            PlayerStatus = newstatus.ToString();
        }

        private void OnDisconnectedEvent()
        {
            Connected = false;

            _timer.Stop();
        }

        private static void CheckForUpdates( object obj )
        {
            string updaterPath = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory,
                "ClassicAssist.Updater.exe" );

            Version version = null;

            if ( System.Version.TryParse(
                FileVersionInfo.GetVersionInfo( Assembly.GetExecutingAssembly().Location ).ProductVersion,
                out Version v ) )
            {
                version = v;
            }

            if ( !File.Exists( updaterPath ) )
            {
                return;
            }

            ProcessStartInfo psi = new ProcessStartInfo( updaterPath,
                $"--pid {Process.GetCurrentProcess().Id} --path {Engine.StartupPath}" + ( version != null
                    ? $" --version {version}"
                    : "" ) ) { UseShellExecute = false };

            Process.Start( psi );
        }

        private void OnConnectedEvent()
        {
            Connected = true;
            ConnectedTime = DateTime.Now;

            _timer = new Timer( 1000 ) { AutoReset = true };
            _timer.Elapsed += ( sender, args ) => { NotifyPropertyChanged( nameof( ConnectedTime ) ); };
            _timer.Start();

            _pingTimer = new Timer( 3000 ) { AutoReset = true };
            _pingTimer.Elapsed += ( sender, args ) => PingServer();
            _pingTimer.Start();
        }

        private void PingServer()
        {
            _pingTimer.Interval = 30000;

            Random random = new Random();

            byte value = (byte) random.Next( 1, byte.MaxValue );

            Stopwatch sw = new Stopwatch();
            sw.Start();

            PacketWaitEntry we = Engine.PacketWaitEntries.Add(
                new PacketFilterInfo( 0x73, new[] { new PacketFilterCondition( 1, new[] { value }, 1 ) } ),
                PacketDirection.Incoming, true );

            Engine.SendPacketToServer( new Ping( value ) );

            bool result = we.Lock.WaitOne( 5000 );

            sw.Stop();

            if ( result )
            {
                Latency = sw.ElapsedMilliseconds;
            }
        }

        internal static DateTime GetBuildDateTime( Assembly assembly )
        {
            System.Version.TryParse( FileVersionInfo.GetVersionInfo( assembly.Location ).ProductVersion,
                out Version version );

            DateTime buildDateTime =
                new DateTime( 2020, 2, 27 ).Add( new TimeSpan( TimeSpan.TicksPerDay * version.Build ) );

            return buildDateTime;
        }
    }
}