using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Assistant;
using ClassicAssist.Shared.UI;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.Data.Counters
{
    public class CountersManager : SetPropertyNotifyChanged
    {
        private static CountersManager _instance;
        private static readonly object _instanceLock = new object();
        private ObservableCollection<CountersAgentEntry> _items = new ObservableCollection<CountersAgentEntry>();
        private bool _listening;

        private CountersManager()
        {
            Items.CollectionChanged += OnCollectionChanged;

            IncomingPacketHandlers.ContainerContentsEvent += OnContainerContentsEvent;
            IncomingPacketHandlers.NewWorldItemEvent += OnNewWorldItemEvent;

            Engine.DisconnectedEvent += () =>
            {
                if ( Engine.Player != null )
                {
                    Engine.Player.Backpack.Container.CollectionChanged -= OnBackpackContentsChanged;
                }

                _listening = false;
            };
        }

        public ObservableCollection<CountersAgentEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public Action RecountAll { get; set; }

        private void OnNewWorldItemEvent( Item item )
        {
            RecountAll?.Invoke();
        }

        private void OnContainerContentsEvent( int serial, ItemCollection container )
        {
            if ( Engine.Player == null )
            {
                return;
            }

            if ( Engine.Player.Backpack?.Container == null )
            {
                return;
            }

            if ( !Engine.Items.GetItem( serial, out Item item ) )
            {
                return;
            }

            if ( item.Owner == Engine.Player.Serial && item.Layer == Layer.Backpack && !_listening )
            {
                Engine.Player.Backpack.Container.CollectionChanged += OnBackpackContentsChanged;

                RecountAll?.Invoke();

                _listening = true;

                return;
            }

            if ( item.IsDescendantOf( Engine.Player.Backpack.Serial ) )
            {
                // Is child item of backpack
                RecountAll?.Invoke();
            }
        }

        private void OnBackpackContentsChanged( int totalcount, bool added, Item[] items )
        {
            RecountAll?.Invoke();
        }

        private void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            OnPropertyChanged( nameof( Items ) );
        }

        public static CountersManager GetInstance()
        {
            // ReSharper disable once InvertIf
            if ( _instance == null )
            {
                lock ( _instanceLock )
                {
                    if ( _instance == null )
                    {
                        _instance = new CountersManager();
                    }
                }
            }

            return _instance;
        }
    }
}