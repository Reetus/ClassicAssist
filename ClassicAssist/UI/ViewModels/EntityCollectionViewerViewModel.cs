using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Data.Misc;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Models;
using ClassicAssist.UI.Views;
using ClassicAssist.UI.Views.ECV;
using ClassicAssist.UI.Views.ECV.Filter.Models;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.UI.ViewModels
{
    public class EntityCollectionViewerViewModel : BaseViewModel
    {
        private readonly Func<ItemCollection> _customRefreshCommand;
        private ICommand _applyFiltersCommand;
        private ICommand _autolootContainerCommand;
        private ICommand _changeSortStyleCommand;
        private ItemCollection _collection = new ItemCollection( 0 );

        private ICommand _combineStacksCommand;
        private ICommand _configureCommand;
        private ICommand _contextContextMenuRequestCommand;
        private ICommand _contextCustomActionCommand;
        private ICommand _contextMoveToBackpackCommand;
        private ICommand _contextMoveToBankCommand;
        private ICommand _contextMoveToContainerCommand;
        private ICommand _contextMoveToGroundCommand;
        private ICommand _contextMoveToSetCommand;
        private ICommand _contextOpenContainerCommand;
        private ICommand _contextTargetCommand;
        private ICommand _contextUseItemCommand;
        private ObservableCollection<EntityCollectionData> _entities;
        private ICommand _equipItemCommand;
        private List<EntityCollectionFilterGroup> _filters;
        private ICommand _hideItemCommand;
        private ICommand _itemDoubleClickCommand;
        private ICommand _openAllContainersCommand;
        private EntityCollectionViewerOptions _options;
        private ICommand _refreshCommand;
        private ICommand _replaceNameCommand;

        private ObservableCollection<EntityCollectionData> _selectedItems = new ObservableCollection<EntityCollectionData>();

        private bool _showFilter;
        private bool _showOrganizer;

        private bool _showProperties;

        private IComparer<Entity> _sorter = new IDThenSerialComparer();
        private string _statusLabel;
        private ICommand _targetContainerCommand;
        private ICommand _toggleAlwaysOnTopCommand;
        private ICommand _toggleChildItemsCommand;
        private ICommand _togglePropertiesCommand;
        private bool _tooltipsEnabled;
        private bool _topmost;

        public EntityCollectionViewerViewModel()
        {
#if DEBUG
            if ( DesignerProperties.GetIsInDesignMode( new DependencyObject() ) )
            {
                Environment.CurrentDirectory = @"C:\Users\johns\Documents\UO\ClassicAssist\Output\net48";
                const string uoFolder = @"C:\Users\johns\Documents\UO\Ultima Online Classic";

                Art.Initialize( uoFolder );
                TileData.Initialize( uoFolder );
                Statics.Initialize( uoFolder );
                Cliloc.Initialize( uoFolder );
            }
#endif

            Collection = new ItemCollection( 0 );

            Entities = new ObservableCollection<EntityCollectionData>( Collection.ToEntityCollectionData( _sorter, _nameOverrides ) );
        }

        public EntityCollectionViewerViewModel( ItemCollection collection, Func<ItemCollection> refreshCommand, Dictionary<string, Action<Item>> customActions = null ) :
            this( collection )
        {
            _customRefreshCommand = refreshCommand;

            if ( customActions == null )
            {
                return;
            }

            foreach ( KeyValuePair<string, Action<Item>> customAction in customActions )
            {
                CustomContextActions.Add( customAction );
            }
        }

        public EntityCollectionViewerViewModel( ItemCollection collection )
        {
            Collection = collection;
            Options = LoadOptions();

            Entities = new ObservableCollection<EntityCollectionData>( !Options.ShowChildItems
                ? collection.ToEntityCollectionData( _sorter, _nameOverrides )
                : new ItemCollection( collection.Serial ) { ItemCollection.GetAllItems( collection.GetItems() ) }.ToEntityCollectionData( _sorter, _nameOverrides ) );

            SelectedItems.CollectionChanged += ( sender, args ) =>
            {
                if ( !( sender is ObservableCollection<EntityCollectionData> ) )
                {
                    return;
                }

                UpdateStatusLabel();
            };

            UpdateStatusLabel();

            Collection.CollectionChanged += OnCollectionChanged;

            TooltipsEnabled = Engine.CharacterListFlags.HasFlag( CharacterListFlags.PaladinNecromancerClassTooltips );

            ThreadQueue = new ThreadPriorityQueue<QueueAction>( ProcessQueue );
            QueueActions.CollectionChanged += QueueActions_CollectionChanged;
        }

        public ICommand ApplyFiltersCommand => _applyFiltersCommand ?? ( _applyFiltersCommand = new RelayCommand( ApplyFilters, o => true ) );

        public ICommand AutolootContainerCommand => _autolootContainerCommand ?? ( _autolootContainerCommand = new RelayCommand( AutolootContainer, o => true ) );

        public ICommand ChangeSortStyleCommand => _changeSortStyleCommand ?? ( _changeSortStyleCommand = new RelayCommand( ChangeSortStyle, o => true ) );

        public ItemCollection Collection
        {
            get => _collection;
            set => SetProperty( ref _collection, value );
        }

        public ICommand CombineStacksCommand => _combineStacksCommand ?? ( _combineStacksCommand = new RelayCommandAsync( CombineStacks, o => Collection.Serial > 0 ) );
        public ICommand ConfigureCommand => _configureCommand ?? ( _configureCommand = new RelayCommand( Configure, o => true ) );

        public ICommand ContextContextMenuRequestCommand =>
            _contextContextMenuRequestCommand ?? ( _contextContextMenuRequestCommand = new RelayCommand( ContextMenuRequest, o => SelectedItems.Count > 0 ) );

        public ICommand ContextCustomActionCommand =>
            _contextCustomActionCommand ?? ( _contextCustomActionCommand = new RelayCommand( ContextCustomAction, o => SelectedItems != null ) );

        public ICommand ContextMoveToBackpackCommand =>
            _contextMoveToBackpackCommand ?? ( _contextMoveToBackpackCommand = new RelayCommandAsync( ContextMoveToBackpack, o => SelectedItems != null ) );

        public ICommand ContextMoveToBankCommand =>
            _contextMoveToBankCommand ?? ( _contextMoveToBankCommand = new RelayCommandAsync( ContextMoveToBank, o => SelectedItems != null ) );

        public ICommand ContextMoveToContainerCommand =>
            _contextMoveToContainerCommand ?? ( _contextMoveToContainerCommand = new RelayCommandAsync( ContextMoveToContainer, o => SelectedItems != null ) );

        public ICommand ContextMoveToGroundCommand =>
            _contextMoveToGroundCommand ?? ( _contextMoveToGroundCommand = new RelayCommandAsync( ContextMoveToGround, o => SelectedItems != null ) );

        public ICommand ContextMoveToSetCommand => _contextMoveToSetCommand ?? ( _contextMoveToSetCommand = new RelayCommand( ContextMoveToSet, o => SelectedItems != null ) );

        public ICommand ContextOpenContainerCommand =>
            _contextOpenContainerCommand ?? ( _contextOpenContainerCommand = new RelayCommand( ContextOpenContainer,
                o => SelectedItems != null && SelectedItems.Any( e => e.Entity is Item item && item.Owner != 0 && !UOMath.IsMobile( item.Owner ) ) ) );

        public ICommand ContextTargetCommand => _contextTargetCommand ?? ( _contextTargetCommand = new RelayCommand( ContextTarget ) );

        public ICommand ContextUseItemCommand => _contextUseItemCommand ?? ( _contextUseItemCommand = new RelayCommandAsync( ContextUseItem, o => SelectedItems != null ) );

        public ObservableCollection<KeyValuePair<string, Action<Item>>> CustomContextActions { get; set; } = new ObservableCollection<KeyValuePair<string, Action<Item>>>();

        public ObservableCollection<EntityCollectionData> Entities
        {
            get => _entities;
            set => SetProperty( ref _entities, value );
        }

        public ICommand EquipItemCommand => _equipItemCommand ?? ( _equipItemCommand = new RelayCommandAsync( EquipItem, o => SelectedItems != null ) );

        public ICommand HideItemCommand => _hideItemCommand ?? ( _hideItemCommand = new RelayCommand( HideItem, o => SelectedItems != null ) );

        public ICommand ItemDoubleClickCommand => _itemDoubleClickCommand ?? ( _itemDoubleClickCommand = new RelayCommand( ItemDoubleClick, o => true ) );

        public static Lazy<Dictionary<int, int>> MountIDEntries { get; set; } = new Lazy<Dictionary<int, int>>( LoadMountIDEntries );

        public ICommand OpenAllContainersCommand => _openAllContainersCommand ?? ( _openAllContainersCommand = new RelayCommand( OpenAllContainers, o => true ) );

        public EntityCollectionViewerOptions Options
        {
            get => _options;
            set => SetProperty( ref _options, value );
        }

        public ObservableCollectionEx<QueueAction> QueueActions { get; set; } = new ObservableCollectionEx<QueueAction>();

        public ICommand RefreshCommand => _refreshCommand ?? ( _refreshCommand = new RelayCommand( Refresh, o => true ) );

        public ICommand ReplaceNameCommand => _replaceNameCommand ?? ( _replaceNameCommand = new RelayCommandAsync( ReplaceName, o => true ) );

        public ObservableCollection<EntityCollectionData> SelectedItems
        {
            get => _selectedItems;
            set => SetProperty( ref _selectedItems, value );
        }

        public bool ShowFilter
        {
            get => _showFilter;
            set => SetProperty( ref _showFilter, value );
        }

        public bool ShowOrganizer
        {
            get => _showOrganizer;
            set => SetProperty( ref _showOrganizer, value );
        }

        public bool ShowProperties
        {
            get => _showProperties;
            set => SetProperty( ref _showProperties, value );
        }

        public string StatusLabel
        {
            get => _statusLabel;
            set => SetProperty( ref _statusLabel, value );
        }

        public ICommand TargetContainerCommand => _targetContainerCommand ?? ( _targetContainerCommand = new RelayCommand( TargetContainer, o => Collection.Serial > 0 ) );

        public ThreadPriorityQueue<QueueAction> ThreadQueue { get; set; }

        public ICommand ToggleAlwaysOnTopCommand => _toggleAlwaysOnTopCommand ?? ( _toggleAlwaysOnTopCommand = new RelayCommand( ToggleAlwaysOnTop, o => true ) );

        public ICommand ToggleChildItemsCommand => _toggleChildItemsCommand ?? ( _toggleChildItemsCommand = new RelayCommand( ToggleChildItems, o => true ) );

        public ICommand TogglePropertiesCommand => _togglePropertiesCommand ?? ( _togglePropertiesCommand = new RelayCommand( ToggleProperties, o => true ) );

        public bool TooltipsEnabled
        {
            get => _tooltipsEnabled;
            set => SetProperty( ref _tooltipsEnabled, value );
        }

        public bool Topmost
        {
            get => _topmost;
            set => SetProperty( ref _topmost, value );
        }

        private Dictionary<int, string> _nameOverrides { get; } = new Dictionary<int, string>();

        private void AutolootContainer( object param )
        {
            Item[] items = Collection.GetItems();

            if ( items.Length == 0 )
            {
                return;
            }

            List<Item> lootItems = AutolootManager.GetInstance().CheckItems?.Invoke( items );

            if ( lootItems == null || lootItems.Count == 0 )
            {
                return;
            }

            EnqueueAction( async obj =>
            {
                foreach ( var item in lootItems.Select( ( value, i ) => new { i, value } ) )
                {
                    if ( obj.CancellationTokenSource.IsCancellationRequested )
                    {
                        _dispatcher.Invoke( () => { obj.Status = Strings.Cancel; } );
                        return false;
                    }

                    _dispatcher.Invoke( () => { obj.Status = string.Format( Strings.Moving_item__0_____1_, item.i, lootItems.Count ); } );

                    int attempts;

                    for ( attempts = 0; attempts < 5; attempts++ )
                    {
                        if ( !await EnqueueDragDrop( item.value.Serial, -1, Engine.Player.Backpack.Serial, obj.CancellationTokenSource.Token ) )
                        {
                            Commands.SystemMessage( $"Retrying 0x{item.value.Serial:x}..." );
                            continue;
                        }

                        break;
                    }
                }

                return true;
            }, string.Format( Strings.Moving_item__0_____1_, 0, lootItems.Count ) );
        }

        private void ContextCustomAction( object arg )
        {
            if ( !( arg is KeyValuePair<string, Action<Item>> action ) )
            {
                return;
            }

            var items = SelectedItems.Select( ( e, i ) => new { e.Entity, i } ).ToList();

            EnqueueAction( obj =>
            {
                foreach ( var item in items )
                {
                    _dispatcher.Invoke( () => { obj.Status = $"{action.Key} {item.i} / {items.Count}"; } );
                    action.Value.Invoke( item.Entity as Item );
                }

                return Task.FromResult( true );
            }, action.Key );
        }

        private async Task ContextMoveToGround( object arg )
        {
            ( TargetType _, TargetFlags _, int _, int x, int y, int z, int _ ) = await Commands.GetTargetInfoAsync( Strings.Target_location___ );

            if ( x == -1 || y == -1 )
            {
                return;
            }

            int[] items = SelectedItems.Select( i => i.Entity.Serial ).ToArray();

            EnqueueAction( async obj =>
            {
                foreach ( var item in items.Select( ( value, i ) => new { i, value } ) )
                {
                    if ( obj.CancellationTokenSource.IsCancellationRequested )
                    {
                        _dispatcher.Invoke( () => { obj.Status = Strings.Cancel; } );
                        return false;
                    }

                    _dispatcher.Invoke( () => { obj.Status = string.Format( Strings.Moving_item__0_____1_, item.i, items.Length ); } );

                    await ActionPacketQueue.EnqueueDragDropGround( item.value, -1, x, y, z );
                }

                return true;
            }, string.Format( Strings.Moving_item__0_____1_, 0, items.Length ) );
        }

        private void ContextMoveToSet( object arg )
        {
            List<int> usedContainers = new List<int>();

            if ( !( arg is ObservableCollection<int> containers ) )
            {
                return;
            }

            Item[] items = SelectedItems.Where( i => i.Entity is Item ).Select( i => i.Entity ).Cast<Item>().ToArray();

            EnqueueAction( async obj =>
            {
                foreach ( var item in items.Select( ( value, i ) => new { i, value } ) )
                {
                    if ( obj.CancellationTokenSource.IsCancellationRequested )
                    {
                        _dispatcher.Invoke( () => { obj.Status = Strings.Cancel; } );
                        return false;
                    }

                    _dispatcher.Invoke( () => { obj.Status = string.Format( Strings.Moving_item__0_____1_, item.i, items.Length ); } );

                    int attempts;

                    for ( attempts = 0; attempts < 5; attempts++ )
                    {
                        int serial = await GetContainer();

                        if ( item.value.Owner == serial )
                        {
                            break;
                        }

                        PacketFilterInfo pfi = new PacketFilterInfo( 0x25, new[] { PacketFilterConditions.IntAtPositionCondition( item.value.Serial, 1 ), PacketFilterConditions.IntAtPositionCondition( serial, 15 ) } );
                        PacketWaitEntry waitEntry = Engine.PacketWaitEntries.Add( pfi, PacketDirection.Incoming, true );

                        if ( !await EnqueueDragDrop( item.value.Serial, -1, serial, obj.CancellationTokenSource.Token ) )
                        {
                            Commands.SystemMessage( $"Retrying 0x{item.value.Serial:x}..." );
                            continue;
                        }

                        bool result = waitEntry.Lock.WaitOne( 3000 );

                        if ( !result )
                        {
                            Commands.SystemMessage( $"Retrying 0x{item.value.Serial:x}..." );
                            continue;
                        }

                        break;
                    }
                }

                return true;
            }, string.Format( Strings.Moving_item__0_____1_, 0, items.Length ) );

            return;

            async Task<int> GetContainer()
            {
                if ( !Engine.TooltipsEnabled )
                {
                    return containers.FirstOrDefault();
                }

                int serial = containers.FirstOrDefault();

                foreach ( int container in containers )
                {
                    Item item = Engine.Items.GetItem( container );

                    if ( item == null )
                    {
                        continue;
                    }

                    if ( item.Properties == null )
                    {
                        await Commands.WaitForPropertiesAsync( new[] { item }, 5000 );
                    }

                    Property property = item.Properties?.FirstOrDefault( e => e.Cliloc == 1073841 );

                    if ( property == null )
                    {
                        continue;
                    }

                    if ( property.Arguments[0].Equals( property.Arguments[1] ) )
                    {
                        continue;
                    }

                    serial = container;

                    if ( !usedContainers.Contains( serial ) )
                    {
                        Commands.WaitForContainerContentsUse( serial, 5000 );
                        await Task.Delay( CurrentOptions.ActionDelayMS );
                        usedContainers.Add( serial );
                    }

                    break;
                }

                return serial;
            }
        }

        public EntityCollectionViewerOptions LoadOptions()
        {
            string fileName = Path.Combine( Engine.StartupPath, "EntityCollectionViewerOptions.json" );

            if ( !File.Exists( fileName ) )
            {
                return new EntityCollectionViewerOptions();
            }

            string json = File.ReadAllText( fileName );

            return EntityCollectionViewerOptions.Deserialize( JObject.Parse( json ) );
        }

        public void SaveOptions( EntityCollectionViewerOptions options )
        {
            string fileName = Path.Combine( Engine.StartupPath, "EntityCollectionViewerOptions.json" );

            JToken jObject = EntityCollectionViewerOptions.Serialize( options );

            string hash = jObject.ToString().SHA1();

            if ( options.Hash == hash )
            {
                return;
            }

            options.Hash = hash;

            File.WriteAllText( fileName, jObject.ToString() );
        }

        private void ContextOpenContainer( object arg )
        {
            int[] containerSerials = SelectedItems.Where( e => e.Entity is Item item && item.Owner != 0 && !UOMath.IsMobile( item.Owner ) ).Select( e => ( (Item) e.Entity ).Owner )
                .ToArray();

            EnqueueAction( async obj =>
            {
                foreach ( var item in containerSerials.Select( ( value, i ) => new { Index = i, Value = value } ) )
                {
                    _dispatcher.Invoke( () => { obj.Status = string.Format( Strings.Opening_container__0_____1____, item.Index, containerSerials.Length ); } );
                    await ActionPacketQueue.EnqueuePacket( new UseObject( item.Value ) );
                }

                return true;
            }, Strings.Open_container );
        }

        private void Configure( object obj )
        {
            EntityCollectionViewerSettingsWindow window = new EntityCollectionViewerSettingsWindow { Topmost = Options.AlwaysOnTop, Options = Options };

            window.ShowDialog();

            SaveOptions( Options );
        }

        private async Task ContextMoveToBank( object arg )
        {
            Item item = Engine.Player.GetEquippedItems().FirstOrDefault( i => i.Layer == Layer.Bank );

            if ( item != null )
            {
                await ContextMoveToContainer( item.Serial );
            }
        }

        private void QueueActions_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            if ( e?.NewItems == null )
            {
                return;
            }

            foreach ( object eNewItem in e.NewItems )
            {
                if ( !( eNewItem is QueueAction queueAction ) )
                {
                    continue;
                }

                ThreadQueue?.Enqueue( queueAction, QueuePriority.Low );
            }
        }

        private void ProcessQueue( QueueAction obj )
        {
            if ( !obj?.CancellationTokenSource.IsCancellationRequested ?? false )
            {
                obj.Action.Invoke( obj ).Wait();
            }

            _dispatcher.Invoke( () => { QueueActions.Remove( obj ); } );
        }

        private async Task ReplaceName( object arg )
        {
            Dictionary<int, StringBuilder> temp = new Dictionary<int, StringBuilder>();
            int[] serials = Entities.Select( e => e.Entity.Serial ).ToArray();
            AutoResetEvent are = new AutoResetEvent( false );

            void IncomingPacketHandlersOnJournalEntryAddedEvent( JournalEntry je )
            {
                if ( !serials.Contains( je.Serial ) || je.SpeechType != JournalSpeech.Label )
                {
                    return;
                }

                if ( !temp.ContainsKey( je.Serial ) )
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append( je.Text );

                    temp.Add( je.Serial, sb );
                }
                else
                {
                    temp[je.Serial].AppendLine();
                    temp[je.Serial].Append( je.Text );
                }

                if ( temp.Count == serials.Length )
                {
                    are.Set();
                }
            }

            IncomingPacketHandlers.JournalEntryAddedEvent += IncomingPacketHandlersOnJournalEntryAddedEvent;

            foreach ( int serial in serials )
            {
                Engine.SendPacketToServer( new LookRequest( serial ) );
            }

            Task<bool> journalTask = are.ToTask();
            Task delayTask = Task.Delay( 5000 );

            await Task.WhenAny( journalTask, delayTask );

            IncomingPacketHandlers.JournalEntryAddedEvent -= IncomingPacketHandlersOnJournalEntryAddedEvent;

            foreach ( KeyValuePair<int, StringBuilder> stringBuilder in temp )
            {
                if ( _nameOverrides.ContainsKey( stringBuilder.Key ) )
                {
                    _nameOverrides.Remove( stringBuilder.Key );
                }

                _nameOverrides.Add( stringBuilder.Key, stringBuilder.Value.ToString() );
            }

            Refresh( null );
        }

        private Task CombineStacks( object arg )
        {
            EnqueueAction( async action =>
            {
                try
                {
                    List<int> ignoreList = new List<int>();

                    while ( true )
                    {
                        Item destStack = Collection.SelectEntity( i =>
                            i.Count < 60000 && TileData.GetStaticTile( i.ID ).Flags.HasFlag( TileFlags.Stackable ) && !ignoreList.Contains( i.Serial ) && !Excluded( i ) );

                        if ( destStack == null )
                        {
                            return false;
                        }

                        int needed = 60000 - destStack.Count;

                        Item sourceStack = Collection
                            .SelectEntities( i =>
                                i.ID == destStack.ID && i.Hue == destStack.Hue && i.Serial != destStack.Serial && i.Count != 60000 &&
                                ( !Engine.TooltipsEnabled || StackNamesMatch( i, destStack ) ) )?.OrderBy( i => i.Count ).FirstOrDefault();

                        if ( sourceStack == null )
                        {
                            ignoreList.Add( destStack.Serial );
                            continue;
                        }

                        Commands.SystemMessage( $"{sourceStack.Name} => {destStack.Name}", SystemMessageHues.Green );

                        await ActionPacketQueue.EnqueueDragDrop( sourceStack.Serial, needed > sourceStack.Count ? sourceStack.Count : needed, destStack.Serial,
                            options: new DragDropOptions { CheckExisting = true, DelaySend = false } );

                        await Task.Delay( TimeSpan.FromMilliseconds( Data.Options.CurrentOptions.ActionDelayMS ), action.CancellationTokenSource.Token );

                        RefreshCommand?.Execute( null );

                        action.CancellationTokenSource.Token.ThrowIfCancellationRequested();
                    }
                }
                catch ( TaskCanceledException )
                {
                }

                return !action.CancellationTokenSource.IsCancellationRequested;
            }, Strings.Combine_stacks );

            return Task.CompletedTask;

            bool Excluded( Entity item )
            {
                if ( Options.CombineStacksIgnore == null || Options.CombineStacksIgnore.Count == 0 )
                {
                    return false;
                }

                return Options.CombineStacksIgnore.Any( e =>
                    ( e.ID == -1 || e.ID == item.ID ) && ( e.Hue == -1 || e.Hue == item.Hue ) && ( e.Cliloc == -1 || e.Cliloc == item.Properties?[0].Cliloc ) );
            }
        }

        private static bool StackNamesMatch( Item item, Item destStack )
        {
            string sourceStackName = GetNameMinusAmount( item );
            string destStackName = GetNameMinusAmount( destStack );

            if ( item.Count > 1 && sourceStackName.EndsWith( "s" ) && destStack.Count == 1 && !destStackName.EndsWith( "s" ) )
            {
                sourceStackName = sourceStackName.TrimEnd( 's' );
            }

            if ( destStack.Count > 1 && destStackName.EndsWith( "s" ) && item.Count == 1 && !sourceStackName.EndsWith( "s" ) )
            {
                destStackName = destStackName.TrimEnd( 's' );
            }

            return sourceStackName.Equals( destStackName );
        }

        private static string GetNameMinusAmount( Item item )
        {
            if ( item.Properties == null || item.Properties.Length == 0 )
            {
                return item.Name.Trim();
            }

            Property property = item.Properties.First();

            if ( property.Arguments == null || property.Arguments.Length == 0 )
            {
                return item.Name.Trim();
            }

            List<string> newArguments = ( from argument in property.Arguments select argument.Equals( item.Count.ToString() ) ? string.Empty : argument ).ToList();

            return newArguments.Count == 0 ? item.Name.Trim() : Cliloc.GetLocalString( property.Cliloc, newArguments.ToArray() ).Trim();
        }

        private void TargetContainer( object obj )
        {
            TargetCommands.Target( Collection.Serial );
        }

        private void OnCollectionChanged( int totalcount, bool added, Item[] entities )
        {
            if ( added )
            {
                _dispatcher.Invoke( () =>
                {
                    List<Item> newEntities = entities.Where( e => !Entities.Any( f => f.Entity.Equals( e ) ) )
                        .Where( entity => Options.ShowChildItems || entity.Owner == Collection.Serial ).ToList();

                    foreach ( Item entity in newEntities )
                    {
                        Entities.Add( entity.ToEntityCollectionData( _nameOverrides ) );
                    }

                    if ( _filters != null )
                    {
                        ApplyFilters( _filters );
                    }
                } );
            }
            else
            {
                _dispatcher.Invoke( () =>
                {
                    foreach ( EntityCollectionData ecd in entities.ToList().Select( item => Entities.FirstOrDefault( e => e.Entity.Equals( item ) ) ).Where( ecd => ecd != null ) )
                    {
                        Entities.Remove( ecd );
                    }
                } );
            }

            UpdateStatusLabel();
        }

        private void ContextTarget( object obj )
        {
            List<EntityCollectionData> items = SelectedItems.ToList();

            if ( !items.Any() )
            {
                return;
            }

            EnqueueAction( async queueAction =>
            {
                foreach ( var item in items.Select( ( value, i ) => new { i, value } ) )
                {
                    if ( queueAction.CancellationTokenSource.IsCancellationRequested )
                    {
                        _dispatcher.Invoke( () => { queueAction.Status = Strings.Cancel; } );
                        return false;
                    }

                    _dispatcher.Invoke( () => { queueAction.Status = string.Format( Strings.Targeting_item__0_____1_, item.i, items.Count ); } );

                    if ( await WaitForTarget( -1, queueAction.CancellationTokenSource.Token ) )
                    {
                        TargetCommands.Target( item.value.Entity.Serial );
                    }
                }

                return true;
            }, string.Format( Strings.Targeting_item__0_____1_, 0, items.Count ) );
            return;

            async Task<bool> WaitForTarget( int timeout, CancellationToken cancellationToken )
            {
                if ( Engine.TargetExists )
                {
                    return true;
                }

                PacketFilterInfo pfi = new PacketFilterInfo( 0x6C );

                Engine.WaitingForTarget = true;

                PacketWaitEntry we = Engine.PacketWaitEntries.Add( pfi, PacketDirection.Incoming, includeInternal: true );

                try
                {
                    do
                    {
                        Task<bool> task = we.Lock.ToTask();
                        Task completedTask = await Task.WhenAny( task, Task.Delay( timeout, cancellationToken ) );

                        if ( completedTask != task )
                        {
                            return false;
                        }

                        byte[] packet = we.Packet;

                        if ( packet[6] == 0x03 )
                        {
                            continue;
                        }

                        return true;
                    }
                    while ( true );
                }
                finally
                {
                    Engine.PacketWaitEntries.Remove( we );

                    Engine.WaitingForTarget = false;
                }
            }
        }

        private void HideItem( object obj )
        {
            foreach ( Entity entity in SelectedItems.Select( e => e.Entity ) )
            {
                Commands.RemoveObject( entity.Serial );
            }
        }

        ~EntityCollectionViewerViewModel()
        {
            SaveOptions( Options );
            Collection.CollectionChanged -= OnCollectionChanged;
            ThreadQueue?.Dispose();
        }

        private void OpenAllContainers( object obj )
        {
            Dictionary<int, int> containerGumpIds = null;

            if ( Options.OpenContainersOnlyKnownContainers )
            {
                string fileName = Path.Combine( Engine.StartupPath, "Data", "ContainerGumpIDs.json" );

                if ( File.Exists( fileName ) )
                {
                    containerGumpIds = JsonConvert.DeserializeObject<Dictionary<int, int>>( File.ReadAllText( fileName ) );
                }
            }

            EnqueueAction( async action =>
            {
                List<Item> containers = Collection.Where( i => TileData.GetStaticTile( i.ID ).Flags.HasFlag( TileFlags.Container ) && !Excluded( i ) ).ToList();

                do
                {
                    containers = await OpenContainers( containers, action );

                    if ( action.CancellationTokenSource.IsCancellationRequested )
                    {
                        break;
                    }
                }
                while ( containers.Count > 0 );

                Thread.Sleep( 1000 );
                RefreshCommand.Execute( null );

                return true;
            }, Strings.Open_All_Containers );

            return;

            bool Excluded( Entity item )
            {
                return Options.OpenContainersIgnore.Any( e =>
                    ( e.ID == -1 || e.ID == item.ID ) && ( e.Cliloc == -1 || item.Properties == null || item.Properties.Any( p => p.Cliloc == e.Cliloc ) ) &&
                    ( e.Hue == -1 || item.Hue == e.Hue ) ) || Options.OpenContainersOnlyKnownContainers && containerGumpIds != null && !containerGumpIds.ContainsKey( item.ID );
            }

            async Task<List<Item>> OpenContainers( IEnumerable<Item> items, QueueAction queueAction )
            {
                List<Item> containerItem = new List<Item>();

                var containers = items.Select( ( item, index ) => new { item, index } ).ToList();

                IEnumerable<Task<bool>> tasks = containers.Select( item => ActionPacketQueue.EnqueueAction( item, arg =>
                {
                    _dispatcher.Invoke( () => { queueAction.Status = string.Format( Strings.Opening_container__0_____1____, item.index, containers.Count ); } );
                    int pos = 19;

                    if ( Engine.ClientVersion < new Version( 6, 0, 1, 7 ) )
                    {
                        pos = 18;
                    }

                    PacketFilterInfo pfi = new PacketFilterInfo( 0x3C, new[] { PacketFilterConditions.IntAtPositionCondition( arg.item.Serial, pos ) } );

                    PacketWaitEntry we = Engine.PacketWaitEntries.Add( pfi, PacketDirection.Incoming, true );

                    Engine.SendPacketToServer( new UseObject( arg.item.Serial ) );

                    try
                    {
                        bool result = we.Lock.WaitOne( 1000 );

                        IEnumerable<Item> newItem = Engine.Items.GetItem( arg.item.Serial ).Container?.Where( i =>
                            TileData.GetStaticTile( i.ID ).Flags.HasFlag( TileFlags.Container ) && !Excluded( i ) ) ?? Enumerable.Empty<Item>();

                        containerItem.AddRange( newItem );

                        return result;
                    }
                    finally
                    {
                        Engine.PacketWaitEntries.Remove( we );
                    }
                }, cancellationToken: queueAction.CancellationTokenSource.Token ) );

                await Task.WhenAll( tasks );

                return containerItem;
            }
        }

        private void ToggleAlwaysOnTop( object obj )
        {
            if ( !( obj is bool alwaysOnTop ) )
            {
                return;
            }

            Options.AlwaysOnTop = alwaysOnTop;
        }

        private static Dictionary<int, int> LoadMountIDEntries()
        {
            Dictionary<int, int> entries = new Dictionary<int, int>();

            string fileName = Path.Combine( Engine.StartupPath, "Data", "MountID.json" );

            if ( !File.Exists( fileName ) )
            {
                return entries;
            }

            try
            {
                entries = JsonConvert.DeserializeObject<Dictionary<int, int>>( File.ReadAllText( fileName ) );
            }
            catch ( Exception )
            {
                // ignored
            }

            return entries;
        }

        private Task EquipItem( object obj )
        {
            EnqueueAction( async action =>
            {
                IEnumerable<EquipItemData> data = SelectedItems.Select( e => new EquipItemData { Serial = e.Entity.Serial, Layer = TileData.GetLayer( e.Entity.ID ) } )
                    .Where( e => e.Layer != Layer.Invalid );

                IEnumerable<Task<bool>> tasks = data.Select( e => ActionPacketQueue.EnqueueAction( e, arg =>
                {
                    Engine.SendPacketToServer( new DragItem( e.Serial, 1, Data.Options.CurrentOptions.DragDelay ? Data.Options.CurrentOptions.DragDelayMS : 0 ) );
                    Thread.Sleep( 50 );
                    Engine.SendPacketToServer( new EquipRequest( e.Serial, e.Layer, Engine.Player?.Serial ?? 0 ) );

                    return true;
                }, cancellationToken: action.CancellationTokenSource.Token ) );

                await Task.WhenAll( tasks );
                return true;
            }, Strings.Equip_Item );

            return Task.CompletedTask;
        }

        private void ApplyFilters( object obj )
        {
            if ( !( obj is List<EntityCollectionFilterGroup> list ) )
            {
                _filters = null;

                Entities = new ObservableCollection<EntityCollectionData>( Collection.ToEntityCollectionData( _sorter, _nameOverrides ) );

                UpdateStatusLabel();

                return;
            }

            Entities.Clear();

            ItemCollection collection = new ItemCollection( Collection.Serial ) { Collection.GetItems() };

            ItemCollection items = collection.Filter( list[0].Items );

            for ( int i = 1; i < list.Count; i++ )
            {
                EntityCollectionFilterGroup groups = list[i];

                ItemCollection groupItems = groups.Operation == BooleanOperation.Or ? collection.Filter( groups.Items ) : items.Filter( groups.Items );

                switch ( groups.Operation )
                {
                    case BooleanOperation.And:
                        items = groupItems;
                        break;
                    case BooleanOperation.Or:
                        items.Add( groupItems.GetItems() );
                        break;
                    case BooleanOperation.Not:
                        items.Remove( groupItems.GetItems() );
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            Entities = new ObservableCollection<EntityCollectionData>( items.ToEntityCollectionData( _sorter, _nameOverrides ) );

            UpdateStatusLabel();

            _filters = list;
        }

        private void ChangeSortStyle( object obj )
        {
            if ( !( obj is EntityCollectionSortStyle val ) )
            {
                return;
            }

            switch ( val )
            {
                case EntityCollectionSortStyle.Name:
                    _sorter = new NameThenSerialComparer();

                    break;

                case EntityCollectionSortStyle.Serial:
                    _sorter = new SerialComparer();

                    break;

                case EntityCollectionSortStyle.Hue:
                    _sorter = new HueThenAmountComparer();

                    break;

                case EntityCollectionSortStyle.ID:
                    _sorter = new IDThenSerialComparer();

                    break;

                case EntityCollectionSortStyle.Quantity:
                    _sorter = new QuantityThenSerialComparer();

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            Entities = new ObservableCollection<EntityCollectionData>( Collection.ToEntityCollectionData( _sorter, _nameOverrides ) );
        }

        private void UpdateStatusLabel()
        {
            StatusLabel = string.Format( Strings._0__items___1__selected___2__total_amount, Entities.Count, SelectedItems?.Count ?? 0,
                SelectedItems?.Select( i => i.Entity ).Where( i => i is Item ).Cast<Item>().Sum( i => i.Count ) );
        }

        private void Refresh( object obj )
        {
            if ( _customRefreshCommand != null )
            {
                Task.Run( () => _customRefreshCommand.Invoke() ).ContinueWith( t =>
                {
                    Collection = t.Result;
                    Entities = new ObservableCollection<EntityCollectionData>( Collection.ToEntityCollectionData( _sorter, _nameOverrides ) );

                    if ( _filters != null )
                    {
                        ApplyFilters( _filters );
                    }

                    UpdateStatusLabel();
                } );

                return;
            }

            ItemCollection collection = new ItemCollection( Collection.Serial );

            if ( collection.Serial == 0 )
            {
                Item[] e = ItemCollection.GetAllItems( Engine.Items.GetItems() );
                Collection.Clear();
                Collection.Add( e );
                Entities = new ObservableCollection<EntityCollectionData>( Collection.ToEntityCollectionData( _sorter, _nameOverrides ) );

                if ( _filters != null )
                {
                    ApplyFilters( _filters );
                }

                return;
            }

            Entity entity = Engine.Items.GetItem( Collection.Serial ) ?? (Entity) Engine.Mobiles.GetMobile( Collection.Serial );

            if ( entity == null )
            {
                Commands.SystemMessage( Strings.Cannot_find_item___ );
                return;
            }

            switch ( entity )
            {
                case Item item:
                    if ( item.Container == null )
                    {
                        Commands.WaitForContainerContentsUse( item.Serial, 1000 );
                    }

                    collection = item.Container;
                    break;
                case Mobile mobile:
                    collection = new ItemCollection( entity.Serial ) { mobile.GetEquippedItems() };
                    break;
            }

            if ( collection == null )
            {
                return;
            }

            Collection = !Options.ShowChildItems ? collection : new ItemCollection( collection.Serial ) { ItemCollection.GetAllItems( collection.GetItems() ) };

            Entities = new ObservableCollection<EntityCollectionData>( Collection.ToEntityCollectionData( _sorter, _nameOverrides ) );

            if ( _filters != null )
            {
                ApplyFilters( _filters );
            }
        }

        private Task ContextUseItem( object arg )
        {
            int[] items = SelectedItems.Select( i => i.Entity.Serial ).ToArray();

            EnqueueAction( async obj =>
            {
                await ActionPacketQueue.EnqueuePackets( items.Select( item => new UseObject( item ) ), cancellationToken: obj.CancellationTokenSource.Token );

                return obj.CancellationTokenSource.IsCancellationRequested;
            }, Strings.Use_item );

            return Task.CompletedTask;
        }

        private async Task ContextMoveToContainer( object arg )
        {
            int[] items = SelectedItems.Select( i => i.Entity.Serial ).ToArray();

            int serial = 0;

            if ( arg is int s )
            {
                serial = s;
            }

            if ( serial == 0 )
            {
                serial = await Commands.GetTargetSerialAsync( Strings.Target_container___ );
            }

            if ( serial == 0 )
            {
                Commands.SystemMessage( Strings.Invalid_container___ );
                return;
            }

            EnqueueAction( async obj =>
            {
                foreach ( var item in items.Select( ( value, i ) => new { i, value } ) )
                {
                    if ( obj.CancellationTokenSource.IsCancellationRequested )
                    {
                        _dispatcher.Invoke( () => { obj.Status = Strings.Cancel; } );
                        return false;
                    }

                    _dispatcher.Invoke( () => { obj.Status = string.Format( Strings.Moving_item__0_____1_, item.i, items.Length ); } );

                    int attempts;

                    for ( attempts = 0; attempts < 5; attempts++ )
                    {
                        if ( !await EnqueueDragDrop( item.value, -1, serial, obj.CancellationTokenSource.Token ) )
                        {
                            Commands.SystemMessage( $"Retrying 0x{item.value:x}..." );
                            continue;
                        }

                        break;
                    }
                }

                return true;
            }, string.Format( Strings.Moving_item__0_____1_, 0, items.Length ) );
        }

        private static Task<bool> EnqueueDragDrop( int serial, int amount, int containerSerial, CancellationToken cancellationToken = default,
            [CallerMemberName] string caller = "" )
        {
            ActionQueueItem actionQueueItem = new ActionQueueItem( param =>
            {
                if ( cancellationToken.IsCancellationRequested )
                {
                    return false;
                }

                PacketFilterInfo pfi1 = new PacketFilterInfo( 0x27 );
                PacketFilterInfo pfi2 = new PacketFilterInfo( 0x54,
                    new[]
                    {
                        PacketFilterConditions.ShortAtPositionCondition( 0x57, 2 ), PacketFilterConditions.ShortAtPositionCondition( Engine.Player.X, 6 ),
                        PacketFilterConditions.ShortAtPositionCondition( Engine.Player.Y, 8 ), PacketFilterConditions.ShortAtPositionCondition( Engine.Player.Z, 10 )
                    } );

                PacketWaitEntry pwe1 = Engine.PacketWaitEntries.Add( pfi1, PacketDirection.Incoming, true );
                PacketWaitEntry pwe2 = Engine.PacketWaitEntries.Add( pfi2, PacketDirection.Incoming, true );

                Engine.SendPacketToServer( new DragItem( serial, amount, Data.Options.CurrentOptions.DragDelay ? Data.Options.CurrentOptions.DragDelayMS : 0 ) );
                Engine.LastActionPacket = DateTime.Now;

                int result = Task.WaitAny( new Task[] { pwe1.Lock.ToTask(), pwe2.Lock.ToTask() }, 2000 );

                if ( result <= 0 )
                {
                    return false;
                }

                Engine.SendPacketToServer( new DropItem( serial, containerSerial, -1, -1, 0 ) );

                return true;
            } )
            {
                CheckRange = false,
                DelaySend = true,
                Serial = serial,
                Arguments = false,
                Caller = caller,
                Options = null
            };

            ActionPacketQueue.Enqueue( actionQueueItem, QueuePriority.High );

            return actionQueueItem.WaitHandle.ToTask( () => actionQueueItem.Result );
        }

        private async Task ContextMoveToBackpack( object arg )
        {
            await ContextMoveToContainer( Engine.Player.Backpack.Serial );
        }

        private void ToggleChildItems( object obj )
        {
            if ( !( obj is bool showChildItems ) )
            {
                return;
            }

            Options.ShowChildItems = showChildItems;

            RefreshCommand.Execute( null );
        }

        private void ToggleProperties( object obj )
        {
            if ( !( obj is bool showProperties ) )
            {
                return;
            }

            ShowProperties = showProperties;
        }

        private static void ItemDoubleClick( object obj )
        {
            if ( !( obj is EntityCollectionData ecd ) )
            {
                return;
            }

            if ( ecd.Entity is Item item && item.Container != null )
            {
                EntityCollectionViewer window = new EntityCollectionViewer { DataContext = new EntityCollectionViewerViewModel( item.Container ) };

                window.Show();
            }
            else
            {
                ObjectInspectorWindow window = new ObjectInspectorWindow { DataContext = new ObjectInspectorViewModel( ecd.Entity ) };

                window.Show();
            }
        }

        private void ContextMenuRequest( object obj )
        {
            int[] items = SelectedItems.Select( i => i.Entity.Serial ).ToArray();

            EnqueueAction( action =>
            {
                if ( action.CancellationTokenSource.IsCancellationRequested )
                {
                    return Task.FromResult( false );
                }

                foreach ( int serial in items )
                {
                    Engine.SendPacketToServer( new ContextMenuRequest( serial ) );
                }

                return Task.FromResult( true );
            }, Strings.Context_menu_request );
        }

        private void EnqueueAction( Func<QueueAction, Task<bool>> action, string message )
        {
            QueueActions.Add( new QueueAction { Action = action, CancellationTokenSource = new CancellationTokenSource(), Status = message } );
        }

        private class EquipItemData
        {
            public Layer Layer { get; set; }
            public int Serial { get; set; }
        }
    }

    public static class ExtensionMethods
    {
        public static ItemCollection Filter( this ItemCollection collection, IEnumerable<EntityCollectionFilterItem> filterList )
        {
            ItemCollection newCollection = new ItemCollection( collection.Serial );

            IEnumerable<Predicate<Item>> predicates = FiltersToPredicates( filterList.Where( f => f.Enabled ) ).ToList();

            if ( !predicates.Any() )
            {
                return collection;
            }

            foreach ( Item item in collection.GetItems() )
            {
                if ( predicates.All( p => p( item ) ) )
                {
                    newCollection.Add( item );
                }
            }

            return newCollection;
        }

        public static IEnumerable<Predicate<Item>> FiltersToPredicates( IEnumerable<EntityCollectionFilterItem> filterList )
        {
            List<Predicate<Item>> predicates = new List<Predicate<Item>>();

            foreach ( EntityCollectionFilterItem filter in filterList )
            {
                PropertyEntry constraint = filter.Constraint;

                switch ( constraint.ConstraintType )
                {
                    case PropertyType.Properties:
                    {
                        if ( filter.Operator != AutolootOperator.NotPresent )
                        {
                            predicates.Add( i => i.Properties != null && constraint.Clilocs.Any( cliloc =>
                                i.Properties.Any( p => AutolootHelpers.MatchProperty( p, cliloc, constraint, filter.Operator, filter.Value ) ) ) );
                        }
                        else
                        {
                            predicates.Add( i => i.Properties != null && !constraint.Clilocs.Any( cliloc => i.Properties.Any( p => p.Cliloc == cliloc ) ) );
                        }

                        break;
                    }
                    case PropertyType.Object:
                    {
                        predicates.Add( i =>
                            AutolootHelpers.ItemHasObjectProperty( i, constraint.Name ) && AutolootHelpers.Operation( filter.Operator,
                                AutolootHelpers.GetItemObjectPropertyValue<int>( i, constraint.Name ), filter.Value ) );
                        break;
                    }
                    case PropertyType.Predicate:
                    {
                        predicates.Add( i => constraint.Predicate != null && constraint.Predicate.Invoke( i,
                            new AutolootConstraintEntry { Operator = filter.Operator, Property = constraint, Value = filter.Value } ) );

                        break;
                    }
                    case PropertyType.PredicateWithValue:
                    {
                        predicates.Add( i => constraint.Predicate != null && constraint.Predicate.Invoke( i,
                            new AutolootConstraintEntry { Operator = filter.Operator, Property = constraint, Value = filter.Value, Additional = filter.Additional, Values = filter.Values} ) );

                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return predicates;
        }

        public static string TrimTrailingNewLine( this string s )
        {
            return s.TrimEnd( '\r', '\n' );
        }

        public static List<EntityCollectionData> ToEntityCollectionData( this ItemCollection itemCollection, IComparer<Entity> comparer, Dictionary<int, string> nameOverrides )
        {
            if ( itemCollection == null )
            {
                return new List<EntityCollectionData>();
            }

            Item[] items = itemCollection.GetItems();

            return items.OrderBy( i => i, comparer ).Select( item => item.ToEntityCollectionData( nameOverrides ) ).ToList();
        }

        public static EntityCollectionData ToEntityCollectionData( this Item item, Dictionary<int, string> nameOverrides )
        {
            StaticTile tileData = TileData.GetStaticTile( item.ID );

            if ( string.IsNullOrEmpty( item.Name ) )
            {
                item.Name = nameOverrides.TryGetValue( item.Serial, out string @override ) ? @override : tileData.ID != 0 ? tileData.Name : $"0x{item.Serial:x8}";
            }

            if ( nameOverrides.TryGetValue( item.Serial, out string nameOverride ) )
            {
                item.Name = nameOverride;
            }

            if ( Engine.RehueList.TryGetValue( item.Serial, out RehueEntry entry ) )
            {
                item.Hue = entry.Hue;
            }

            return new EntityCollectionData { Entity = item };
        }
    }

    public class QueueAction : SetPropertyNotifyChanged
    {
        private ICommand _cancelCommand;
        private string _status;
        public Func<QueueAction, Task<bool>> Action { get; set; }

        public ICommand CancelCommand => _cancelCommand ?? ( _cancelCommand = new RelayCommand( Cancel, o => true ) );

        public CancellationTokenSource CancellationTokenSource { get; set; }

        public string Status
        {
            get => _status;
            set => SetProperty( ref _status, value );
        }

        private void Cancel( object obj )
        {
            CancellationTokenSource.Cancel();
            Status = Strings.Cancel;
        }
    }
}