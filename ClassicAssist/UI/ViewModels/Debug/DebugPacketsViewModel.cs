using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Controls;
using ClassicAssist.UI.Views;
using ClassicAssist.UO.Network.PacketFilter;
using Microsoft.Scripting.Utils;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.UI.ViewModels.Debug
{
    public class DebugPacketsViewModel : SetPropertyNotifyChanged, ISettingProvider
    {
        private readonly Dispatcher _dispatcher;
        private readonly List<PacketEntry> _tempItems = new List<PacketEntry>();
        private ICommand _changePacketEnabledCommand;
        private ICommand _clearCommand;
        private PacketDirection _direction = PacketDirection.Any;

        private bool _enabled;
        private ICommand _exportLogCommand;
        private bool _includeInternal = true;
        private ObservableCollection<PacketEntry> _items = new ObservableCollection<PacketEntry>();
        private DateTime _lastUIUpdate;

        private ObservableCollection<PacketEnabledEntry> _packetEntries = new ObservableCollection<PacketEnabledEntry>();
        private ICommand _resetCommand;

        private bool _topmost = true;
        private ICommand _viewPlayerEquipmentCommand;

        public DebugPacketsViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

            PacketEntries.Add( new PacketEnabledEntry { Name = Strings.All_Packets, PacketID = -1, Enabled = true } );

            for ( byte i = 0; i < 0xFF; i++ )
            {
                PacketEntries.Add( new PacketEnabledEntry { Name = $"0x{i:x2}", PacketID = i, Enabled = i != 0x73 } );
            }

            Queue = new ThreadQueue<PacketEntry>( ProcessQueue );
            Engine.PacketReceivedEvent += OnPacketReceivedEvent;
            Engine.PacketSentEvent += OnPacketSentEvent;
            Engine.InternalPacketReceivedEvent += OnInternalPacketReceivedEvent;
            Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;
        }

        public ICommand ChangePacketEnabledCommand => _changePacketEnabledCommand ?? ( _changePacketEnabledCommand = new RelayCommand( EnableDisable, o => true ) );

        public ICommand ClearCommand => _clearCommand ?? ( _clearCommand = new RelayCommand( Clear, o => true ) );

        public PacketDirection Direction
        {
            get => _direction;
            set => SetProperty( ref _direction, value );
        }

        public bool Enabled
        {
            get => _enabled;
            set => SetProperty( ref _enabled, value );
        }

        public ICommand ExportLogCommand => _exportLogCommand ?? ( _exportLogCommand = new RelayCommand( ExportLog, o => true ) );

        public bool IncludeInternal
        {
            get => _includeInternal;
            set => SetProperty( ref _includeInternal, value );
        }

        public ObservableCollection<PacketEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public ObservableCollection<PacketEnabledEntry> PacketEntries
        {
            get => _packetEntries;
            set => SetProperty( ref _packetEntries, value );
        }

        public ThreadQueue<PacketEntry> Queue { get; set; }
        public ICommand ResetCommand => _resetCommand ?? ( _resetCommand = new RelayCommand( Reset ) );

        public bool Topmost
        {
            get => _topmost;
            set => SetProperty( ref _topmost, value );
        }

        public ICommand ViewPlayerEquipmentCommand => _viewPlayerEquipmentCommand ?? ( _viewPlayerEquipmentCommand = new RelayCommand( ViewPlayerEquipment, o => true ) );

        public void Serialize( JObject json, bool global = false )
        {
            if ( json == null )
            {
                return;
            }

            JObject options = new JObject { { "Enabled", Enabled }, { "IncludeInternal", IncludeInternal }, { "Direction", Direction.ToString() } };

            JArray packets = new JArray();

            foreach ( PacketEnabledEntry packetEntry in PacketEntries )
            {
                packets.Add( new JObject { { "PacketID", packetEntry.PacketID }, { "Enabled", packetEntry.Enabled } } );
            }

            options.Add( "Packets", packets );

            json.Add( "Packets", options );
        }

        public void Deserialize( JObject json, Options options, bool global = false )
        {
            if ( json?["Packets"] == null )
            {
                return;
            }

            Enabled = json["Packets"]["Enabled"]?.ToObject<bool>() ?? false;
            IncludeInternal = json["Packets"]["IncludeInternal"]?.ToObject<bool>() ?? false;
            Direction = Enum.TryParse( json["Packets"]["Direction"]?.ToObject<string>() ?? string.Empty, out PacketDirection direction ) ? direction : PacketDirection.Any;

            if ( json["Packets"]["Packets"] == null )
            {
                return;
            }

            foreach ( JToken packet in json["Packets"]["Packets"] )
            {
                if ( packet["PacketID"] == null || packet["Enabled"] == null )
                {
                    continue;
                }

                PacketEnabledEntry entry = PacketEntries.FirstOrDefault( e => e.PacketID == packet["PacketID"].ToObject<int>() );

                if ( entry == null )
                {
                    continue;
                }

                entry.Enabled = packet["Enabled"].ToObject<bool>();
            }
        }

        private void Reset( object obj )
        {
            Direction = PacketDirection.Any;
            IncludeInternal = true;
            Enabled = false;

            foreach ( PacketEnabledEntry packetEnabledEntry in PacketEntries )
            {
                packetEnabledEntry.Enabled = packetEnabledEntry.PacketID != 0x73;
            }
        }

        private void ExportLog( object obj )
        {
            if ( !( obj is IEnumerable<PacketEntry> items ) )
            {
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                InitialDirectory = Engine.StartupPath ?? Environment.CurrentDirectory, Filter = "JSON Packet Log|*.packets.json", FileName = "export.packets.json"
            };

            bool? result = sfd.ShowDialog();

            if ( !result.HasValue || !result.Value || string.IsNullOrEmpty( sfd.FileName ) )
            {
                return;
            }

            string fileName = sfd.FileName;

            JArray jArray = new JArray();

            foreach ( PacketEntry packetEntry in items )
            {
                jArray.Add( new JObject
                {
                    { "Title", packetEntry.Title },
                    { "DateTime", packetEntry.DateTime },
                    { "Direction", packetEntry.Direction.ToString() },
                    { "Length", packetEntry.Length },
                    { "Data", packetEntry.Data.Aggregate( string.Empty, ( current, b ) => current + $"{b:x2} " ) },
                    { "Base64", Convert.ToBase64String( packetEntry.Data ) }
                } );
            }

            File.WriteAllText( fileName, jArray.ToString() );
        }

        private void ProcessQueue( PacketEntry entry )
        {
            _tempItems.Add( entry );

            if ( DateTime.Now - _lastUIUpdate < TimeSpan.FromMilliseconds( 100 ) )
            {
                return;
            }

            _dispatcher.Invoke( () =>
            {
                Items.AddRange( _tempItems );

                _lastUIUpdate = DateTime.Now;
                _tempItems.Clear();
            } );
        }

        private void OnInternalPacketSentEvent( byte[] data, int length )
        {
            if ( !IncludeInternal )
            {
                return;
            }

            if ( Direction != PacketDirection.Any && Direction != PacketDirection.Outgoing )
            {
                return;
            }

            if ( PacketEntries.FirstOrDefault( e => e.PacketID == data[0] )?.Enabled == false )
            {
                return;
            }

            if ( !Enabled )
            {
                return;
            }

            PacketEntry entry = new PacketEntry { Title = "Internal Outgoing Packet", Data = data, Direction = PacketDirection.Outgoing };

            Queue.Enqueue( entry );
        }

        private void OnInternalPacketReceivedEvent( byte[] data, int length )
        {
            if ( !IncludeInternal )
            {
                return;
            }

            if ( Direction != PacketDirection.Any && Direction != PacketDirection.Incoming )
            {
                return;
            }

            if ( PacketEntries.FirstOrDefault( e => e.PacketID == data[0] )?.Enabled == false )
            {
                return;
            }

            if ( !Enabled )
            {
                return;
            }

            PacketEntry entry = new PacketEntry { Title = "Internal Incoming Packet", Data = data, Direction = PacketDirection.Incoming };

            Queue.Enqueue( entry );
        }

        private void OnPacketSentEvent( byte[] data, int length )
        {
            if ( Direction != PacketDirection.Any && Direction != PacketDirection.Outgoing )
            {
                return;
            }

            if ( PacketEntries.FirstOrDefault( e => e.PacketID == data[0] )?.Enabled == false )
            {
                return;
            }

            if ( !Enabled )
            {
                return;
            }

            PacketEntry entry = new PacketEntry { Title = "Outgoing Packet", Data = data, Direction = PacketDirection.Outgoing };

            Queue.Enqueue( entry );
        }

        private void OnPacketReceivedEvent( byte[] data, int length )
        {
            if ( Direction != PacketDirection.Any && Direction != PacketDirection.Incoming )
            {
                return;
            }

            if ( PacketEntries.FirstOrDefault( e => e.PacketID == data[0] )?.Enabled == false )
            {
                return;
            }

            if ( !Enabled )
            {
                return;
            }

            PacketEntry entry = new PacketEntry { Title = "Incoming Packet", Data = data, Direction = PacketDirection.Incoming };

            Queue.Enqueue( entry );
        }

        private void ViewPlayerEquipment( object obj )
        {
            EntityCollectionViewer window = new EntityCollectionViewer { DataContext = new EntityCollectionViewerViewModel( Engine.Player?.Equipment ) { Topmost = Topmost } };

            window.Show();
        }

        private void Clear( object obj )
        {
            Items.Clear();
        }

        private void EnableDisable( object obj )
        {
            if ( !( obj is PacketEnabledEntry packetEnabledEntry ) )
            {
                return;
            }

            if ( packetEnabledEntry.PacketID != -1 )
            {
                return;
            }

            foreach ( PacketEnabledEntry entry in PacketEntries )
            {
                entry.Enabled = packetEnabledEntry.Enabled;
            }
        }

        public class PacketEnabledEntry : SetPropertyNotifyChanged
        {
            private bool _enabled;
            private string _name;
            private int _packetId;

            public bool Enabled
            {
                get => _enabled;
                set => SetProperty( ref _enabled, value );
            }

            public string Name
            {
                get => _name;
                set => SetProperty( ref _name, value );
            }

            public int PacketID
            {
                get => _packetId;
                set => SetProperty( ref _packetId, value );
            }
        }
    }
}