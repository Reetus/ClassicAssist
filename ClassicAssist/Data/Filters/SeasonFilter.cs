using System;
using Assistant;
using ClassicAssist.UI.Views.Filters;
using ClassicAssist.UO.Network.PacketFilter;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Data.Filters
{
    [FilterOptions( Name = "Seasons", DefaultEnabled = false )]
    public class SeasonFilter : DynamicFilterEntry, IConfigurableFilter
    {
        public static bool IsEnabled { get; set; }
        public Season SelectedSeason { get; set; } = Season.Spring;

        public void Configure()
        {
            SeasonFilterConfigureWindow window = new SeasonFilterConfigureWindow( SelectedSeason );
            window.ShowDialog();
            SelectedSeason = window.SelectedSeason;

            SendSeason();
        }

        public void Deserialize( JToken token )
        {
            if ( token == null )
            {
                return;
            }

            JObject config = (JObject) token;

            if ( Enum.TryParse( config["Season"]?.ToString(), out Season season ) )
            {
                SelectedSeason = season;
            }
        }

        public JObject Serialize()
        {
            JObject config = new JObject { { "Season", SelectedSeason.ToString() } };

            return config;
        }

        public void ResetOptions()
        {
            SelectedSeason = Season.Spring;
        }

        protected override void OnChanged( bool enabled )
        {
            IsEnabled = enabled;
        }

        public override bool CheckPacket( ref byte[] packet, ref int length, PacketDirection direction )
        {
            if ( packet == null || !IsEnabled )
            {
                return false;
            }

            if ( packet[0] != 0xBF || packet[4] != 0x08 || direction != PacketDirection.Incoming )
            {
                return packet[0] == 0xBC && direction == PacketDirection.Incoming;
            }

            SendSeason();
            return false;

        }

        private void SendSeason()
        {
            byte[] season = { 0xBC, (byte) SelectedSeason, 0x00 };

            Engine.SendPacketToClient( season, season.Length, false );
        }
    }

    public enum Season : byte
    {
        Spring,
        Summer,
        Fall,
        Winter,
        Desolation
    }
}