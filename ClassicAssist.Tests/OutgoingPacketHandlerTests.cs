#region License

// Copyright (C) 2020 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using Assistant;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests
{
    [TestClass]
    public class OutgoingPacketHandlerTests
    {
        [TestInitialize]
        public void Initialize()
        {
            Engine.Items = new ItemCollection( 0 );
            Engine.Mobiles = new MobileCollection( Engine.Items );
            Engine.Player = new PlayerMobile( 1 );

            OutgoingPacketHandlers.Initialize();
        }

        [TestMethod]
        public void WillSetHolding()
        {
            byte[] packet = { 0x07, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };

            PacketHandler handler = OutgoingPacketHandlers.GetHandler( 0x07 );

            handler.OnReceive( new PacketReader( packet, packet.Length, true ) );

            Assert.AreEqual( unchecked( (int) 0xAABBCCDD ), Engine.Player.Holding );
            Assert.AreEqual( 0xEEFF, Engine.Player.HoldingAmount );
        }

        [TestCleanup]
        public void Cleanup()
        {
        }
    }
}