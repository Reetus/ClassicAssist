﻿#region License

// Copyright (C) 2024 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.Shared.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.ViewModels.Agents;
using ClassicAssist.UI.Views.ECV.Filter.Models;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry;

namespace ClassicAssist.UI.Views.ECV.Filter
{
    public class EntityCollectionFilterViewModel : SetPropertyNotifyChanged
    {
        private readonly string _propertiesFileCustom = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Data", "Properties.Custom.json" );

        private ICommand _addCommand;
        private ICommand _addProfileCommand;

        private ICommand _applyCommand;
        private Assembly[] _assemblies;
        private ICommand _changeProfileCommand;

        private ObservableCollection<PropertyEntry> _constraints = new ObservableCollection<PropertyEntry>();

        private EntityCollectionFilterEntry _item = new EntityCollectionFilterEntry();

        private ICommand _newGroupCommand;

        private ObservableCollection<EntityCollectionFilterEntry> _profiles = new ObservableCollection<EntityCollectionFilterEntry>();

        private ICommand _removeCommand;
        private ICommand _removeGroupCommand;
        private ICommand _removeProfileCommand;

        private ICommand _resetCommand;
        private ICommand _saveProfilesCommand;
        private EntityCollectionFilterItem _selectedItem;
        private EntityCollectionFilterEntry _selectedProfile;

        public EntityCollectionFilterViewModel()
        {
#if DEBUG
            if ( DesignerProperties.GetIsInDesignMode( new DependencyObject() ) )
            {
                Environment.CurrentDirectory = @"C:\Users\johns\Documents\UO\ClassicAssist\Output\net48";
            }
#endif

            string propertiesFile = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Data", "Properties.json" );

            if ( !File.Exists( propertiesFile ) )
            {
                return;
            }

            JsonSerializer serializer = new JsonSerializer();

            using ( StreamReader sr = new StreamReader( propertiesFile ) )
            {
                using ( JsonTextReader reader = new JsonTextReader( sr ) )
                {
                    PropertyEntry[] constraints = serializer.Deserialize<PropertyEntry[]>( reader );

                    foreach ( PropertyEntry constraint in constraints )
                    {
                        Constraints.AddSorted( constraint );
                    }
                }
            }

            // TODO: Use autoloot methods to not duplicate code

            if ( File.Exists( _propertiesFileCustom ) )
            {
                LoadCustomProperties();
            }

            Constraints.AddSorted( new PropertyEntry
            {
                Name = Strings.Layer,
                ConstraintType = PropertyType.Predicate,
                Predicate = ( item, entry ) =>
                {
                    Layer layer = TileData.GetLayer( item.ID );

                    return entry.Operator == AutolootOperator.NotPresent || AutolootHelpers.Operation( entry.Operator, entry.Value, (int) layer );
                },
                AllowedValuesEnum = typeof( Layer )
            } );

            Constraints.AddSorted( new PropertyEntry
            {
                Name = Strings.Skill_Bonus,
                ConstraintType = PropertyType.PredicateWithValue,
                Predicate = ( item, entry ) =>
                {
                    int[] clilocs = { 1060451, 1060452, 1060453, 1060454, 1060455, 1072394, 1072395 };

                    if ( item.Properties == null )
                    {
                        return false;
                    }

                    IEnumerable<Property> properties = item.Properties.Where( e => e != null && clilocs.Contains( e.Cliloc ) ).ToList();

                    if ( entry.Operator != AutolootOperator.NotPresent )
                    {
                        return properties
                            .Where( property => property.Arguments != null && property.Arguments.Length >= 1 &&
                                                ( property.Arguments[0].Equals( entry.Additional, StringComparison.CurrentCultureIgnoreCase ) ||
                                                  string.IsNullOrEmpty( entry.Additional ) ) ).Any( property =>
                                AutolootHelpers.Operation( entry.Operator, Convert.ToInt32( property.Arguments[1] ), entry.Value ) );
                    }

                    Property match = properties.FirstOrDefault( property =>
                        property.Arguments != null && property.Arguments.Length >= 1 &&
                        ( property.Arguments[0].Equals( entry.Additional, StringComparison.CurrentCultureIgnoreCase ) || string.IsNullOrEmpty( entry.Additional ) ) );

                    return match == null;
                },
                AllowedValuesEnum = typeof( SkillBonusSkills )
            } );

            Constraints.AddSorted( new PropertyEntry
            {
                Name = Strings.Name,
                ConstraintType = PropertyType.PredicateWithValue,
                Predicate = ( item, entry ) =>
                {
                    string propString = item.Properties == null ? item.Name : item.Properties.Aggregate( string.Empty, ( current, property ) => current + property.Text );

                    if ( entry.Operator != AutolootOperator.NotPresent )
                    {
                        return propString.IndexOf( entry.Additional, StringComparison.CurrentCultureIgnoreCase ) >= 0;
                    }

                    return propString.IndexOf( entry.Additional, StringComparison.CurrentCultureIgnoreCase ) == -1;
                }
            } );

            Constraints.AddSorted( new PropertyEntry
            {
                Name = nameof( TileFlags ),
                ConstraintType = PropertyType.Predicate,
                Predicate = ( item, entry ) =>
                {
                    StaticTile tileFlags = TileData.GetStaticTile( item.ID );

                    switch ( entry.Operator )
                    {
                        case AutolootOperator.NotEqual:
                        case AutolootOperator.NotPresent:
                            return !tileFlags.Flags.HasFlag( (TileFlags) entry.Value );
                        case AutolootOperator.Equal:
                            return tileFlags.Flags.HasFlag( (TileFlags) entry.Value );
                        case AutolootOperator.GreaterThan:
                        case AutolootOperator.LessThan:
                        default:
                            return false;
                    }
                },
                AllowedValuesEnum = typeof( TileFlags )
            } );

            LoadFilterProfiles();
        }

