using Assistant;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests
{
    [TestClass]
    public class ItemCollectionTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            Engine.Items.Clear();
        }

        [TestMethod]
        public void WillFireCollectionChangedOnRemoveOfNestedItem()
        {
            // Container registered in the top-level Engine.Items collection.
            Item container = new Item( 0x40000001 ) { Container = new ItemCollection( 0x40000001 ) };
            Engine.Items.Add( container );

            // Item nested inside the container's own collection.
            Item item = new Item( 0x40000002, 0x40000001 ) { ID = 0xff };
            container.Container.Add( item );

            bool removedFired = false;

            container.Container.CollectionChanged += ( total, added, entities ) =>
            {
                if ( !added )
                {
                    removedFired = true;
                }
            };

            // Simulate the server deleting the item (OnItemDeleted -> Engine.Items.Remove).
            Engine.Items.Remove( item.Serial );

            Assert.AreEqual( 0, container.Container.GetItemCount(), "Item was not removed from the container collection." );
            Assert.IsTrue( removedFired, "CollectionChanged did not fire for the removed item." );
        }

        [TestMethod]
        public void WillBubbleCollectionChangedToRootContainerFromNestedContainer()
        {
            // Three-level hierarchy: root -> sub -> subsub -> item.
            Item root = new Item( 0x40000001 ) { Container = new ItemCollection( 0x40000001 ) };
            Engine.Items.Add( root );

            Item sub = new Item( 0x40000002, 0x40000001 ) { Container = new ItemCollection( 0x40000002 ) };
            root.Container.Add( sub );

            Item subSub = new Item( 0x40000003, 0x40000002 ) { Container = new ItemCollection( 0x40000003 ) };
            sub.Container.Add( subSub );

            Item item = new Item( 0x40000004, 0x40000003 ) { ID = 0xff };
            subSub.Container.Add( item );

            bool addedFired = false;
            bool removedFired = false;

            // A viewer on the root container (as in ShowChildItems mode) subscribes here.
            root.Container.CollectionChanged += ( total, added, entities ) =>
            {
                if ( added )
                {
                    addedFired = true;
                }
                else
                {
                    removedFired = true;
                }
            };

            // Add a new deeply-nested item - should bubble to the root.
            Item newItem = new Item( 0x40000005, 0x40000003 ) { ID = 0xfe };
            subSub.Container.Add( newItem );

            Assert.IsTrue( addedFired, "Add of a deeply-nested item did not bubble up to the root container." );

            // Remove a deeply-nested item - should bubble to the root.
            Engine.Items.Remove( item.Serial );

            Assert.IsTrue( removedFired, "Removal of a deeply-nested item did not bubble up to the root container." );
        }

        [TestMethod]
        public void WillFireCollectionChangedOnRemoveOfDirectItem()
        {
            ItemCollection collection = new ItemCollection( 0x40000001 );

            Item item = new Item( 0x40000002, 0x40000001 ) { ID = 0xff };
            collection.Add( item );

            bool removedFired = false;

            collection.CollectionChanged += ( total, added, entities ) =>
            {
                if ( !added )
                {
                    removedFired = true;
                }
            };

            collection.Remove( item.Serial );

            Assert.AreEqual( 0, collection.GetItemCount() );
            Assert.IsTrue( removedFired, "CollectionChanged did not fire for the removed item." );
        }
    }
}
