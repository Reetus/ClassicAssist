using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Assistant;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.Data.Misc;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
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
        private ICommand _cancelActionCommand;
        private CancellationTokenSource _cancellationToken;
        private ICommand _changeSortStyleCommand;
        private ItemCollection _collection;
        private ICommand _contextContextMenuRequestCommand;
        private ICommand _contextMoveToBackpackCommand;
        private ICommand _contextMoveToContainerCommand;
        private ICommand _contextUseItemCommand;
        private ObservableCollection<EntityCollectionData> _entities;
        private ICommand _equipItemCommand;
        private IEnumerable<EntityCollectionFilter> _filters;
        private ICommand _hideItemCommand;
        private bool _isPerformingAction;
        private ICommand _itemDoubleClickCommand;
        private ICommand _openAllContainersCommand;
        private EntityCollectionViewerOptions _options;
        private ICommand _refreshCommand;

        private ObservableCollection<EntityCollectionData> _selectedItems =
            new ObservableCollection<EntityCollectionData>();

        private bool _showProperties;

        private IComparer<Entity> _sorter = new IDThenSerialComparer();
        private string _statusLabel;
        private ICommand _toggleAlwaysOnTopCommand;
        private ICommand _toggleChildItemsCommand;
        private ICommand _togglePropertiesCommand;
        private bool _topmost;

        public EntityCollectionViewerViewModel()
        {
            _collection = new ItemCollection( 0 );

            for ( int i = 0; i < 5; i++ )
            {
                _collection.Add( new Item( i + 1 ) { ID = 100 + i } );
            }

            _collection.Add( new Item( 6 ) { ID = 106, Hue = 2413 } );

            Entities = new ObservableCollection<EntityCollectionData>( _collection.ToEntityCollectionData( _sorter ) );
        }

        public EntityCollectionViewerViewModel( ItemCollection collection )
        {
            _collection = collection;
            Options = Data.Options.CurrentOptions.EntityCollectionViewerOptions;

            Entities = new ObservableCollection<EntityCollectionData>( !Options.ShowChildItems
                ? collection.ToEntityCollectionData( _sorter )
                : new ItemCollection( collection.Serial ) { ItemCollection.GetAllItems( collection.GetItems() ) }
                    .ToEntityCollectionData( _sorter ) );

            SelectedItems.CollectionChanged += ( sender, args ) =>
            {
                if ( !( sender is ObservableCollection<EntityCollectionData> ) )
                {
                    return;
                }

                UpdateStatusLabel();
            };

            UpdateStatusLabel();
        }

        public ICommand ApplyFiltersCommand =>
            _applyFiltersCommand ?? ( _applyFiltersCommand = new RelayCommand( ApplyFilters, o => true ) );

        public ICommand CancelActionCommand =>
            _cancelActionCommand ?? ( _cancelActionCommand = new RelayCommandAsync( CancelAction,
                o => _cancellationToken != null ) );

        public ICommand ChangeSortStyleCommand =>
            _changeSortStyleCommand ?? ( _changeSortStyleCommand = new RelayCommand( ChangeSortStyle, o => true ) );

        public ICommand ContextContextMenuRequestCommand =>
            _contextContextMenuRequestCommand ?? ( _contextContextMenuRequestCommand =
                new RelayCommand( ContextMenuRequest, o => SelectedItems.Count > 0 ) );

        public ICommand ContextMoveToBackpackCommand =>
            _contextMoveToBackpackCommand ?? ( _contextMoveToBackpackCommand =
                new RelayCommandAsync( ContextMoveToBackpack, o => SelectedItems != null && !IsPerformingAction ) );

        public ICommand ContextMoveToContainerCommand =>
            _contextMoveToContainerCommand ?? ( _contextMoveToContainerCommand =
                new RelayCommandAsync( ContextMoveToContainer, o => SelectedItems != null && !IsPerformingAction ) );

        public ICommand ContextUseItemCommand =>
            _contextUseItemCommand ?? ( _contextUseItemCommand =
                new RelayCommandAsync( ContextUseItem, o => SelectedItems != null && !IsPerformingAction ) );

        public ObservableCollection<EntityCollectionData> Entities
        {
            get => _entities;
            set => SetProperty( ref _entities, value );
        }

        public ICommand EquipItemCommand =>
            _equipItemCommand ?? ( _equipItemCommand = new RelayCommandAsync( EquipItem, o => SelectedItems != null ) );

        public ICommand HideItemCommand =>
            _hideItemCommand ?? ( _hideItemCommand = new RelayCommand( HideItem, o => SelectedItems != null ) );

        public bool IsPerformingAction
        {
            get => _isPerformingAction;
            set => SetProperty( ref _isPerformingAction, value );
        }

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

        public ICommand RefreshCommand =>
            _refreshCommand ?? ( _refreshCommand = new RelayCommand( Refresh, o => _collection?.Serial != 0 ) );

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

        public static Lazy<int[]> ShrinkEntries { get; set; } = new Lazy<int[]>( LoadShrinkTable );

        public string StatusLabel
        {
            get => _statusLabel;
            set => SetProperty( ref _statusLabel, value );
        }

        public ICommand ToggleAlwaysOnTopCommand =>
            _toggleAlwaysOnTopCommand ??
            ( _toggleAlwaysOnTopCommand = new RelayCommand( ToggleAlwaysOnTop, o => true ) );

        public ICommand ToggleChildItemsCommand =>
            _toggleChildItemsCommand ?? ( _toggleChildItemsCommand = new RelayCommand( ToggleChildItems, o => true ) );

        public ICommand TogglePropertiesCommand =>
            _togglePropertiesCommand ?? ( _togglePropertiesCommand = new RelayCommand( ToggleProperties, o => true ) );

        public bool Topmost
        {
            get => _topmost;
            set => SetProperty( ref _topmost, value );
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
        }

        private void ToggleAlwaysOnTop( object obj )
        {
            if ( !( obj is bool alwaysOnTop ) )
            {
                return;
            }

            Options.AlwaysOnTop = alwaysOnTop;
        }

        private void OpenAllContainers( object obj )
        {
            Task.Run( () =>
            {
                IsPerformingAction = true;
                _cancellationToken = new CancellationTokenSource();

                List<Task> tasks = _collection
                    .Where( i => TileData.GetStaticTile( i.ID ).Flags.HasFlag( TileFlags.Container ) ).Select( item =>
                        ActionPacketQueue.EnqueuePacket( new UseObject( item.Serial ),
                            cancellationToken: _cancellationToken.Token ) ).ToList();

                Task.WhenAll( tasks ).ContinueWith( t =>
                {
                    Thread.Sleep( 1000 );
                    RefreshCommand.Execute( null );
                    IsPerformingAction = false;
                } );
            } );
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

        private static int[] LoadShrinkTable()
        {
            int[] table = new int[ushort.MaxValue];

            using ( StringReader reader = new StringReader( Properties.Resources.Shrink ) )
            {
                string line;

                while ( ( line = reader.ReadLine() ) != null )
                {
                    if ( line.Length == 0 || line.StartsWith( "#" ) )
                    {
                        continue;
                    }

                    string[] split = line.Split( '\t' );

                    if ( split.Length < 2 )
                    {
                        continue;
                    }

                    int body = Convert.ToInt32( split[0] );
                    int item = Convert.ToInt32( split[1], 16 );

                    if ( body >= 0 && body < table.Length )
                    {
                        table[body] = item;
                    }
                }
            }

            return table;
        }

        private async Task EquipItem( object obj )
        {
            IsPerformingAction = true;
            _cancellationToken = new CancellationTokenSource();

            IEnumerable<EquipItemData> data = SelectedItems.Select( e =>
                    new EquipItemData { Serial = e.Entity.Serial, Layer = TileData.GetLayer( e.Entity.ID ) } )
                .Where( e => e.Layer != Layer.Invalid );

            IEnumerable<Task<bool>> tasks = data.Select( e => ActionPacketQueue.EnqueueAction( e, arg =>
            {
                Engine.SendPacketToServer( new DragItem( e.Serial, 1 ) );
                Thread.Sleep( 50 );
                Engine.SendPacketToServer( new EquipRequest( e.Serial, e.Layer, Engine.Player?.Serial ?? 0 ) );

                return true;
            }, cancellationToken: _cancellationToken.Token ) );

            await Task.WhenAll( tasks );

            IsPerformingAction = false;
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
                .ToEntityCollectionData( _sorter ) );

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

            Entities = new ObservableCollection<EntityCollectionData>( _collection.ToEntityCollectionData( _sorter ) );
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

            Entities = new ObservableCollection<EntityCollectionData>( _collection.ToEntityCollectionData( _sorter ) );
        }

        private async Task ContextUseItem( object arg )
        {
            _cancellationToken = new CancellationTokenSource();

            await Task.Run( () =>
            {
                IsPerformingAction = true;

                int[] items = SelectedItems.Select( i => i.Entity.Serial ).ToArray();

                ActionPacketQueue.EnqueuePackets( items.Select( item => new UseObject( item ) ),
                    cancellationToken: _cancellationToken.Token ).ContinueWith( t => { IsPerformingAction = false; } );
            } );
        }

        private async Task ContextMoveToContainer( object arg )
        {
            _cancellationToken = new CancellationTokenSource();

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

            try
            {
                IsPerformingAction = true;

                int[] items = SelectedItems.Select( i => i.Entity.Serial ).ToArray();

                foreach ( int item in items )
                {
                    await ActionPacketQueue.EnqueueDragDrop( item, -1, serial,
                        cancellationToken: _cancellationToken.Token );
                }
            }
            finally
            {
                IsPerformingAction = false;
            }
        }

        private async Task ContextMoveToBackpack( object arg )
        {
            await ContextMoveToContainer( Engine.Player.Backpack.Serial );
        }

        private async Task CancelAction( object arg )
        {
            _cancellationToken?.Cancel();

            await Task.CompletedTask;
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

                window.ShowDialog();
            }
            else
            {
                ObjectInspectorWindow window =
                    new ObjectInspectorWindow { DataContext = new ObjectInspectorViewModel( ecd.Entity ) };

                window.ShowDialog();
            }
        }

        private void ContextMenuRequest( object obj )
        {
            int[] items = SelectedItems.Select( i => i.Entity.Serial ).ToArray();

            foreach ( int serial in items )
            {
                Engine.SendPacketToServer( new ContextMenuRequest( serial ) );
            }
        }

        private class EquipItemData
        {
            public Layer Layer { get; set; }
            public int Serial { get; set; }
        }
    }

    public class EntityCollectionData
    {
        public BitmapSource Bitmap
        {
            get
            {
                BitmapSource result = Art.GetStatic( Entity.ID, Entity.Hue ).ToBitmapSource();

                if ( !( Entity is Item item ) || item.Layer != Layer.Mount )
                {
                    return result;
                }

                if ( !( EntityCollectionViewerViewModel.MountIDEntries.Value?.ContainsKey( Entity.ID ) ?? false ) )
                {
                    return result;
                }

                int id = (int) EntityCollectionViewerViewModel.MountIDEntries.Value?[Entity.ID];

                result = Art.GetStatic( id, Entity.Hue ).ToBitmapSource();

                return result;
            }
        }

        public Entity Entity { get; set; }
        public string FullName => GetProperties( Entity );
        public string Name => GetName( Entity );

        private static string GetProperties( Entity entity )
        {
            return entity.Properties == null
                ? GetName( entity )
                : entity.Properties.Aggregate( "",
                    ( current, entityProperty ) => current + entityProperty.Text + "\r\n" ).TrimTrailingNewLine();
        }

        private static string GetName( Entity entity )
        {
            if ( !( entity is Item item ) || item.Layer != Layer.Mount )
            {
                return entity.Name;
            }

            if ( !( EntityCollectionViewerViewModel.MountIDEntries.Value?.ContainsKey( entity.ID ) ?? false ) )
            {
                return entity.Name;
            }

            int id = (int) EntityCollectionViewerViewModel.MountIDEntries.Value?[entity.ID];

            if ( id == 0 )
            {
                return entity.Name;
            }

            StaticTile tileData = TileData.GetStaticTile( id );

            return !string.IsNullOrEmpty( tileData.Name ) ? tileData.Name : entity.Name;
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
                        predicates.Add( i => i.Properties != null && constraint.Clilocs.Any( cliloc =>
                                                 i.Properties.Any( p => AutolootHelpers.MatchProperty( p, cliloc,
                                                     constraint, filter.Operator, filter.Value ) ) ) );

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
            IComparer<Entity> comparer )
        {
            if ( itemCollection == null )
            {
                return new List<EntityCollectionData>();
            }

            Item[] items = itemCollection.GetItems();

            IEnumerable<Item> noNames = items.Where( i => string.IsNullOrEmpty( i.Name ) );

            foreach ( Item item in noNames )
            {
                item.Name = $"0x{item.Serial:x8}";
            }

            return items.OrderBy( i => i, comparer ).Select( item => new EntityCollectionData { Entity = item } )
                .ToList();
        }
    }
}