        public ICommand AddCommand => _addCommand ?? ( _addCommand = new RelayCommand( AddItem, o => true ) );

        public ICommand AddProfileCommand => _addProfileCommand ?? ( _addProfileCommand = new RelayCommand( AddProfile, o => true ) );

        public ICommand ApplyCommand => _applyCommand ?? ( _applyCommand = new RelayCommand( Apply, o => true ) );

        public Assembly[] Assemblies
        {
            get => _assemblies;
            set
            {
                SetProperty( ref _assemblies, value );
                LoadAssemblies( value );
            }
        }

        public ICommand ChangeProfileCommand => _changeProfileCommand ?? ( _changeProfileCommand = new RelayCommand( ChangeProfile, o => o is EntityCollectionFilterEntry ) );

        public ICommand Command { get; set; }

        public ObservableCollection<PropertyEntry> Constraints
        {
            get => _constraints;
            set => SetProperty( ref _constraints, value );
        }

        public EntityCollectionFilterEntry Item
        {
            get => _item;
            set => SetProperty( ref _item, value );
        }

        public ICommand NewGroupCommand => _newGroupCommand ?? ( _newGroupCommand = new RelayCommand( NewGroup, o => true ) );

        public ObservableCollection<EntityCollectionFilterEntry> Profiles
        {
            get => _profiles;
            set => SetProperty( ref _profiles, value );
        }

        public ICommand RemoveCommand => _removeCommand ?? ( _removeCommand = new RelayCommand( Remove, o => o == null || ( (GroupItem) o ).Group.Count > 1 ) );

        public ICommand RemoveGroupCommand => _removeGroupCommand ?? ( _removeGroupCommand = new RelayCommand( RemoveGroup, o => Item.Groups.Count > 1 ) );

        public ICommand RemoveProfileCommand => _removeProfileCommand ?? ( _removeProfileCommand = new RelayCommand( RemoveProfile, o => true ) );

        public ICommand ResetCommand => _resetCommand ?? ( _resetCommand = new RelayCommand( Reset, o => true ) );

        public ICommand SaveProfilesCommand => _saveProfilesCommand ?? ( _saveProfilesCommand = new RelayCommand( o => SaveFilterProfiles(), o => true ) );

