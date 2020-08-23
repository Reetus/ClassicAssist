using System.Collections.ObjectModel;

namespace ClassicAssist.Launcher
{
    public class ShardManager
    {
        private static ShardManager _instance;
        private static readonly object _lock = new object();

        private ShardManager()
        {
            Shards.Add( new ShardEntry
            {
                Name = "Official EA Servers",
                Address = "login.ultimaonline.com",
                Port = 7775,
                IsPreset = true,
                HasStatusProtocol = false,
                Encryption = true,
                Website = "http://www.uo.com/"
            } );

            Shards.Add( new ShardEntry
            {
                Name = "UOGamers: Demise",
                Address = "login.uodemise.com",
                Port = 2593,
                IsPreset = true,
                Website = "https://uogdemise.com/"
            } );

            Shards.Add( new ShardEntry
            {
                Name = "UOGamers: Hybrid",
                Address = "login.uohybrid.com",
                Port = 2593,
                IsPreset = true,
                Website = "http://uohybrid.com/"
            } );

            Shards.Add( new ShardEntry
            {
                Name = "Heritage UO",
                Address = "play.trueuo.com",
                Port = 2593,
                IsPreset = true,
                Website = "https://trueuo.com/"
            } );

            Shards.Add( new ShardEntry
            {
                Name = "UO Forever",
                Address = "login.uoforever.com",
                Port = 2599,
                IsPreset = true,
                Website = "https://www.uoforever.com/"
            } );

            Shards.Add( new ShardEntry
            {
                Name = "UO Outlands",
                Address = "play.uooutlands.com",
                Port = 2593,
                IsPreset = true,
                HasStatusProtocol = false,
                ShardType = 2,
                Website = "https://uooutlands.com/"
            } );

            Shards.Add( new ShardEntry
            {
                Name = "UO Players",
                Address = "login.uoplayers.com",
                Port = 2593,
                IsPreset = true,
                HasStatusProtocol = true,
                Website = "https://uoplayers.com/"
            } );

            Shards.Add( new ShardEntry
            {
                Name = "UO:Renaissance",
                Address = "login.uorenaissance.com",
                Port = 2593,
                IsPreset = true,
                HasStatusProtocol = true,
                Website = "http://www.uorenaissance.com/"
            } );

            Shards.Add( new ShardEntry
            {
                Name = "UO Evolution",
                Address = "play.uoevolution.com",
                Port = 2593,
                IsPreset = true,
                HasStatusProtocol = true,
                Website = "http://uoevolution.com/"
            } );
        }

        public ObservableCollection<ShardEntry> Shards { get; set; } = new ObservableCollection<ShardEntry>();

        public static ShardManager GetInstance()
        {
            // ReSharper disable once InvertIf
            if ( _instance == null )
            {
                lock ( _lock )
                {
                    if ( _instance != null )
                    {
                        return _instance;
                    }

                    _instance = new ShardManager();
                    return _instance;
                }
            }

            return _instance;
        }
    }
}