#region License

// Copyright (C) 2025 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using Assistant;
using ClassicAssist.Shared.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UI.ViewModels.Agents;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ClassicAssist.Data.Autoloot
{
    public class AutolootManager
    {
        private static AutolootManager _instance;
        private static readonly object _lock = new object();
        public Action<int, bool> CheckContainer { get; set; }
        public Func<IEnumerable<Item>, List<Item>> CheckItems { get; set; }

        public Func<List<AutolootEntry>> GetEntries { get; set; } = () => new List<AutolootEntry>();

        public Func<bool> IsEnabled { get; set; }
        public Func<bool> IsRunning { get; set; }
        public Func<bool> MatchTextValue { get; set; }
        public Action<bool> SetEnabled { get; set; }

        public static AutolootManager GetInstance()
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

                    _instance = new AutolootManager();
                    return _instance;
                }
            }

            return _instance;
        }

        public void LoadAssemblies( ObservableCollection<PropertyEntry> constraints )
        {
            if ( AssistantOptions.Assemblies == null )
            {
                return;
            }

            foreach ( string fileName in AssistantOptions.Assemblies )
            {
                try
                {
                    Assembly asm = Assembly.LoadFrom( fileName );
                    IEnumerable<MethodInfo> initializeMethods = asm.GetTypes()
                        .Where( e => e.IsClass && e.IsPublic && e.GetMethod( "Initialize", BindingFlags.Public | BindingFlags.Static, null,
                            new[] { typeof( ObservableCollection<PropertyEntry> ) }, null ) != null ).Select( e => e.GetMethod( "Initialize",
                            BindingFlags.Public | BindingFlags.Static, null, new[] { typeof( ObservableCollection<PropertyEntry> ) }, null ) );

                    foreach ( MethodInfo initializeMethod in initializeMethods )
                    {
                        // ReSharper disable once CoVariantArrayConversion
                        initializeMethod?.Invoke( null, new[] { constraints } );
                    }
                }
                catch ( Exception )
                {
                    // ignored
                }
            }
        }

        public void LoadProperties( ObservableCollection<PropertyEntry> constraints )
        {
            string propertiesFile = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Data", "Properties.json" );
            string propertiesFileCustom = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Data", "Properties.Custom.json" );

            if ( File.Exists( propertiesFile ) )
            {
                IEnumerable<PropertyEntry> propertyEntry = LoadFile( propertiesFile );

                foreach ( PropertyEntry constraint in propertyEntry )
                {
                    constraints.AddSorted( constraint );
                }
            }

            if ( File.Exists( propertiesFileCustom ) )
            {
                IEnumerable<PropertyEntry> propertyEntry = LoadFile( propertiesFileCustom );

                foreach ( PropertyEntry constraint in propertyEntry )
                {
                    constraints.AddSorted( constraint );
                }
            }

            constraints.AddSorted(new PropertyEntry
            {
                Name = Strings.Layer,
                ConstraintType = PropertyType.Predicate,
                AllowedOperators = AutolootAllowedOperators.Equal | AutolootAllowedOperators.NotEqual,
                Predicate = (item, entry) =>
                {
                    Layer layer = TileData.GetLayer(item.ID);

                    return entry.Operator == AutolootOperator.NotPresent || AutolootHelpers.Operation(entry.Operator, entry.Value, (int)layer);
                },
                AllowedValuesEnum = typeof(Layer)
            });

            constraints.AddSorted( new PropertyEntry
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

                    List<Property> properties = item.Properties.Where( e => e != null && clilocs.Contains( e.Cliloc ) ).ToList();

                    return MatchSkillBonus( entry, properties );
                },
                AllowedValuesEnum = typeof( SkillBonusSkills )
            } );

            constraints.AddSorted(new PropertyEntry
            {
                Name = Strings.ID__Multiple_,
                ConstraintType = PropertyType.PredicateWithValue,
                UseMultipleValues = true,
                AllowedOperators = AutolootAllowedOperators.Equal | AutolootAllowedOperators.NotEqual,
                Predicate = (item, entry) =>
                {
                    switch (entry.Operator)
                    {
                        case AutolootOperator.NotEqual:
                        case AutolootOperator.NotPresent:
                            return entry.Values == null || !entry.Values.Contains(item.ID);
                        case AutolootOperator.Equal:
                            return entry.Values != null && entry.Values.Contains(item.ID);
                        case AutolootOperator.GreaterThan:
                        case AutolootOperator.LessThan:
                        default:
                            return false;
                    }
                }
            });

            constraints.AddSorted( new PropertyEntry
            {
                Name = Strings.Autoloot_Match,
                ConstraintType = PropertyType.PredicateWithValue,
                AllowedOperators = AutolootAllowedOperators.Equal | AutolootAllowedOperators.NotEqual,
                Predicate = ( entity, entry ) =>
                {
                    AutolootEntry autoLootEntry = GetEntries().FirstOrDefault( ale => ale.Name == entry.Additional );

                    if ( autoLootEntry == null )
                    {
                        return false;
                    }

                    if ( !( entity is Item item ) )
                    {
                        return false;
                    }

                    IEnumerable<Item> matchItems = AutolootHelpers.AutolootFilter( new[] { item }, autoLootEntry );

                    if ( entry.Operator == AutolootOperator.NotEqual )
                    {
                        return !matchItems.Any();
                    }

                    return matchItems.Any();
                }
            } );

            constraints.AddSorted( new PropertyEntry
            {
                Name = Strings.Talisman_Skill_Bonus,
                ConstraintType = PropertyType.PredicateWithValue,
                Predicate = ( item, entry ) =>
                {
                    if ( item.Properties == null )
                    {
                        return false;
                    }

                    List<Property> properties = item.Properties.Where( e => e != null && e.Cliloc == 1072394 ).ToList();

                    return MatchSkillBonus( entry, properties );
                },
                AllowedValuesEnum = typeof( SkillBonusSkills )
            } );

            constraints.AddSorted( new PropertyEntry
            {
                Name = Strings.Talisman_Exceptional_Skill_Bonus,
                ConstraintType = PropertyType.PredicateWithValue,
                Predicate = ( item, entry ) =>
                {
                    if ( item.Properties == null )
                    {
                        return false;
                    }

                    List<Property> properties = item.Properties.Where( e => e != null && e.Cliloc == 1072395 ).ToList();

                    return MatchSkillBonus( entry, properties );
                },
                AllowedValuesEnum = typeof( SkillBonusSkills )
            } );

            IEnumerable<PropertyEntry> LoadFile( string fileName )
            {
                JsonSerializer serializer = new JsonSerializer();

                using ( StreamReader sr = new StreamReader( fileName ) )
                {
                    using ( JsonTextReader reader = new JsonTextReader( sr ) )
                    {
                        return serializer.Deserialize<PropertyEntry[]>( reader );
                    }
                }
            }
        }

        private static bool MatchSkillBonus( AutolootConstraintEntry entry, List<Property> properties )
        {
            if ( entry.Operator != AutolootOperator.NotPresent )
            {
                return properties.Where( property => PropertyMatches( entry, property ) )
                    .Any( property => AutolootHelpers.Operation( entry.Operator, Convert.ToInt32( property.Arguments[1] ), entry.Value ) );
            }

            Property match = properties.FirstOrDefault( property => PropertyMatches( entry, property ) );

            return match == null;

            bool PropertyMatches( AutolootConstraintEntry e, Property p )
            {
                return p.Arguments != null && p.Arguments.Length >= 1 && ( e.Additional == nameof( SkillBonusSkills.Any ) ||
                                                                           p.Arguments[0].Equals( e.Additional, StringComparison.CurrentCultureIgnoreCase ) ||
                                                                           string.IsNullOrEmpty( e.Additional ) );
            }
        }
    }
}