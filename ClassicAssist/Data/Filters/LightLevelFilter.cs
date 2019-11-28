using Assistant;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.Data.Filters
{
    [FilterOptions( Name = "Light Level", DefaultEnabled = true )]
    public class LightLevelFilter : FilterEntry
    {
        protected override void OnChanged( bool enabled )
        {
            PacketFilterInfo pfi = new PacketFilterInfo( 0x4F, OnLightLevel );

            if ( enabled )
            {
                Engine.AddReceiveFilter( pfi );

                byte[] packet = { 0x4F, (byte) Options.CurrentOptions.LightLevel };

                Engine.SendPacketToClient( packet, packet.Length );
            }
            else
            {
                Engine.RemoveReceiveFilter( pfi );
            }
        }

        private static void OnLightLevel( byte[] arg1, PacketFilterInfo arg2 )
        {
            byte[] packet = { 0x4F, (byte) Options.CurrentOptions.LightLevel };

            Engine.SendPacketToClient( packet, packet.Length );
        }
    }
}