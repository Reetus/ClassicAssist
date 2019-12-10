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
        private bool _ignorePingPackets = true;
        private ObservableCollection<HexDumpControl> _items = new ObservableCollection<HexDumpControl>();
        private bool _running = true;
        private bool _topmost = true;
        private ICommand _viewPlayerEquipmentCommand;

        public DebugViewModel()
        {
            Engine.PacketReceivedEvent += OnPacketReceivedEvent;
            Engine.PacketSentEvent += OnPacketSentEvent;
            Engine.InternalPacketReceivedEvent += OnInternalPacketReceivedEvent;
            Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;
        }

        public bool IgnorePingPackets
        {
            get => _ignorePingPackets;
            set => SetProperty( ref _ignorePingPackets, value );
        }

        public ObservableCollection<HexDumpControl> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

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
            if ( json?["Debug"] == null )
            {
                return;
            }

            IgnorePingPackets = json["Debug"]["IgnorePingPackets"].ToObject<bool>();
        }

        private void OnInternalPacketSentEvent( byte[] data, int length )
        {
            _dispatcher.Invoke( () =>
            {
                if ( IgnorePingPackets && data[0] == 0x73 )
                {
                    return;
                }

                if ( !Running )
                {
                    return;
                }

                HexDumpControl hd = new HexDumpControl( "Internal Outgoing Packet", data );

                Items.Add( hd );
            } );
        }

        private void OnInternalPacketReceivedEvent( byte[] data, int length )
        {
            _dispatcher.Invoke( () =>
            {
                if ( IgnorePingPackets && data[0] == 0x73 )
                {
                    return;
                }

                if ( !Running )
                {
                    return;
                }

                HexDumpControl hd = new HexDumpControl( "Internal Incoming Packet", data );

                Items.Add( hd );
            } );
        }

        private void OnPacketSentEvent( byte[] data, int length )
        {
            _dispatcher.Invoke( () =>
            {
                if ( IgnorePingPackets && data[0] == 0x73 )
                {
                    return;
                }

                if ( !Running )
                {
                    return;
                }

                HexDumpControl hd = new HexDumpControl( "Outgoing Packet", data );

                Items.Add( hd );
            } );
        }

        private void OnPacketReceivedEvent( byte[] data, int length )
        {
            _dispatcher.Invoke( () =>
            {
                if ( IgnorePingPackets && data[0] == 0x73 )
                {
                    return;
                }

                if ( !Running )
                {
                    return;
                }

                HexDumpControl hd = new HexDumpControl( "Incoming Packet", data );

                Items.Add( hd );
            } );
        }

        private void ViewPlayerEquipment( object obj )
        {
            EntityCollectionViewer window = new EntityCollectionViewer
            {
                DataContext = new EntityCollectionViewerViewModel( Engine.Player?.Equipment ) { Topmost = Topmost }
            };

            window.Show();
        }
    }
}