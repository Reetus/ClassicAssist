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
using ClassicAssist.Data.Filters;
using ClassicAssist.UO.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.Filters
{
    [TestClass]
    public class RepeatedMessageFilterTests
    {
        [TestMethod]
        public void WillFilterOldAsciiSystemMessages()
        {
            AppDomain appDomain = AppDomain.CreateDomain( "WillFilterOldAsciiSystemMessages",
                AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation );

            appDomain.DoCallBack( () =>
            {
                byte[] packet =
                {
                    0x1C, 0x00, 0x4B, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x04, 0x81, 0x00, 0x03, 0x53, 0x79,
                    0x73, 0x74, 0x65, 0x6D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x54, 0x61, 0x72, 0x67,
                    0x65, 0x74, 0x20, 0x69, 0x73, 0x20, 0x6E, 0x6F, 0x74, 0x20, 0x69, 0x6E, 0x20, 0x6C, 0x69, 0x6E,
                    0x65, 0x20, 0x6F, 0x66, 0x20, 0x73, 0x69, 0x67, 0x68, 0x74, 0x00
                };

                IncomingPacketFilters.Initialize();
                RepeatedMessagesFilter.IsEnabled = true;

                IncomingPacketFilters.CheckPacket( packet, packet.Length );
                IncomingPacketFilters.CheckPacket( packet, packet.Length );
                IncomingPacketFilters.CheckPacket( packet, packet.Length );
                IncomingPacketFilters.CheckPacket( packet, packet.Length );
                IncomingPacketFilters.CheckPacket( packet, packet.Length );
                IncomingPacketFilters.CheckPacket( packet, packet.Length );

                bool result = IncomingPacketFilters.CheckPacket( packet, packet.Length );

                Assert.IsTrue( result );
            } );
        }
    }
}