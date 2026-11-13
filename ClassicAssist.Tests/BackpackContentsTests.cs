using System.Collections.Generic;
using System.Linq;
using Assistant;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests
{
    /// <summary>
    ///     The item store must not lose the backpack's contents while the client still has them. Each test
    ///     here is a packet sequence (or a sweep) that the client survives: it ignores a 0x1D for the
    ///     player, never forgets the player by distance, and moves an item out of its old container when a
    ///     0x25 / 0x3C puts it in a new one.
    /// </summary>
    [TestClass]
    public class BackpackContentsTests
    {
        private const int PLAYER = 0x00AAB001;
        private const int BACKPACK = 0x40A25F52;
        private const int ATLAS = 0x42C21A11;
        private const int BANDAGES = 0x42C21A12;
        private const int POUCH = 0x42C21A13;
        private const int CHEST = 0x40B00001;
        private const int SMOKE_BOMBS = 0x43D3F2EC;
        private const int LOCKPICKS = 0x43D3F2ED;

        [TestInitialize]
        public void Initialize()
        {
            Engine.Items = new ItemCollection( 0 );
            Engine.Mobiles = new MobileCollection( Engine.Items );
            Engine.Player = new PlayerMobile( PLAYER ) { X = 1000, Y = 1000 };

            IncomingPacketHandlers.Initialize();

            Send( Equipped( BACKPACK, 0xE75, Layer.Backpack, PLAYER ), true );
            Send( ContainerContents( ( ATLAS, 0x9C16, 1, BACKPACK ), ( BANDAGES, 0xE21, 50, BACKPACK ),
                ( POUCH, 0xE79, 1, BACKPACK ) ), false );
        }

        [TestCleanup]
        public void Cleanup()
        {
            Engine.Player = null;
            Engine.Items = new ItemCollection( 0 );
            Engine.Mobiles = new MobileCollection( Engine.Items );
        }

        [TestMethod]
        public void SetupGivesTheBackpackItsContents()
        {
            AssertBackpackHolds( ATLAS, BANDAGES, POUCH );
        }

        /// <summary>
        ///     Clients drop a 0x1D that names the player. Handling it removed every item the player owns,
        ///     the backpack and all its contents included; the next equip packet then brought back an empty
        ///     backpack, which only ever regained what arrived afterwards.
        /// </summary>
        [TestMethod]
        public void DeleteObjectForThePlayerKeepsTheBackpack()
        {
            Send( DeleteObject( PLAYER ), true );

            // What follows a teleport: the backpack is listed as equipment again, then a restock.
            Send( Equipped( BACKPACK, 0xE75, Layer.Backpack, PLAYER ), true );
            Send( AddToContainer( SMOKE_BOMBS, 0x2808, 70, BACKPACK ), true );
            Send( AddToContainer( LOCKPICKS, 0x14FC, 72, BACKPACK ), true );

            AssertBackpackHolds( ATLAS, BANDAGES, POUCH, SMOKE_BOMBS, LOCKPICKS );
        }

        /// <summary>
        ///     0x20 puts the player in the mobile collection, so the distance sweep can pick it whenever
        ///     the coordinates it was given and the player's own disagree - the tick samples them while a
        ///     teleport is being applied.
        /// </summary>
        [TestMethod]
        public void DistanceSweepNeverTakesThePlayer()
        {
            Send( MobileUpdate( PLAYER, 3000, 3000 ), true );

            // Coordinates read before the teleport above landed.
            Engine.Mobiles.RemoveByDistance( 32, 1000, 1000 );

            AssertBackpackHolds( ATLAS, BANDAGES, POUCH );
        }

        /// <summary>
        ///     A server need not send a 0x1D before the 0x25 that moves an item. The client takes the item
        ///     out of the container it was in.
        /// </summary>
        [TestMethod]
        public void AddToContainerTakesTheItemOutOfItsOldContainer()
        {
            Engine.Items.Add( new Item( CHEST ) { ID = 0xE41, X = 1000, Y = 1000 } );
            Send( ContainerContents( ( SMOKE_BOMBS, 0x2808, 70, CHEST ) ), false );

            Send( AddToContainer( SMOKE_BOMBS, 0x2808, 70, BACKPACK ), true );

            Item chest = Engine.Items.GetItem( CHEST );

            Assert.IsNull( chest.Container.GetItem( SMOKE_BOMBS ), "the chest must no longer list the moved stack" );
            AssertBackpackHolds( ATLAS, BANDAGES, POUCH, SMOKE_BOMBS );
        }

        /// <summary>
        ///     The same move, then the player recalls away and the chest is swept. Removing the chest used
        ///     to remove everything its collection still listed from every other container too.
        /// </summary>
        [TestMethod]
        public void SweepingTheOldContainerKeepsItemsMovedOutOfIt()
        {
            Engine.Items.Add( new Item( CHEST ) { ID = 0xE41, X = 1000, Y = 1000 } );
            Send( ContainerContents( ( SMOKE_BOMBS, 0x2808, 70, CHEST ), ( LOCKPICKS, 0x14FC, 72, CHEST ) ), false );

            Send( AddToContainer( SMOKE_BOMBS, 0x2808, 70, BACKPACK ), true );
            Send( AddToContainer( LOCKPICKS, 0x14FC, 72, BACKPACK ), true );

            Engine.Items.RemoveByDistance( 32, 3000, 3000 );

            Assert.IsNull( Engine.Items.GetItem( CHEST ), "the chest is out of range and goes" );
            AssertBackpackHolds( ATLAS, BANDAGES, POUCH, SMOKE_BOMBS, LOCKPICKS );
        }

        /// <summary>
        ///     Independent of how a container came to list an item that now belongs elsewhere: removing
        ///     the container must only take what it still owns.
        /// </summary>
        [TestMethod]
        public void RemovingAContainerLeavesItemsOwnedElsewhere()
        {
            Item chest = new Item( CHEST ) { ID = 0xE41, X = 1000, Y = 1000, Container = new ItemCollection( CHEST ) };
            Engine.Items.Add( chest );

            // A stale entry: the atlas is the backpack's, yet the chest still lists it.
            chest.Container.Add( Engine.Items.GetItem( ATLAS ) );
            chest.Container.Add( new Item( LOCKPICKS, CHEST ) { ID = 0x14FC } );

            Engine.Items.RemoveByDistance( 32, 3000, 3000 );

            Assert.IsNull( Engine.Items.GetItem( CHEST ) );
            Assert.IsNull( Engine.Items.GetItem( LOCKPICKS ), "what the chest owned goes with it" );
            AssertBackpackHolds( ATLAS, BANDAGES, POUCH );
        }

        /// <summary>
        ///     A 0x3C replaces what its container held, as it does in the client: a container reopened after
        ///     others emptied it out of sight must not keep items that are gone.
        /// </summary>
        [TestMethod]
        public void ContainerContentsReplacesTheContainer()
        {
            Send( ContainerContents( ( SMOKE_BOMBS, 0x2808, 70, BACKPACK ), ( LOCKPICKS, 0x14FC, 72, BACKPACK ) ), false );

            AssertBackpackHolds( SMOKE_BOMBS, LOCKPICKS );
            Assert.IsNull( Engine.Items.GetItem( ATLAS ) );
        }

        /// <summary>
        ///     Only the first entry's container is replaced. An entry naming another container goes into
        ///     that one, not into the first entry's.
        /// </summary>
        [TestMethod]
        public void ContainerContentsPutsEachEntryInItsOwnContainer()
        {
            Send( ContainerContents( ( LOCKPICKS, 0x14FC, 72, POUCH ) ), false );

            Send( ContainerContents( ( ATLAS, 0x9C16, 1, BACKPACK ), ( BANDAGES, 0xE21, 50, BACKPACK ),
                ( POUCH, 0xE79, 1, BACKPACK ), ( SMOKE_BOMBS, 0x2808, 70, POUCH ) ), false );

            AssertBackpackHolds( ATLAS, BANDAGES, POUCH );

            Item pouch = Engine.Items.GetItem( POUCH );

            CollectionAssert.AreEquivalent( new[] { LOCKPICKS, SMOKE_BOMBS }, pouch.Container.GetItems().Select( i => i.Serial ).ToArray(),
                "the pouch keeps what it had and gains its own entry" );
            Assert.AreEqual( POUCH, Engine.Items.GetItem( SMOKE_BOMBS ).Owner );
        }

        private static void AssertBackpackHolds( params int[] serials )
        {
            Item backpack = Engine.Player.Backpack;

            Assert.IsNotNull( backpack, "the player's backpack is gone from the item store" );
            Assert.IsNotNull( backpack.Container, "the backpack has no contents" );

            CollectionAssert.AreEquivalent( serials, backpack.Container.GetItems().Select( i => i.Serial ).ToArray() );

            foreach ( int serial in serials )
            {
                Assert.IsNotNull( Engine.Items.GetItem( serial ), $"0x{serial:x8} is not in the item store" );
            }
        }

        private static void Send( byte[] packet, bool fixedSize )
        {
            PacketHandler handler = IncomingPacketHandlers.GetHandler( packet[0] );

            Assert.IsNotNull( handler );

            handler.OnReceive( new PacketReader( packet, packet.Length, fixedSize ) );
        }

        private static byte[] DeleteObject( int serial )
        {
            List<byte> packet = new List<byte> { 0x1D };
            AddInt( packet, serial );

            return packet.ToArray();
        }

        private static byte[] Equipped( int serial, int id, Layer layer, int mobile )
        {
            List<byte> packet = new List<byte> { 0x2E };
            AddInt( packet, serial );
            AddShort( packet, id );
            packet.Add( 0 );
            packet.Add( (byte) layer );
            AddInt( packet, mobile );
            AddShort( packet, 0 );

            return packet.ToArray();
        }

        private static byte[] MobileUpdate( int serial, int x, int y )
        {
            List<byte> packet = new List<byte> { 0x20 };
            AddInt( packet, serial );
            AddShort( packet, 0x190 );
            packet.Add( 0 );
            AddShort( packet, 0 );
            packet.Add( 0 );
            AddShort( packet, x );
            AddShort( packet, y );
            AddShort( packet, 0 );
            packet.Add( 0 );
            packet.Add( 0 );

            return packet.ToArray();
        }

        private static byte[] AddToContainer( int serial, int id, int amount, int container )
        {
            List<byte> packet = new List<byte> { 0x25 };
            AddEntry( packet, serial, id, amount, container );

            return packet.ToArray();
        }

        private static byte[] ContainerContents( params (int Serial, int ID, int Amount, int Container)[] entries )
        {
            List<byte> packet = new List<byte> { 0x3C };
            AddShort( packet, 5 + entries.Length * 20 );
            AddShort( packet, entries.Length );

            foreach ( (int serial, int id, int amount, int container) in entries )
            {
                AddEntry( packet, serial, id, amount, container );
            }

            return packet.ToArray();
        }

        /// <summary>
        ///     The 20 bytes 0x25 and 0x3C share (the form with the grid index).
        /// </summary>
        private static void AddEntry( List<byte> packet, int serial, int id, int amount, int container )
        {
            AddInt( packet, serial );
            AddShort( packet, id );
            packet.Add( 0 );
            AddShort( packet, amount );
            AddShort( packet, 44 );
            AddShort( packet, 65 );
            packet.Add( 0 );
            AddInt( packet, container );
            AddShort( packet, 0 );
        }

        private static void AddInt( List<byte> packet, int value )
        {
            packet.Add( (byte) ( value >> 24 ) );
            packet.Add( (byte) ( value >> 16 ) );
            packet.Add( (byte) ( value >> 8 ) );
            packet.Add( (byte) value );
        }

        private static void AddShort( List<byte> packet, int value )
        {
            packet.Add( (byte) ( value >> 8 ) );
            packet.Add( (byte) value );
        }
    }
}
