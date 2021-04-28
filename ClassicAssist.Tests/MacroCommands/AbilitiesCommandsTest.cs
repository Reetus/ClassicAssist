using System.Threading;
using Assistant;
using ClassicAssist.Data.Abilities;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.MacroCommands
{
    [TestClass]
    public class AbilitiesCommandsTest
    {
        private readonly AutoResetEvent _are = new AutoResetEvent( false );

        [TestMethod]
        public void WillOnlySetPrimaryAbilityOnce()
        {
            AbilitiesManager manager = AbilitiesManager.GetInstance();

            Engine.Player = new PlayerMobile( 0x01 );

            Engine.InternalPacketSentEvent += ExpectAbilityPacket;

            AbilitiesCommands.SetAbility( "primary", "on" );

            bool result = _are.WaitOne( 5000 );

            if ( !result )
            {
                Assert.Fail();
            }

            Assert.AreEqual( AbilityType.Primary, manager.Enabled );
            Assert.IsTrue( manager.IsPrimaryEnabled );

            Engine.InternalPacketSentEvent -= ExpectAbilityPacket;
            Engine.InternalPacketSentEvent += NotExpectAbilityPacket;

            AbilitiesCommands.SetAbility( "primary", "on" );

            Engine.InternalPacketSentEvent -= NotExpectAbilityPacket;

            AbilitiesCommands.ClearAbility();

            Engine.Player = null;
        }

        [TestMethod]
        public void WillTogglePrimaryAbility()
        {
            AbilitiesManager manager = AbilitiesManager.GetInstance();

            Engine.Player = new PlayerMobile( 0x01 );

            manager.Enabled = AbilityType.None;

            Engine.InternalPacketSentEvent += ExpectAbilityPacket;

            AbilitiesCommands.SetAbility( "primary" );

            bool result = _are.WaitOne( 5000 );

            if ( !result )
            {
                Assert.Fail();
            }

            Assert.AreEqual( AbilityType.Primary, manager.Enabled );
            Assert.IsTrue( manager.IsPrimaryEnabled );

            AbilitiesCommands.SetAbility( "primary" );

            result = _are.WaitOne( 5000 );

            if ( !result )
            {
                Assert.Fail();
            }

            Assert.AreEqual( AbilityType.None, manager.Enabled );
            Assert.IsFalse( manager.IsPrimaryEnabled );

            Engine.InternalPacketSentEvent -= ExpectAbilityPacket;

            Engine.Player = null;
        }

        [TestMethod]
        public void WillOnlySetSecondaryAbilityOnce()
        {
            AbilitiesManager manager = AbilitiesManager.GetInstance();

            Engine.Player = new PlayerMobile( 0x01 );

            Engine.InternalPacketSentEvent += ExpectAbilityPacket;

            AbilitiesCommands.SetAbility( "secondary", "on" );

            bool result = _are.WaitOne( 5000 );

            if ( !result )
            {
                Assert.Fail();
            }

            Assert.AreEqual( AbilityType.Secondary, manager.Enabled );
            Assert.IsTrue( manager.IsSecondaryEnabled );

            Engine.InternalPacketSentEvent -= ExpectAbilityPacket;
            Engine.InternalPacketSentEvent += NotExpectAbilityPacket;

            AbilitiesCommands.SetAbility( "secondary", "on" );

            Engine.InternalPacketSentEvent -= NotExpectAbilityPacket;

            AbilitiesCommands.ClearAbility();

            Engine.Player = null;
        }

        [TestMethod]
        public void WillToggleSecondaryAbility()
        {
            AbilitiesManager manager = AbilitiesManager.GetInstance();

            Engine.Player = new PlayerMobile( 0x01 );

            manager.Enabled = AbilityType.None;

            Engine.InternalPacketSentEvent += ExpectAbilityPacket;

            AbilitiesCommands.SetAbility( "secondary" );

            bool result = _are.WaitOne( 5000 );

            if ( !result )
            {
                Assert.Fail();
            }

            Assert.AreEqual( AbilityType.Secondary, manager.Enabled );
            Assert.IsTrue( manager.IsSecondaryEnabled );

            AbilitiesCommands.SetAbility( "secondary" );

            result = _are.WaitOne( 5000 );

            if ( !result )
            {
                Assert.Fail();
            }

            Assert.AreEqual( AbilityType.None, manager.Enabled );
            Assert.IsFalse( manager.IsSecondaryEnabled );

            Engine.InternalPacketSentEvent -= ExpectAbilityPacket;

            Engine.Player = null;
        }

        [TestMethod]
        public void WillClearAbilities()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            AbilitiesManager manager = AbilitiesManager.GetInstance();

            Engine.InternalPacketSentEvent += ExpectAbilityPacket;

            manager.Enabled = AbilityType.Primary;

            AbilitiesCommands.ClearAbility();

            bool result = _are.WaitOne( 5000 );

            if ( !result )
            {
                Assert.Fail();
            }

            Assert.AreEqual( AbilityType.None, manager.Enabled );

            manager.Enabled = AbilityType.Secondary;

            AbilitiesCommands.ClearAbility();

            result = _are.WaitOne( 5000 );

            if ( !result )
            {
                Assert.Fail();
            }

            Assert.AreEqual( AbilityType.None, manager.Enabled );

            Engine.InternalPacketSentEvent -= ExpectAbilityPacket;

            Engine.Player = null;
        }

        [TestMethod]
        public void WontSetAbilitySetOff()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            Engine.InternalPacketSentEvent += NotExpectAbilityPacket;

            AbilitiesCommands.SetAbility( "primary", "off" );

            Assert.AreEqual( AbilityType.None, AbilitiesManager.GetInstance().Enabled );

            Engine.Player = null;
        }

        private static void NotExpectAbilityPacket( byte[] data, int length )
        {
            if ( data[0] == 0xD7 && data[8] == 0x19 )
            {
                Assert.Fail();
            }
        }

        private void ExpectAbilityPacket( byte[] data, int length )
        {
            if ( data[0] == 0xD7 && data[8] == 0x19 )
            {
                if ( data[13] == 0x00 )
                {
                    byte[] packet = { 0xBF, 0x00, 0x05, 0x00, 0x21 };
                    IncomingPacketHandlers.Initialize();
                    PacketHandler handler = IncomingPacketHandlers.GetHandler( 0xBF );
                    handler.OnReceive( new PacketReader( packet, packet.Length, false ) );
                }

                _are.Set();
            }
        }
    }
}