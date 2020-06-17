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
using System.Linq;
using System.Threading;
using Assistant;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests
{
    [TestClass]
    public class OldPacketTests
    {
        [TestMethod]
        public void WillSendProperDropRequest()
        {
            AppDomain appDomain = AppDomain.CreateDomain( "WillSendProperDropRequest", AppDomain.CurrentDomain.Evidence,
                AppDomain.CurrentDomain.SetupInformation );

            appDomain.DoCallBack( () =>
            {
                Engine.ClientVersion = new Version( 5, 0, 9, 1 );
                AutoResetEvent are = new AutoResetEvent( false );

                void OnInternalPacketSentEvent( byte[] data, int length )
                {
                    if ( data[0] != 0x08 )
                    {
                        return;
                    }

                    int serial = ( data[1] << 24 ) | ( data[2] << 16 ) | ( data[3] << 8 ) | data[4];
                    int x = ( data[5] << 8 ) | data[6];
                    int y = ( data[7] << 8 ) | data[8];
                    int z = data[9];
                    int containerSerial = ( data[10] << 24 ) | ( data[11] << 16 ) | ( data[12] << 8 ) | data[13];

                    Assert.AreEqual( 0x11223344, serial );
                    Assert.AreEqual( 1, x );
                    Assert.AreEqual( 2, y );
                    Assert.AreEqual( 3, z );
                    Assert.AreEqual( 0x55667788, containerSerial );

                    are.Set();
                }

                Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;

                Engine.SendPacketToServer( new DropItem( 0x11223344, 0x55667788, 1, 2, 3 ) );
                are.WaitOne( 5000 );
                Engine.ClientVersion = new Version( 7, 0, 45, 1 );
            } );
        }

        [TestMethod]
        public void WillParseOldContainerContents()
        {
            AppDomain appDomain = AppDomain.CreateDomain( "WillParseOldContainerContents",
                AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation );

            appDomain.DoCallBack( () =>
            {
                Engine.ClientVersion = new Version( 5, 0, 9, 1 );
                byte[] packet =
                {
                    0x3C, 0x00, 0x2B, 0x00, 0x02, 0x46, 0x13, 0xFF, 0x71, 0x0E, 0x76, 0x00, 0x00, 0x01, 0x00, 0x2C,
                    0x00, 0x7F, 0x46, 0x13, 0xFF, 0x6D, 0x00, 0x00, 0x46, 0x58, 0x64, 0xFB, 0x0E, 0xED, 0x00, 0x03,
                    0xE8, 0x00, 0x4C, 0x00, 0x48, 0x46, 0x13, 0xFF, 0x6D, 0x00, 0x00
                };

                IncomingPacketHandlers.Initialize();
                Engine.Items = new ItemCollection( 0 );

                PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x3C );
                handler?.OnReceive( new PacketReader( packet, packet.Length, false ) );

                Assert.AreEqual( 1, Engine.Items.GetItemCount() );
                Assert.IsTrue( Engine.Items.Any( i => i.Serial == 0x4613ff6d ) );
                Item container = Engine.Items.GetItem( 0x4613ff6d );
                Assert.IsNotNull( container );
                Assert.AreEqual( 2, container.Container.GetItemCount() );
                Engine.Items = null;
                Engine.ClientVersion = new Version( 7, 0, 45, 1 );
            } );
        }

        [TestMethod]
        public void WillParseOldHealthbarColour()
        {
            AppDomain appDomain = AppDomain.CreateDomain( "WillParseOldHealthbarColour",
                AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation );

            appDomain.DoCallBack( () =>
            {
                Engine.ClientVersion = new Version( 5, 0, 9, 1 );

                byte[] packet =
                {
                    0x17, 0x00, 0x0F, 0x00, 0x07, 0x5D, 0x67, 0x00, 0x02, 0x00, 0x01, 0x01, 0x00, 0x02, 0x00
                };

                IncomingPacketHandlers.Initialize();

                Mobile mobile = new Mobile( 0x00075D67 );

                Engine.Mobiles.Add( mobile );

                PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x17 );
                handler?.OnReceive( new PacketReader( packet, packet.Length, false ) );

                Assert.IsTrue( mobile.IsPoisoned );
            } );
        }

        [TestMethod]
        public void WillSetPoisonedFromOldMoving()
        {
            AppDomain appDomain = AppDomain.CreateDomain( "WillSetPoisonedFromOldMoving",
                AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation );

            appDomain.DoCallBack( () =>
            {
                Engine.ClientVersion = new Version( 5, 0, 9, 1 );

                byte[] packet =
                {
                    0x77, 0x00, 0x25, 0x49, 0xbb, 0x01, 0x90, 0x0d, 0xf9, 0x0a, 0x37, 0x00, 0x84, 0x83, 0xea, 0x04,
                    0x01
                };

                IncomingPacketHandlers.Initialize();

                PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x77 );
                handler?.OnReceive( new PacketReader( packet, packet.Length, true ) );

                Mobile mobile = Engine.Mobiles.GetMobile( 0x2549bb );

                Assert.IsNotNull( mobile );
                Assert.IsTrue( mobile.IsPoisoned );

                byte[] packet2 =
                {
                    0x77, 0x00, 0x06, 0xbc, 0x06, 0x01, 0x90, 0x09, 0xd0, 0x02, 0x2b, 0x00, 0x02, 0x83, 0xea, 0x44,
                    0x01
                };

                handler?.OnReceive( new PacketReader( packet2, packet2.Length, true ) );

                mobile = Engine.Mobiles.GetMobile( 0x6bc06 );

                Assert.IsNotNull( mobile );
                Assert.IsTrue( mobile.IsPoisoned );
            } );
        }

        //[TestMethod]
        //public void WillSetPoisonedFromOldIncoming()
        //{
        //    AppDomain appDomain = AppDomain.CreateDomain( "WillSetPoisonedFromOldIncoming",
        //        AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation );

        //    appDomain.DoCallBack( () =>
        //    {
        //        Engine.ClientVersion = new Version( 5, 0, 9, 1 );

        //        byte[] packet =
        //        {
        //            0x78, 0x00, 0x6e, 0x00, 0x05, 0xca, 0x65, 0x01, 0x90, 0x09, 0xba, 0x02, 0x23, 0x00, 0x03, 0x03,
        //            0xff, 0x00, 0x01, 0x40, 0x05, 0xca, 0x5d, 0xa0, 0x48, 0x0b, 0x04, 0x65, 0x40, 0x05, 0xca, 0x58,
        //            0xa0, 0x3e, 0x10, 0x04, 0x65, 0x40, 0x05, 0xca, 0x57, 0x95, 0x17, 0x05, 0x03, 0x1c, 0x40, 0x05,
        //            0xca, 0x54, 0x95, 0x2e, 0x04, 0x01, 0xb6, 0x40, 0x05, 0xca, 0x53, 0x14, 0x15, 0x0d, 0x40, 0x05,
        //            0xca, 0x51, 0x14, 0x11, 0x18, 0x40, 0x05, 0xca, 0x4f, 0x14, 0x10, 0x13, 0x40, 0x05, 0xca, 0x46,
        //            0x95, 0x41, 0x11, 0x03, 0xaf, 0x40, 0x05, 0xca, 0x3b, 0x0e, 0x75, 0x15, 0x40, 0x05, 0xca, 0x35,
        //            0x3e, 0xa0, 0x19, 0x40, 0x05, 0xca, 0x42, 0x14, 0x3e, 0x02, 0x00, 0x00, 0x00, 0x00
        //        };

        //        IncomingPacketHandlers.Initialize();

        //        PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x78 );
        //        handler?.OnReceive( new PacketReader( packet, packet.Length, false ) );

        //        Mobile mobile = Engine.Mobiles.GetMobile( 0x5ca65 );

        //        Assert.IsNotNull( mobile );
        //        Assert.IsTrue( mobile.IsPoisoned );
        //    } );
        //}

        [TestMethod]
        public void WillParseItemListMenu()
        {
            /*
             * Display Item List Menu Packet.
                from server
                byte	ID (7C)
                word	Packet Size
                dword	Sender Serial
                word	Gump ID
                byte	Title Length
                char[Title Length]	Title
                byte	Number Of Lines
                loop	Lines
                word	Choice ID
                word	Hue
                byte	Line Length
                char[Line Length]	Texte
                endloop	Lines
             */
            byte[] packet =
            {
                0x7C, 0x00, 0x39, 0x00, 0x07, 0x76, 0x7C, 0x01, 0xCF, 0x0D, 0x42, 0x6C, 0x61, 0x63, 0x6B, 0x73,
                0x6D, 0x69, 0x74, 0x68, 0x69, 0x6E, 0x67, 0x03, 0x0F, 0xAF, 0x00, 0x00, 0x06, 0x52, 0x65, 0x70,
                0x61, 0x69, 0x72, 0x0F, 0xB1, 0x00, 0x00, 0x05, 0x53, 0x6D, 0x65, 0x6C, 0x74, 0x13, 0xB9, 0x00,
                0x00, 0x07, 0x57, 0x65, 0x61, 0x70, 0x6F, 0x6E, 0x73
            };

            /*
             * Item List Menu Response Packet.
                13 bytesfrom server
                byte	ID (7D)
                dword	Sender Serial
                word	Gump ID
                word	Index
                word	Item ID
                word	Hue 
             */

            byte[] packet2 = { 0x7D, 0x00, 0x07, 0x76, 0x7C, 0x01, 0xCF, 0x00, 0x03, 0x13, 0xB9, 0x00, 0x00 };
        }
    }
}