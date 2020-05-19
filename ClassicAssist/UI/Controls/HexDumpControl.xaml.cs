using System;
using System.Text;
using System.Windows;

namespace ClassicAssist.UI.Controls
{
    public class PacketEntry
    {
        public byte[] Data { get; set; }
        public int Length => Data.Length;
        public string Title { get; set; }
    }

    /// <summary>
    ///     Interaction logic for HexDumpControl.xaml
    /// </summary>
    public partial class HexDumpControl
    {
        public static readonly DependencyProperty PacketProperty = DependencyProperty.Register( "Packet",
            typeof( PacketEntry ), typeof( HexDumpControl ), new UIPropertyMetadata() );

        public HexDumpControl()
        {
            InitializeComponent();
        }

        public string BinaryData
        {
            get
            {
                if ( Packet?.Data == null )
                {
                    return string.Empty;
                }

                StringBuilder binaryBuilder = new StringBuilder();

                for ( int i = 0; i < Packet.Length; i++ )
                {
                    byte b1 = (byte) ( Packet.Data[i] >> 4 );
                    byte b2 = (byte) ( Packet.Data[i] & 0xF );
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
        }

        public PacketEntry Packet
        {
            get => (PacketEntry) GetValue( PacketProperty );
            set => SetValue( PacketProperty, value );
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
            get
            {
                if ( Packet?.Data == null )
                {
                    return string.Empty;
                }

                StringBuilder textBuilder = new StringBuilder();

                for ( int i = 0; i < Packet.Length; i++ )
                {
                    byte b1 = Packet.Data[i];

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
        }

        private void MenuItem_OnClick( object sender, RoutedEventArgs e )
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
    }
}