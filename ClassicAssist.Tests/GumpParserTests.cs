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
            const string layout = "{ page 0 }{ resizepic 10 10 2600 500 135 }{ xmfhtmlgump 52 35 420 55 0 1 1 }{ button 60 95 4005 4007 1 0 1 }{ xmfhtmlgump 95 96 150 35 0 0 0 }{ button 285 95 4017 4019 1 0 0 }{ xmfhtmlgump 320 96 150 35 0 0 0 }";

            Gump gump = GumpParser.Parse( 0x01, 0x93a564c3, 10, 10, layout, new string[0] );

            Assert.IsNotNull( gump );

            GumpElement gumpElement = gump.GetElementByXY( 285, 95 );

            Assert.IsNotNull( gumpElement );

            Assert.AreEqual( ElementType.button, gumpElement.Type );
        }
    }
}