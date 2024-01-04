using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ClassicAssist.Launcher.Annotations;
using ClassicAssist.Shared.Misc;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.Launcher
{
    public class ShardManager : INotifyPropertyChanged
    {
        private static ShardManager _instance;
        private static readonly object _lock = new object();
        private readonly ShardEntryComparer _comparer = new ShardEntryComparer();

        private ShardManager()
        {
            Shards.AddSorted(
                new ShardEntry
                {
                    Name = "Official EA Servers",
                    Address = "login.ultimaonline.com",
                    Port = 7775,
                    IsPreset = true,
                    HasStatusProtocol = false,
                    Encryption = true,
                    Website = "http://www.uo.com/"
                }, _comparer );

            Shards.AddSorted(
                new ShardEntry
                {
                    Name = "UOGamers: Demise",
                    Address = "login.uogdemise.com",
                    Port = 2593,
                    IsPreset = true,
                    Website = "https://uogdemise.com/"
                }, _comparer );

            Shards.AddSorted(
                new ShardEntry
                {
                    Name = "UOGamers: Hybrid",
                    Address = "login.uohybrid.com",
                    Port = 2593,
                    IsPreset = true,
                    Website = "http://uohybrid.com/"
                }, _comparer );

            Shards.AddSorted(
                new ShardEntry
                {
                    Name = "Heritage UO",
                    Address = "play.trueuo.com",
                    Port = 2593,
                    IsPreset = true,
                    Website = "https://trueuo.com/"
                }, _comparer );

            Shards.AddSorted(
                new ShardEntry
                {
                    Name = "UO Forever",
                    Address = "login.uoforever.com",
                    Port = 2599,
                    IsPreset = true,
                    Website = "https://www.uoforever.com/"
                }, _comparer );

            Shards.AddSorted(
                new ShardEntry
                {
                    Name = "UO Elemental",
                    Address = "login.uoelemental.com",
                    Port = 2593,
                    IsPreset = true,
                    HasStatusProtocol = true,
                    Website = "https://uoelemental.com/"
                }, _comparer );

            Shards.AddSorted(
                new ShardEntry
                {
                    Name = "UO:Renaissance",
                    Address = "login.uorenaissance.com",
                    Port = 2593,
                    IsPreset = true,
                    HasStatusProtocol = true,
                    Website = "http://www.uorenaissance.com/"
                }, _comparer );

            Shards.AddSorted(
                new ShardEntry
                {
                    Name = "UO Evolution",
                    Address = "play.uoevolution.com",
                    Port = 2593,
                    IsPreset = true,
                    HasStatusProtocol = true,
                    Website = "http://uoevolution.com/"
                }, _comparer );

            Shards.AddSorted(
                new ShardEntry
                {
                    Name = "NoTramAos",
                    Address = "notramaos.servegame.com",
                    Port = 2593,
                    IsPreset = true,
                    HasStatusProtocol = true,
                    Website = "http://notramaos.com/"
                }, _comparer );

            Shards.AddSorted(
                new ShardEntry
                {
                    Name = "UOAlive",
                    Address = "login.uoalive.com",
                    Port = 2593,
                    IsPreset = true,
                    HasStatusProtocol = true,
                    Website = "https://uoalive.com/"
                }, _comparer );

            Shards.CollectionChanged += ( sender, args ) => { OnPropertyChanged( nameof( VisibleShards ) ); };
        }

        public bool OverridePresets { get; set; }

        public ObservableCollectionEx<ShardEntry> Shards { get; set; } = new ObservableCollectionEx<ShardEntry>();

        public ObservableCollection<ShardEntry> VisibleShards =>
            new ObservableCollection<ShardEntry>( Shards.Where( e => !e.Deleted ) );

        public event PropertyChangedEventHandler PropertyChanged;

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

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        public void ImportPresets( List<ShardEntry> shards )
        {
            OverridePresets = true;

            IEnumerable<ShardEntry> deletedShards = Shards.Where( e => e.IsPreset && !shards.Contains( e ) );

            foreach ( ShardEntry deletedShard in deletedShards )
            {
                Shards.Remove( deletedShard );
            }

            foreach ( ShardEntry shardEntry in shards )
            {
                ShardEntry existing = Shards.FirstOrDefault( e => e.Equals( shardEntry ) );

                if ( existing != null )
                {
                    existing.Address = shardEntry.Address;
                    existing.Port = shardEntry.Port;
                    existing.HasStatusProtocol = shardEntry.HasStatusProtocol;
                    existing.Website = shardEntry.Website;
                    existing.Encryption = shardEntry.Encryption;
                }
                else
                {
                    Shards.AddSorted( shardEntry, _comparer );
                }
            }
        }
    }
}