using Assistant;
using ClassicAssist.Data.Macros;
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
        private Mobile _mobile;

        [TestInitialize]
        public void Initialize()
        {
            MacroManager.GetInstance().IsRecording = () => false;

            _mobile = new Mobile( 0x01 ) { Name = "Shmoo", Notoriety = Notoriety.Murderer };
            Engine.Mobiles = new MobileCollection( Engine.Items );
            Engine.Mobiles.Add( _mobile );

            Engine.Player = new PlayerMobile( 0x0004C88B ) { Name = "Shmoo", Notoriety = Notoriety.Murderer };

            byte[] packet =
            {
                0x11, 0x00, 0x79, 0x00, 0x04, 0xC8, 0x8B, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6D, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7E, 0x00, 0x7E, 0x00, 0x06, 0x00, 0x00, 0x7D, 0x00, 0x14,
                0x00, 0x81, 0x00, 0x1B, 0x00, 0x1B, 0x00, 0x8D, 0x00, 0x8D, 0x00, 0x00, 0x08, 0xAE, 0x00, 0x46,
                0x01, 0x0D, 0x02, 0x19, 0x01, 0x00, 0xF5, 0x01, 0x05, 0x00, 0x46, 0x00, 0x46, 0x00, 0x46, 0x00,
                0x46, 0x00, 0xC8, 0x00, 0x01, 0x00, 0x06, 0x00, 0x00, 0x01, 0x45, 0x00, 0x46, 0x00, 0x46, 0x00,
                0x46, 0x00, 0x46, 0x00, 0x46, 0x00, 0x22, 0x00, 0x2D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x64, 0x00, 0x23, 0x00, 0x06, 0x00, 0x04, 0x00, 0x20
            };
            IncomingPacketHandlers.Initialize();

            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x11 );

            handler?.OnReceive( new PacketReader( packet, packet.Length, false ) );

            AliasCommands.SetAlias( "self", 0x0004C88B );
        }

        [TestMethod]
        public void WillGetName()
        {
            string name = EntityCommands.Name( "self" );

            Assert.AreEqual( "System", name );
        }

        [TestMethod]
        public void WillTrimNameItem()
        {
            Engine.Items.Add( new Item( 0x40000000 ) { Name = " need trim " } );

            Assert.AreEqual( "need trim", EntityCommands.Name( 0x40000000 ) );

            Assert.AreEqual( string.Empty, EntityCommands.Name( 0x40000001 ) );

            Engine.Items.Remove( 0x40000000 );
        }

        [TestMethod]
        public void WillGetNotoriety()
        {
            Assert.IsTrue( NotorietyCommands.Murderer( "self" ) );
        }

        [TestMethod]
        public void WillGetHitsNoParam()
        {
            int val = MobileCommands.Hits();

            Assert.AreEqual( 126, val );

            _mobile.Hits = 0;
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
            _mobile.Status |= MobileStatus.Hidden;
            _mobile.Status |= MobileStatus.Female;

            bool hidden = MobileCommands.Hidden( _mobile.Serial );

            Assert.IsTrue( hidden );
        }

        [TestMethod]
        public void WillGetDead()
        {
            Assert.IsFalse( MobileCommands.Dead( _mobile.Serial ) );

            _mobile.ID = 0x192;

            Assert.IsTrue( MobileCommands.Dead( _mobile.Serial ) );

            _mobile.ID = 0x190;
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

            _mobile.SetLayer( Layer.Mount, 2 );

            Assert.IsTrue( MobileCommands.Mounted( "self" ) );

            Engine.Items.Remove( mount );
            _mobile.SetLayer( Layer.Mount, 0 );
        }

        [TestMethod]
        public void WillGetMaxHits()
        {
            _mobile.HitsMax = 100;

            Assert.AreEqual( 100, MobileCommands.MaxHits( _mobile.Serial ) );
        }

        [TestMethod]
        public void WillGetHits()
        {
            _mobile.Hits = 100;

            Assert.AreEqual( 100, MobileCommands.Hits( _mobile.Serial ) );
        }

        [TestMethod]
        public void WillGetDiffHits()
        {
            _mobile.Hits = 90;
            _mobile.HitsMax = 100;

            Assert.AreEqual( 10, MobileCommands.DiffHits( _mobile.Serial ) );
        }

        [TestMethod]
        public void WillGetStats()
        {
            Assert.AreEqual( 125, MobileCommands.Str() );
            Assert.AreEqual( 129, MobileCommands.Int() );
            Assert.AreEqual( 20, MobileCommands.Dex() );
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

        [TestMethod]
        public void WillGetFasterCastRecovery()
        {
            double fasterCastRecovery = MobileCommands.FasterCastRecovery();

            Assert.AreEqual( 6, fasterCastRecovery );
        }

        [TestMethod]
        public void WillGetFasterCasting()
        {
            double fasterCasting = MobileCommands.FasterCasting();

            Assert.AreEqual( 4, fasterCasting );
        }

        [TestMethod]
        public void WillGetLuck()
        {
            int luck = MobileCommands.Luck();

            Assert.AreEqual( 200, luck );
        }

        [TestMethod]
        public void WillGetTithingPoints()
        {
            int tithingPoints = MobileCommands.TithingPoints();

            Assert.AreEqual( 325, tithingPoints );
        }

        [TestMethod]
        public void WillGetGold()
        {
            int gold = MobileCommands.Gold();

            Assert.AreEqual( 2222, gold );
        }

        [TestMethod]
        public void WillGetFollowers()
        {
            int followers = MobileCommands.Followers();

            Assert.AreEqual( 1, followers );
        }

        [TestMethod]
        public void WillGetMaxFollowers()
        {
            int maxFollowers = MobileCommands.MaxFollowers();

            Assert.AreEqual( 5, maxFollowers );
        }

        [TestCleanup]
        public void Cleanup()
        {
            Engine.Player = null;
            Engine.Items.Clear();
            Engine.Mobiles.Clear();
        }
    }
}