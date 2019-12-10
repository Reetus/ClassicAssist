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

            byte[] packet = { 0x20, 0x00, 0x00, 0x00, 0x01, 0x01, 0x90, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x20 );

            Assert.IsNotNull( handler );

            handler.OnReceive( new PacketReader( packet, packet.Length, true ) );

            Assert.IsTrue( Engine.Player.IsFrozen );

            Engine.Player = null;
        }
    }
}