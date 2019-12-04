using System.IO;
using ClassicAssist.UO.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests
{
    [TestClass]
    public class PacketReaderTests
    {
        [TestMethod]
        public void PacketReaderWillReadCorrect()
        {
            byte[] packet = { 0x1B, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x0A };

            PacketReader reader = new PacketReader( packet, packet.Length, true );

            int int32 = reader.ReadInt32();
            int int16 = reader.ReadInt16();
            int int8 = reader.ReadByte();

            Assert.AreEqual( unchecked( (int) 0xAABBCCDD ), int32 );
            Assert.AreEqual( unchecked( (short) 0xEEFF ), int16 );
            Assert.AreEqual( 0x0A, int8 );

            reader.Seek( 1, SeekOrigin.Begin );

            uint uint32 = reader.ReadUInt32();
            uint uint16 = reader.ReadUInt16();
            sbyte uint8 = reader.ReadSByte();

            Assert.AreEqual( 0xAABBCCDD, uint32 );
            Assert.AreEqual( (ushort) 0xEEFF, uint16 );
            Assert.AreEqual( 0x0A, uint8 );
        }
    }
}