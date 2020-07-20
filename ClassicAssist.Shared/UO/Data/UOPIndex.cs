using System;
using System.Collections.Generic;
using System.IO;

namespace ClassicAssist.UO.Data
{
    public class UOPIndex : IDisposable
    {
        private readonly UOPEntry[] _entries;
        private readonly int _length;
        private readonly BinaryReader _reader;

        public UOPIndex( Stream stream )
        {
            _reader = new BinaryReader( stream );
            _length = (int) stream.Length;

            if ( _reader.ReadInt32() != 0x50594D )
            {
                throw new ArgumentException( "Invalid UOP file." );
            }

            Version = _reader.ReadInt32();
            _reader.ReadInt32();
            int nextTable = _reader.ReadInt32();

            List<UOPEntry> entries = new List<UOPEntry>();

            do
            {
                stream.Seek( nextTable, SeekOrigin.Begin );
                int count = _reader.ReadInt32();
                nextTable = _reader.ReadInt32();
                _reader.ReadInt32();

                for ( int i = 0; i < count; ++i )
                {
                    int offset = _reader.ReadInt32();

                    if ( offset == 0 )
                    {
                        stream.Seek( 30, SeekOrigin.Current );
                        continue;
                    }

                    _reader.ReadInt64();
                    int length = _reader.ReadInt32();

                    entries.Add( new UOPEntry( offset, length ) );

                    stream.Seek( 18, SeekOrigin.Current );
                }
            }
            while ( nextTable != 0 && nextTable < _length );

            entries.Sort( OffsetComparer.Instance );

            foreach ( UOPEntry t in entries )
            {
                stream.Seek( t.Offset + 2, SeekOrigin.Begin );

                int dataOffset = _reader.ReadInt16();
                t.Offset += 4 + dataOffset;

                stream.Seek( dataOffset, SeekOrigin.Current );
                t.Order = _reader.ReadInt32();
            }

            entries.Sort();
            _entries = entries.ToArray();
        }

        public int Version { get; }

        public int Lookup( int offset )
        {
            int total = 0;

            foreach ( UOPEntry t in _entries )
            {
                int newTotal = total + t.Length;

                if ( offset < newTotal )
                {
                    return t.Offset + ( offset - total );
                }

                total = newTotal;
            }

            return _length;
        }

        public void Close()
        {
            _reader.Close();
        }

        private class OffsetComparer : IComparer<UOPEntry>
        {
            public static readonly IComparer<UOPEntry> Instance = new OffsetComparer();

            public int Compare( UOPEntry x, UOPEntry y )
            {
                if ( x == null || y == null )
                {
                    return -1;
                }

                return x.Offset.CompareTo( y.Offset );
            }
        }

        private class UOPEntry : IComparable<UOPEntry>
        {
            public readonly int Length;
            public int Offset;
            public int Order;

            public UOPEntry( int offset, int length )
            {
                Offset = offset;
                Length = length;
                Order = 0;
            }

            public int CompareTo( UOPEntry other )
            {
                return Order.CompareTo( other.Order );
            }
        }

        #region IDisposable Support

        private bool disposedValue; // To detect redundant calls

        protected virtual void Dispose( bool disposing )
        {
            if ( disposedValue )
            {
                return;
            }

            if ( disposing )
            {
                _reader.Dispose();
            }

            disposedValue = true;
        }

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        #endregion
    }
}