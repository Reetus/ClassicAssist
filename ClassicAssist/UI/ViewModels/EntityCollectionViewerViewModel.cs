using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json;

namespace ClassicAssist.UI.ViewModels
{
    public class EntityCollectionViewerViewModel : BaseViewModel
    {
        private ICommand _applyFiltersCommand;
        private ICommand _changeSortStyleCommand;
        private ItemCollection _collection;
        private ICommand _combineStacksCommand;
        private ICommand _contextContextMenuRequestCommand;
        private ICommand _contextMoveToBackpackCommand;
        private ICommand _contextMoveToContainerCommand;
        private ICommand _contextTargetCommand;
        private ICommand _contextUseItemCommand;
        private ObservableCollection<EntityCollectionData> _entities;
        private ICommand _equipItemCommand;
        private IEnumerable<EntityCollectionFilter> _filters;
        private ICommand _hideItemCommand;
        private ICommand _itemDoubleClickCommand;
        private ICommand _openAllContainersCommand;
        private EntityCollectionViewerOptions _options;
        private ICommand _refreshCommand;
        private ICommand _replaceNameCommand;

        private ObservableCollection<EntityCollectionData> _selectedItems =
            new ObservableCollection<EntityCollectionData>();

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
            _collection = new ItemCollection( 0 );

            for ( int i = 0; i < 5; i++ )
            {
                _collection.Add( new Item( i + 1 ) { ID = 100 + i } );
            }

            _collection.Add( new Item( 6 ) { ID = 106, Hue = 2413 } );

            Entities = new ObservableCollection<EntityCollectionData>(
                _collection.ToEntityCollectionData( _sorter, _nameOverrides ) );
        }

        public EntityCollectionViewerViewModel( ItemCollection collection )
        {
            _collection = collection;
            Options = Data.Options.CurrentOptions.EntityCollectionViewerOptions;

            Entities = new ObservableCollection<EntityCollectionData>( !Options.ShowChildItems
                ? collection.ToEntityCollectionData( _sorter, _nameOverrides )
                : new ItemCollection( collection.Serial ) { ItemCollection.GetAllItems( collection.GetItems() ) }
                    .ToEntityCollectionData( _sorter, _nameOverrides ) );

            SelectedItems.CollectionChanged += ( sender, args ) =>
            {
                if ( !( sender is ObservableCollection<EntityCollectionData> ) )
                {
                    return;
                }

                UpdateStatusLabel();
            };

            UpdateStatusLabel();

            _collection.CollectionChanged += OnCollectionChanged;

            TooltipsEnabled = Engine.CharacterListFlags.HasFlag( CharacterListFlags.PaladinNecromancerClassTooltips );

            ThreadQueue = new ThreadPriorityQueue<QueueAction>( ProcessQueue );
            QueueActions.CollectionChanged += QueueActions_CollectionChanged;
        }

        public ICommand ApplyFiltersCommand =>
            _applyFiltersCommand ?? ( _applyFiltersCommand = new RelayCommand( ApplyFilters, o => true ) );

        public ICommand ChangeSortStyleCommand =>
            _changeSortStyleCommand ?? ( _changeSortStyleCommand = new RelayCommand( ChangeSortStyle, o => true ) );

        public ICommand CombineStacksCommand =>
            _combineStacksCommand ?? ( _combineStacksCommand =
                new RelayCommandAsync( CombineStacks, o => _collection.Serial > 0 ) );

        public ICommand ContextContextMenuRequestCommand =>
            _contextContextMenuRequestCommand ?? ( _contextContextMenuRequestCommand =
                new RelayCommand( ContextMenuRequest, o => SelectedItems.Count > 0 ) );

        public ICommand ContextMoveToBackpackCommand =>
            _contextMoveToBackpackCommand ?? ( _contextMoveToBackpackCommand =
                new RelayCommandAsync( ContextMoveToBackpack, o => SelectedItems != null ) );

        public ICommand ContextMoveToContainerCommand =>
            _contextMoveToContainerCommand ?? ( _contextMoveToContainerCommand =
                new RelayCommandAsync( ContextMoveToContainer, o => SelectedItems != null ) );

        public ICommand ContextTargetCommand =>
            _contextTargetCommand ?? ( _contextTargetCommand = new RelayCommand( ContextTarget,
                o => Engine.TargetExists ) );

        public ICommand ContextUseItemCommand =>
            _contextUseItemCommand ?? ( _contextUseItemCommand =
                new RelayCommandAsync( ContextUseItem, o => SelectedItems != null ) );