        public EntityCollectionFilterItem SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        public EntityCollectionFilterEntry SelectedProfile
        {
            get => _selectedProfile;
            set => SetProperty( ref _selectedProfile, value );
        }

        private void LoadAssemblies( IEnumerable<Assembly> assemblies )
        {
            foreach ( Assembly asm in assemblies )
            {
                IEnumerable<MethodInfo> initializeMethods = asm.GetTypes()
                    .Where( e => e.IsClass && e.IsPublic && e.GetMethod( "Initialize", BindingFlags.Public | BindingFlags.Static, null,
                        new[] { typeof( ObservableCollection<PropertyEntry> ) }, null ) != null ).Select( e =>
                        e.GetMethod( "Initialize", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof( ObservableCollection<PropertyEntry> ) }, null ) );

                foreach ( MethodInfo initializeMethod in initializeMethods )
                {
                    // ReSharper disable once CoVariantArrayConversion
                    initializeMethod?.Invoke( null, new[] { Constraints } );
                }

                Profiles.Clear();
                LoadFilterProfiles();
            }
        }

        private void ChangeProfile( object obj )
        {
            if ( !( obj is EntityCollectionFilterEntry entry ) )
            {
                return;
            }

            SelectedProfile = Item = entry;
        }

        private void RemoveProfile( object obj )
        {
            Profiles.Remove( SelectedProfile );

            if ( Profiles.Count > 0 )
            {
                SelectedProfile = Item = Profiles[0];
            }
            else
            {
                AddDefaultEntry();
            }
        }

        private void AddProfile( object obj )
        {
            EntityCollectionFilterEntry newProfile = new EntityCollectionFilterEntry
            {
                Name = "New Filter Profile",
                Groups = new ObservableCollection<EntityCollectionFilterGroup>
                {
                    new EntityCollectionFilterGroup
                    {
                        Items = new ObservableCollection<EntityCollectionFilterItem> { new EntityCollectionFilterItem { Constraint = Constraints.FirstOrDefault() } }
                    }
                }
            };

            Profiles.Add( newProfile );
            SelectedProfile = Item = newProfile;
        }

        public void LoadFilterProfiles()
        {
            string file = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "FilterProfiles.json" );

