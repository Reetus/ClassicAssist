using System.Collections.Generic;
using System.Text;
using System.Threading;
using Assistant;
using ClassicAssist.Data.Filters;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.Filters
{
    [TestClass]
    public class ClilocFilterTests
    {
        [TestMethod]
        public void WillFilterLocalizedText()
        {
            Cliloc.Initialize( () =>
                new Dictionary<int, string> { { 500118, "You must wait a few moments to use another skill." } } );

            byte[] packet =
            {
                0xC1, 0x00, 0x32, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x03, 0xB2, 0x00, 0x03, 0x00, 0x07,
                0xA1, 0x96, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00
            };

            int length = packet.Length;

            const int cliloc = 500118;
            const string replaceText = "The quick brown fox jumped over the lazy dog.";

            AutoResetEvent are = new AutoResetEvent( false );

            void OnReceivedEvent( byte[] data, int _ )
            {
                if ( data[0] != 0xAE )
                {
                    Assert.Fail();
                }

                string text = Encoding.BigEndianUnicode.GetString( data, 48, data.Length - 48 );

                if ( text != replaceText )
                {
                    Assert.Fail();
                }

                are.Set();
            }

            Engine.InternalPacketReceivedEvent += OnReceivedEvent;

            ClilocFilter.IsEnabled = true;
            ClilocFilter.Filters.Add( cliloc, replaceText );

            IncomingPacketFilters.Initialize();
            IncomingPacketFilters.CheckPacket( ref packet, ref length );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.InternalPacketReceivedEvent -= OnReceivedEvent;
        }

        [TestMethod]
        public void WillFilterLocalizedTextAffix()
        {
            Cliloc.Initialize( () => new Dictionary<int, string> { { 1008158, "some damage has been healed : " } } );

            byte[] packet =
            {
                0xCC, 0x00, 0x36, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x06, 0x03, 0xB2, 0x00, 0x03, 0x00, 0x0F,
                0x62, 0x1E, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x31, 0x34, 0x00, 0x00, 0x00
            };

            int length = packet.Length;

            const int cliloc = 1008158;
            const string replaceText = "The value is: ";

            AutoResetEvent are = new AutoResetEvent( false );

            void OnReceivedEvent( byte[] data, int _ )
            {
                if ( data[0] != 0xAE )
                {
                    Assert.Fail();
                }

                string text = Encoding.BigEndianUnicode.GetString( data, 48, data.Length - 48 );

                if ( !text.StartsWith( replaceText ) )
                {
                    Assert.Fail();
                }

                are.Set();
            }

            Engine.InternalPacketReceivedEvent += OnReceivedEvent;

            ClilocFilter.IsEnabled = true;
            ClilocFilter.Filters.Add( cliloc, replaceText );

            IncomingPacketFilters.Initialize();
            IncomingPacketFilters.CheckPacket( ref packet, ref length );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.InternalPacketReceivedEvent -= OnReceivedEvent;
        }
    }
}