using System;
using System.IO;
using System.Text;

namespace ClassicAssist.UO.Data
{
    public class PacketReader
    {
        private readonly MemoryStream _stream;

        public PacketReader( byte[] data, int size, bool fixedSize )
        {
            _stream = new MemoryStream( data, 0, size, false );

            _stream.Seek( fixedSize ? 1 : 3, SeekOrigin.Current );
        }

        public long Index => _stream.Position;
        public long Size => _stream.Length;

        public byte[] GetData()
        {
            return _stream.ToArray();
        }

        public static bool IsSafeChar( int c )
        {
            return c >= 0x20 && c < 0xFFFE;
        }

        public bool ReadBoolean()
        {
            return _stream.ReadByte() != 0;
        }

        public byte ReadByte()
        {
            return (byte) _stream.ReadByte();
        }

        public byte[] ReadByteArray( int length )
        {
            byte[] bytes = new byte[length];

            for ( int i = 0; i < length; i++ )
            {
                bytes[i] = ReadByte();
            }

            return bytes;
        }

        public short ReadInt16()
        {
            byte[] buffer = new byte[2];
            _stream.Read( buffer, 0, 2 );

            return (short) ( ( buffer[0] << 8 ) | buffer[1] );
        }

        public int ReadInt32()
        {
            byte[] buffer = new byte[4];
            _stream.Read( buffer, 0, 4 );

            return ( buffer[0] << 24 ) | ( buffer[1] << 16 ) | ( buffer[2] << 8 ) | buffer[3];
        }

        public sbyte ReadSByte()
        {
            return (sbyte) ReadByte();
        }

        public string ReadString()
        {
            StringBuilder sb = new StringBuilder();

            int c;

            while ( Index + 1 < Size && ( c = ReadByte() ) != 0 )
            {
                sb.Append( (char) c );
            }

            return sb.ToString();
        }

        public string ReadString( int fixedLength )
        {
            byte[] buffer = new byte[fixedLength];

            _stream.Read( buffer, 0, fixedLength );

            return Encoding.ASCII.GetString( buffer ).TrimEnd( '\0' );
        }

        public string ReadStringSafe()
        {
            throw new NotImplementedException();
        }

        public string ReadStringSafe( int fixedLength )
        {
            StringBuilder output = new StringBuilder();

            for ( int i = 0; i < fixedLength; i++ )
            {
                char c = (char) ReadByte();

                if ( c == '\0' )
                {
                    ReadByteArray( fixedLength - i - 1 );
                    break;
                }

                if ( IsSafeChar( c ) )
                {
                    output.Append( c );
                }
            }

            return output.ToString();
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
            StringBuilder sb = new StringBuilder();

            int c;

            while ( Index + 1 < Size && ( c = ( ReadByte() << 8 ) | ReadByte() ) != 0 )
            {
                sb.Append( (char) c );
            }

            return sb.ToString();
        }

        public string ReadUnicodeString( int fixedLength )
        {
            byte[] buffer = new byte[fixedLength];

            _stream.Read( buffer, 0, fixedLength );

            return Encoding.Unicode.GetString( buffer );
        }

        public string ReadUnicodeStringBE( int fixedLength )
        {
            byte[] buffer = new byte[fixedLength*2];

            _stream.Read( buffer, 0, fixedLength * 2 );

            return Encoding.BigEndianUnicode.GetString( buffer );
        }

        public string ReadUnicodeStringLE()
        {
            StringBuilder sb = new StringBuilder();

            int c;

            while ( Index + 1 < Size && ( c = ReadByte() | ( ReadByte() << 8 ) ) != 0 )
            {
                sb.Append( (char) c );
            }

            return sb.ToString();
        }

        public string ReadUnicodeStringLESafe( int fixedLength )
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

        public string ReadUnicodeStringSafe( int fixedLength )
        {
            throw new NotImplementedException();
        }

        public long Seek( int offset, SeekOrigin origin )
        {
            return _stream.Seek( offset, origin );
        }
    }
}