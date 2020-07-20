using ClassicAssist.Shared;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.Data.Filters
{
    [FilterOptions( Name = "Seasons", DefaultEnabled = false )]
    public class SeasonFilter : FilterEntry
    {
        private readonly byte[] _desolationSeason = { 0xBC, 0x04, 0x00 };
        private readonly byte[] _standardSeason = { 0xBC, 0x00, 0x00 };

        protected override void OnChanged( bool enabled )
        {
            PacketFilterInfo pfi = new PacketFilterInfo( 0xBC, OnSeasonChange );

            if ( enabled )
            {
                Engine.AddReceiveFilter( pfi );

                if ( Engine.Player != null && Engine.Player.Map == Map.Felucca )
                {
                    Engine.SendPacketToClient( _desolationSeason, _desolationSeason.Length );
                    return;
                }

                Engine.SendPacketToClient( _standardSeason, _standardSeason.Length );
            }
            else
            {
                Engine.RemoveReceiveFilter( pfi );
            }
        }

        private void OnSeasonChange( byte[] arg1, PacketFilterInfo arg2 )
        {
            if ( Engine.Player != null && Engine.Player.Map == Map.Felucca )
            {
                Engine.SendPacketToClient( _desolationSeason, _desolationSeason.Length );
                return;
            }

            Engine.SendPacketToClient( _standardSeason, _standardSeason.Length );
        }
    }
}