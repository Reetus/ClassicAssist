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
                Assembly asm = Assembly.LoadFrom( fileName );
                IEnumerable<MethodInfo> initializeMethods = asm.GetTypes()
                    .Where( e => e.IsClass && e.IsPublic && e.GetMethod( "Initialize", BindingFlags.Public | BindingFlags.Static, null,
                        new[] { typeof( ObservableCollection<PropertyEntry> ) }, null ) != null ).Select( e =>
                        e.GetMethod( "Initialize", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof( ObservableCollection<PropertyEntry> ) }, null ) );

                foreach ( MethodInfo initializeMethod in initializeMethods )
                {
                    // ReSharper disable once CoVariantArrayConversion
                    initializeMethod?.Invoke( null, new[] { constraints } );
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
                Predicate = (item, entry) =>
                {
                    Layer layer = TileData.GetLayer(item.ID);

                    return entry.Operator == AutolootOperator.NotPresent || AutolootHelpers.Operation(entry.Operator, entry.Value, (int)layer);
                },
                AllowedValuesEnum = typeof(Layer)
            });

            constraints.AddSorted(new PropertyEntry
            {
                Name = Strings.Skill_Bonus,
                ConstraintType = PropertyType.PredicateWithValue,
                Predicate = (item, entry) =>
                {
                    int[] clilocs = { 1060451, 1060452, 1060453, 1060454, 1060455, 1072394, 1072395 };

                    if (item.Properties == null)
                    {
                        return false;
                    }

                    IEnumerable<Property> properties = item.Properties.Where(e => e != null && clilocs.Contains(e.Cliloc)).ToList();

                    if (entry.Operator != AutolootOperator.NotPresent)
                    {
                        return properties
                            .Where(property => property.Arguments != null && property.Arguments.Length >= 1 &&
                                                (property.Arguments[0].Equals(entry.Additional, StringComparison.CurrentCultureIgnoreCase) ||
                                                  string.IsNullOrEmpty(entry.Additional))).Any(property =>
                                AutolootHelpers.Operation(entry.Operator, Convert.ToInt32(property.Arguments[1]), entry.Value));
                    }

                    Property match = properties.FirstOrDefault(property =>
                        property.Arguments != null && property.Arguments.Length >= 1 &&
                        (property.Arguments[0].Equals(entry.Additional, StringComparison.CurrentCultureIgnoreCase) || string.IsNullOrEmpty(entry.Additional)));

                    return match == null;
                },
                AllowedValuesEnum = typeof(SkillBonusSkills)
            });


            return;

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
    }
}