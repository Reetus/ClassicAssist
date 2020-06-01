//#region License

//// Copyright (C) 2020 Reetus
//// 
//// This program is free software: you can redistribute it and/or modify
//// it under the terms of the GNU General Public License as published by
//// the Free Software Foundation, either version 3 of the License, or
//// (at your option) any later version.
//// 
//// This program is distributed in the hope that it will be useful,
//// but WITHOUT ANY WARRANTY; without even the implied warranty of
//// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//// GNU General Public License for more details.
//// 
//// You should have received a copy of the GNU General Public License
//// along with this program.  If not, see <https://www.gnu.org/licenses/>.

//#endregion

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using Assistant;
//using ClassicAssist.UO.Data;
//using ClassicAssist.UO.Network;
//using ClassicAssist.UO.Network.PacketFilter;
//using ClassicAssist.UO.Objects;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

//namespace ClassicAssist.Tests
//{
//    [TestClass]
//    public class ReplayPacketLog
//    {
//        [TestMethod]
//        public void CheckPoisonStatus()
//        {
//            AppDomain appDomain = AppDomain.CreateDomain( "CheckPoisonStatus", AppDomain.CurrentDomain.Evidence,
//                AppDomain.CurrentDomain.SetupInformation );

//            appDomain.DoCallBack( () =>
//            {
//                Engine.ClientVersion = new Version( 5, 0, 9, 1 );

//                string fileName = @"C:\Users\johns\source\repos\ClassicAssist\Output\net48\export.packets.json";

//                if ( !File.Exists( fileName ) )
//                {
//                    return;
//                }

//                IEnumerable<PacketEntry> packetEntries = ParseLog( fileName );

//                byte[] packetIds = { 0x20, 0x77, 0x78 };

//                IEnumerable<PacketEntry> filteredEntries = packetEntries.Where( i =>
//                    packetIds.Contains( i.Data[0] ) && i.Direction == PacketDirection.Incoming );

//                IncomingPacketHandlers.Initialize();

//                foreach ( PacketEntry filteredEntry in filteredEntries )
//                {
//                    PacketHandler handler = IncomingPacketHandlers.GetHandler( filteredEntry.Data[0] );

//                    handler?.OnReceive( new PacketReader( filteredEntry.Data, filteredEntry.Data.Length, true ) );

//                    Mobile mobile = Engine.Mobiles.GetMobile( 0x002549bb );

//                    Assert.IsTrue( mobile.IsPoisoned );
//                }
//            } );
//        }

//        private static IEnumerable<PacketEntry> ParseLog( string fileName )
//        {
//            string text = File.ReadAllText( fileName );

//            List<PacketEntry> packets = new List<PacketEntry>();

//            JObject[] data = JsonConvert.DeserializeObject<JObject[]>( text );

//            if ( data == null )
//            {
//                return packets;
//            }

//            packets.AddRange( from entry in data
//                where entry != null
//                select new PacketEntry
//                {
//                    Title = entry?["Title"].ToObject<string>() ?? string.Empty,
//                    Direction = entry["Direction"].ToObject<PacketDirection>(),
//                    DateTime = entry["DateTime"].ToObject<DateTime>(),
//                    Data = Convert.FromBase64String( entry["Base64"].ToObject<string>() )
//                } );

//            return packets;
//        }

//        public class PacketEntry
//        {
//            public byte[] Data { get; set; }
//            public DateTime DateTime { get; set; }
//            public PacketDirection Direction { get; set; }
//            public string Title { get; set; }
//        }
//    }
//}

