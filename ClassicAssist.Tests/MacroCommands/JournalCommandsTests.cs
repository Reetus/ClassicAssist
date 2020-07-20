using System.Diagnostics;
using System.IO;
using ClassicAssist.Shared;
using ClassicAssist.Data;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.MacroCommands
{
    [TestClass]
    public class JournalCommandsTests
    {
        [TestMethod]
        public void WillMatchSystem()
        {
            Engine.Journal.Write( new JournalEntry
            {
                Text = "You are now under the protection of town guards.", SpeechType = JournalSpeech.System
            } );

            Assert.IsTrue( JournalCommands.InJournal( "town guards", "system" ) );

            Engine.Journal.Clear();
        }

        [TestMethod]
        public void WillMatchSystemRegular()
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
                0xC1, 0x00, 0x32, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x03, 0xB2, 0x00, 0x03, 0x00, 0x07,
                0xA4, 0xE1, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00
            };

            IncomingPacketHandlers.Initialize();

            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0xc1 );

            Assert.IsNotNull( handler );

            handler.OnReceive( new PacketReader( packet, packet.Length, false ) );

            Assert.IsTrue( JournalCommands.InJournal( "fingers slip!", "system" ) );
        }

        [TestMethod]
        public void WillMatchAuthor()
        {
            Engine.Journal.Write( new JournalEntry
            {
                Text = "The quick brown bow jumped over the lazy dog.",
                SpeechType = JournalSpeech.Say,
                Name = "Tony"
            } );

            Assert.IsTrue( JournalCommands.InJournal( "lazy dog", "tony" ) );

            Engine.Journal.Clear();
        }

        [TestMethod]
        public void WillMatchAny()
        {
            Engine.Journal.Write( new JournalEntry
            {
                Text = "You are now under the protection of town guards.", SpeechType = JournalSpeech.System
            } );

            Engine.Journal.Write( new JournalEntry
            {
                Text = "The quick brown bow jumped over the lazy dog.",
                SpeechType = JournalSpeech.Say,
                Name = "Tony"
            } );

            Assert.IsTrue( JournalCommands.InJournal( "town guards" ) );

            Assert.IsTrue( JournalCommands.InJournal( "lazy dog" ) );

            Engine.Journal.Clear();
        }

        [TestMethod]
        public void WillClearJournal()
        {
            Engine.Journal.Write( new JournalEntry
            {
                Text = "The quick brown bow jumped over the lazy dog.",
                SpeechType = JournalSpeech.Say,
                Name = "Tony"
            } );

            JournalCommands.ClearJournal();

            Assert.AreEqual( 0, Engine.Journal.Count );

            Engine.Journal.Clear();
        }

        //[TestMethod]
        //public void WaitForJournalWillMatch()
        //{
        //    const string text = "You are now under the protection of town guards.";

        //    byte[] textBytes = Encoding.Unicode.GetBytes( text );

        //    PacketWriter pw = new PacketWriter( 48 + textBytes.Length );

        //    pw.Write( (byte) 0xAE );
        //    pw.Write( (short) ( 48 + textBytes.Length ));
        //    pw.Write((int)0  );
        //    pw.Write( (short) 0 );
        //    pw.Write((byte)1  ); // system
        //    pw.Seek( 39, SeekOrigin.Current );
        //    pw.Write(textBytes, 0, textBytes.Length  );

        //    IncomingPacketHandlers.Initialize();

        //    PacketHandler handler = IncomingPacketHandlers.GetHandler( 0xAE );

        //    Task<bool> t = Task.Run(() => JournalCommands.WaitForJournal("town guards", 20000, "system"));

        //    handler?.OnReceive(new PacketReader( pw.ToArray(), pw.ToArray().Length, false ));

        //    bool finished = t.Wait( 20000 );

        //    Assert.IsTrue( finished );

        //    bool result = t.Result;

        //    Assert.IsTrue( result );

        //    Engine.Journal.Clear();
        //}
    }
}