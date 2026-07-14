using System.IO;
using System.Text;
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

        [TestMethod]
        public void ConstructorFixedSizeSkipsOneByte()
        {
            byte[] packet = { 0xFF, 0xAA, 0xBB };

            PacketReader reader = new PacketReader( packet, packet.Length, true );

            Assert.AreEqual( 1, reader.Index );
            Assert.AreEqual( packet.Length, reader.Size );
            Assert.AreEqual( 0xAA, reader.ReadByte() );
        }

        [TestMethod]
        public void ConstructorVariableLengthSkipsThreeBytes()
        {
            byte[] packet = { 0xFF, 0x00, 0x05, 0xAA, 0xBB };

            PacketReader reader = new PacketReader( packet, packet.Length, false );

            Assert.AreEqual( 3, reader.Index );
            Assert.AreEqual( packet.Length, reader.Size );
            Assert.AreEqual( 0xAA, reader.ReadByte() );
        }

        [TestMethod]
        public void ReadByteSingleValue()
        {
            byte[] packet = { 0x00, 0x42 };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            Assert.AreEqual( 0x42, reader.ReadByte() );
        }

        [TestMethod]
        public void ReadBooleanReturnsCorrectValues()
        {
            byte[] packet = { 0x00, 0x01, 0x00, 0xFF };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            Assert.IsTrue( reader.ReadBoolean() );
            Assert.IsFalse( reader.ReadBoolean() );
            Assert.IsTrue( reader.ReadBoolean() );
        }

        [TestMethod]
        public void ReadSByteReturnsSignedValue()
        {
            byte[] packet = { 0x00, 0x80, 0x7F, 0xFF };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            Assert.AreEqual( (sbyte) -128, reader.ReadSByte() );
            Assert.AreEqual( (sbyte) 127, reader.ReadSByte() );
            Assert.AreEqual( (sbyte) -1, reader.ReadSByte() );
        }

        [TestMethod]
        public void ReadInt16BigEndian()
        {
            byte[] packet = { 0x00, 0x00, 0x01, 0x80, 0x00, 0xFF, 0xFF };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            Assert.AreEqual( (short) 1, reader.ReadInt16() );
            Assert.AreEqual( unchecked( (short) 0x8000 ), reader.ReadInt16() );
            Assert.AreEqual( (short) -1, reader.ReadInt16() );
        }

        [TestMethod]
        public void ReadInt32BigEndian()
        {
            byte[] packet = { 0x00, 0x00, 0x00, 0x00, 0x01, 0xFF, 0xFF, 0xFF, 0xFF };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            Assert.AreEqual( 1, reader.ReadInt32() );
            Assert.AreEqual( -1, reader.ReadInt32() );
        }

        [TestMethod]
        public void ReadUInt16ReturnsUnsigned()
        {
            byte[] packet = { 0x00, 0xFF, 0xFF };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            Assert.AreEqual( (ushort) 0xFFFF, reader.ReadUInt16() );
        }

        [TestMethod]
        public void ReadUInt32ReturnsUnsigned()
        {
            byte[] packet = { 0x00, 0xFF, 0xFF, 0xFF, 0xFF };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            Assert.AreEqual( 0xFFFFFFFF, reader.ReadUInt32() );
        }

        [TestMethod]
        public void ReadByteArrayReturnsCorrectData()
        {
            byte[] packet = { 0x00, 0x0A, 0x0B, 0x0C, 0x0D };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            byte[] result = reader.ReadByteArray( 3 );

            Assert.AreEqual( 3, result.Length );
            Assert.AreEqual( 0x0A, result[0] );
            Assert.AreEqual( 0x0B, result[1] );
            Assert.AreEqual( 0x0C, result[2] );
            Assert.AreEqual( 4, reader.Index );
        }

        [TestMethod]
        public void ReadStringNullTerminated()
        {
            byte[] packet = { 0x00, 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x00, 0xFF };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            string result = reader.ReadString();

            Assert.AreEqual( "Hello", result );
            Assert.AreEqual( 7, reader.Index );
        }

        [TestMethod]
        public void ReadStringFixedLength()
        {
            byte[] data = Encoding.ASCII.GetBytes( "Test\0\0\0\0" );
            byte[] packet = new byte[1 + data.Length];
            packet[0] = 0x00;
            System.Array.Copy( data, 0, packet, 1, data.Length );

            PacketReader reader = new PacketReader( packet, packet.Length, true );

            string result = reader.ReadString( 8 );

            Assert.AreEqual( "Test", result );
        }

        [TestMethod]
        public void ReadStringFixedLengthFullString()
        {
            byte[] packet = { 0x00, 0x41, 0x42, 0x43 };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            string result = reader.ReadString( 3 );

            Assert.AreEqual( "ABC", result );
        }

        [TestMethod]
        public void ReadStringSafeFixedLength()
        {
            byte[] packet = { 0x00, 0x48, 0x69, 0x00, 0xFF, 0xFF };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            string result = reader.ReadStringSafe( 5 );

            Assert.AreEqual( "Hi", result );
            Assert.AreEqual( 6, reader.Index );
        }

        [TestMethod]
        public void ReadStringSafeFiltersBadChars()
        {
            byte[] packet = { 0x00, 0x41, 0x01, 0x42, 0x00 };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            string result = reader.ReadStringSafe( 4 );

            Assert.AreEqual( "AB", result );
        }

        [TestMethod]
        public void ReadUnicodeStringNullTerminatedBigEndian()
        {
            byte[] packet = { 0x00, 0x00, 0x48, 0x00, 0x69, 0x00, 0x00 };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            string result = reader.ReadUnicodeString();

            Assert.AreEqual( "Hi", result );
        }

        [TestMethod]
        public void ReadUnicodeStringFixedLengthLittleEndian()
        {
            string text = "Test";
            byte[] encoded = Encoding.Unicode.GetBytes( text );
            byte[] packet = new byte[1 + encoded.Length];
            packet[0] = 0x00;
            System.Array.Copy( encoded, 0, packet, 1, encoded.Length );

            PacketReader reader = new PacketReader( packet, packet.Length, true );

            string result = reader.ReadUnicodeString( encoded.Length );

            Assert.AreEqual( "Test", result );
        }

        [TestMethod]
        public void ReadUnicodeStringBEFixedLength()
        {
            string text = "AB";
            byte[] encoded = Encoding.BigEndianUnicode.GetBytes( text );
            byte[] packet = new byte[1 + encoded.Length];
            packet[0] = 0x00;
            System.Array.Copy( encoded, 0, packet, 1, encoded.Length );

            PacketReader reader = new PacketReader( packet, packet.Length, true );

            string result = reader.ReadUnicodeStringBE( 2 );

            Assert.AreEqual( "AB", result );
        }

        [TestMethod]
        public void ReadUnicodeStringLENullTerminated()
        {
            byte[] packet = { 0x00, 0x48, 0x00, 0x69, 0x00, 0x00, 0x00 };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            string result = reader.ReadUnicodeStringLE();

            Assert.AreEqual( "Hi", result );
        }

        [TestMethod]
        public void SeekBeginSetsAbsolutePosition()
        {
            byte[] packet = { 0x00, 0xAA, 0xBB, 0xCC, 0xDD };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            reader.ReadByte();
            Assert.AreEqual( 2, reader.Index );

            reader.Seek( 1, SeekOrigin.Begin );
            Assert.AreEqual( 1, reader.Index );
            Assert.AreEqual( 0xAA, reader.ReadByte() );
        }

        [TestMethod]
        public void SeekCurrentAdvancesPosition()
        {
            byte[] packet = { 0x00, 0xAA, 0xBB, 0xCC, 0xDD };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            reader.Seek( 2, SeekOrigin.Current );
            Assert.AreEqual( 3, reader.Index );
            Assert.AreEqual( 0xCC, reader.ReadByte() );
        }

        [TestMethod]
        public void SeekEndPositionsFromEnd()
        {
            byte[] packet = { 0x00, 0xAA, 0xBB, 0xCC, 0xDD };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            reader.Seek( -1, SeekOrigin.End );
            Assert.AreEqual( 4, reader.Index );
            Assert.AreEqual( 0xDD, reader.ReadByte() );
        }

        [TestMethod]
        public void GetDataReturnsFullPacket()
        {
            byte[] packet = { 0x00, 0xAA, 0xBB, 0xCC };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            reader.ReadByte();

            byte[] data = reader.GetData();

            Assert.AreEqual( packet.Length, data.Length );
            Assert.AreEqual( 0x00, data[0] );
            Assert.AreEqual( 0xAA, data[1] );
            Assert.AreEqual( 0xBB, data[2] );
            Assert.AreEqual( 0xCC, data[3] );
        }

        [TestMethod]
        public void IndexAndSizeProperties()
        {
            byte[] packet = { 0x00, 0x00, 0x06, 0xAA, 0xBB, 0xCC };
            PacketReader reader = new PacketReader( packet, packet.Length, false );

            Assert.AreEqual( 3, reader.Index );
            Assert.AreEqual( 6, reader.Size );

            reader.ReadByte();

            Assert.AreEqual( 4, reader.Index );
            Assert.AreEqual( 6, reader.Size );
        }

        [TestMethod]
        public void IsSafeCharValidatesCorrectly()
        {
            Assert.IsTrue( PacketReader.IsSafeChar( 0x20 ) );
            Assert.IsTrue( PacketReader.IsSafeChar( 'A' ) );
            Assert.IsTrue( PacketReader.IsSafeChar( '~' ) );
            Assert.IsFalse( PacketReader.IsSafeChar( 0x00 ) );
            Assert.IsFalse( PacketReader.IsSafeChar( 0x1F ) );
            Assert.IsFalse( PacketReader.IsSafeChar( 0xFFFE ) );
            Assert.IsFalse( PacketReader.IsSafeChar( 0xFFFF ) );
        }

        [TestMethod]
        public void SequentialReadsAdvancePosition()
        {
            byte[] packet =
            {
                0x00, 0x00, 0x0D,
                0x11, 0x22, 0x33, 0x44,
                0x55, 0x66,
                0x77,
                0x48, 0x69, 0x00
            };

            PacketReader reader = new PacketReader( packet, packet.Length, false );

            Assert.AreEqual( 3, reader.Index );

            int i32 = reader.ReadInt32();
            Assert.AreEqual( 0x11223344, i32 );
            Assert.AreEqual( 7, reader.Index );

            short i16 = reader.ReadInt16();
            Assert.AreEqual( 0x5566, i16 );
            Assert.AreEqual( 9, reader.Index );

            byte b = reader.ReadByte();
            Assert.AreEqual( 0x77, b );
            Assert.AreEqual( 10, reader.Index );

            string s = reader.ReadString();
            Assert.AreEqual( "Hi", s );
        }

        [TestMethod]
        public void ReadStringEmptyNullTerminated()
        {
            byte[] packet = { 0x00, 0x00 };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            string result = reader.ReadString();

            Assert.AreEqual( "", result );
        }

        [TestMethod]
        public void ReadUnicodeStringEmptyNullTerminated()
        {
            byte[] packet = { 0x00, 0x00, 0x00 };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            string result = reader.ReadUnicodeString();

            Assert.AreEqual( "", result );
        }

        [TestMethod]
        public void ReadUnicodeStringLEEmptyNullTerminated()
        {
            byte[] packet = { 0x00, 0x00, 0x00 };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            string result = reader.ReadUnicodeStringLE();

            Assert.AreEqual( "", result );
        }

        [TestMethod]
        public void ReadByteArrayZeroLength()
        {
            byte[] packet = { 0x00, 0xAA };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            byte[] result = reader.ReadByteArray( 0 );

            Assert.AreEqual( 0, result.Length );
            Assert.AreEqual( 1, reader.Index );
        }

        [TestMethod]
        public void SeekReturnsNewPosition()
        {
            byte[] packet = { 0x00, 0xAA, 0xBB, 0xCC };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            long pos = reader.Seek( 3, SeekOrigin.Begin );

            Assert.AreEqual( 3, pos );
            Assert.AreEqual( 3, reader.Index );
        }

        [TestMethod]
        public void ReadMixedTypesFromVariableLengthPacket()
        {
            byte[] name = Encoding.ASCII.GetBytes( "Shmoo\0" );
            byte[] packet = new byte[3 + 4 + 2 + name.Length + 1];
            packet[0] = 0xAA;
            packet[1] = 0x00;
            packet[2] = (byte) packet.Length;
            packet[3] = 0x00;
            packet[4] = 0x00;
            packet[5] = 0x00;
            packet[6] = 0x01;
            packet[7] = 0x00;
            packet[8] = 0x0A;
            System.Array.Copy( name, 0, packet, 9, name.Length );

            PacketReader reader = new PacketReader( packet, packet.Length, false );

            int serial = reader.ReadInt32();
            short id = reader.ReadInt16();
            string readName = reader.ReadString();

            Assert.AreEqual( 1, serial );
            Assert.AreEqual( 10, id );
            Assert.AreEqual( "Shmoo", readName );
        }

        [TestMethod]
        public void GetDataReturnsIndependentCopy()
        {
            byte[] packet = { 0x00, 0xAA, 0xBB };
            PacketReader reader = new PacketReader( packet, packet.Length, true );

            byte[] data = reader.GetData();

            data[1] = 0xFF;

            byte[] data2 = reader.GetData();
            Assert.AreEqual( 0xAA, data2[1] );
        }

        [TestMethod]
        public void ReadWithSizeSubset()
        {
            byte[] packet = { 0x00, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE };
            PacketReader reader = new PacketReader( packet, 3, true );

            Assert.AreEqual( 3, reader.Size );
            Assert.AreEqual( 0xAA, reader.ReadByte() );
            Assert.AreEqual( 0xBB, reader.ReadByte() );
        }
    }
}
