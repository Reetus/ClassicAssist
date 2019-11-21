using System;
using System.IO;
using System.Text;

namespace ClassicAssist.UO.Data
{
    public class PacketReader
    {
        private readonly MemoryStream _stream;
        public long Size => _stream.Length;

        public PacketReader(byte[] data, int size, bool fixedSize)
        {
            _stream = new MemoryStream(data, 0, size, false);

            _stream.Seek( fixedSize ? 1 : 3, SeekOrigin.Current );
        }

        public byte[] GetData()
        {
            return _stream.ToArray();
        }

        public static bool IsSafeChar(int c)
        {
            return c >= 0x20 && c < 0xFFFE;
        }

        public bool ReadBoolean()
        {
            return _stream.ReadByte() != 0;
        }

        public byte ReadByte()
        {
            return (byte)_stream.ReadByte();
        }

        public byte[] ReadByteArray(int length)
        {
            byte[] bytes = new byte[length];

            for (int i = 0; i < length; i++)
            {
                bytes[i] = ReadByte();
            }

            return bytes;
        }

        public short ReadInt16()
        {
            byte[] buffer = new byte[2];
            _stream.Read(buffer, 0, 2);

            return (short)(buffer[0] << 8 | buffer[1]);
        }

        public int ReadInt32()
        {
            byte[] buffer = new byte[4];
            _stream.Read(buffer, 0, 4);

            return buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3];
        }

        public sbyte ReadSByte()
        {
            return (sbyte) ReadByte();
        }


        public string ReadString()
        {
            throw new NotImplementedException();
        }

        public string ReadString(int fixedLength)
        {
            throw new NotImplementedException();
        }

        public string ReadStringSafe()
        {
            throw new NotImplementedException();
        }

        public string ReadStringSafe(int fixedLength)
        {
            throw new NotImplementedException();
        }

        public ushort ReadUInt16()
        {
            return (ushort) ReadInt16();
        }

        public uint ReadUInt32()
        {
            return (uint) ReadInt32();
        }

        public string ReadUnicodeString()
        {
            throw new NotImplementedException();
        }

        public string ReadUnicodeString(int fixedLength)
        {
            throw new NotImplementedException();
        }

        public string ReadUnicodeStringLE()
        {
            throw new NotImplementedException();
        }

        public string ReadUnicodeStringLESafe(int fixedLength)
        {
            throw new NotImplementedException();
        }

        public string ReadUnicodeStringLESafe()
        {
            throw new NotImplementedException();
        }

        public string ReadUnicodeStringSafe()
        {
            throw new NotImplementedException();
        }

        public string ReadUnicodeStringSafe(int fixedLength)
        {
            throw new NotImplementedException();
        }

        public long Seek(int offset, SeekOrigin origin)
        {
            return _stream.Seek( offset, origin );
        }
    }
}