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

using ClassicAssist.Data.BuffIcons;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests
{
    [TestClass]
    public class BuffIconTests
    {
        [TestMethod]
        public void WillSetExpireTime()
        {
            byte[] packet =
            {
                0xDF, 0x00, 0x44, 0x00, 0x00, 0x77, 0x42, 0x04, 0x45, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x04,
                0x45, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x10, 0x2E, 0xF4,
                0x00, 0x11, 0x9B, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x09, 0x31, 0x00,
                0x30, 0x00, 0x09, 0x00, 0x32, 0x00, 0x34, 0x00, 0x09, 0x00, 0x34, 0x00, 0x30, 0x00, 0x00, 0x00,
                0x00, 0x01, 0x00, 0x00
            };

            IncomingPacketHandlers.Initialize();
            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0xDF );

            BuffIconManager manager = BuffIconManager.GetInstance();

            handler?.OnReceive( new PacketReader( packet, packet.Length, false ) );

            double milliseconds = manager.BuffTime( "Confidence" );

            Assert.IsTrue( milliseconds > 0 );
        }

        [TestMethod]
        public void UnsetBuffWillReturnTimeZero()
        {
            BuffIconManager manager = BuffIconManager.GetInstance();

            double milliseconds = manager.BuffTime( "Enemy Of One" );

            Assert.AreEqual( 0, milliseconds );
        }
    }
}