        public ObservableCollection<EntityCollectionData> Entities
        {
            get => _entities;
            set => SetProperty( ref _entities, value );
        }

        public ICommand EquipItemCommand =>
            _equipItemCommand ?? ( _equipItemCommand = new RelayCommandAsync( EquipItem, o => SelectedItems != null ) );

        public ICommand HideItemCommand =>
            _hideItemCommand ?? ( _hideItemCommand = new RelayCommand( HideItem, o => SelectedItems != null ) );

        public ICommand ItemDoubleClickCommand =>
            _itemDoubleClickCommand ?? ( _itemDoubleClickCommand = new RelayCommand( ItemDoubleClick, o => true ) );

        public static Lazy<Dictionary<int, int>> MountIDEntries { get; set; } =
            new Lazy<Dictionary<int, int>>( LoadMountIDEntries );

        public ICommand OpenAllContainersCommand =>
            _openAllContainersCommand ??
            ( _openAllContainersCommand = new RelayCommand( OpenAllContainers, o => true ) );

        public EntityCollectionViewerOptions Options
        {
            get => _options;
            set => SetProperty( ref _options, value );
        }

        public ObservableCollectionEx<QueueAction> QueueActions { get; set; } =
            new ObservableCollectionEx<QueueAction>();

        public ICommand RefreshCommand =>
            _refreshCommand ?? ( _refreshCommand = new RelayCommand( Refresh, o => true ) );

        public ICommand ReplaceNameCommand =>
            _replaceNameCommand ?? ( _replaceNameCommand = new RelayCommandAsync( ReplaceName, o => true ) );

