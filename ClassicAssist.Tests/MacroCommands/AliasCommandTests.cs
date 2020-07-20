using ClassicAssist.Shared;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.MacroCommands
{
    [TestClass]
    public class AliasCommandTests
    {
        private PlayerMobile _player;

        [TestInitialize]
        public void Initialize()
        {
            _player = new PlayerMobile( 0x01 );
            Engine.Player = _player;
        }

        [TestMethod]
        public void WillSetDefaultAliases()
        {
            AliasCommands.SetDefaultAliases();

            Assert.AreEqual( _player.Serial, AliasCommands.GetAlias( "self" ) );
        }

        [TestMethod]
        public void WillChangeLastAliasOnTargetSent()
        {
            _player.LastTargetSerial = 0x02;

            AliasCommands.SetDefaultAliases();

            Assert.AreEqual( 0x02, AliasCommands.GetAlias( "last" ) );

            OutgoingPacketHandlers.Initialize();
            PacketHandler handler = OutgoingPacketHandlers.GetHandler( 0x6C );

            byte[] packet =
            {
                0x6C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xAA, 0xBB, 0xCC, 0xDD, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            handler?.OnReceive( new PacketReader( packet, packet.Length, true ) );

            Assert.AreEqual( 0xAABBCCDD, (uint) AliasCommands.GetAlias( "last" ) );
        }
    }
}