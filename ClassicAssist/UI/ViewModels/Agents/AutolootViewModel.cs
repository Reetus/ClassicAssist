using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.Data.Regions;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
using ClassicAssist.UI.Misc;
using ClassicAssist.UI.Views;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using Microsoft.Scripting.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.UI.ViewModels.Agents
{
    public class AutolootViewModel : BaseViewModel, ISettingProvider
    {
        private ObservableCollection<AutolootConstraints>
            _constraints = new ObservableCollection<AutolootConstraints>();

        private int _containerSerial;

        private bool _disableInGuardzone;
        private bool _enabled;

        private ICommand _insertCommand;
        private ICommand _insertConstraintCommand;

        private ObservableCollectionEx<AutolootEntry> _items = new ObservableCollectionEx<AutolootEntry>();
        private ICommand _removeCommand;
        private ICommand _removeConstraintCommand;
        private AutolootEntry _selectedItem;
        private ICommand _selectHueCommand;
        private ICommand _setContainerCommand;

        public AutolootViewModel()
        {
            string constraintsFile = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Data",
                "Autoloot.json" );

            if ( !File.Exists( constraintsFile ) )
            {
                return;
            }

            JsonSerializer serializer = new JsonSerializer();

            using ( StreamReader sr = new StreamReader( constraintsFile ) )
            {
                using ( JsonTextReader reader = new JsonTextReader( sr ) )
                {
                    AutolootConstraints[] constraints = serializer.Deserialize<AutolootConstraints[]>( reader );

                    foreach ( AutolootConstraints constraint in constraints )
                    {
                        Constraints.AddSorted( constraint );
                    }
                }
            }

            IncomingPacketHandlers.CorpseContainerDisplayEvent += OnCorpseContainerDisplayEvent;
        }

        public ObservableCollection<AutolootConstraints> Constraints
        {
            get => _constraints;
            set => SetProperty( ref _constraints, value );
        }

        public int ContainerSerial
        {
            get => _containerSerial;
            set => SetProperty( ref _containerSerial, value );
        }

        public bool DisableInGuardzone
        {
            get => _disableInGuardzone;
            set => SetProperty( ref _disableInGuardzone, value );
        }

        public bool Enabled
        {
            get => _enabled;
            set => SetProperty( ref _enabled, value );
        }

        public ICommand InsertCommand =>
            _insertCommand ?? ( _insertCommand = new RelayCommandAsync( Insert, o => Engine.Connected ) );

        public ICommand InsertConstraintCommand =>
            _insertConstraintCommand ?? ( _insertConstraintCommand =
                new RelayCommand( InsertConstraint, o => SelectedItem != null ) );

        public ObservableCollectionEx<AutolootEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public ICommand RemoveCommand =>
            _removeCommand ?? ( _removeCommand = new RelayCommandAsync( Remove, o => SelectedItem != null ) );

        public ICommand RemoveConstraintCommand =>
            _removeConstraintCommand ?? ( _removeConstraintCommand =
                new RelayCommand( RemoveConstraint, o => SelectedConstraint != null ) );

        public AutolootConstraints SelectedConstraint { get; set; }

        public AutolootEntry SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
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

            JObject autolootObj = new JObject
            {
                { "Enabled", Enabled },
                { "DisableInGuardzone", DisableInGuardzone },
                { "Container", ContainerSerial }
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
                    { "RehueHue", entry.RehueHue }
                };

                if ( entry.Constraints != null )
                {
                    JArray constraintsArray = new JArray();

                    foreach ( AutolootConstraints constraint in entry.Constraints )
                    {
                        JObject constraintObj = new JObject
                        {
                            { "Name", constraint.Name },
                            { "Clilocs", constraint.Clilocs.ToJArray() },
                            { "ConstraintType", constraint.ConstraintType.ToString() },
                            { "Operator", constraint.Operator.ToString() },
                            { "Value", constraint.Value }
                        };

                        constraintsArray.Add( constraintObj );
                    }

                    entryObj.Add( "Constraints", constraintsArray );
                }

                itemsArray.Add( entryObj );
            }

            autolootObj.Add( "Items", itemsArray );

            json.Add( "Autoloot", autolootObj );
        }

        public void Deserialize( JObject json, Options options )
        {
            if ( json?["Autoloot"] == null )
            {
                return;
            }

            JToken config = json["Autoloot"];

            Enabled = config["Enabled"]?.ToObject<bool>() ?? true;
            DisableInGuardzone = config["DisableInGuardzone"]?.ToObject<bool>() ?? false;
            ContainerSerial = config["Container"]?.ToObject<int>() ?? 0;

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
                        RehueHue = token["RehueHue"]?.ToObject<int>() ?? 0
                    };

                    if ( token["Constraints"] != null )
                    {
                        List<AutolootConstraints> constraintsList = new List<AutolootConstraints>();

                        // ReSharper disable once LoopCanBeConvertedToQuery
                        foreach ( JToken constraintToken in token["Constraints"] )
                        {
                            AutolootConstraints constraintObj = new AutolootConstraints
                            {
                                Name = constraintToken["Name"]?.ToObject<string>() ?? "Unknown",
                                Clilocs = constraintToken["Clilocs"]?.ToIntArray() ?? new[] { 0 },
                                ConstraintType =
                                    constraintToken["ConstraintType"]?.ToObject<AutolootConstraintType>() ??
                                    AutolootConstraintType.Object,
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
        }

        private void OnCorpseContainerDisplayEvent( int serial )
        {
            if ( !Enabled )
            {
                return;
            }

            PacketWaitEntry we = Engine.PacketWaitEntries.Add( new PacketFilterInfo( 0x3C,
                    new[] { PacketFilterConditions.IntAtPositionCondition( serial, 19 ) } ),
                PacketDirection.Incoming );

            bool result = we.Lock.WaitOne( 5000 );

            if ( !result )
            {
                return;
            }

            IEnumerable<Item> items = Engine.Items.GetItem( serial )?.Container.GetItems();

            if ( items == null )
            {
                return;
            }

            foreach ( AutolootEntry entry in Items )
            {
                IEnumerable<Item> matchItems = AutolootFilter( items, entry );

                if ( matchItems == null )
                {
                    continue;
                }

                List<Item> lootItems = new List<Item>();

                foreach ( Item matchItem in matchItems )
                {
                    if ( entry.Rehue )
                    {
                        Engine.SendPacketToClient( new ContainerContentUpdate( matchItem.Serial, matchItem.ID,
                            matchItem.Direction, matchItem.Count,
                            matchItem.X, matchItem.Y, matchItem.Grid, matchItem.Owner, entry.RehueHue ) );
                    }

                    if ( DisableInGuardzone &&
                         Engine.Player.GetRegion().Attributes.HasFlag( RegionAttributes.Guarded ) )
                    {
                        continue;
                    }

                    if ( entry.Autoloot )
                    {
                        lootItems.Add( matchItem );
                    }
                }

                foreach ( Item lootItem in lootItems )
                {
                    int containerSerial = ContainerSerial;

                    if ( containerSerial == 0 )
                    {
                        if ( Engine.Player.Backpack == null )
                        {
                            return;
                        }

                        containerSerial = Engine.Player.Backpack.Serial;
                    }

                    Thread.Sleep( Options.CurrentOptions.ActionDelayMS );
                    UOC.SystemMessage( string.Format( Strings.Autolooting___0__, lootItem.Name ) );
                    UOC.DragDropAsync( lootItem.Serial, lootItem.Count, containerSerial ).Wait();
                }
            }
        }

        private void RemoveConstraint( object obj )
        {
            if ( !( obj is AutolootConstraints constraint ) )
            {
                return;
            }

            SelectedItem.Constraints.Remove( constraint );
        }

        private void InsertConstraint( object obj )
        {
            if ( !( obj is AutolootConstraints constraint ) )
            {
                return;
            }

            List<AutolootConstraints> constraints =
                new List<AutolootConstraints>( SelectedItem.Constraints ) { constraint };

            SelectedItem.Constraints = new ObservableCollection<AutolootConstraints>( constraints );
        }

        private async Task SetContainer( object arg )
        {
            int serial = await UOC.GetTargeSerialAsync( Strings.Target_container___ );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            ContainerSerial = serial;
        }

        private async Task Insert( object arg )
        {
            int serial = await UOC.GetTargeSerialAsync( Strings.Target_object___ );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            Item item = Engine.Items.GetItem( serial );

            if ( item == null )
            {
                UOC.SystemMessage( Strings.Cannot_find_item___ );
                return;
            }

            Items.Add( new AutolootEntry
            {
                Name = TileData.GetStaticTile( item.ID ).Name,
                ID = item.ID,
                Constraints = new ObservableCollection<AutolootConstraints>()
            } );
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

        public static IEnumerable<Item> AutolootFilter( IEnumerable<Item> items, AutolootEntry entry )
        {
            return items == null
                ? null
                : ( from item in items
                    where item.ID == entry.ID
                    let predicates = ConstraintsToPredicates( entry.Constraints )
                    where !predicates.Any() || CheckPredicates( item, predicates )
                    select item ).ToList();
        }

        private static bool CheckPredicates( Item item, IEnumerable<Predicate<Item>> predicates )
        {
            return predicates.All( predicate => predicate( item ) );
        }

        public static IEnumerable<Predicate<Item>> ConstraintsToPredicates(
            IEnumerable<AutolootConstraints> constraints )
        {
            List<Predicate<Item>> predicates = new List<Predicate<Item>>();

            foreach ( AutolootConstraints constraint in constraints )
            {
                switch ( constraint.ConstraintType )
                {
                    case AutolootConstraintType.Properties:
                        predicates.Add( i => i.Properties != null && constraint.Clilocs.Any( cliloc =>
                                                 i.Properties.Any( p =>
                                                     p.Cliloc == cliloc && ( constraint.ClilocIndex == -1 || Operation(
                                                                                 constraint.Operator,
                                                                                 int.Parse( p.Arguments[
                                                                                     constraint.ClilocIndex] ),
                                                                                 constraint.Value ) ) ) ) );
                        break;
                    case AutolootConstraintType.Object:

                        predicates.Add( i =>
                            ItemHasObjectProperty( i, constraint.Name ) && Operation( constraint.Operator,
                                GetItemObjectPropertyValue<int>( i, constraint.Name ), constraint.Value ) );

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return predicates;
        }

        public static bool ItemHasObjectProperty( Item item, string propertyName )
        {
            PropertyInfo propertyInfo = item.GetType().GetProperty( propertyName );

            if ( propertyInfo == null )
            {
                return false;
            }

            return true;
        }

        private static T GetItemObjectPropertyValue<T>( Item item, string propertyName )
        {
            PropertyInfo propertyInfo = item.GetType().GetProperty( propertyName );

            if ( propertyInfo == null )
            {
                return default;
            }

            T val = (T) propertyInfo.GetValue( item );

            return val;
        }

        public static bool Operation( AutolootOperator @operator, int x, int y )
        {
            switch ( @operator )
            {
                case AutolootOperator.GreaterThan:
                    return x >= y;
                case AutolootOperator.LessThan:
                    return x <= y;
                case AutolootOperator.Equal:
                    return x == y;
                case AutolootOperator.NotEqual:
                    return x != y;
                default:
                    throw new ArgumentOutOfRangeException( nameof( @operator ), @operator, null );
            }
        }
    }
}