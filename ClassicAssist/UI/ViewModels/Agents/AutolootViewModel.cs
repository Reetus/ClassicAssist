using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.Data.Regions;
using ClassicAssist.Data.Targeting;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Misc;
using ClassicAssist.UI.Views;
using ClassicAssist.UI.Views.Autoloot;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using DraggableTreeView;
using Microsoft.Scripting.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.UI.ViewModels.Agents
{
    public class AutolootViewModel : BaseViewModel, ISettingProvider
    {
        private const int LOOT_TIMEOUT = 5000;
        private readonly object _autolootLock = new object();

        private readonly string _propertiesFile = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory,
            "Data", "Properties.json" );

        private readonly string _propertiesFileCustom =
            Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Data", "Properties.Custom.json" );

        private ICommand _clipboardCopyCommand;
        private ICommand _clipboardPasteCommand;

        private ObservableCollection<PropertyEntry> _constraints = new ObservableCollection<PropertyEntry>();

        private int _containerSerial;
        private ICommand _defineCustomPropertiesCommand;

        private bool _disableInGuardzone;
        private ObservableCollection<IDraggable> _draggables = new ObservableCollection<IDraggable>();
        private bool _enabled;

        private ICommand _insertCommand;
        private ICommand _insertConstraintCommand;
        private ICommand _insertMatchAnyCommand;

        private ObservableCollectionEx<AutolootEntry> _items = new ObservableCollectionEx<AutolootEntry>();
        private double _leftColumnWidth = 200;
        private bool _lootHumanoids;
        private ICommand _newGroupCommand;

        private ICommand _removeCommand;
        private ICommand _removeConstraintCommand;
        private ICommand _removeGroupCommand;
        private bool _requeueFailedItems;
        private RelayCommand _resetContainerCommand;
        private AutolootGroup _selectedGroup;
        private AutolootEntry _selectedItem;

        private ObservableCollection<AutolootConstraintEntry> _selectedProperties =
            new ObservableCollection<AutolootConstraintEntry>();

        private AutolootConstraintEntry _selectedProperty;
        private ICommand _selectHueCommand;
        private ICommand _setContainerCommand;

        public AutolootViewModel()
        {
            if ( !File.Exists( _propertiesFile ) )
            {
                return;
            }

            LoadProperties();
            LoadCustomProperties();

            AutolootHelpers.SetAutolootContainer = serial => ContainerSerial = serial;
            IncomingPacketHandlers.CorpseContainerDisplayEvent += OnCorpseEvent;
            AutolootManager.GetInstance().GetEntries = () => _items.ToList();
            Items.CollectionChanged += UpdateDraggables;
        }

        public ICommand ClipboardCopyCommand =>
            _clipboardCopyCommand ?? ( _clipboardCopyCommand = new RelayCommand( ClipboardCopy, o => true ) );

        public ICommand ClipboardPasteCommand =>
            _clipboardPasteCommand ?? ( _clipboardPasteCommand = new RelayCommand( ClipboardPaste, o => true ) );

        public ObservableCollection<PropertyEntry> Constraints
        {
            get => _constraints;
            set => SetProperty( ref _constraints, value );
        }

        public int ContainerSerial
        {
            get => _containerSerial;
            set => SetProperty( ref _containerSerial, value );
        }

        public ICommand DefineCustomPropertiesCommand =>
            _defineCustomPropertiesCommand ??
            ( _defineCustomPropertiesCommand = new RelayCommand( DefineCustomProperties, o => true ) );

        public bool DisableInGuardzone
        {
            get => _disableInGuardzone;
            set => SetProperty( ref _disableInGuardzone, value );
        }

        public ObservableCollection<IDraggable> Draggables
        {
            get => _draggables;
            set => SetProperty( ref _draggables, value );
        }

        public bool Enabled
        {
            get => _enabled;
            set => SetProperty( ref _enabled, value );
        }

        public ICommand InsertCommand =>
            _insertCommand ?? ( _insertCommand = new RelayCommandAsync( Insert, o => /*Engine.Connected*/true ) );

        public ICommand InsertConstraintCommand =>
            _insertConstraintCommand ?? ( _insertConstraintCommand =
                new RelayCommand( InsertConstraint, o => SelectedItem != null ) );

        public ICommand InsertMatchAnyCommand =>
            _insertMatchAnyCommand ?? ( _insertMatchAnyCommand = new RelayCommand( InsertMatchAny, o => true ) );

        public ObservableCollectionEx<AutolootEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public double LeftColumnWidth
        {
            get => _leftColumnWidth;
            set => SetProperty( ref _leftColumnWidth, value );
        }

        public bool LootHumanoids
        {
            get => _lootHumanoids;
            set => SetProperty( ref _lootHumanoids, value );
        }

        public ICommand NewGroupCommand =>
            _newGroupCommand ?? ( _newGroupCommand = new RelayCommand( NewGroup, o => true ) );

        public ICommand RemoveCommand =>
            _removeCommand ?? ( _removeCommand = new RelayCommandAsync( Remove, o => SelectedItem != null ) );

        public ICommand RemoveConstraintCommand =>
            _removeConstraintCommand ?? ( _removeConstraintCommand =
                new RelayCommand( RemoveConstraint, o => SelectedProperty != null ) );

        public ICommand RemoveGroupCommand =>
            _removeGroupCommand ?? ( _removeGroupCommand = new RelayCommand( RemoveGroup, o => o is IDraggableGroup ) );

        public bool RequeueFailedItems
        {
            get => _requeueFailedItems;
            set => SetProperty( ref _requeueFailedItems, value );
        }

        public ICommand ResetContainerCommand =>
            _resetContainerCommand ?? ( _resetContainerCommand = new RelayCommand( ResetContainer, o => true ) );

        public AutolootGroup SelectedGroup
        {
            get => _selectedGroup;
            set => SetProperty( ref _selectedGroup, value );
        }

        public AutolootEntry SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        public ObservableCollection<AutolootConstraintEntry> SelectedProperties
        {
            get => _selectedProperties;
            set => SetProperty( ref _selectedProperties, value );
        }

        public AutolootConstraintEntry SelectedProperty
        {
            get => _selectedProperty;
            set => SetProperty( ref _selectedProperty, value );
        }

        public ICommand SelectHueCommand =>
            _selectHueCommand ?? ( _selectHueCommand = new RelayCommand( SelectHue, o => SelectedItem != null ) );

        public ICommand SetContainerCommand =>
            _setContainerCommand ?? ( _setContainerCommand = new RelayCommandAsync( SetContainer, o => true ) );

        public void Serialize( JObject json )
        {
            if ( json == null )
            {
                return;
            }

            JArray groupArray = new JArray();

            foreach ( AutolootGroup draggableGroup in Draggables.Where( i => i is AutolootGroup )
                .Cast<AutolootGroup>() )
            {
                JObject entry = new JObject { { "Name", draggableGroup.Name }, { "Enabled", draggableGroup.Enabled } };

                groupArray.Add( entry );
            }

            JObject autolootObj = new JObject
            {
                { "Enabled", Enabled },
                { "DisableInGuardzone", DisableInGuardzone },
                { "Container", ContainerSerial },
                { "RequeueFailedItems", RequeueFailedItems },
                { "LootHumanoids", LootHumanoids },
                { "LeftColumnWidth", LeftColumnWidth },
                { "Groups", groupArray }
            };

            JArray itemsArray = new JArray();

            foreach ( AutolootEntry entry in Items )
            {
                JObject entryObj = new JObject
                {
                    { "Name", entry.Name },
                    { "ID", entry.ID },
                    { "Autoloot", entry.Autoloot },
                    { "Rehue", entry.Rehue },
                    { "RehueHue", entry.RehueHue },
                    { "Enabled", entry.Enabled },
                    { "Priority", entry.Priority.ToString() },
                    { "Group", entry.Group?.Name }
                };

                if ( entry.Constraints != null )
                {
                    JArray constraintsArray = new JArray();

                    foreach ( AutolootConstraintEntry constraint in entry.Constraints )
                    {
                        JObject constraintObj = new JObject
                        {
                            { "Name", constraint.Property.Name },
                            { "Operator", constraint.Operator.ToString() },
                            { "Value", constraint.Value }
                        };

                        constraintsArray.Add( constraintObj );
                    }

                    entryObj.Add( "Properties", constraintsArray );
                }

                itemsArray.Add( entryObj );
            }

            autolootObj.Add( "Items", itemsArray );

            json.Add( "Autoloot", autolootObj );
        }

        public void Deserialize( JObject json, Options options )
        {
            Items.Clear();
            Draggables.Clear();

            if ( json?["Autoloot"] == null )
            {
                return;
            }

            JToken config = json["Autoloot"];

            Enabled = config["Enabled"]?.ToObject<bool>() ?? true;
            DisableInGuardzone = config["DisableInGuardzone"]?.ToObject<bool>() ?? false;
            ContainerSerial = config["Container"]?.ToObject<int>() ?? 0;
            RequeueFailedItems = config["RequeueFailedItems"]?.ToObject<bool>() ?? false;
            LootHumanoids = config["LootHumanoids"]?.ToObject<bool>() ?? true;
#if !DEVELOP
            LeftColumnWidth = config["LeftColumnWidth"]?.ToObject<double>() ?? 200;
#endif

            if ( config["Groups"] != null )
            {
                JToken groups = config["Groups"];

                foreach ( JToken token in groups )
                {
                    AutolootGroup group = new AutolootGroup
                    {
                        Name = token["Name"]?.ToObject<string>() ?? "Unknown",
                        Enabled = token["Enabled"]?.ToObject<bool>() ?? false
                    };

                    Draggables.Add( group );
                }
            }

            if ( config["Items"] != null )
            {
                JToken items = config["Items"];

                foreach ( JToken token in items )
                {
                    AutolootEntry entry = new AutolootEntry
                    {
                        Name = token["Name"]?.ToObject<string>() ?? "Unknown",
                        ID = token["ID"]?.ToObject<int>() ?? 0,
                        Autoloot = token["Autoloot"]?.ToObject<bool>() ?? false,
                        Rehue = token["Rehue"]?.ToObject<bool>() ?? false,
                        RehueHue = token["RehueHue"]?.ToObject<int>() ?? 0,
                        Enabled = token["Enabled"]?.ToObject<bool>() ?? true,
                        Priority = token["Priority"]?.ToObject<AutolootPriority>() ?? AutolootPriority.Normal
                    };

                    string groupName = token["Group"]?.ToObject<string>();

                    if ( !string.IsNullOrEmpty( groupName ) )
                    {
                        AutolootGroup group = (AutolootGroup) Draggables.FirstOrDefault(
                            i => i is AutolootGroup gr && gr.Name == groupName );

                        if ( group == null )
                        {
                            group = new AutolootGroup { Name = groupName };
                            Draggables.Add( group );
                        }

                        entry.Group = group;
                    }

                    if ( token["Properties"] != null )
                    {
                        List<AutolootConstraintEntry> constraintsList = new List<AutolootConstraintEntry>();

                        // ReSharper disable once LoopCanBeConvertedToQuery
                        foreach ( JToken constraintToken in token["Properties"] )
                        {
                            string constraintName = constraintToken["Name"]?.ToObject<string>() ?? "Unknown";

                            PropertyEntry propertyEntry = Constraints.FirstOrDefault( c => c.Name == constraintName );

                            if ( propertyEntry == null )
                            {
                                continue;
                            }

                            AutolootConstraintEntry constraintObj = new AutolootConstraintEntry
                            {
                                Property = propertyEntry,
                                Operator = constraintToken["Operator"]?.ToObject<AutolootOperator>() ??
                                           AutolootOperator.Equal,
                                Value = constraintToken["Value"]?.ToObject<int>() ?? 0
                            };

                            constraintsList.Add( constraintObj );
                        }

                        entry.Constraints.AddRange( constraintsList );
                    }

                    Items.Add( entry );
                }
            }

            if ( SelectedItem != null && !Items.Contains( SelectedItem ) )
            {
                SelectedItem = null;
            }

            if ( SelectedGroup != null && !Draggables.Contains( SelectedGroup ) )
            {
                SelectedGroup = null;
            }
        }

        private void NewGroup( object obj )
        {
            int count = Draggables.Count( i => i is IDraggableGroup );

            string name = $"Group-{count + 1}";

            while ( Draggables.Any( e => e is IDraggableGroup && e.Name == name ) )
            {
                name += "-";
            }

            Draggables.Add( new AutolootGroup { Name = name } );
        }

        private void LoadCustomProperties()
        {
            if ( !File.Exists( _propertiesFileCustom ) )
            {
                return;
            }

            JsonSerializer serializer = new JsonSerializer();

            using ( StreamReader sr = new StreamReader( _propertiesFileCustom ) )
            {
                using ( JsonTextReader reader = new JsonTextReader( sr ) )
                {
                    PropertyEntry[] constraints = serializer.Deserialize<PropertyEntry[]>( reader );

                    if ( constraints == null )
                    {
                        return;
                    }

                    foreach ( PropertyEntry constraint in constraints )
                    {
                        Constraints.AddSorted( constraint );
                    }
                }
            }
        }

        private void RemoveGroup( object obj )
        {
            if ( !( obj is IDraggableGroup group ) )
            {
                return;
            }

            foreach ( AutolootEntry groupChild in
                group.Children.Where( i => i is AutolootEntry ).Cast<AutolootEntry>() )
            {
                Draggables.Add( groupChild );

                groupChild.Group = null;
            }

            Draggables.Remove( group );
        }

        private void LoadProperties()
        {
            JsonSerializer serializer = new JsonSerializer();

            using ( StreamReader sr = new StreamReader( _propertiesFile ) )
            {
                using ( JsonTextReader reader = new JsonTextReader( sr ) )
                {
                    PropertyEntry[] constraints = serializer.Deserialize<PropertyEntry[]>( reader );

                    if ( constraints == null )
                    {
                        return;
                    }

                    foreach ( PropertyEntry constraint in constraints )
                    {
                        Constraints.AddSorted( constraint );
                    }
                }
            }
        }

        private void ClipboardPaste( object obj )
        {
            string text = Clipboard.GetText();

            try
            {
                IEnumerable<AutolootConstraintEntry> entries =
                    JsonConvert.DeserializeObject<IEnumerable<AutolootConstraintEntry>>( text );

                if ( entries == null )
                {
                    return;
                }

                foreach ( AutolootConstraintEntry entry in entries )
                {
                    if ( !SelectedItem.Constraints.Contains( entry ) )
                    {
                        SelectedItem?.Constraints.Add( entry );
                    }
                }
            }
            catch ( Exception )
            {
                // ignored
            }
        }

        private static void ClipboardCopy( object obj )
        {
            if ( !( obj is IList<AutolootConstraintEntry> entries ) )
            {
                return;
            }

            string text = JsonConvert.SerializeObject( entries );

            Clipboard.SetText( text );
        }

        internal void OnCorpseEvent( int serial )
        {
            if ( !Enabled )
            {
                return;
            }

            lock ( _autolootLock )
            {
                Item item = Engine.Items.GetItem( serial );

                if ( item == null || item.ID != 0x2006 )
                {
                    return;
                }

                if ( !LootHumanoids && TargetManager.GetInstance().BodyData
                    .Where( bd => bd.BodyType == TargetBodyType.Humanoid ).Select( bd => bd.Graphic )
                    .Contains( item.Count ) )
                {
                    return;
                }

                PacketWaitEntry we = Engine.PacketWaitEntries.Add(
                    new PacketFilterInfo( 0x3C, new[] { PacketFilterConditions.IntAtPositionCondition( serial, 19 ) } ),
                    PacketDirection.Incoming );

                we.Lock.WaitOne( 2000 );

                IEnumerable<Item> items = Engine.Items.GetItem( serial )?.Container?.GetItems();

                if ( items == null )
                {
                    return;
                }

                if ( Engine.TooltipsEnabled )
                {
                    Engine.SendPacketToServer( new BatchQueryProperties( items.Select( i => i.Serial ).ToArray() ) );
                    Thread.Sleep( 1000 );
                }

                List<Item> lootItems = new List<Item>();

                // If change logic, also change in DebugAutolootViewModel

                foreach ( AutolootEntry entry in Items.OrderByDescending( x => x.Priority ) )
                {
                    if ( !entry.Enabled )
                    {
                        continue;
                    }

                    if ( entry.Group != null && !entry.Group.Enabled )
                    {
                        continue;
                    }

                    IEnumerable<Item> matchItems = AutolootHelpers.AutolootFilter( items, entry );

                    if ( matchItems == null )
                    {
                        continue;
                    }

                    foreach ( Item matchItem in matchItems )
                    {
                        if ( entry.Rehue )
                        {
                            Engine.SendPacketToClient( new ContainerContentUpdate( matchItem.Serial, matchItem.ID,
                                matchItem.Direction, matchItem.Count, matchItem.X, matchItem.Y, matchItem.Grid,
                                matchItem.Owner, entry.RehueHue ) );
                        }

                        if ( DisableInGuardzone &&
                             Engine.Player.GetRegion().Attributes.HasFlag( RegionAttributes.Guarded ) )
                        {
                            continue;
                        }

                        if ( entry.Autoloot && !lootItems.Contains( matchItem ) )
                        {
                            lootItems.Add( matchItem );
                        }
                    }
                }

                foreach ( Item lootItem in lootItems.Distinct() )
                {
                    int containerSerial = ContainerSerial;

                    if ( containerSerial == 0 ||
                         Engine.Player?.Backpack?.Container?.GetItem( containerSerial ) == null )
                    {
                        containerSerial = Engine.Player?.GetLayer( Layer.Backpack ) ?? 0;
                    }

                    UOC.SystemMessage( string.Format( Strings.Autolooting___0__, lootItem.Name ),
                        (int) UOC.SystemMessageHues.Yellow );

                    Task t = ActionPacketQueue.EnqueueDragDrop( lootItem.Serial, lootItem.Count, containerSerial,
                        QueuePriority.High, true, true, requeueOnFailure: RequeueFailedItems,
                        successPredicate: CheckItemContainer );

                    t.Wait( LOOT_TIMEOUT );
                }
            }
        }

        private static bool CheckItemContainer( int serial, int containerSerial )
        {
            Item item = Engine.Items.GetItem( serial );

            return item == null || item.Owner == containerSerial;
        }

        private void RemoveConstraint( object obj )
        {
            if ( !( obj is IEnumerable<AutolootConstraintEntry> constraints ) )
            {
                return;
            }

            foreach ( AutolootConstraintEntry constraintEntry in constraints.ToList() )
            {
                SelectedItem?.Constraints.Remove( constraintEntry );
            }
        }

        private void InsertConstraint( object obj )
        {
            if ( !( obj is PropertyEntry propertyEntry ) )
            {
                return;
            }

            List<AutolootConstraintEntry> constraints =
                new List<AutolootConstraintEntry>( SelectedItem.Constraints )
                {
                    new AutolootConstraintEntry { Property = propertyEntry }
                };

            SelectedItem.Constraints = new ObservableCollection<AutolootConstraintEntry>( constraints );
        }

        private void DefineCustomProperties( object obj )
        {
            Engine.Dispatcher.Invoke( () =>
            {
                CustomPropertiesWindow window = new CustomPropertiesWindow();
                window.ShowDialog();

                Constraints.Clear();
                LoadProperties();
                LoadCustomProperties();
            } );
        }

        private void ResetContainer( object obj )
        {
            ContainerSerial = 0;
        }

        private async Task SetContainer( object arg )
        {
            int serial = await UOC.GetTargetSerialAsync( Strings.Target_container___ );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
                return;
            }

            ContainerSerial = serial;
        }

        private void InsertMatchAny( object obj )
        {
            AutolootEntry entry = new AutolootEntry
            {
                Name = Strings.Any, ID = -1, Constraints = new ObservableCollection<AutolootConstraintEntry>()
            };

            Items.Add( entry );
        }

        private async Task Insert( object arg )
        {
            int serial = await UOC.GetTargetSerialAsync( Strings.Target_object___ );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
                return;
            }

            Item item = Engine.Items.GetItem( serial );

            if ( item == null )
            {
                UOC.SystemMessage( Strings.Cannot_find_item___ );
                return;
            }

            AutolootEntry entry = new AutolootEntry
            {
                Name = TileData.GetStaticTile( item.ID ).Name,
                ID = item.ID,
                Constraints = new ObservableCollection<AutolootConstraintEntry>()
            };

            Items.Add( entry );
        }

        private async Task Remove( object arg )
        {
            if ( !( arg is AutolootEntry entry ) )
            {
                return;
            }

            Items.Remove( entry );

            await Task.CompletedTask;
        }

        private static void SelectHue( object obj )
        {
            if ( !( obj is AutolootEntry entry ) )
            {
                return;
            }

            if ( HuePickerWindow.GetHue( out int hue ) )
            {
                entry.RehueHue = hue;
            }
        }

        private void UpdateDraggables( object sender, NotifyCollectionChangedEventArgs e )
        {
            if ( e.NewItems != null )
            {
                foreach ( object newItem in e.NewItems )
                {
                    if ( !( newItem is AutolootEntry autolootEntry ) )
                    {
                        continue;
                    }

                    if ( autolootEntry.Group != null )
                    {
                        autolootEntry.Group.Children.Add( autolootEntry );
                    }
                    else
                    {
                        Draggables.Add( autolootEntry );
                    }
                }
            }

            if ( e.OldItems == null )
            {
                return;
            }

            foreach ( object newItem in e.OldItems )
            {
                if ( !( newItem is AutolootEntry autolootEntry ) )
                {
                    continue;
                }

                if ( autolootEntry.Group != null )
                {
                    autolootEntry.Group.Children.Remove( autolootEntry );
                }
                else
                {
                    Draggables.Remove( autolootEntry );
                }
            }
        }
    }
}