#region License

// Copyright (C) 2026 Reetus
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
using System.Collections.ObjectModel;
using System.Linq;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UI.Views.ECV.Filter.Models;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.ECV
{
    [TestClass]
    public class EntityCollectionFilterGroupTests
    {
        // Predicate-based constraint matching on item ID, so filtering behaviour can be asserted
        // without touching game state.
        private static readonly PropertyEntry IdConstraint = new PropertyEntry
        {
            Name = "Test ID",
            ConstraintType = PropertyType.Predicate,
            Predicate = ( entity, entry ) =>
            {
                switch ( entry.Operator )
                {
                    case AutolootOperator.Equal:
                        return entity.ID == entry.Value;
                    case AutolootOperator.NotEqual:
                        return entity.ID != entry.Value;
                    case AutolootOperator.GreaterThan:
                        return entity.ID >= entry.Value;
                    case AutolootOperator.LessThan:
                        return entity.ID <= entry.Value;
                    default:
                        return false;
                }
            }
        };

        private static ItemCollection CreateSource( params int[] ids )
        {
            ItemCollection collection = new ItemCollection( 0x40000000 );
            int serial = 0x40000001;

            foreach ( int id in ids )
            {
                collection.Add( new Item( serial++ ) { ID = id } );
            }

            return collection;
        }

        private static List<int> GetIds( ItemCollection collection )
        {
            return collection.GetItems().Select( i => i.ID ).OrderBy( i => i ).ToList();
        }

        private static EntityCollectionFilterItem IdFilter( AutolootOperator op, int value )
        {
            return new EntityCollectionFilterItem { Constraint = IdConstraint, Operator = op, Value = value };
        }

        private static EntityCollectionFilterItem IdFilter( int value )
        {
            return IdFilter( AutolootOperator.Equal, value );
        }

        private static EntityCollectionFilterGroup LeafGroup( params EntityCollectionFilterItem[] items )
        {
            return new EntityCollectionFilterGroup { Items = new ObservableCollection<EntityCollectionFilterItem>( items ) };
        }

        private static EntityCollectionFilterGroup LeafGroup( BooleanOperation operation, params EntityCollectionFilterItem[] items )
        {
            return new EntityCollectionFilterGroup { Operation = operation, Items = new ObservableCollection<EntityCollectionFilterItem>( items ) };
        }

        private static EntityCollectionFilterGroup BranchGroup( params EntityCollectionFilterGroup[] children )
        {
            EntityCollectionFilterGroup group = new EntityCollectionFilterGroup();

            foreach ( EntityCollectionFilterGroup child in children )
            {
                group.Children.Add( child );
            }

            return group;
        }

        [TestMethod]
        public void SingleLeafGroupFiltersMatchingItems()
        {
            ItemCollection source = CreateSource( 0x01, 0x02, 0x03, 0x04 );

            ItemCollection result = EntityCollectionViewerViewModel.EvaluateGroups(
                new List<EntityCollectionFilterGroup> { LeafGroup( IdFilter( 0x02 ) ) }, source );

            CollectionAssert.AreEqual( new[] { 0x02 }, GetIds( result ) );
        }

        [TestMethod]
        public void SingleTopLevelGroupOperationIsIgnored()
        {
            ItemCollection source = CreateSource( 0x01, 0x02, 0x03, 0x04 );

            ItemCollection result = EntityCollectionViewerViewModel.EvaluateGroups(
                new List<EntityCollectionFilterGroup> { LeafGroup( BooleanOperation.Not, IdFilter( 0x02 ) ) }, source );

            CollectionAssert.AreEqual( new[] { 0x02 }, GetIds( result ) );
        }

        [TestMethod]
        public void MultipleItemsInGroupAreAnded()
        {
            ItemCollection source = CreateSource( 0x01, 0x02, 0x03, 0x04 );

            ItemCollection result = EntityCollectionViewerViewModel.EvaluateGroups(
                new List<EntityCollectionFilterGroup>
                {
                    LeafGroup( IdFilter( AutolootOperator.GreaterThan, 0x02 ), IdFilter( AutolootOperator.LessThan, 0x03 ) )
                },
                source );

            CollectionAssert.AreEqual( new[] { 0x02, 0x03 }, GetIds( result ) );
        }

        [TestMethod]
        public void MultipleTopLevelGroupsAreAnded()
        {
            ItemCollection source = CreateSource( 0x01, 0x02, 0x03, 0x04 );

            List<EntityCollectionFilterGroup> groups = new List<EntityCollectionFilterGroup>
            {
                LeafGroup( IdFilter( AutolootOperator.LessThan, 0x03 ) ),
                LeafGroup( BooleanOperation.And, IdFilter( AutolootOperator.GreaterThan, 0x02 ) )
            };

            ItemCollection result = EntityCollectionViewerViewModel.EvaluateGroups( groups, source );

            CollectionAssert.AreEqual( new[] { 0x02, 0x03 }, GetIds( result ) );
        }

        [TestMethod]
        public void MultipleTopLevelGroupsAreOred()
        {
            ItemCollection source = CreateSource( 0x01, 0x02, 0x03, 0x04 );

            List<EntityCollectionFilterGroup> groups = new List<EntityCollectionFilterGroup>
            {
                LeafGroup( IdFilter( 0x01 ) ),
                LeafGroup( BooleanOperation.Or, IdFilter( 0x03 ) )
            };

            ItemCollection result = EntityCollectionViewerViewModel.EvaluateGroups( groups, source );

            CollectionAssert.AreEqual( new[] { 0x01, 0x03 }, GetIds( result ) );
        }

        [TestMethod]
        public void MultipleTopLevelGroupsAreNotted()
        {
            ItemCollection source = CreateSource( 0x01, 0x02, 0x03, 0x04 );

            List<EntityCollectionFilterGroup> groups = new List<EntityCollectionFilterGroup>
            {
                LeafGroup( IdFilter( AutolootOperator.NotEqual, 0x99 ) ),
                LeafGroup( BooleanOperation.Not, IdFilter( 0x02 ) )
            };

            ItemCollection result = EntityCollectionViewerViewModel.EvaluateGroups( groups, source );

            CollectionAssert.AreEqual( new[] { 0x01, 0x03, 0x04 }, GetIds( result ) );
        }

        [TestMethod]
        public void SubGroupsAreAnded()
        {
            ItemCollection source = CreateSource( 0x01, 0x02, 0x03, 0x04 );

            EntityCollectionFilterGroup group = BranchGroup(
                LeafGroup( IdFilter( AutolootOperator.LessThan, 0x03 ) ),
                LeafGroup( BooleanOperation.And, IdFilter( AutolootOperator.GreaterThan, 0x02 ) ) );

            ItemCollection result = EntityCollectionViewerViewModel.EvaluateGroups(
                new List<EntityCollectionFilterGroup> { group }, source );

            CollectionAssert.AreEqual( new[] { 0x02, 0x03 }, GetIds( result ) );
        }

        [TestMethod]
        public void SubGroupsAreOred()
        {
            ItemCollection source = CreateSource( 0x01, 0x02, 0x03, 0x04 );

            EntityCollectionFilterGroup group = BranchGroup(
                LeafGroup( IdFilter( 0x01 ) ),
                LeafGroup( BooleanOperation.Or, IdFilter( 0x03 ) ) );

            ItemCollection result = EntityCollectionViewerViewModel.EvaluateGroups(
                new List<EntityCollectionFilterGroup> { group }, source );

            CollectionAssert.AreEqual( new[] { 0x01, 0x03 }, GetIds( result ) );
        }

        [TestMethod]
        public void SubGroupsAreNotted()
        {
            ItemCollection source = CreateSource( 0x01, 0x02, 0x03, 0x04 );

            EntityCollectionFilterGroup group = BranchGroup(
                LeafGroup( IdFilter( AutolootOperator.NotEqual, 0x99 ) ),
                LeafGroup( BooleanOperation.Not, IdFilter( 0x02 ) ) );

            ItemCollection result = EntityCollectionViewerViewModel.EvaluateGroups(
                new List<EntityCollectionFilterGroup> { group }, source );

            CollectionAssert.AreEqual( new[] { 0x01, 0x03, 0x04 }, GetIds( result ) );
        }

        [TestMethod]
        public void FirstChildOperationIsIgnored()
        {
            ItemCollection source = CreateSource( 0x01, 0x02, 0x03, 0x04 );

            EntityCollectionFilterGroup group = BranchGroup(
                LeafGroup( BooleanOperation.Not, IdFilter( 0x01 ) ),
                LeafGroup( BooleanOperation.Or, IdFilter( 0x02 ) ) );

            ItemCollection result = EntityCollectionViewerViewModel.EvaluateGroups(
                new List<EntityCollectionFilterGroup> { group }, source );

            CollectionAssert.AreEqual( new[] { 0x01, 0x02 }, GetIds( result ) );
        }

        [TestMethod]
        public void BranchGroupIgnoresItsOwnItems()
        {
            ItemCollection source = CreateSource( 0x01, 0x02, 0x03, 0x04 );

            EntityCollectionFilterGroup group = BranchGroup( LeafGroup( IdFilter( 0x02 ) ) );
            group.Items = new ObservableCollection<EntityCollectionFilterItem> { IdFilter( 0x01 ) };

            ItemCollection result = EntityCollectionViewerViewModel.EvaluateGroups(
                new List<EntityCollectionFilterGroup> { group }, source );

            CollectionAssert.AreEqual( new[] { 0x02 }, GetIds( result ) );
        }

        [TestMethod]
        public void DeeplyNestedSubGroupsFilter()
        {
            ItemCollection source = CreateSource( 0x01, 0x02, 0x03, 0x04 );

            EntityCollectionFilterGroup group = BranchGroup(
                BranchGroup(
                    LeafGroup( IdFilter( 0x01 ) ),
                    LeafGroup( BooleanOperation.Or, IdFilter( 0x02 ) ) ),
                LeafGroup( BooleanOperation.Or, IdFilter( 0x03 ) ) );

            ItemCollection result = EntityCollectionViewerViewModel.EvaluateGroups(
                new List<EntityCollectionFilterGroup> { group }, source );

            CollectionAssert.AreEqual( new[] { 0x01, 0x02, 0x03 }, GetIds( result ) );
        }

        [TestMethod]
        public void EmptyGroupListReturnsSource()
        {
            ItemCollection source = CreateSource( 0x01, 0x02, 0x03, 0x04 );

            ItemCollection result = EntityCollectionViewerViewModel.EvaluateGroups( new List<EntityCollectionFilterGroup>(), source );

            CollectionAssert.AreEqual( new[] { 0x01, 0x02, 0x03, 0x04 }, GetIds( result ) );
        }
    }
}
