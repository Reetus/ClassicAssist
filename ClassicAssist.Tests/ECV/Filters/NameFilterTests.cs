#region License

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

using System.Collections.Generic;
using System.Linq;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UI.Views.ECV.Filter;
using ClassicAssist.UI.Views.ECV.Filter.Models;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.ECV.Filters
{
    [TestClass]
    public class NameFilterTests
    {
        [TestMethod]
        public void WillFilterItemNameNoProperties()
        {
            EntityCollectionFilterViewModel filterViewModel = new EntityCollectionFilterViewModel();
            PropertyEntry nameConstraint = filterViewModel.Constraints.FirstOrDefault( e => e.Name == "Name" );

            Item item = new Item( 0x40000000 ) { Name = "Test Item" };
            Item item2 = new Item( 0x40000001 ) { Name = "Unrelated" };

            ItemCollection itemCollection = new ItemCollection( 0x40000002 ) { item, item2 };

            EntityCollectionFilterItem filter =
                new EntityCollectionFilterItem { Constraint = nameConstraint, Operator = AutolootOperator.Equal, Value = 0, Additional = "Test Item" };

            ItemCollection newCollection = itemCollection.Filter( new[] { filter } );

            Assert.AreEqual( 1, newCollection.GetItemCount() );
        }

        [TestMethod]
        public void WillFilterItemNameProperties()
        {
            Cliloc.Initialize( () => new Dictionary<int, string> { { 1, "Test Item" }, { 1060413, "faster casting ~NUMBER~" } } );

            EntityCollectionFilterViewModel filterViewModel = new EntityCollectionFilterViewModel();
            PropertyEntry nameConstraint = filterViewModel.Constraints.FirstOrDefault( e => e.Name == "Name" );

            Item item = new Item( 0x40000000 ) { Properties = new[] { new Property { Text = Cliloc.GetProperty( 1 ) } } };
            Item item2 = new Item( 0x40000001 ) { Properties = new[] { new Property { Text = Cliloc.GetLocalString( 1060413, new[] { "3" } ) } } };

            ItemCollection itemCollection = new ItemCollection( 0x40000002 ) { item, item2 };

            EntityCollectionFilterItem filter =
                new EntityCollectionFilterItem { Constraint = nameConstraint, Operator = AutolootOperator.Equal, Value = 0, Additional = "faster casting 3" };

            ItemCollection newCollection = itemCollection.Filter( new[] { filter } );

            Assert.AreEqual( 1, newCollection.GetItemCount() );
        }

        [TestMethod]
        public void WillFilterItemNameCaseInsensitive()
        {
            EntityCollectionFilterViewModel filterViewModel = new EntityCollectionFilterViewModel();
            PropertyEntry nameConstraint = filterViewModel.Constraints.FirstOrDefault( e => e.Name == "Name" );

            Item item = new Item( 0x40000000 ) { Name = "Test Item" };
            Item item2 = new Item( 0x40000001 ) { Name = "Unrelated" };

            ItemCollection itemCollection = new ItemCollection( 0x40000002 ) { item, item2 };

            EntityCollectionFilterItem filter =
                new EntityCollectionFilterItem { Constraint = nameConstraint, Operator = AutolootOperator.Equal, Value = 0, Additional = "test item" };

            ItemCollection newCollection = itemCollection.Filter( new[] { filter } );

            Assert.AreEqual( 1, newCollection.GetItemCount() );
        }

        [TestMethod]
        public void WillFilterItemNameNoPropertiesNotPresent()
        {
            EntityCollectionFilterViewModel filterViewModel = new EntityCollectionFilterViewModel();
            PropertyEntry nameConstraint = filterViewModel.Constraints.FirstOrDefault( e => e.Name == "Name" );

            Item item = new Item( 0x40000000 ) { Name = "Test Item" };

            ItemCollection itemCollection = new ItemCollection( 0x40000002 ) { item };

            EntityCollectionFilterItem filter =
                new EntityCollectionFilterItem { Constraint = nameConstraint, Operator = AutolootOperator.NotPresent, Value = 0, Additional = "Test Item" };

            ItemCollection newCollection = itemCollection.Filter( new[] { filter } );

            Assert.AreEqual( 0, newCollection.GetItemCount() );
        }

        [TestMethod]
        public void WillFilterItemNamePropertiesNotPresent()
        {
            Cliloc.Initialize( () => new Dictionary<int, string> { { 1, "Test Item" } } );

            EntityCollectionFilterViewModel filterViewModel = new EntityCollectionFilterViewModel();
            PropertyEntry nameConstraint = filterViewModel.Constraints.FirstOrDefault( e => e.Name == "Name" );

            Item item = new Item( 0x40000000 ) { Properties = new[] { new Property { Text = Cliloc.GetProperty( 1 ) } } };

            ItemCollection itemCollection = new ItemCollection( 0x40000002 ) { item };

            EntityCollectionFilterItem filter =
                new EntityCollectionFilterItem { Constraint = nameConstraint, Operator = AutolootOperator.NotPresent, Value = 0, Additional = "Test Item" };

            ItemCollection newCollection = itemCollection.Filter( new[] { filter } );

            Assert.AreEqual( 0, newCollection.GetItemCount() );
        }
    }
}