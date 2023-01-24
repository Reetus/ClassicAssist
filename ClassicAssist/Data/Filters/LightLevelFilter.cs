using Assistant;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.Data.Filters
{
    [FilterOptions( Name = "Light Level", DefaultEnabled = true )]
    public class LightLevelFilter : DynamicFilterEntry
    {
        public LightLevelFilter()
        {
            Options.LightLevelChanged += level =>
            {
                if ( Engine.Connected )
                {
                    SendLightLevel( level );
                }
            };

            Engine.ConnectedEvent += () => { SendLightLevel( Options.CurrentOptions.LightLevel ); };
        }

        private void SendLightLevel( int level )
        {
            if ( Enabled )
            {
                Engine.SendPacketToClient( new byte[] { 0x4F, (byte) level }, 2 );
            }
        }

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