using Assistant;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.Data.Filters
{
    [FilterOptions(Name = "Weather", DefaultEnabled = true)]
    public class WeatherFilter : FilterEntry
    {
        private static void OnWeatherPacket(byte[] arg1, PacketFilterInfo arg2)
        {
            byte[] packet = { 0x65, 0xFF, 0x00, 0x00 };

            Engine.SendPacketToClient( packet, packet.Length );
        }

        protected override void OnChanged(bool enabled)
        {
            PacketFilterInfo pfi = new PacketFilterInfo(0x65, null, OnWeatherPacket);

            if (enabled)
            {
                Engine.AddReceiveFilter(pfi);
            }
            else
            {
                Engine.RemoveReceiveFilter(pfi);
            }
        }
    }
}