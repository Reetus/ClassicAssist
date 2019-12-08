using System.Diagnostics;
using System.IO;
using Assistant;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.MacroCommands
{
    [TestClass]
    public class PropertiesCommandTests
    {
        [TestMethod]
        public void WillWaitForProperties()
        {
            Engine.PacketWaitEntries = new PacketWaitEntries();

            void OnInternalPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] != 0xD6 )
                {
                    Assert.Fail();
                }

                int serial = ( data[3] << 24 ) | ( data[4] << 16 ) | ( data[5] << 8 ) | data[6];

                byte[] packet = { 0xD6, 0x00, 0x09, 0x00, 0x01, data[3], data[4], data[5], data[6] };

                Engine.PacketWaitEntries.CheckWait( packet, PacketDirection.Incoming );
            }

            Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;

            bool result = PropertiesCommands.WaitForProperties( 0x00aabbcc, 5000 );

            Assert.IsTrue( result );

            Engine.PacketWaitEntries = null;
            Engine.InternalPacketSentEvent -= OnInternalPacketSentEvent;
        }

        [TestMethod]
        [TestCategory( "Data" )]
        public void WillReturnTrueOnPropertyName()
        {
            const string localPath = @"C:\Users\johns\Desktop\KvG Client 2.0";

            if ( !Directory.Exists( localPath ) )
            {
                Debug.WriteLine( "Not running test, requires Cliloc.enu" );
                return;
            }

            Cliloc.Initialize( localPath );

            byte[] packet =
            {
                0xD6, 0x00, 0x43, 0x00, 0x01, 0x40, 0xC5, 0xF6, 0x09, 0x00, 0x00, 0x02, 0xA0, 0x3B, 0xCF, 0x00,
                0x0F, 0x9E, 0xD5, 0x00, 0x00, 0x00, 0x10, 0x5E, 0x95, 0x00, 0x04, 0x37, 0x00, 0x34, 0x00, 0x00,
                0x10, 0x5C, 0x71, 0x00, 0x1A, 0x34, 0x00, 0x39, 0x00, 0x09, 0x00, 0x31, 0x00, 0x32, 0x00, 0x35,
                0x00, 0x09, 0x00, 0x37, 0x00, 0x31, 0x00, 0x09, 0x00, 0x35, 0x00, 0x35, 0x00, 0x30, 0x00, 0x00,
                0x00, 0x00, 0x00
            };

            Item item = new Item( 0x40c5f609 );

            Engine.Items.Add( item );

            IncomingPacketHandlers.Initialize();

            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0xD6 );

            handler.OnReceive( new PacketReader( packet, packet.Length, false ) );

            Assert.IsNotNull( item.Properties );

            bool result = PropertiesCommands.Property( item.Serial, "Contents" );

            Assert.IsTrue( result );

            result = PropertiesCommands.Property( item.Serial, "Donkey" );

            Assert.IsFalse( result );

            Engine.Items.Clear();
        }

        [TestMethod]
        [TestCategory( "Data" )]
        public void WillGetPropertyValue()
        {
            const string localPath = @"C:\Users\johns\Desktop\KvG Client 2.0";

            if (!Directory.Exists( localPath ))
            {
                Debug.WriteLine( "Not running test, requires Cliloc.enu" );
                return;
            }

            Cliloc.Initialize( localPath );

            byte[] packet =
            {
                0xD6, 0x00, 0x43, 0x00, 0x01, 0x40, 0xC5, 0xF6, 0x09, 0x00, 0x00, 0x02, 0xA0, 0x3B, 0xCF, 0x00,
                0x0F, 0x9E, 0xD5, 0x00, 0x00, 0x00, 0x10, 0x5E, 0x95, 0x00, 0x04, 0x37, 0x00, 0x34, 0x00, 0x00,
                0x10, 0x5C, 0x71, 0x00, 0x1A, 0x34, 0x00, 0x39, 0x00, 0x09, 0x00, 0x31, 0x00, 0x32, 0x00, 0x35,
                0x00, 0x09, 0x00, 0x37, 0x00, 0x31, 0x00, 0x09, 0x00, 0x35, 0x00, 0x35, 0x00, 0x30, 0x00, 0x00,
                0x00, 0x00, 0x00
            };

            Item item = new Item( 0x40c5f609 );

            Engine.Items.Add( item );

            IncomingPacketHandlers.Initialize();

            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0xD6 );

            handler.OnReceive( new PacketReader( packet, packet.Length, false ) );

            Assert.IsNotNull( item.Properties );

            int value = PropertiesCommands.PropertyValue<int>( item.Serial, "Contents" );

            Assert.AreEqual( 49, value );

            value = PropertiesCommands.PropertyValue<int>( item.Serial, "Contents", 2 );

            Assert.AreEqual( 71, value );

            Engine.Items.Clear();
        }
    }
}