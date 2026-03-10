using Assistant;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.Data.Filters
{
    [FilterOptions( Name = "Weather", DefaultEnabled = true )]
    public class WeatherFilter : DynamicFilterEntry
    {
        public static bool IsEnabled { get; set; }

        protected override void OnChanged( bool enabled )
        {
            IsEnabled = enabled;
        }

        public override bool CheckPacket( ref byte[] packet, ref int length, PacketDirection direction )
        {
            if ( !IsEnabled || direction != PacketDirection.Incoming )
            {
                return false;
            }

            return packet[0] == 0x65;
        }
    }
}