using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Models;
using ClassicAssist.UI.Views;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using Microsoft.Scripting.Utils;

namespace ClassicAssist.UI.ViewModels
{
    public class ObjectInspectorViewModel : BaseViewModel
    {
        private ICommand _copyToClipboardCommand;
        private ObservableCollection<ObjectInspectorData> _items = new ObservableCollection<ObjectInspectorData>();
        private ObjectInspectorData _selectedItem;

        public ObjectInspectorViewModel()
        {
        }

        public ObjectInspectorViewModel( Entity entity )
        {
            AddEntityProperties( entity );

            if ( entity is Mobile mobile )
            {
                AddMobileProperties( mobile );
            }

            AddPublicProperties( entity );
        }

        public ObjectInspectorViewModel( StaticTile staticTile )
        {
            AddStaticProperties( staticTile );
        }

        public ObjectInspectorViewModel( LandTile landTile )
        {
            AddLandProperties( landTile );
        }

        public ICommand CopyToClipboardCommand
        {
            get
            {
                return _copyToClipboardCommand ?? ( _copyToClipboardCommand =
                    new RelayCommand( o => CopyToClipboard(), o => _selectedItem != null ) );
            }
        }

        public ObservableCollection<ObjectInspectorData> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public ObjectInspectorData SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        private void AddLandProperties( LandTile landTile )
        {
            AddData( typeof( Entity ), nameof( Entity.Name ), landTile.Name, Strings.Entity );
            AddData( "Flags", landTile.Flags.ToString(), Strings.Entity );
            AddData( typeof( Entity ), nameof( Entity.ID ), landTile.ID, Strings.Entity, Strings.Graphic );
            AddData( "Position", $"{landTile.X}, {landTile.Y}, {landTile.Z}", Strings.Entity );
        }

        private void AddStaticProperties( StaticTile staticTile )
        {
            AddData( typeof( Entity ), nameof( Entity.Name ), staticTile.Name, Strings.Entity );
            AddData( "Flags", staticTile.Flags.ToString(), Strings.Entity );
            AddData( typeof( Entity ), nameof( Entity.ID ), staticTile.ID, Strings.Entity, Strings.Graphic );
            AddData( typeof( Entity ), nameof( Entity.Hue ), staticTile.Hue, Strings.Entity, Strings.Color );
            AddData( "Position", $"{staticTile.X}, {staticTile.Y}, {staticTile.Z}", Strings.Entity );
            AddData( "Weight", staticTile.Weight.ToString(), Strings.Entity );
            AddData( "Height", staticTile.Height.ToString(), Strings.Entity );
            AddData( "Quality", staticTile.Quality.ToString(), Strings.Entity );
            AddData( "Quantity", staticTile.Quantity.ToString(), Strings.Entity );
        }

        private void AddEntityProperties( Entity entity )
        {
            AddData( typeof( Entity ), nameof( Entity.Name ), entity.Name, Strings.Entity );
            AddData( typeof( Entity ), nameof( Entity.Serial ), entity.Serial, Strings.Entity );
            AddData( typeof( Entity ), nameof( Entity.ID ), entity.ID, Strings.Entity, Strings.Graphic );
            AddData( typeof( Entity ), nameof( Entity.Hue ), entity.Hue, Strings.Entity, Strings.Color );
            AddData( "Position", $"{entity.X}, {entity.Y}, {entity.Z}", Strings.Entity );
        }

