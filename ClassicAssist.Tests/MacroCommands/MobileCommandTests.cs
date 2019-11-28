using Assistant;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.UO.Data;
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
            _player = new PlayerMobile( 0x01 );
            Engine.Player = _player;
        }

        [TestMethod]
        public void WillGetHitsNoParam()
        {
            _player.Hits = 100;

            int val = MobileCommands.Hits();

            Assert.AreEqual( 100, _player.Hits );

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
    }
}