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

using System;
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

        [TestMethod]
        public void WontThrowExceptionDuplicateKey()
        {
            byte[] packet =
            {
                0xb1, 0x00, 0x83, 0x00, 0x1d, 0x78, 0xb9, 0x04, 0x5d, 0xd3, 0xaa, 0x00, 0x00, 0x00, 0x01, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x05, 0xb3, 0xd9, 0xd5, 0x89, 0x00,
                0x20, 0xc2, 0xb9, 0xb0, 0x99, 0x00, 0x00, 0x00, 0x18, 0x00, 0x46, 0x00, 0x69, 0x00, 0x73, 0x00,
                0x68, 0x00, 0x61, 0x00, 0x6e, 0x00, 0x64, 0x00, 0x43, 0x00, 0x68, 0x00, 0x69, 0x00, 0x70, 0x00,
                0x73, 0x00, 0x20, 0xc7, 0x74, 0x00, 0x20, 0xb2, 0xf9, 0xc2, 0xe0, 0xc7, 0x44, 0x00, 0x20, 0xcd,
                0x08, 0xb3, 0x00, 0xd5, 0x69, 0xb2, 0xc8, 0xb2, 0xe4, 0x00, 0x00, 0x00, 0x0b, 0xc2, 0x18, 0xb7,
                0x7d, 0xc5, 0xec, 0xbd, 0x80, 0xb9, 0x7c, 0x00, 0x20, 0xd0, 0x74, 0xb9, 0xad, 0xd5, 0x58, 0xc1,
                0x38, 0xc6, 0x94, 0x00, 0x00, 0x00, 0x02, 0xc2, 0x18, 0xb7, 0x7d, 0x00, 0x00, 0x00, 0x02, 0xac,
                0x70, 0xc8, 0x08
            };

            PacketHandler handler = OutgoingPacketHandlers.GetHandler( packet[0] );

            try
            {
                handler.OnReceive( new PacketReader( packet, packet.Length, false ) );
            }
            catch ( Exception )
            {
                Assert.Fail();
                throw;
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
        }
    }
}