        private void AddMobileProperties( Mobile mobile )
        {
            AddData( typeof( Mobile ), nameof( Mobile.Name ), mobile.Name, Strings.Mobile );
            AddData( "Sex", mobile.Status.HasFlag( MobileStatus.Female ) ? "Female" : "Male", Strings.Mobile );

            if ( mobile is PlayerMobile player )
            {
                AddData( typeof( Mobile ), nameof( Mobile.Hits ), player.Hits, Strings.Mobile );
                AddData( typeof( Mobile ), nameof( Mobile.HitsMax ), player.HitsMax, Strings.Mobile, "Max Health" );
            }
            else
            {
                float health = mobile.Hits;
                float maxHealth = mobile.HitsMax;
                float per = health / maxHealth * 100;
                AddData( "Hits", ( (uint) per ).ToString() + '%', Strings.Mobile );
            }

            AddData( typeof( Mobile ), nameof( Mobile.Notoriety ), mobile.Notoriety, Strings.Mobile );
            AddData( typeof( Mobile ), nameof( Mobile.Status ), mobile.Status, Strings.Mobile, "Flags" );

            #region Equipment Properties

            Item[] equipment = GetEquipmentItems( mobile );

            if ( equipment == null )
            {
                return;
            }

            AddEquipmentProperty( equipment, new[] { 1060448, 1153735 }, new[] { 0, 0 }, "Physical Resist",
                Strings.Resistances );
            AddEquipmentProperty( equipment, new[] { 1060447, 1153737 }, new[] { 0, 0 }, "Fire Resist",
                Strings.Resistances );
            AddEquipmentProperty( equipment, new[] { 1060445, 1153739 }, new[] { 0, 0 }, "Cold Resist",
                Strings.Resistances );
            AddEquipmentProperty( equipment, new[] { 1060449, 1153736 }, new[] { 0, 0 }, "Poison Resist",
                Strings.Resistances );
            AddEquipmentProperty( equipment, new[] { 1060446, 1153738 }, new[] { 0, 0 }, "Energy Resist",
                Strings.Resistances );

            AddEquipmentProperty( equipment, 1060413, 0, "Faster Casting", Strings.Magical );
            AddEquipmentProperty( equipment, 1060412, 0, "Faster Cast Recovery", Strings.Magical );
            AddEquipmentProperty( equipment, 1060434, 0, "Lower Reagent Cost", Strings.Magical );
            AddEquipmentProperty( equipment, 1060433, 0, "Lower Mana Cost", Strings.Magical );
            AddEquipmentProperty( equipment, 1060483, 0, "Spell Damage Increase", Strings.Magical );

            AddEquipmentProperty( equipment, 1060408, 0, "Defense Chance Increase", Strings.Combat );
            AddEquipmentProperty( equipment, 1060415, 0, "Hit Chance Increase", Strings.Combat );
            AddEquipmentProperty( equipment, 1060401, 0, "Damage Increase", Strings.Combat );
            AddEquipmentProperty( equipment, 1060402, 0, "Damage Increase", Strings.Combat );
            AddEquipmentProperty( equipment, 1060486, 0, "Swing Speed Increase", Strings.Combat );

            AddEquipmentProperty( equipment, 1060485, 0, "Strength Bonus", Strings.Stats );
            AddEquipmentProperty( equipment, 1060432, 0, "Intelligence Bonus", Strings.Stats );
            AddEquipmentProperty( equipment, 1060409, 0, "Dexterity Bonus", Strings.Stats );

            AddEquipmentProperty( equipment, 1060431, 0, "Hit Point Increase", Strings.Stats );
            AddEquipmentProperty( equipment, 1060439, 0, "Mana Increase", Strings.Stats );
            AddEquipmentProperty( equipment, 1060484, 0, "Stamina Increase", Strings.Stats );

            AddEquipmentProperty( equipment, 1060444, 0, "Hit Point Regeneration", Strings.Regeneration );
            AddEquipmentProperty( equipment, 1060440, 0, "Mana Regeneration", Strings.Regeneration );
            AddEquipmentProperty( equipment, 1060443, 0, "Stamina Regeneration", Strings.Regeneration );

            AddEquipmentProperty( equipment, 1060436, 0, "Luck", "Misc" );

            AddEquipmentPropertyWithSymbol( equipment, 1153733, 0, 1, "Max Defense Chance Increase",
                Strings.Buffs___Penalties );
            AddEquipmentPropertyWithSymbol( equipment, 1153735, 1, 2, "Physical Resist", Strings.Buffs___Penalties );
            AddEquipmentPropertyWithSymbol( equipment, 1153737, 1, 2, "Fire Resist", Strings.Buffs___Penalties );
            AddEquipmentPropertyWithSymbol( equipment, 1153739, 1, 2, "Cold Resist", Strings.Buffs___Penalties );
            AddEquipmentPropertyWithSymbol( equipment, 1153736, 1, 2, "Poison Resist", Strings.Buffs___Penalties );
            AddEquipmentPropertyWithSymbol( equipment, 1153738, 1, 2, "Energy Resist", Strings.Buffs___Penalties );

            #endregion
        }

        private static Item[] GetEquipmentItems( Mobile mobile )
        {
            List<Item> items = new List<Item>();

            foreach ( Item item in mobile.GetEquippedItems() )
            {
                items.Add( item );
            }

            return items.ToArray();
        }

        private void AddEquipmentProperty( Item[] items, int cliloc, int argumentIndex, string name, string category )
        {
            AddEquipmentProperty( items, new[] { cliloc }, new[] { argumentIndex }, name, category );
        }

        private void AddEquipmentProperty( Item[] items, IReadOnlyList<int> clilocs, IReadOnlyList<int> argumentIndexs,
            string name, string category )
        {
            int amount = clilocs.Select( ( t, i ) => CountPropertyList( items.ToArray(), t, argumentIndexs[i] ) ).Sum();

            if ( amount > 0 )
            {
                AddData( new ObjectInspectorData
                {
                    Name = name, Value = amount.ToString(), Category = category, IsExpanded = false
                } );
            }
        }

        private void AddEquipmentPropertyWithSymbol( Item[] items, int cliloc, int symbolIndex, int argumentIndex,
            string name, string category )
        {
            int amount = CountPropertyListWithSymbol( items.ToArray(), cliloc, symbolIndex, argumentIndex );

            if ( amount != 0 )
            {
                AddData( new ObjectInspectorData
                {
                    Name = name, Value = amount.ToString(), Category = category, IsExpanded = false
                } );
            }
        }

