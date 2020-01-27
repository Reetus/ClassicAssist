using System.Diagnostics;
using System.IO;
using ClassicAssist.UO.Data;
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
    }
}