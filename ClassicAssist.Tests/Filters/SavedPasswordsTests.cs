using System;
using System.Text;
using ClassicAssist.Data;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.Filters
{
    [TestClass]
    public class SavedPasswordsTests
    {
        [TestMethod]
        public void WillSavePasswordIfEnabled()
        {
            AssistantOptions.SavedPasswords.Clear();

            OutgoingPacketFilters.Initialize();

            PacketWriter writer = new PacketWriter( 62 );
            writer.Write( (byte) 0x80 );
            writer.WriteAsciiFixed( "fakeusername", 30 );
            writer.WriteAsciiFixed( "fakepassword", 30 );
            writer.Write( (byte) 0 );

            byte[] packet = writer.ToArray();
            int length = packet.Length;

            OutgoingPacketFilters.CheckPacket( ref packet, ref length );

            Assert.AreEqual( 0, AssistantOptions.SavedPasswords.Count );

            AssistantOptions.SavePasswords = true;

            OutgoingPacketFilters.CheckPacket( ref packet, ref length );

            Assert.AreEqual( 1, AssistantOptions.SavedPasswords.Count );

            AssistantOptions.SavedPasswords.Clear();
        }

        [TestMethod]
        public void WillSavePasswordOnlyIfBlanl()
        {
            AssistantOptions.SavedPasswords.Clear();

            OutgoingPacketFilters.Initialize();

            AssistantOptions.SavePasswords = true;
            AssistantOptions.SavePasswordsOnlyBlank = true;

            PacketWriter writer = new PacketWriter( 62 );
            writer.Write( (byte) 0x80 );
            writer.WriteAsciiFixed( "fakeusername", 30 );
            writer.WriteAsciiFixed( "fakepassword", 30 );
            writer.Write( (byte) 0 );

            byte[] packet = writer.ToArray();
            int length = packet.Length;

            OutgoingPacketFilters.CheckPacket( ref packet, ref length );

            Assert.AreEqual( 1, AssistantOptions.SavedPasswords.Count );

            PacketWriter writer2 = new PacketWriter( 62 );
            writer2.Write( (byte) 0x80 );
            writer2.WriteAsciiFixed( "fakeusername", 30 );
            writer2.WriteAsciiFixed( "", 30 );
            writer2.Write( (byte) 0 );

            byte[] packet2 = writer2.ToArray();
            int length2 = packet2.Length;

            OutgoingPacketFilters.CheckPacket( ref packet2, ref length2 );

            byte[] tmp = new byte[30];
            Buffer.BlockCopy( packet2, 31, tmp, 0, 30 );

            Assert.AreEqual( "fakepassword", Encoding.ASCII.GetString( tmp ).TrimEnd( '\0' ) );

            PacketWriter writer3 = new PacketWriter( 62 );
            writer3.Write( (byte) 0x80 );
            writer3.WriteAsciiFixed( "fakeusername", 30 );
            writer3.WriteAsciiFixed( "butter", 30 );
            writer3.Write( (byte) 0 );

            byte[] packet3 = writer3.ToArray();
            int length3 = packet3.Length;

            OutgoingPacketFilters.CheckPacket( ref packet3, ref length3 );

            byte[] tmp2 = new byte[30];
            Buffer.BlockCopy( packet3, 31, tmp2, 0, 30 );

            Assert.AreEqual( "butter", Encoding.ASCII.GetString( tmp2 ).TrimEnd( '\0' ) );

            AssistantOptions.SavedPasswords.Clear();
        }
    }
}