        private static int CountProperty( int serial, int cliloc, int argumentIndex )
        {
            Item item = Engine.Items.GetItem( serial );

            Property[] properties = item?.Properties;

            if ( properties == null )
            {
                return 0;
            }

            return properties.Where( p => p.Cliloc == cliloc ).Select( p => 
                {
                if ( int.TryParse( p.Arguments[argumentIndex], out int result ) )
                    return result;
                return -1;
                } )
                .FirstOrDefault();
        }

        private static int CountPropertyList( IEnumerable<Item> items, int cliloc, int argumentIndex )
        {
            return items.Sum( t => CountProperty( t.Serial, cliloc, argumentIndex ) );
        }

        private static int CountPropertyListWithSymbol( IReadOnlyList<Item> items, int cliloc, int symbolIndex,
            int argumentIndex )
        {
            int total = 0;

            foreach ( Item t in items )
            {
                int count = CountProperty( t.Serial, cliloc, argumentIndex );

                Property[] properties = t.Properties;

                if ( properties == null )
                {
                    continue;
                }

                foreach ( Property p in properties )
                {
                    if ( p.Cliloc != cliloc )
                    {
                        continue;
                    }

                    string symbol = p.Arguments[symbolIndex];

                    if ( symbol == "-" )
                    {
                        total -= count;
                    }
                    else
                    {
                        total += count;
                    }
                }
            }

            return total;
        }

        private static string GetDisplayFormat( Type type, string propertyName, object value )
        {
            if ( propertyName == null )
            {
                return value.ToString();
            }

            DisplayFormatAttribute attr = type.GetPropertyAttribute<DisplayFormatAttribute>( propertyName );

            return attr != null ? attr.ToString( value ) : value?.ToString() ?? string.Empty;
        }

        private void AddData( Type type, string propertyName, object value, string category, string nameOverride = "",
            Action<object> action = null )
        {
            AddData( new ObjectInspectorData
            {
                Name = string.IsNullOrEmpty( nameOverride ) ? propertyName : nameOverride,
                Value = GetDisplayFormat( type, propertyName, value ),
                Category = category,
                OnDoubleClick = action
            } );
        }

        private void AddData( string name, string value, string category )
        {
            AddData( new ObjectInspectorData { Name = name, Value = value, Category = category } );
        }

        private void AddData( ObjectInspectorData data )
        {
            if ( !_dispatcher.CheckAccess() )
            {
                _dispatcher.Invoke( () => { Items.Add( data ); } );
            }
            else
            {
                Items.Add( data );
            }
        }

        private void CopyToClipboard()
        {
            try
            {
                Clipboard.SetData( DataFormats.Text, _selectedItem.Value );
            }
            catch ( Exception )
            {
                // ignored
            }
        }

        private void AddPublicProperties( Entity entity )
        {
            List<ObjectInspectorData> data = new List<ObjectInspectorData>();

            IOrderedEnumerable<PropertyInfo> properties = entity.GetType().GetProperties().OrderBy( p => p.Name );

            foreach ( PropertyInfo p in properties )
            {
                object value = p.GetValue( entity );
                string valueString = "null";

                if ( value != null )
                {
                    valueString = value.ToString();
                }

                DisplayFormatAttribute attr = entity.GetType().GetPropertyAttribute<DisplayFormatAttribute>( p.Name );

                if ( attr != null )
                {
                    valueString = attr.ToString( value );
                }

                data.Add( new ObjectInspectorData
                {
                    Name = p.Name,
                    Value = valueString,
                    Category = Strings.Public_Properties,
                    IsExpanded = true,
                    OnDoubleClick = GetDoubleClickAction( value )
                } );
            }

            Items.AddRange( data );
        }

        private static Action<object> GetDoubleClickAction( object value )
        {
            if ( value == null )
            {
                return null;
            }

            if ( value is IObjectInspectorDoubleClick )
            {
                return o => ( (IObjectInspectorDoubleClick) value ).OnDoubleClick( o );
            }

            if ( value is ItemCollection collection )
            {
                return o => ShowItemCollection( collection );
            }

            if ( value is Item item )
            {
                return o => InspectObject( item.Serial );
            }

            return null;
        }

        private static void InspectObject( int serial )
        {
            Entity entity;

            if ( UOMath.IsMobile( serial ) )
            {
                entity = Engine.Mobiles.GetMobile( serial );
            }
            else
            {
                entity = Engine.Items.GetItem( serial );
            }

            if ( entity == null )
            {
                return;
            }

            ObjectInspectorWindow window = new ObjectInspectorWindow
            {
                DataContext = new ObjectInspectorViewModel( entity ), Topmost = true
            };
            window.ShowDialog();
        }

        private static void ShowItemCollection( ItemCollection collection )
        {
            if ( collection == null )
            {
                return;
            }

            EntityCollectionViewer window = new EntityCollectionViewer
            {
                DataContext = new EntityCollectionViewerViewModel( collection ), Topmost = true
            };

            window.ShowDialog();
        }
    }

    public interface IObjectInspectorDoubleClick
    {
        void OnDoubleClick( object o );
    }
}