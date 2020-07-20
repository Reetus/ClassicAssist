//TODO
//using System.Threading;
//using Assistant;
//using ClassicAssist.Data.Vendors;
//using ClassicAssist.UO.Data;
//using ClassicAssist.UO.Network;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace ClassicAssist.Tests.Agents
//{
//    [TestClass]
//    public class SellAgentTests
//    {
//        [TestMethod]
//        public void WontSellIncorrectHue()
//        {
//            //TODO
//            //void OnPacket( byte[] data, int length )
//            //{
//            //    Assert.Fail();
//            //}

//            //Engine.InternalPacketSentEvent += OnPacket;

//            //VendorSellTabViewModel vsvm = new VendorSellTabViewModel();

//            //vsvm.Items.Add( new VendorSellAgentEntry
//            //{
//            //    Enabled = true,
//            //    Graphic = 0xff,
//            //    MinPrice = 10,
//            //    Name = "Shmoo",
//            //    Hue = 0xff
//            //} );

//            //IncomingPacketHandlers.Initialize();
//            //PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x9E );

//            //PacketWriter pw = new PacketWriter( 23 );
//            //pw.Write( (byte) 0x9E );
//            //pw.Write( (short) 21 );
//            //pw.Write( 0 );
//            //pw.Write( (short) 1 );
//            //pw.Write( 0 );
//            //pw.Write( (short) 0xFF );
//            //pw.Write( (short) 0 );
//            //pw.Write( (short) 0 );
//            //pw.Write( (short) 9 );
//            //pw.Write( (short) 0 );

//            //byte[] packet = pw.ToArray();

//            //handler.OnReceive( new PacketReader( packet, packet.Length, false ) );

//            //vsvm.Items.Clear();
//            //Engine.InternalPacketSentEvent -= OnPacket;
//        }

//        [TestMethod]
//        public void WontSellUnderMinPrice()
//        {
//            //TODO
//            //void OnPacket( byte[] data, int length )
//            //{
//            //    Assert.Fail();
//            //}

//            //Engine.InternalPacketSentEvent += OnPacket;

//            //VendorSellTabViewModel vsvm = new VendorSellTabViewModel();

//            //vsvm.Items.Add( new VendorSellAgentEntry
//            //{
//            //    Enabled = true, Graphic = 0xff, MinPrice = 10, Name = "Shmoo"
//            //} );

//            //IncomingPacketHandlers.Initialize();
//            //PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x9E );

//            //PacketWriter pw = new PacketWriter( 23 );
//            //pw.Write( (byte) 0x9E );
//            //pw.Write( (short) 21 );
//            //pw.Write( 0 );
//            //pw.Write( (short) 1 );
//            //pw.Write( 0 );
//            //pw.Write( (short) 0xFF );
//            //pw.Write( (short) 0 );
//            //pw.Write( (short) 0 );
//            //pw.Write( (short) 9 );
//            //pw.Write( (short) 0 );

//            //byte[] packet = pw.ToArray();

//            //handler.OnReceive( new PacketReader( packet, packet.Length, false ) );

//            //vsvm.Items.Clear();
//            //Engine.InternalPacketSentEvent -= OnPacket;
//        }

//        [TestMethod]
//        public void WillSellMinPrice()
//        {
//            AutoResetEvent are = new AutoResetEvent( false );

//            void OnPacket( byte[] data, int length )
//            {
//                if ( data[0] == 0x9F )
//                {
//                    are.Set();
//                }
//            }

//            Engine.InternalPacketSentEvent += OnPacket;

//            VendorSellTabViewModel vsvm = new VendorSellTabViewModel();

//            vsvm.Items.Add( new VendorSellAgentEntry { Enabled = true, Graphic = 0xff, MinPrice = 9, Name = "Shmoo" } );

//            IncomingPacketHandlers.Initialize();
//            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x9E );

//            PacketWriter pw = new PacketWriter( 23 );
//            pw.Write( (byte) 0x9E );
//            pw.Write( (short) 21 );
//            pw.Write( 0 );
//            pw.Write( (short) 1 );
//            pw.Write( 0 );
//            pw.Write( (short) 0xFF );
//            pw.Write( (short) 0 );
//            pw.Write( (short) 0 );
//            pw.Write( (short) 9 );
//            pw.Write( (short) 0 );

//            byte[] packet = pw.ToArray();

//            handler.OnReceive( new PacketReader( packet, packet.Length, false ) );

//            bool result = are.WaitOne( 1000 );

//            Assert.IsTrue( result );

//            vsvm.Items.Clear();
//            Engine.InternalPacketSentEvent -= OnPacket;
//        }

//        [TestMethod]
//        public void WontSellMoreThanAmount()
//        {
//            AutoResetEvent are = new AutoResetEvent( false );

//            void OnPacket( byte[] data, int length )
//            {
//                if ( data[0] != 0x9F )
//                {
//                    return;
//                }

//                int amount = ( data[13] << 8 ) | data[14];

//                if ( amount != 10 )
//                {
//                    Assert.Fail();
//                    return;
//                }

//                are.Set();
//            }

//            Engine.InternalPacketSentEvent += OnPacket;

//            VendorSellTabViewModel vsvm = new VendorSellTabViewModel();

//            vsvm.Items.Add( new VendorSellAgentEntry { Enabled = true, Graphic = 0xff, Amount = 10, Name = "Shmoo" } );

//            IncomingPacketHandlers.Initialize();
//            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x9E );

//            PacketWriter pw = new PacketWriter( 23 );
//            pw.Write( (byte) 0x9E );
//            pw.Write( (short) 21 );
//            pw.Write( 0 );
//            pw.Write( (short) 1 );
//            pw.Write( 0 );
//            pw.Write( (short) 0xFF );
//            pw.Write( (short) 0 );
//            pw.Write( (short) 100 );
//            pw.Write( (short) 9 );
//            pw.Write( (short) 0 );

//            byte[] packet = pw.ToArray();

//            handler.OnReceive( new PacketReader( packet, packet.Length, false ) );

//            bool result = are.WaitOne( 1000 );

//            Assert.IsTrue( result );

//            vsvm.Items.Clear();
//            Engine.InternalPacketSentEvent -= OnPacket;
//        }
//    }
//}