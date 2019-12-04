using ClassicAssist.Data.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests
{
    [TestClass]
    public class CommandsManagerTests
    {
        [TestMethod]
        public void WillHandleEncodedCommands()
        {
            byte[] packet =
            {
                0xAD, 0x1B, 0x00, 0xC0, 0x00, 0x5D, 0x00, 0x03, 0x45, 0x4E, 0x41, 0x00, 0x00, 0x10, 0x02, 0x2B,
                0x77, 0x68, 0x65, 0x72, 0x65, 0x20, 0x62, 0x61, 0x6E, 0x6B, 0x00
            };

            string text = CommandsManager.ParseUnicodeSpeech( packet, packet.Length );

            Assert.AreEqual( "+where bank", text );
        }
    }
}