using System;
using Assistant;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Views.Filters;
using ClassicAssist.UO.Network.PacketFilter;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Data.Filters
{
    [FilterOptions( Name = "Light Level", DefaultEnabled = true )]
    public class LightLevelFilter : DynamicFilterEntry, IConfigurableFilter
    {
        public const int MAX_LIGHT_LEVEL = 31;

        private static LightLevelOptions _filterOptions;
        private static int _serverLightLevel;

        static LightLevelFilter()
        {
            FilterOptions = new LightLevelOptions();
        }

        public LightLevelFilter()
        {
            Engine.ConnectedEvent += SendLightLevel;
        }

        public static LightLevelOptions FilterOptions
        {
            get => _filterOptions;
            set
            {
                if ( _filterOptions != null )
                {
                    _filterOptions.PropertyChanged -= OnOptionsChanged;
                }

                _filterOptions = value;

                if ( _filterOptions != null )
                {
                    _filterOptions.PropertyChanged += OnOptionsChanged;
                }

                SendLightLevel();
            }
        }

        public static bool IsEnabled { get; set; }

        public void Configure()
        {
            LightLevelFilterConfigureWindow window = new LightLevelFilterConfigureWindow( FilterOptions );
            window.ShowDialog();
        }

        public void Deserialize( JToken token )
        {
            if ( token == null )
            {
                return;
            }

            LightLevelOptions options = new LightLevelOptions();

            if ( Enum.TryParse( token["Mode"]?.ToString(), out LightLevelMode mode ) )
            {
                options.Mode = mode;
            }

            options.FixedLevel = token["FixedLevel"]?.ToObject<int>() ?? options.FixedLevel;
            options.MinLevel = token["MinLevel"]?.ToObject<int>() ?? options.MinLevel;
            options.MaxLevel = token["MaxLevel"]?.ToObject<int>() ?? options.MaxLevel;

            FilterOptions = options;
        }

        public JObject Serialize()
        {
            return new JObject
            {
                { "Mode", FilterOptions.Mode.ToString() },
                { "FixedLevel", FilterOptions.FixedLevel },
                { "MinLevel", FilterOptions.MinLevel },
                { "MaxLevel", FilterOptions.MaxLevel }
            };
        }

        public void ResetOptions()
        {
            FilterOptions = new LightLevelOptions();
        }

        // Profiles saved before the light level became a filter option stored it as General.LightLevel
        public static void MigrateLegacyLightLevel( JToken token )
        {
            if ( token == null )
            {
                return;
            }

            FilterOptions = new LightLevelOptions { FixedLevel = token.ToObject<int>() };
        }

        protected override void OnChanged( bool enabled )
        {
            IsEnabled = enabled;

            if ( !Engine.Connected )
            {
                return;
            }

            // Restore the server light level when the filter is turned off
            byte level = (byte) ( enabled ? GetLightLevel( _serverLightLevel ) : _serverLightLevel );

            Engine.SendPacketToClient( new byte[] { 0x4F, level }, 2 );
        }

        private static void OnOptionsChanged( object sender, EventArgs e )
        {
            SendLightLevel();
        }

        private static void SendLightLevel()
        {
            if ( !IsEnabled || !Engine.Connected )
            {
                return;
            }

            Engine.SendPacketToClient( new byte[] { 0x4F, (byte) GetLightLevel( _serverLightLevel ) }, 2 );
        }

        private static int GetLightLevel( int serverLevel )
        {
            if ( FilterOptions.Mode == LightLevelMode.Fixed )
            {
                return FilterOptions.FixedLevel;
            }

            return Math.Min( Math.Max( serverLevel, FilterOptions.MinLevel ), FilterOptions.MaxLevel );
        }

        public override bool CheckPacket( ref byte[] packet, ref int length, PacketDirection direction )
        {
            if ( packet[0] == 0x4E && IsEnabled )
            {
                return true;
            }

            if ( packet[0] != 0x4F )
            {
                return false;
            }

            _serverLightLevel = packet[1];

            if ( !IsEnabled )
            {
                return false;
            }

            packet[1] = (byte) GetLightLevel( _serverLightLevel );

            return false;
        }
    }

    public enum LightLevelMode
    {
        Fixed,
        MinMax
    }

    public class LightLevelOptions : SetPropertyNotifyChanged
    {
        private int _fixedLevel = LightLevelFilter.MAX_LIGHT_LEVEL;
        private int _maxLevel = LightLevelFilter.MAX_LIGHT_LEVEL;
        private int _minLevel;
        private LightLevelMode _mode = LightLevelMode.Fixed;

        public int FixedLevel
        {
            get => _fixedLevel;
            set => SetProperty( ref _fixedLevel, Clamp( value ) );
        }

        public bool IsFixed
        {
            get => Mode == LightLevelMode.Fixed;
            set
            {
                if ( value )
                {
                    Mode = LightLevelMode.Fixed;
                }
            }
        }

        public bool IsMinMax
        {
            get => Mode == LightLevelMode.MinMax;
            set
            {
                if ( value )
                {
                    Mode = LightLevelMode.MinMax;
                }
            }
        }

        public int MaxLevel
        {
            get => _maxLevel;
            set
            {
                SetProperty( ref _maxLevel, Clamp( value ) );

                if ( MinLevel > _maxLevel )
                {
                    MinLevel = _maxLevel;
                }
            }
        }

        public int MinLevel
        {
            get => _minLevel;
            set
            {
                SetProperty( ref _minLevel, Clamp( value ) );

                if ( MaxLevel < _minLevel )
                {
                    MaxLevel = _minLevel;
                }
            }
        }

        public LightLevelMode Mode
        {
            get => _mode;
            set
            {
                SetProperty( ref _mode, value );
                OnPropertyChanged( nameof( IsFixed ) );
                OnPropertyChanged( nameof( IsMinMax ) );
            }
        }

        private static int Clamp( int value )
        {
            return Math.Min( Math.Max( value, 0 ), LightLevelFilter.MAX_LIGHT_LEVEL );
        }
    }
}
