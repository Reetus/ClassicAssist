using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.Data.Filters
{
    [FilterOptions( Name = "Light Level", DefaultEnabled = true )]
    public class LightLevelFilter : DynamicFilterEntry
    {
        public override bool CheckPacket( ref byte[] packet, ref int length, PacketDirection direction )
        {
            if ( packet[0] == 0x4E && Enabled )
            {
                return true;
            }

            if ( packet[0] != 0x4F || !Enabled )
            {
                return false;
            }

            packet[1] = (byte) Options.CurrentOptions.LightLevel;

            return false;
        }
    }
}