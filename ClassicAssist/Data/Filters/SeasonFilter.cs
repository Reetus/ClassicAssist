using Assistant;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.Data.Filters
{
    [FilterOptions( Name = "Seasons", DefaultEnabled = true )]
    public class SeasonFilter : FilterEntry
    {
        private readonly byte[] _standardSeason = { 0xBC, 0x00, 0x00 };

        protected override void OnChanged( bool enabled )
        {
            PacketFilterInfo pfi = new PacketFilterInfo( 0xBC, OnSeasonChange );

            if ( enabled )
            {
                Engine.AddReceiveFilter( pfi );

                Engine.SendPacketToClient( _standardSeason, _standardSeason.Length );
            }
            else
            {
                Engine.RemoveReceiveFilter( pfi );
            }
        }

        private void OnSeasonChange( byte[] arg1, PacketFilterInfo arg2 )
        {
            Engine.SendPacketToClient( _standardSeason, _standardSeason.Length );
        }
    }
}