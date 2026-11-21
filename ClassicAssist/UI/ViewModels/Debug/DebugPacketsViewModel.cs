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
        /// <summary>
        ///     Every captured packet, whatever the display filter. <see cref="Items" /> is the filtered view
        ///     of it, so changing the filter re-applies to what has already been captured. UI thread only.
        /// </summary>
        private readonly List<PacketEntry> _allItems = new List<PacketEntry>();

        private readonly Dispatcher _dispatcher;
        private readonly List<PacketEntry> _tempItems = new List<PacketEntry>();
        private ICommand _changePacketEnabledCommand;
        private ICommand _clearCommand;
        private PacketDirection _direction = PacketDirection.Any;

        private bool _enabled;
        private ICommand _exportLogCommand;
        private string _filter = string.Empty;
        private string _filterError;

        /// <summary>
        ///     The last filter that parsed, or null for none. While the text is invalid (mid-edit, say)
        ///     this keeps the previous filter applied rather than flashing the whole log back.
        /// </summary>
        private volatile PacketMatchExpression _filterExpression;

        private bool _includeInternal = true;
        private ObservableCollection<PacketEntry> _items = new ObservableCollection<PacketEntry>();
        private DateTime _lastUIUpdate;

        private ObservableCollection<PacketEnabledEntry> _packetEntries = new ObservableCollection<PacketEnabledEntry>();
        private ICommand _resetCommand;

        private bool _topmost = true;
        private ICommand _viewPlayerEquipmentCommand;
        private readonly bool[] _packetEnabled = new bool[256];

        public DebugPacketsViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

            PacketEntries.Add( new PacketEnabledEntry { Name = Strings.All_Packets, PacketID = -1, Enabled = true } );

            for ( byte i = 0; i < 0xFF; i++ )
            {
                bool enabled = i != 0x73;
                PacketEntries.Add( new PacketEnabledEntry { Name = $"0x{i:x2}", PacketID = i, Enabled = enabled } );
                _packetEnabled[i] = enabled;
            }

            foreach ( PacketEnabledEntry entry in PacketEntries )
            {
                entry.PropertyChanged += ( s, e ) =>
                {
                    if ( e.PropertyName == "Enabled" && s is PacketEnabledEntry pee && pee.PacketID >= 0 && pee.PacketID < 256 )
                    {
                        _packetEnabled[pee.PacketID] = pee.Enabled;
                    }
                };
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

        /// <summary>
        ///     Display filter over the captured packets - see <see cref="PacketMatchExpression" /> for the
        ///     syntax, e.g. <c>[0] = 0xF3 &amp;&amp; [4..7] = 0x4001ABCD</c>. Packets are still captured while
        ///     hidden, so clearing or changing the filter shows them again.
        /// </summary>
        public string Filter
        {
            get => _filter;
            set
            {
                SetProperty( ref _filter, value );
                UpdateFilter();
            }
        }

        /// <summary>Why <see cref="Filter" /> doesn't parse, or null when it does.</summary>
        public string FilterError
        {
            get => _filterError;
            set => SetProperty( ref _filterError, value );
        }

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

            JObject options = new JObject
            {
                { "Enabled", Enabled },
                { "IncludeInternal", IncludeInternal },
                { "Direction", Direction.ToString() },
                { "Filter", Filter ?? string.Empty }
            };

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
            Filter = json["Packets"]["Filter"]?.ToObject<string>() ?? string.Empty;

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
                AddCapturedEntries( _tempItems );

                _lastUIUpdate = DateTime.Now;
                _tempItems.Clear();
            } );
        }

        /// <summary>Records newly captured packets and shows those that pass the filter. UI thread only.</summary>
        internal void AddCapturedEntries( IEnumerable<PacketEntry> entries )
        {
            PacketMatchExpression expression = _filterExpression;

            foreach ( PacketEntry entry in entries )
            {
                _allItems.Add( entry );

                if ( expression == null || expression.IsMatch( entry.Data ) )
                {
                    Items.Add( entry );
                }
            }
        }

        private void UpdateFilter()
        {
            if ( string.IsNullOrWhiteSpace( Filter ) )
            {
                _filterExpression = null;
                FilterError = null;
            }
            else if ( PacketMatchExpression.TryParse( Filter, out PacketMatchExpression expression, out string error ) )
            {
                _filterExpression = expression;
                FilterError = null;
            }
            else
            {
                FilterError = error;

                return;
            }

            if ( _dispatcher.CheckAccess() )
            {
                RebuildItems();
            }
            else
            {
                _dispatcher.Invoke( RebuildItems );
            }
        }

        /// <summary>
        ///     Replaces <see cref="Items" /> rather than clearing and re-adding to it: one reset for the
        ///     list instead of a change notification per packet, which matters with a long capture.
        /// </summary>
        private void RebuildItems()
        {
            PacketMatchExpression expression = _filterExpression;

            IEnumerable<PacketEntry> matching = expression == null ? _allItems : _allItems.Where( e => expression.IsMatch( e.Data ) );

            Items = new ObservableCollection<PacketEntry>( matching );
        }

        private void OnInternalPacketSentEvent( byte[] data, int length )
        {
            if ( !Enabled || !IncludeInternal )
            {
                return;
            }

            if ( Direction != PacketDirection.Any && Direction != PacketDirection.Outgoing )
            {
                return;
            }

            if ( !_packetEnabled[data[0]] )
            {
                return;
            }

            Queue.Enqueue( new PacketEntry { Title = "Internal Outgoing Packet", Data = data, Direction = PacketDirection.Outgoing } );
        }

        private void OnInternalPacketReceivedEvent( byte[] data, int length )
        {
            if ( !Enabled || !IncludeInternal )
            {
                return;
            }

            if ( Direction != PacketDirection.Any && Direction != PacketDirection.Incoming )
            {
                return;
            }

            if ( !_packetEnabled[data[0]] )
            {
                return;
            }

            Queue.Enqueue( new PacketEntry { Title = "Internal Incoming Packet", Data = data, Direction = PacketDirection.Incoming } );
        }

        private void OnPacketSentEvent( byte[] data, int length )
        {
            if ( !Enabled )
            {
                return;
            }

            if ( Direction != PacketDirection.Any && Direction != PacketDirection.Outgoing )
            {
                return;
            }

            if ( !_packetEnabled[data[0]] )
            {
                return;
            }

            Queue.Enqueue( new PacketEntry { Title = "Outgoing Packet", Data = data, Direction = PacketDirection.Outgoing } );
        }

        private void OnPacketReceivedEvent( byte[] data, int length )
        {
            if ( !Enabled )
            {
                return;
            }

            if ( Direction != PacketDirection.Any && Direction != PacketDirection.Incoming )
            {
                return;
            }

            if ( !_packetEnabled[data[0]] )
            {
                return;
            }

            Queue.Enqueue( new PacketEntry { Title = "Incoming Packet", Data = data, Direction = PacketDirection.Incoming } );
        }

        private void ViewPlayerEquipment( object obj )
        {
            EntityCollectionViewer window = new EntityCollectionViewer { DataContext = new EntityCollectionViewerViewModel( Engine.Player?.Equipment ) { Topmost = Topmost } };

            window.Show();
        }

        private void Clear( object obj )
        {
            _allItems.Clear();
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