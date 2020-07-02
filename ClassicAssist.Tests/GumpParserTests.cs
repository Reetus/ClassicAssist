using System;
using System.Diagnostics;
using System.IO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Objects.Gumps;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests
{
    [TestClass]
    public class GumpParserTests
    {
        [TestMethod]
        public void WillParseGump()
        {
            /*
            Gump ID: 0x93a564c3
            Pages: 1

            Layout: (230)

            { page 0 }
            { resizepic 10 10 2600 500 135 }
            { xmfhtmlgump 52 35 420 55 1006046 1 1 }
            { button 60 95 4005 4007 1 0 1 }
            { xmfhtmlgump 95 96 150 35 1006044 0 0 }
            { button 285 95 4017 4019 1 0 0 }
            { xmfhtmlgump 320 96 150 35 1006045 0 0 }

            Elements (6):
               
               X: 10, Y: 10, Type: resizepic, Cliloc: 0, Text: 
               X: 52, Y: 35, Type: xmfhtmlgump, Cliloc: 1006046, Text: You have reward items available.  Click 'ok' below to get the selection menu or 'cancel' to be prompted upon your next login.
               X: 60, Y: 95, Type: button, Cliloc: 0, Text: 
               X: 95, Y: 96, Type: xmfhtmlgump, Cliloc: 1006044, Text: OK
               X: 285, Y: 95, Type: button, Cliloc: 0, Text: 
               X: 320, Y: 96, Type: xmfhtmlgump, Cliloc: 1006045, Text: Cancel
             */

            // Clilocs removed to prevent dependancy on Cliloc.enu
            const string layout =
                "{ page 0 }{ resizepic 10 10 2600 500 135 }{ xmfhtmlgump 52 35 420 55 0 1 1 }{ button 60 95 4005 4007 1 0 1 }{ xmfhtmlgump 95 96 150 35 0 0 0 }{ button 285 95 4017 4019 1 0 0 }{ xmfhtmlgump 320 96 150 35 0 0 0 }";

            Gump gump = GumpParser.Parse( 0x01, 0x93a564c3, 10, 10, layout, new string[0] );

            Assert.IsNotNull( gump );

            GumpElement gumpElement = gump.GetElementByXY( 285, 95 );

            Assert.IsNotNull( gumpElement );

            Assert.AreEqual( ElementType.button, gumpElement.Type );
        }

        [TestMethod]
        public void WillParseAuctionSafeGump()
        {
            const string localPath = @"C:\Users\johns\Desktop\KvG Client 2.0";

            if ( !Directory.Exists( localPath ) )
            {
                Debug.WriteLine( "Not running test, requires Cliloc.enu" );
                return;
            }

            Cliloc.Initialize( localPath );

            const string layout =
                "{ resizepic 0 0 9300 400 600 }{ xmfhtmltok 10 10 400 18 0 0 22660 1114513 @Heritage Raffle@ }{ xmfhtmltok 0 70 198 18 0 0 22660 1114514 @Can Buy Unlimited Tickets:@ }{ htmlgump 202 70 200 18 0 0 0 }{ xmfhtmltok 0 100 198 18 0 0 22660 1114514 @Currency Type:@ }{ htmlgump 202 100 200 18 1 0 0 }{ xmfhtmltok 0 130 198 18 0 0 22660 1114514 @Ticket Price:@ }{ htmlgump 202 130 200 18 2 0 0 }{ xmfhtmltok 0 160 198 18 0 0 22660 1114514 @Tickets Bought:@ }{ htmlgump 202 160 200 18 3 0 0 }{ xmfhtmltok 0 190 200 18 0 0 22660 1114514 @Odds of Winning:@ }{ htmlgump 202 190 200 18 4 0 0 }{ xmfhtmltok 0 220 198 18 0 0 22660 1114514 @Raffled Item:@ }{ xmfhtmltok 202 220 200 20 0 0 16 1114779 @The Lucky Sovereign Steed@ }{ tilepichue 90 314 8413 1152 }{ itemproperty 1074292781 }{ xmfhtmltok 200 243 200 18 0 0 22660 1154645 @Raffled Item Description:@ }{ htmlgump 202 263 197 140 5 1 1 }{ xmfhtmltok 0 450 198 18 0 0 22660 1114514 @Raffle Ticket Amount@ }{ resizepic 202 450 9350 193 22 }{ textentry 204 450 192 20 0 0 6 }{ button 168 480 4005 4007 1 0 1 }{ htmlgump 202 480 198 18 7 0 0 }{ xmfhtmltok 0 530 198 18 0 0 22660 1114514 @Raffle Ends:@ }{ htmlgump 202 530 198 18 8 0 0 }{ button 360 570 4020 4022 1 0 0 }{ xmfhtmltok 250 570 100 20 0 0 0 1114514 @#1060675@ }";

            Gump gump = GumpParser.Parse( 0x01, 0x01, 1, 1, layout,
                new[] { "unknown", "unknown", "unknown", "unknown", "unknown", "unknown", "", "", "" } );
        }

        [TestMethod]
        public void WillParseXmfHtmlTokString()
        {
            const string localPath = @"C:\Users\johns\Desktop\KvG Client 2.0";

            if ( !Directory.Exists( localPath ) )
            {
                Debug.WriteLine( "Not running test, requires Cliloc.enu" );
                return;
            }

            Cliloc.Initialize( localPath );

            const string layout = "{ xmfhtmltok 10 10 400 18 0 0 22660 1114513 @Heritage Raffle@ }";

            Gump gump = GumpParser.Parse( 0x01, 0x01, 1, 1, layout, new string[] { } );

            GumpElement ge = gump.GetElementByXY( 10, 10 );

            Assert.AreEqual( "<DIV ALIGN=CENTER>Heritage Raffle</DIV>", ge.Text );
        }

        [TestMethod]
        public void WillParseXmfHtmlTokCliloc()
        {
            const string localPath = @"C:\Users\johns\Desktop\KvG Client 2.0";

            if ( !Directory.Exists( localPath ) )
            {
                Debug.WriteLine( "Not running test, requires Cliloc.enu" );
                return;
            }

            Cliloc.Initialize( localPath );

            const string layout = "{ xmfhtmltok 250 570 100 20 0 0 0 1114514 @#1060675@ }";

            Gump gump = GumpParser.Parse( 0x01, 0x01, 1, 1, layout, new string[] { } );

            GumpElement ge = gump.GetElementByXY( 250, 570 );

            Assert.AreEqual( "<DIV ALIGN=RIGHT>CLOSE</DIV>", ge.Text );
        }

        [TestMethod]
        public void WillParseXmfHtmlTokNullArgs()
        {
            const string localPath = @"C:\Users\johns\Desktop\KvG Client 2.0";

            if ( !Directory.Exists( localPath ) )
            {
                Debug.WriteLine( "Not running test, requires Cliloc.enu" );
                return;
            }

            Cliloc.Initialize( localPath );

            const string base64 =
                "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";

            byte[] packet = Convert.FromBase64String( base64 );

            IncomingPacketHandlers.Initialize();
            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0xDD );

            try
            {
                handler.OnReceive( new PacketReader( packet, packet.Length, false ) );
            }
            catch ( Exception )
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void WontThrowExceptionEmptyLayout()
        {
            string base64 = "3QAsAHmUqh85QMYAAAEsAAAAyAAAAA0AAAABeJxjAAAAAQABAAAAAAAAAAA=";

            byte[] packet = Convert.FromBase64String( base64 );

            IncomingPacketHandlers.Initialize();
            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0xDD );

            try
            {
                handler.OnReceive( new PacketReader( packet, packet.Length, false ) );
            }
            catch ( Exception )
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void WillParseTilepicComma()
        {
            const string localPath = @"C:\Users\johns\Desktop\KvG Client 2.0";

            if ( !Directory.Exists( localPath ) )
            {
                Debug.WriteLine( "Not running test, requires Cliloc.enu" );
                return;
            }

            Cliloc.Initialize( localPath );

            const string base64 = "3QD8BZkjmgAPPvAAAACWAAAAyAAAAMkAAAFCeAFtUEuOwjAM5SiW2M4idpxPd1wFSoCKtomQKyEh7j52ZxYg2Fjyy/s5D2j7cwEHzwecl6m1odfFATuMaGDpL/v5OJZhbosYcFhE6gwpQs5Kc4j/E1WGRNFIUusoQwPE0HEMBt2n00WmUerVtB0BZg8U1jRPKSYlIwf0sNv+yXh1GsZipZCjunfA5Bl/3vqij0DZOjsi0/S32lo5SrkLYNDHlFW/phF3WSOV9HHYS0FP4JFXU7LfcPClYfJBD9v8AsDyVswAAAABAAAAFAAAAAh4AWNgZjBhMGAwAAAB4QCY";

            byte[] packet = Convert.FromBase64String( base64 );

            IncomingPacketHandlers.Initialize();
            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0xDD );

            try
            {
                handler.OnReceive( new PacketReader( packet, packet.Length, false ) );
            }
            catch ( Exception )
            {
                Assert.Fail();
            }
        }
    }
}