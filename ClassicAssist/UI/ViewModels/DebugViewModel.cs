using System.Collections.ObjectModel;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Misc;
using ClassicAssist.UI.Controls;
using ClassicAssist.UI.Views;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.UI.ViewModels
{
    public class DebugViewModel : BaseViewModel, ISettingProvider
    {
        private ICommand _clearCommand;
        private bool _ignorePingPackets = true;
        private ObservableCollection<PacketEntry> _items = new ObservableCollection<PacketEntry>();
        private bool _running = true;
        private bool _topmost = true;
        private ICommand _viewPlayerEquipmentCommand;

        public DebugViewModel()
        {
            Queue = new ThreadQueue<PacketEntry>( ProcessQueue );
            Engine.PacketReceivedEvent += OnPacketReceivedEvent;
            Engine.PacketSentEvent += OnPacketSentEvent;
            Engine.InternalPacketReceivedEvent += OnInternalPacketReceivedEvent;
            Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;
        }

        public ICommand ClearCommand => _clearCommand ?? ( _clearCommand = new RelayCommand( Clear, o => true ) );

        public bool IgnorePingPackets
        {
            get => _ignorePingPackets;
            set => SetProperty( ref _ignorePingPackets, value );
        }

        public ObservableCollection<PacketEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public ThreadQueue<PacketEntry> Queue { get; set; }

        public bool Running
        {
            get => _running;
            set => SetProperty( ref _running, value );
        }

        public bool Topmost
        {
            get => _topmost;
            set => SetProperty( ref _topmost, value );
        }

        public ICommand ViewPlayerEquipmentCommand =>
            _viewPlayerEquipmentCommand ??
            ( _viewPlayerEquipmentCommand = new RelayCommand( ViewPlayerEquipment, o => true ) );

        public void Serialize( JObject json )
        {
            if ( json?["Debug"] != null )
            {
                return;
            }

            JObject options = new JObject { { "IgnorePingPackets", IgnorePingPackets } };

            json?.Add( "Debug", options );
        }

        public void Deserialize( JObject json, Options options )
        {
            Items.Clear();

            if ( json?["Debug"] == null )
            {
                return;
            }

            IgnorePingPackets = json["Debug"]["IgnorePingPackets"].ToObject<bool>();
        }

        private void ProcessQueue( PacketEntry entry )
        {
            _dispatcher.Invoke( () => { Items.Add( entry ); } );
        }

        private void OnInternalPacketSentEvent( byte[] data, int length )
        {
            if ( IgnorePingPackets && data[0] == 0x73 )
            {
                return;
            }

            if ( !Running )
            {
                return;
            }

            PacketEntry entry = new PacketEntry { Title = "Internal Outgoing Packet", Data = data };

            Queue.Enqueue( entry );
        }

        private void OnInternalPacketReceivedEvent( byte[] data, int length )
        {
            if ( IgnorePingPackets && data[0] == 0x73 )
            {
                return;
            }

            if ( !Running )
            {
                return;
            }

            PacketEntry entry = new PacketEntry { Title = "Internal Incoming Packet", Data = data };

            Queue.Enqueue( entry );
        }

        private void OnPacketSentEvent( byte[] data, int length )
        {
            if ( IgnorePingPackets && data[0] == 0x73 )
            {
                return;
            }

            if ( !Running )
            {
                return;
            }

            PacketEntry entry = new PacketEntry { Title = "Outgoing Packet", Data = data };

            Queue.Enqueue( entry );
        }

        private void OnPacketReceivedEvent( byte[] data, int length )
        {
            if ( IgnorePingPackets && data[0] == 0x73 )
            {
                return;
            }

            if ( !Running )
            {
                return;
            }

            PacketEntry entry = new PacketEntry { Title = "Incoming Packet", Data = data };

            Queue.Enqueue( entry );
        }

        private void ViewPlayerEquipment( object obj )
        {
            EntityCollectionViewer window = new EntityCollectionViewer
            {
                DataContext = new EntityCollectionViewerViewModel( Engine.Player?.Equipment ) { Topmost = Topmost }
            };

            window.Show();
        }

        private void Clear( object obj )
        {
            Items.Clear();
        }
    }
}