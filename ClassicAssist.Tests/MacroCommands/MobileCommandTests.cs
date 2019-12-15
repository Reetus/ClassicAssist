using Assistant;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Data.SpecialMoves;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.MacroCommands
{
    [TestClass]
    public class MobileCommandTests
    {
        private PlayerMobile _player;

        [TestInitialize]
        public void Initialize()
        {
            _player = new PlayerMobile( 0x01 ) { Name = "Shmoo", Notoriety = Notoriety.Murderer };
            Engine.Player = _player;
            AliasCommands.SetAlias( "self", 0x01 );
        }

        [TestMethod]
        public void WillGetName()
        {
            string name = MobileCommands.Name( "self" );

            Assert.AreEqual( "Shmoo", name );
        }

        [TestMethod]
        public void WillGetNotoriety()
        {
            Assert.IsTrue( NotorietyCommands.Murderer( "self" ) );
        }

        [TestMethod]
        public void WillGetHitsNoParam()
        {
            _player.Hits = 100;

            int val = MobileCommands.Hits();

            Assert.AreEqual( 100, val );

            _player.Hits = 0;
        }

        [TestMethod]
        public void WillGetHitsSerialParam()
        {
            Mobile mobile = new Mobile( 2 ) { Hits = 200 };

            Engine.Mobiles.Add( mobile );

            int val = MobileCommands.Hits( mobile.Serial );

            Assert.AreEqual( 200, val );

            Engine.Mobiles.Remove( mobile );
        }

        [TestMethod]
        public void WillGetHitsStringParam()
        {
            Mobile mobile = new Mobile( 2 ) { Hits = 250 };

            Engine.Mobiles.Add( mobile );

            AliasCommands.SetAlias( "mobile", mobile.Serial );

            int val = MobileCommands.Hits( "mobile" );

            Assert.AreEqual( 250, val );

            Engine.Mobiles.Remove( mobile );
        }

        [TestMethod]
        public void WillGetHiddenNoParam()
        {
            _player.Status |= MobileStatus.Hidden;
            _player.Status |= MobileStatus.Female;

            bool hidden = MobileCommands.Hidden();

            Assert.IsTrue( hidden );
        }

        [TestMethod]
        public void WillGetDead()
        {
            Assert.IsFalse( MobileCommands.Dead() );

            _player.ID = 0x192;

            Assert.IsTrue( MobileCommands.Dead() );

            _player.ID = 0x190;
        }

        [TestMethod]
        public void WillGetDeadParam()
        {
            Mobile m = new Mobile( 2 );

            Engine.Mobiles.Add( m );

            Assert.IsFalse( MobileCommands.Dead( 2 ) );

            m.ID = 0x192;

            Assert.IsTrue( MobileCommands.Dead( 2 ) );

            Engine.Mobiles.Remove( m );
        }

        [TestMethod]
        public void WillGetMountedParam()
        {
            Item mount = new Item( 2 );

            Engine.Items.Add( mount );

            AliasCommands.SetAlias( "self", 1 );
            AliasCommands.SetAlias( "mount", 2 );

            Assert.IsFalse( MobileCommands.Mounted( "self" ) );

            _player.SetLayer( Layer.Mount, 2 );

            Assert.IsTrue( MobileCommands.Mounted( "self" ) );

            Engine.Items.Remove( mount );
            _player.SetLayer( Layer.Mount, 0 );
        }

        [TestMethod]
        public void WillGetMaxHits()
        {
            _player.HitsMax = 100;

            Assert.AreEqual( 100, MobileCommands.MaxHits( _player.Serial ) );
        }

        [TestMethod]
        public void WillGetHits()
        {
            _player.Hits = 100;

            Assert.AreEqual( 100, MobileCommands.Hits( _player.Serial ) );
        }

        [TestMethod]
        public void WillGetDiffHits()
        {
            _player.Hits = 90;
            _player.HitsMax = 100;

            Assert.AreEqual( 10, MobileCommands.DiffHits( _player.Serial ) );
        }

        [TestMethod]
        public void WillEnableDisableSpecialMove()
        {
            SpecialMovesManager manager = SpecialMovesManager.GetInstance();

            byte[] enablePacket = { 0xBF, 0x00, 0x08, 0x00, 0x25, 0x01, 0xF6, 0x01 };
            byte[] disablePacket = { 0xBF, 0x00, 0x08, 0x00, 0x25, 0x01, 0xF6, 0x00 };

            IncomingPacketHandlers.Initialize();

            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0xBF );

            handler?.OnReceive( new PacketReader( enablePacket, enablePacket.Length, false ) );

            bool result = EntityCommands.SpecialMoveExists( "Death Strike" );

            Assert.IsTrue( result );

            handler?.OnReceive( new PacketReader( disablePacket, disablePacket.Length, false ) );

            result = EntityCommands.SpecialMoveExists( "Death Strike" );

            Assert.IsFalse( result );
        }
    }
}