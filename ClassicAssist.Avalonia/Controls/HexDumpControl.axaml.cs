using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml;
using ClassicAssist.Shared;
using ClassicAssist.UI.Misc;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.Avalonia.Controls
{
    public class HexDumpControl : UserControl
    {
        public static readonly DirectProperty<HexDumpControl, PacketEntry> PacketProperty =
            AvaloniaProperty.RegisterDirect<HexDumpControl, PacketEntry>( nameof( Packet ), o => o.Packet,
                ( o, v ) => o.Packet = v, defaultBindingMode: BindingMode.OneWay );

        private PacketEntry _packet;


        public HexDumpControl()
        {
            InitializeComponent();

            this.FindControl<MenuItem>( "ContextCopy" ).PointerPressed += ( sender, args ) =>
            {
                if (Packet == null)
                {
                    return;
                }

                string prepend = "byte[] packet = new byte[] { ";

                for (int i = 0; i < Packet.Data.Length; i++)
                {
                    prepend += $"0x{Packet.Data[i]:X2}";

                    if (i + 1 < Packet.Data.Length)
                    {
                        prepend += ", ";
                    }
                }

                prepend += " };";

                IClipboard service = (IClipboard) AvaloniaLocator.Current.GetService( typeof( IClipboard ) );

                service.SetTextAsync( prepend ).ConfigureAwait( false );
            };

            this.FindControl<MenuItem>( "ContextReplay" ).PointerPressed += ( sender, args ) =>
            {
                if (Packet == null)
                {
                    return;
                }

                switch (Packet.Direction)
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
            };
        }

        public PacketEntry Packet
        {
            get => _packet;
            set => SetAndRaise( PacketProperty, ref _packet, value );
        }

        public string Status
        {
            get
            {
                if (Packet?.Data == null)
                {
                    return "";
                }

                return "Length: " + Packet.Length;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );
        }
    }
}