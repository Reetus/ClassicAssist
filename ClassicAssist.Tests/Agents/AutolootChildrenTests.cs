#region License

// Copyright (C) 2024 Reetus
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

#endregion

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.UI.ViewModels.Agents;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.Agents
{
    [TestClass]
    public class AutolootChildrenTests
    {
        [TestMethod]
        public void WillMatchChildrenAll()
        {
            Cliloc.Initialize( () => new Dictionary<int, string> { { 1060415, "hit chance increase ~1_val~%" }, { 1060434, "lower reagent cost ~1_val~%" } } );

            AutolootViewModel vm = new AutolootViewModel { Enabled = true };

            Item item = new Item( 0x40000001 ) { ID = 0x108a, Properties = new[] { new Property { Cliloc = 1060415, Arguments = new[] { "15" } } } };
            Item item2 = new Item( 0x40000002 ) { ID = 0x1086, Properties = item.Properties };
            Item item3 = new Item( 0x40000002 ) { ID = 0x1086, Properties = new[] { new Property { Cliloc = 1060434, Arguments = new[] { "15" } } } };

            PopulatePropertyText( new[] { item, item2, item3 } );

            AutolootEntry entry = new AutolootEntry
            {
                ID = -1,
                Children = new ObservableCollection<AutolootBaseModel>
                {
                    new AutolootPropertyGroup
                    {
                        Operation = BooleanOperation.Or,
                        Children = new ObservableCollection<AutolootBaseModel>
                        {
                            new AutolootPropertyGroup
                            {
                                Children = new ObservableCollection<AutolootBaseModel>
                                {
                                    new AutolootPropertyEntry
                                    {
                                        Constraints = new ObservableCollection<AutolootConstraintEntry>
                                        {
                                            new AutolootConstraintEntry
                                            {
                                                Property = vm.Constraints.FirstOrDefault( e => e.Name == "Hit Chance Increase" ),
                                                Operator = AutolootOperator.LessThan,
                                                Value = 15
                                            }
                                        }
                                    }
                                }
                            },
                            new AutolootPropertyGroup
                            {
                                Children = new ObservableCollection<AutolootBaseModel>
                                {
                                    new AutolootPropertyEntry
                                    {
                                        Constraints = new ObservableCollection<AutolootConstraintEntry>
                                        {
                                            new AutolootConstraintEntry
                                            {
                                                Property = vm.Constraints.FirstOrDefault( e => e.Name == "Defense Chance Increase" ),
                                                Operator = AutolootOperator.LessThan,
                                                Value = 15
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new AutolootPropertyGroup
                    {
                        Children = new ObservableCollection<AutolootBaseModel>
                        {
                            new AutolootPropertyEntry
                            {
                                Constraints = new ObservableCollection<AutolootConstraintEntry>
                                {
                                    new AutolootConstraintEntry
                                    {
                                        Property = vm.Constraints.FirstOrDefault( e => e.Name == "ID" ),
                                        Operator = AutolootOperator.Equal,
                                        Value = 0x108a
                                    }
                                }
                            }
                        },
                        Operation = BooleanOperation.Not
                    }
                }
            };

            Assert.IsFalse( AutolootHelpers.MatchItemEntryChildren( item, entry ) );
            Assert.IsTrue( AutolootHelpers.MatchItemEntryChildren( item2, entry ) );
            Assert.IsFalse( AutolootHelpers.MatchItemEntryChildren( item3, entry ) );
        }

        [TestMethod]
        public void WillTestAnd()
        {
            AutolootViewModel vm = new AutolootViewModel { Enabled = true };

            Item item = new Item( 0x40000001 ) { Hue = 1000 };

            AutolootEntry entry = new AutolootEntry
            {
                Children = new ObservableCollection<AutolootBaseModel>
                {
                    new AutolootPropertyGroup
                    {
                        Operation = BooleanOperation.And,
                        Children =
                            new ObservableCollection<AutolootBaseModel>
                            {
                                new AutolootPropertyEntry
                                {
                                    Constraints = new ObservableCollection<AutolootConstraintEntry>
                                    {
                                        new AutolootConstraintEntry
                                        {
                                            Property = vm.Constraints.FirstOrDefault( e => e.Name == "Hue" ),
                                            Operator = AutolootOperator.GreaterThan,
                                            Value = 1
                                        }
                                    }
                                }
                            }
                    },
                    new AutolootPropertyGroup
                    {
                        Operation = BooleanOperation.And,
                        Children = new ObservableCollection<AutolootBaseModel>
                        {
                            new AutolootPropertyGroup
                            {
                                Children = new ObservableCollection<AutolootBaseModel>
                                {
                                    new AutolootPropertyEntry
                                    {
                                        Constraints = new ObservableCollection<AutolootConstraintEntry>
                                        {
                                            new AutolootConstraintEntry
                                            {
                                                Property = vm.Constraints.FirstOrDefault( e => e.Name == "Hue" ),
                                                Operator = AutolootOperator.NotEqual,
                                                Value = 1011
                                            }
                                        }
                                    }
                                }
                            },
                            new AutolootPropertyGroup
                            {
                                Operation = BooleanOperation.And,
                                Children = new ObservableCollection<AutolootBaseModel>
                                {
                                    new AutolootPropertyEntry
                                    {
                                        Constraints = new ObservableCollection<AutolootConstraintEntry>
                                        {
                                            new AutolootConstraintEntry
                                            {
                                                Property = vm.Constraints.FirstOrDefault( e => e.Name == "Hue" ),
                                                Operator = AutolootOperator.NotEqual,
                                                Value = 1012
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            Assert.IsTrue( AutolootHelpers.MatchItemEntryChildren( item, entry ) );
        }

        private static void PopulatePropertyText( IEnumerable<Item> items )
        {
            foreach ( Item item in items )
            {
                foreach ( Property property in item.Properties )
                {
                    property.Text = Cliloc.GetLocalString( property.Cliloc, property.Arguments );
                }
            }
        }
    }
}