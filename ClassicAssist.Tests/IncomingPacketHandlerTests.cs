using System.Linq;
using Assistant;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests
{
    [TestClass]
    public class IncomingPacketHandlerTests
    {
        [TestInitialize]
        public void Initialize()
        {
            Engine.Items = new ItemCollection( 0 );
            Engine.Mobiles = new MobileCollection( Engine.Items );
            Engine.Player = new PlayerMobile( 1 );

            IncomingPacketHandlers.Initialize();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Engine.Player = null;
            Engine.Items = new ItemCollection( 0 );
            Engine.Mobiles = new MobileCollection( Engine.Items );
        }

        [TestMethod]
        public void ContainerContentsWillAddToPlayerBackpack()
        {
            const int backpackSerial = 0x40000001;
            const int itemSerial = 0x40000002;

            Item backpack = new Item( backpackSerial, 1 ) { Layer = Layer.Backpack };
            Engine.Player.SetLayer( Layer.Backpack, backpack.Serial );
            Engine.Items.Add( backpack );

            Assert.IsTrue( Engine.Player.Backpack.Serial > 0 );

            ContainerContentsPacket cc =
                new ContainerContentsPacket( new ItemCollection( backpackSerial ) { new Item( itemSerial ) } );

            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x3C );

            Assert.IsNotNull( handler );

            byte[] packet = cc.ToArray();

            handler.OnReceive( new PacketReader( packet, packet.Length, false ) );

            Item item = Engine.Items.GetItem( backpackSerial );

            Assert.IsNotNull( item.Container );
            Assert.AreEqual( 1, item.Container.GetTotalItemCount() );
            Assert.IsNotNull( Engine.Items.GetItem( itemSerial ) );
            Assert.IsNotNull( item.Container.GetItem( itemSerial ) );

            Engine.Items.Clear();
            Engine.Player.SetLayer( Layer.Backpack, 0 );
        }

        private const int NESTED_BACKPACK_SERIAL = 0x40000001;
        private const int NESTED_BAG_SERIAL = 0x40000002;
        private const int NESTED_ITEM1_SERIAL = 0x40000003;
        private const int NESTED_ITEM2_SERIAL = 0x40000004;

        private static void SetupNestedBackpack()
        {
            Item backpack = new Item( NESTED_BACKPACK_SERIAL, 1 ) { Layer = Layer.Backpack };
            Engine.Player.SetLayer( Layer.Backpack, backpack.Serial );
            Engine.Items.Add( backpack );
        }

        private static void ReceiveContainerContents( int containerSerial, params int[] itemSerials )
        {
            ItemCollection collection = new ItemCollection( containerSerial );

            foreach ( int serial in itemSerials )
            {
                collection.Add( new Item( serial ) { ID = 0xff } );
            }

            byte[] packet = new ContainerContentsPacket( collection ).ToArray();

            IncomingPacketHandlers.GetHandler( 0x3C ).OnReceive( new PacketReader( packet, packet.Length, false ) );
        }

        private static void AssertBagHeldOnlyInBackpack()
        {
            Assert.IsFalse( Engine.Items.GetItems().Any( i => i.Serial == NESTED_BAG_SERIAL ), "Bag should not be held at the top level." );

            Item backpack = Engine.Items.GetItem( NESTED_BACKPACK_SERIAL );
            Item bag = backpack.Container.GetItem( NESTED_BAG_SERIAL );

            Assert.IsNotNull( bag, "Bag should be in the backpack." );
            Assert.AreEqual( 2, bag.Container.GetItemCount(), "Bag contents should be kept." );

            Item[] children = Engine.Items.SelectEntities( i => i.IsDescendantOf( NESTED_BAG_SERIAL, 0 ) );

            Assert.IsNotNull( children );
            CollectionAssert.AreEquivalent( new[] { NESTED_ITEM1_SERIAL, NESTED_ITEM2_SERIAL }, children.Select( i => i.Serial ).ToArray() );
        }

        [TestMethod]
        public void NestedContainerOpenedAfterParentIsNotHeldAtTopLevel()
        {
            SetupNestedBackpack();

            ReceiveContainerContents( NESTED_BACKPACK_SERIAL, NESTED_BAG_SERIAL );
            ReceiveContainerContents( NESTED_BAG_SERIAL, NESTED_ITEM1_SERIAL, NESTED_ITEM2_SERIAL );

            AssertBagHeldOnlyInBackpack();

            Engine.Player.SetLayer( Layer.Backpack, 0 );
        }

        [TestMethod]
        public void NestedContainerOpenedBeforeParentIsMovedIntoParent()
        {
            SetupNestedBackpack();

            // Parent unknown when the bag is opened, so it's held at the top level as a fallback
            ReceiveContainerContents( NESTED_BAG_SERIAL, NESTED_ITEM1_SERIAL, NESTED_ITEM2_SERIAL );

            Assert.IsTrue( Engine.Items.GetItems().Any( i => i.Serial == NESTED_BAG_SERIAL ) );

            ReceiveContainerContents( NESTED_BACKPACK_SERIAL, NESTED_BAG_SERIAL );

            AssertBagHeldOnlyInBackpack();

            Engine.Player.SetLayer( Layer.Backpack, 0 );
        }

        [TestMethod]
        public void NestedContainerAddedToParentIsRemovedFromTopLevel()
        {
            SetupNestedBackpack();

            ReceiveContainerContents( NESTED_BAG_SERIAL, NESTED_ITEM1_SERIAL, NESTED_ITEM2_SERIAL );

            byte[] packet = new ContainerContentUpdate( NESTED_BAG_SERIAL, 0xe75, Direction.North, 1, 0, 0, 0, NESTED_BACKPACK_SERIAL, 0 ).ToArray();

            IncomingPacketHandlers.GetHandler( 0x25 ).OnReceive( new PacketReader( packet, packet.Length, true ) );

            AssertBagHeldOnlyInBackpack();

            Engine.Player.SetLayer( Layer.Backpack, 0 );
        }

        [TestMethod]
        public void SelectEntitiesWillNotReturnDuplicatesWhenContainerReachableTwice()
        {
            Item backpack = new Item( NESTED_BACKPACK_SERIAL ) { Container = new ItemCollection( NESTED_BACKPACK_SERIAL ) };
            Item bag = new Item( NESTED_BAG_SERIAL, NESTED_BACKPACK_SERIAL ) { Container = new ItemCollection( NESTED_BAG_SERIAL ) };

            bag.Container.Add( new Item( NESTED_ITEM1_SERIAL, NESTED_BAG_SERIAL ) );
            bag.Container.Add( new Item( NESTED_ITEM2_SERIAL, NESTED_BAG_SERIAL ) );
            backpack.Container.Add( bag );

            Engine.Items.Add( backpack );
            Engine.Items.Add( bag );

            Item[] children = Engine.Items.SelectEntities( i => i.Owner == NESTED_BAG_SERIAL );

            Assert.AreEqual( 2, children.Length );
        }

        [TestMethod]
        public void WillSetPoisonedOnHealthbarColour()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            byte[] packet = { 0x17, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x01 };

            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x17 );

            Assert.IsNotNull( handler );

            handler.OnReceive( new PacketReader( packet, packet.Length, false ) );

            Assert.IsTrue( Engine.Player.IsPoisoned );

            packet = new byte[] { 0x17, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00 };

            handler.OnReceive( new PacketReader( packet, packet.Length, false ) );

            Assert.IsFalse( Engine.Player.IsPoisoned );

            Engine.Player = null;
        }

        [TestMethod]
        public void WillSetYellowHitsOnHealthbarColour()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            byte[] packet = { 0x17, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x02, 0x01 };

            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x17 );

            Assert.IsNotNull( handler );

            handler.OnReceive( new PacketReader( packet, packet.Length, false ) );

            Assert.IsTrue( Engine.Player.IsYellowHits );

            packet = new byte[] { 0x17, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x02, 0x00 };

            handler.OnReceive( new PacketReader( packet, packet.Length, false ) );

            Assert.IsFalse( Engine.Player.IsYellowHits );

            Engine.Player = null;
        }

        [TestMethod]
        public void WillSetFrozenOnMobileStatus()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            Assert.IsFalse( Engine.Player.IsFrozen );

            byte[] packet =
            {
                0x20, 0x00, 0x00, 0x00, 0x01, 0x01, 0x90, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00
            };

            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x20 );

            Assert.IsNotNull( handler );

            handler.OnReceive( new PacketReader( packet, packet.Length, true ) );

            Assert.IsTrue( Engine.Player.IsFrozen );

            Engine.Player = null;
        }

        [TestMethod]
        public void WillFireItemPropertiesUpdatedEventOnProperties()
        {
            const int itemSerial = 0x40000002;

            // 0xD6 (Object Property List): id, length, word, serial, word, hash, then a zero cliloc
            // terminator (empty property list keeps the test independent of Cliloc data files).
            byte[] packet =
            {
                0xD6,
                0x00, 0x13, // length
                0x00, 0x01, // word 1
                0x40, 0x00, 0x00, 0x02, // serial
                0x00, 0x00, // word 0
                0x00, 0x00, 0x00, 0x00, // hash
                0x00, 0x00, 0x00, 0x00 // cliloc terminator
            };

            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0xD6 );

            Assert.IsNotNull( handler );

            Item firedItem = null;

            void OnItemPropertiesUpdated( Item item )
            {
                firedItem = item;
            }

            IncomingPacketHandlers.ItemPropertiesUpdatedEvent += OnItemPropertiesUpdated;

            try
            {
                handler.OnReceive( new PacketReader( packet, packet.Length, false ) );
            }
            finally
            {
                IncomingPacketHandlers.ItemPropertiesUpdatedEvent -= OnItemPropertiesUpdated;
            }

            Assert.IsNotNull( firedItem, "ItemPropertiesUpdatedEvent did not fire." );
            Assert.AreEqual( itemSerial, firedItem.Serial );
        }
    }
}