using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ClassicAssist.UI.Controls
{
    /// <summary>
    ///     Interaction logic for HexDumpControl.xaml
    /// </summary>
    public partial class HexDumpControl : UserControl
    {
        private readonly byte[] _data;

        public HexDumpControl() : this( "Nothing", Encoding.Unicode.GetBytes( "Nothing" ) )
        {
        }

        public HexDumpControl( string title, byte[] data )
        {
            InitializeComponent();

            if ( title != null )
            {
                WindowTitle = title;
            }

            _data = data;
            SetData();
        }

        public string BinaryData { get; private set; }

        public string Status
        {
            get
            {
                if ( _data == null )
                {
                    return "";
                }

                return "Length: " + _data.Length;
            }
        }

        public string TextData { get; private set; }

        public string WindowTitle { get; } = "Hex Dump";

        public void SetData()
        {
            if ( _data == null )
            {
                return;
            }

            StringBuilder binaryBuilder = new StringBuilder();
            StringBuilder textBuilder = new StringBuilder();

            for ( int i = 0; i < _data.Length; i++ )
            {
                byte b1 = (byte) ( _data[i] >> 4 );
                byte b2 = (byte) ( _data[i] & 0xF );
                binaryBuilder.Append( (char) ( b1 > 9 ? b1 + 0x37 : b1 + 0x30 ) );
                binaryBuilder.Append( (char) ( b2 > 9 ? b2 + 0x37 : b2 + 0x30 ) );
                binaryBuilder.Append( ' ' );
                b1 = _data[i];

                if ( b1 < 0x20 || b1 == 0xB7 || b1 == 0xFF )
                {
                    b1 = (byte) '.';
                }

                textBuilder.Append( (char) b1 );

                if ( ( i + 1 ) % 16 == 0 )
                {
                    binaryBuilder.Remove( binaryBuilder.Length - 1, 1 );
                    binaryBuilder.AppendLine();
                    textBuilder.AppendLine();
                }
            }

            BinaryData = binaryBuilder.ToString();
            TextData = textBuilder.ToString();
        }

        private void MenuItem_OnClick( object sender, RoutedEventArgs e )
        {
            try
            {
                string prepend = "byte[] packet = new byte[] { ";

                for ( int i = 0; i < _data.Length; i++ )
                {
                    prepend += $"0x{_data[i]:X2}";

                    if ( i + 1 < _data.Length )
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