            try
            {
                string json = File.ReadAllText( file );
                JObject obj = (JObject) JsonConvert.DeserializeObject( json ) ?? throw new ArgumentOutOfRangeException( nameof( json ) );

                Guid? selectedProfile = obj["LastProfileID"]?.ToObject<Guid>();

                if ( obj["Profiles"] != null )
                {
                    foreach ( JToken profileObj in obj["Profiles"] )
                    {
                        EntityCollectionFilterEntry profile = new EntityCollectionFilterEntry
                        {
                            ID = profileObj["ID"]?.ToObject<Guid>() ?? Guid.NewGuid(),
                            Name = profileObj["Name"]?.ToObject<string>(),
                            Groups = new ObservableCollection<EntityCollectionFilterGroup>()
                        };

                        if ( profileObj["Groups"] != null )
                        {
                            foreach ( JToken groupObj in profileObj["Groups"] )
                            {
                                EntityCollectionFilterGroup group = new EntityCollectionFilterGroup { Operation = groupObj["Operation"]?.ToObject<BooleanOperation>() ?? 0 };

                                if ( groupObj["Items"] != null )
                                {
                                    foreach ( JToken itemObj in groupObj["Items"] )
                                    {
                                        EntityCollectionFilterItem item = new EntityCollectionFilterItem
                                        {
                                            Constraint =
                                                Constraints.FirstOrDefault( e => e.Name == itemObj["Constraint"]?["Name"]?.ToObject<string>() ) ?? Constraints.FirstOrDefault(),
                                            Operator = itemObj["Operator"]?.ToObject<AutolootOperator>() ?? 0,
                                            Value = itemObj["Value"]?.ToObject<int>() ?? 0,
                                            Additional = itemObj["Additional"]?.ToObject<string>()
                                        };

                                        group.Items.Add( item );
                                    }
                                }

                                profile.Groups.Add( group );
                            }
                        }

                        Profiles.Add( profile );
                    }
                }

                if ( Profiles != null && Profiles.Count > 0 )
                {
                    SelectedProfile = Item = Profiles.FirstOrDefault( e => e.ID == selectedProfile ) ?? Profiles[0];
                }
                else
                {
                    AddDefaultEntry();
                }
            }
            catch ( Exception ex )
            {
                SentrySdk.CaptureException( ex );

                AddDefaultEntry();
            }
        }

        public void SaveFilterProfiles()
        {
            string file = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "FilterProfiles.json" );

            JObject obj = new JObject { { "LastProfileID", SelectedProfile.ID } };

            JArray profiles = new JArray();

            foreach ( EntityCollectionFilterEntry profile in Profiles )
            {
                JObject profileObj = new JObject { { "ID", profile.ID }, { "Name", profile.Name } };

                JArray groups = new JArray();

                foreach ( EntityCollectionFilterGroup group in profile.Groups )
                {
                    JObject groupObj = new JObject { { "Operation", (int) group.Operation } };

                    JArray items = new JArray();

                    foreach ( EntityCollectionFilterItem item in group.Items )
                    {
                        JObject itemObj = new JObject { { "Operator", (int) item.Operator }, { "Value", item.Value }, { "Additional", item.Additional } };

                        JObject constraint = new JObject { { "Name", item.Constraint.Name } };

                        itemObj.Add( "Constraint", constraint );

                        items.Add( itemObj );
                    }

                    groupObj.Add( "Items", items );

                    groups.Add( groupObj );
                }

                profileObj.Add( "Groups", groups );

                profiles.Add( profileObj );
            }

            obj.Add( "Profiles", profiles );

            File.WriteAllText( file, JsonConvert.SerializeObject( obj, Formatting.Indented ) );
        }

        private void AddDefaultEntry()
        {
            SelectedProfile = Item = new EntityCollectionFilterEntry
            {
                Name = "Default",
                Groups = new ObservableCollection<EntityCollectionFilterGroup>
                {
                    new EntityCollectionFilterGroup
                    {
                        Items = new ObservableCollection<EntityCollectionFilterItem> { new EntityCollectionFilterItem { Constraint = Constraints.FirstOrDefault() } }
                    }
                }
            };

            Profiles = new ObservableCollection<EntityCollectionFilterEntry> { Item };
        }

        private void LoadCustomProperties()
        {
            JsonSerializer serializer = new JsonSerializer();

            using ( StreamReader sr = new StreamReader( _propertiesFileCustom ) )
            {
                using ( JsonTextReader reader = new JsonTextReader( sr ) )
                {
                    PropertyEntry[] constraints = serializer.Deserialize<PropertyEntry[]>( reader );

                    foreach ( PropertyEntry constraint in constraints )
                    {
                        Constraints.AddSorted( constraint );
                    }
                }
            }
        }

        private void NewGroup( object obj )
        {
            if ( !( obj is EntityCollectionFilterGroup group ) )
            {
                return;
            }

            Item.Groups.Insert( Item.Groups.IndexOf( group ) + 1,
                new EntityCollectionFilterGroup
                {
                    Items = new ObservableCollection<EntityCollectionFilterItem> { new EntityCollectionFilterItem { Constraint = Constraints.First() } }
                } );
        }

        private void RemoveGroup( object obj )
        {
            if ( !( obj is EntityCollectionFilterGroup group ) )
            {
                return;
            }

            Item.Groups.Remove( group );

            OnPropertyChanged( nameof( Item ) );
        }

        private void Apply( object obj )
        {
            Command?.Execute( Item.Groups.ToList() );
        }

        private void Reset( object obj )
        {
            Command?.Execute( null );
        }

        private void Remove( object obj )
        {
            if ( !( obj is GroupItem groupItem ) )
            {
                return;
            }

            groupItem.Group.Remove( groupItem.Item );
        }

        private void AddItem( object obj )
        {
            if ( !( obj is ObservableCollection<EntityCollectionFilterItem> collection ) )
            {
                return;
            }

            collection.Add( new EntityCollectionFilterItem { Constraint = Constraints.FirstOrDefault() } );
        }
    }
}