        public ObservableCollection<EntityCollectionData> SelectedItems
        {
            get => _selectedItems;
            set => SetProperty( ref _selectedItems, value );
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

        public ICommand TargetContainerCommand =>
            _targetContainerCommand ?? ( _targetContainerCommand =
                new RelayCommand( TargetContainer, o => _collection.Serial > 0 ) );

        public ThreadPriorityQueue<QueueAction> ThreadQueue { get; set; }

        public ICommand ToggleAlwaysOnTopCommand =>
            _toggleAlwaysOnTopCommand ??
            ( _toggleAlwaysOnTopCommand = new RelayCommand( ToggleAlwaysOnTop, o => true ) );

        public ICommand ToggleChildItemsCommand =>
            _toggleChildItemsCommand ?? ( _toggleChildItemsCommand = new RelayCommand( ToggleChildItems, o => true ) );

        public ICommand TogglePropertiesCommand =>
            _togglePropertiesCommand ?? ( _togglePropertiesCommand = new RelayCommand( ToggleProperties, o => true ) );

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
                        Item destStack = _collection.SelectEntity( i =>
                            i.Count < 60000 && TileData.GetStaticTile( i.ID ).Flags.HasFlag( TileFlags.Stackable ) &&
                            !ignoreList.Contains( i.Serial ) );

                        if ( destStack == null )
                        {
                            return false;
                        }

                        int needed = 60000 - destStack.Count;

                        Item sourceStack = _collection.SelectEntities( i =>
                            i.ID == destStack.ID && i.Hue == destStack.Hue && i.Serial != destStack.Serial &&
                            i.Count != 60000 )?.OrderBy( i => i.Count ).FirstOrDefault();

                        if ( sourceStack == null )
                        {
                            ignoreList.Add( destStack.Serial );
                            continue;
                        }

                        await ActionPacketQueue.EnqueueDragDrop( sourceStack.Serial,
                            needed > sourceStack.Count ? sourceStack.Count : needed, destStack.Serial,
                            QueuePriority.Low, false, true, false );

                        await Task.Delay( TimeSpan.FromMilliseconds( Data.Options.CurrentOptions.ActionDelayMS ),
                            action.CancellationTokenSource.Token );

                        action.CancellationTokenSource.Token.ThrowIfCancellationRequested();
                    }
                }
                catch ( TaskCanceledException )
                {
                }

                return !action.CancellationTokenSource.IsCancellationRequested;
            }, Strings.Combine_stacks );

            return Task.CompletedTask;
        }

        private void TargetContainer( object obj )
        {
            TargetCommands.Target( _collection.Serial );
        }

        private void OnCollectionChanged( int totalcount, bool added, Item[] entities )
        {
            if ( added )
            {
                _dispatcher.Invoke( () =>
                {
                    foreach ( Item entity in entities.Where( e => !Entities.Any( f => f.Entity.Equals( e ) ) ).ToList()
                                 .Where( entity => Options.ShowChildItems || entity.Owner == _collection.Serial ) )
                    {
                        Entities.Add( entity.ToEntityCollectionData( _nameOverrides ) );
                    }
                } );
            }
            else
            {
                _dispatcher.Invoke( () =>
                {
                    foreach ( EntityCollectionData ecd in entities.ToList()
                                 .Select( item => Entities.FirstOrDefault( e => e.Entity.Equals( item ) ) )
                                 .Where( ecd => ecd != null ) )
                    {
                        Entities.Remove( ecd );
                    }
                } );
            }

            UpdateStatusLabel();
        }

        private void ContextTarget( object obj )
        {
            Entity item = SelectedItems.FirstOrDefault()?.Entity;

            if ( item == null )
            {
                return;
            }

            TargetCommands.Target( item.Serial );
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
            Data.Options.CurrentOptions.EntityCollectionViewerOptions = Options;
            _collection.CollectionChanged -= OnCollectionChanged;
            ThreadQueue?.Dispose();
        }

        private void OpenAllContainers( object obj )
        {
            EnqueueAction( async action =>
            {
                List<Task> tasks = _collection
                    .Where( i => TileData.GetStaticTile( i.ID ).Flags.HasFlag( TileFlags.Container ) ).Select( item =>
                        ActionPacketQueue.EnqueuePacket( new UseObject( item.Serial ),
                            cancellationToken: action.CancellationTokenSource.Token ) ).ToList();

                await Task.WhenAll( tasks ).ContinueWith( t =>
                {
                    Thread.Sleep( 1000 );
                    RefreshCommand.Execute( null );
                } );

                return true;
            }, Strings.Open_All_Containers );
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
                IEnumerable<EquipItemData> data = SelectedItems.Select( e =>
                        new EquipItemData { Serial = e.Entity.Serial, Layer = TileData.GetLayer( e.Entity.ID ) } )
                    .Where( e => e.Layer != Layer.Invalid );

                IEnumerable<Task<bool>> tasks = data.Select( e => ActionPacketQueue.EnqueueAction( e, arg =>
                {
                    Engine.SendPacketToServer( new DragItem( e.Serial, 1 ) );
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
            if ( !( obj is IEnumerable<EntityCollectionFilter> list ) )
            {
                return;
            }

            _filters = list;

            Entities.Clear();

            Entities = new ObservableCollection<EntityCollectionData>( _collection.Filter( _filters )
                .ToEntityCollectionData( _sorter, _nameOverrides ) );

            UpdateStatusLabel();
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

            Entities = new ObservableCollection<EntityCollectionData>(
                _collection.ToEntityCollectionData( _sorter, _nameOverrides ) );
        }

        private void UpdateStatusLabel()
        {
            StatusLabel = string.Format( Strings._0__items___1__selected___2__total_amount, Entities.Count,
                SelectedItems?.Count ?? 0,
                SelectedItems?.Select( i => i.Entity ).Where( i => i is Item ).Cast<Item>().Sum( i => i.Count ) );
        }

        private void Refresh( object obj )
        {
            ItemCollection collection = new ItemCollection( _collection.Serial );

            if ( collection.Serial == 0 )
            {
                Item[] e = ItemCollection.GetAllItems( Engine.Items.GetItems() );
                _collection.Clear();
                _collection.Add( e );
                Entities = new ObservableCollection<EntityCollectionData>(
                    _collection.ToEntityCollectionData( _sorter, _nameOverrides ) );
                return;
            }

            Entity entity = Engine.Items.GetItem( _collection.Serial ) ??
                            (Entity) Engine.Mobiles.GetMobile( _collection.Serial );

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

            _collection = !Options.ShowChildItems
                ? collection
                : new ItemCollection( collection.Serial ) { ItemCollection.GetAllItems( collection.GetItems() ) };

            Entities = new ObservableCollection<EntityCollectionData>(
                _collection.ToEntityCollectionData( _sorter, _nameOverrides ) );
        }

        private Task ContextUseItem( object arg )
        {
            int[] items = SelectedItems.Select( i => i.Entity.Serial ).ToArray();

            EnqueueAction( async obj =>
            {
                await ActionPacketQueue.EnqueuePackets( items.Select( item => new UseObject( item ) ),
                    cancellationToken: obj.CancellationTokenSource.Token );

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

                    _dispatcher.Invoke( () =>
                    {
                        obj.Status = string.Format( Strings.Moving_item__0_____1_, item.i, items.Length );
                    } );

                    await ActionPacketQueue.EnqueueDragDrop( item.value, -1, serial,
                        cancellationToken: obj.CancellationTokenSource.Token );
                }

                return true;
            }, string.Format( Strings.Moving_item__0_____1_, 0, items.Length ) );
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
                EntityCollectionViewer window = new EntityCollectionViewer
                {
                    DataContext = new EntityCollectionViewerViewModel( item.Container )
                };

                window.Show();
            }
            else
            {
                ObjectInspectorWindow window =
                    new ObjectInspectorWindow { DataContext = new ObjectInspectorViewModel( ecd.Entity ) };

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
            QueueActions.Add( new QueueAction
            {
                Action = action, CancellationTokenSource = new CancellationTokenSource(), Status = message
            } );
        }

        private class EquipItemData
        {
            public Layer Layer { get; set; }
            public int Serial { get; set; }
        }
    }

    public static class ExtensionMethods
    {
        public static ItemCollection Filter( this ItemCollection collection,
            IEnumerable<EntityCollectionFilter> filterList )
        {
            ItemCollection newCollection = new ItemCollection( collection.Serial );

            IEnumerable<Predicate<Item>> predicates = FiltersToPredicates( filterList ).ToList();

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

        private static IEnumerable<Predicate<Item>> FiltersToPredicates(
            IEnumerable<EntityCollectionFilter> filterList )
        {
            List<Predicate<Item>> predicates = new List<Predicate<Item>>();

            foreach ( EntityCollectionFilter filter in filterList )
            {
                PropertyEntry constraint = filter.Constraint;

                switch ( constraint.ConstraintType )
                {
                    case PropertyType.Properties:
                    {
                        if ( filter.Operator != AutolootOperator.NotPresent )
                        {
                            predicates.Add( i => i.Properties != null && constraint.Clilocs.Any( cliloc =>
                                i.Properties.Any( p => AutolootHelpers.MatchProperty( p, cliloc,
                                    constraint, filter.Operator, filter.Value ) ) ) );
                        }
                        else
                        {
                            predicates.Add( i =>
                                i.Properties != null && !constraint.Clilocs.Any( cliloc =>
                                    i.Properties.Any( p => p.Cliloc == cliloc ) ) );
                        }

                        break;
                    }
                    case PropertyType.Object:
                    {
                        predicates.Add( i =>
                            AutolootHelpers.ItemHasObjectProperty( i, constraint.Name ) && AutolootHelpers.Operation(
                                filter.Operator, AutolootHelpers.GetItemObjectPropertyValue<int>( i, constraint.Name ),
                                filter.Value ) );
                        break;
                    }
                    case PropertyType.Predicate:
                    {
                        predicates.Add( i => constraint.Predicate != null && constraint.Predicate.Invoke( i,
                            new AutolootConstraintEntry
                            {
                                Operator = filter.Operator, Property = constraint, Value = filter.Value
                            } ) );

                        break;
                    }
                    case PropertyType.PredicateWithValue:
                    {
                        predicates.Add( i => constraint.Predicate != null && constraint.Predicate.Invoke( i,
                            new AutolootConstraintEntry
                            {
                                Operator = filter.Operator,
                                Property = constraint,
                                Value = filter.Value,
                                Additional = filter.Additional
                            } ) );

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

        public static List<EntityCollectionData> ToEntityCollectionData( this ItemCollection itemCollection,
            IComparer<Entity> comparer, Dictionary<int, string> nameOverrides )
        {
            if ( itemCollection == null )
            {
                return new List<EntityCollectionData>();
            }

            Item[] items = itemCollection.GetItems();

            return items.OrderBy( i => i, comparer ).Select( item => item.ToEntityCollectionData( nameOverrides ) )
                .ToList();
        }

        public static EntityCollectionData ToEntityCollectionData( this Item item,
            Dictionary<int, string> nameOverrides )
        {
            StaticTile tileData = TileData.GetStaticTile( item.ID );

            if ( string.IsNullOrEmpty( item.Name ) )
            {
                item.Name = nameOverrides.ContainsKey( item.Serial ) ? nameOverrides[item.Serial] :
                    tileData.ID != 0 ? tileData.Name : $"0x{item.Serial:x8}";
            }

            if ( nameOverrides.ContainsKey( item.Serial ) )
            {
                item.Name = nameOverrides[item.Serial];
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