using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using Assistant;
using ClassicAssist.Annotations;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UI.Controls
{
    public class PacketEntry
    {
        public byte[] Data { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
        public PacketDirection Direction { get; set; }
        public int Length => Data.Length;
        public string Title { get; set; }
    }

    /// <summary>
    ///     Interaction logic for HexDumpControl.xaml
    /// </summary>
    public partial class HexDumpControl : INotifyPropertyChanged
    {
        public static readonly DependencyProperty PacketProperty = DependencyProperty.Register( nameof( Packet ),
            typeof( PacketEntry ), typeof( HexDumpControl ),
            new UIPropertyMetadata( default, PropertyChangedCallback ) );

        public static readonly DependencyProperty PacketEntriesProperty =
            DependencyProperty.Register( nameof( PacketEntries ),
                typeof( ObservableCollection<DebugViewModel.PacketEnabledEntry> ), typeof( HexDumpControl ) );

        private string _binaryData;
        private string _textData;

        public HexDumpControl()
        {
            InitializeComponent();
        }

        public string BinaryData
        {
            get => _binaryData;
            set => SetProperty( ref _binaryData, value );
        }

        public PacketEntry Packet
        {
            get => (PacketEntry) GetValue( PacketProperty );
            set
            {
                SetValue( PacketProperty, value );
                BinaryData = SetBinary( value );
                TextData = SetText( value );
            }
        }

        public ObservableCollection<DebugViewModel.PacketEnabledEntry> PacketEntries
        {
            get => (ObservableCollection<DebugViewModel.PacketEnabledEntry>) GetValue( PacketEntriesProperty );
            set => SetValue( PacketEntriesProperty, value );
        }

        public string Status
        {
            get
            {
                if ( Packet?.Data == null )
                {
                    return "";
                }

                return "Length: " + Packet.Length;
            }
        }

        public string TextData
        {
            get => _textData;
            set => SetProperty( ref _textData, value );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static void PropertyChangedCallback( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            if ( !( d is HexDumpControl hexDump ) )
            {
                return;
            }

            if ( !( e.NewValue is PacketEntry packetEntry ) )
            {
                return;
            }

            hexDump.BinaryData = SetBinary( packetEntry );
            hexDump.TextData = SetText( packetEntry );
        }

        private static string SetText( PacketEntry packetEntry )
        {
            if ( packetEntry?.Data == null )
            {
                return string.Empty;
            }

            StringBuilder textBuilder = new StringBuilder();

            for ( int i = 0; i < packetEntry.Length; i++ )
            {
                byte b1 = packetEntry.Data[i];

                if ( b1 < 0x20 || b1 == 0xB7 || b1 == 0xFF )
                {
                    b1 = (byte) '.';
                }

                textBuilder.Append( (char) b1 );

                if ( ( i + 1 ) % 16 == 0 )
                {
                    textBuilder.AppendLine();
                }
            }

            return textBuilder.ToString();
        }

        private static string SetBinary( PacketEntry packetEntry )
        {
            if ( packetEntry?.Data == null )
            {
                return string.Empty;
            }

            StringBuilder binaryBuilder = new StringBuilder();

            for ( int i = 0; i < packetEntry.Length; i++ )
            {
                byte b1 = (byte) ( packetEntry.Data[i] >> 4 );
                byte b2 = (byte) ( packetEntry.Data[i] & 0xF );
                binaryBuilder.Append( (char) ( b1 > 9 ? b1 + 0x37 : b1 + 0x30 ) );
                binaryBuilder.Append( (char) ( b2 > 9 ? b2 + 0x37 : b2 + 0x30 ) );
                binaryBuilder.Append( ' ' );

                if ( ( i + 1 ) % 16 != 0 )
                {
                    continue;
                }

                binaryBuilder.Remove( binaryBuilder.Length - 1, 1 );
                binaryBuilder.AppendLine();
            }

            return binaryBuilder.ToString();
        }

        private void Copy_OnClick( object sender, RoutedEventArgs e )
        {
            try
            {
                string prepend = "byte[] packet = new byte[] { ";

                for ( int i = 0; i < Packet.Data.Length; i++ )
                {
                    prepend += $"0x{Packet.Data[i]:X2}";

                    if ( i + 1 < Packet.Data.Length )
                    {
                        prepend += ", ";
                    }
                }

                prepend += " };";

                Clipboard.SetData( DataFormats.Text, prepend );
            }
            catch ( Exception )
            {
                // ignored
            }
        }

        private void Replay_OnClick( object sender, RoutedEventArgs e )
        {
            if ( Packet == null )
            {
                return;
            }

            switch ( Packet.Direction )
            {
                case PacketDirection.Incoming:
                    Engine.SendPacketToClient( Packet.Data, Packet.Data.Length );
                    break;
                case PacketDirection.Outgoing:
                    Engine.SendPacketToServer( Packet.Data, Packet.Data.Length );
                    break;
                case PacketDirection.Any:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        // ReSharper disable once RedundantAssignment
        public virtual void SetProperty<T>( ref T obj, T value, [CallerMemberName] string propertyName = "" )
        {
            obj = value;
            OnPropertyChanged( propertyName );
        }

        private void Ignore_OnClick( object sender, RoutedEventArgs e )
        {
            if ( PacketEntries == null )
            {
                return;
            }

            if ( Packet.Data == null || Packet.Data.Length == 0 )
            {
                return;
            }

            byte packetType = Packet.Data[0];

            DebugViewModel.PacketEnabledEntry entry = PacketEntries.FirstOrDefault( i => i.PacketID == packetType );

            if ( entry != null )
            {
                entry.Enabled = false;
            }
        }
    }
}