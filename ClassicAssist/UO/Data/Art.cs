using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using ClassicAssist.Misc;

namespace ClassicAssist.UO.Data
{
    public static class Art
    {
        private static string _dataPath;
        private static bool _isUOPFormat;
        private static Lazy<Dictionary<int, Entry3D>> _lazyIndex;

        public static bool Initialize( string dataPath )
        {
            _dataPath = dataPath;

            if ( File.Exists( Path.Combine( dataPath, "artLegacyMUL.uop" ) ) )
            {
                _isUOPFormat = true;
            }

            _lazyIndex = new Lazy<Dictionary<int, Entry3D>>( () => _isUOPFormat ? LoadUOPIndex() : LoadMULIndex() );

            return true;
        }

        private static Dictionary<int, Entry3D> LoadMULIndex()
        {
            int entrySize = Marshal.SizeOf( typeof( Entry3D ) );
            byte[] buffer = new byte[entrySize];
            GCHandle pinnedBuffer = GCHandle.Alloc( buffer, GCHandleType.Pinned );
            int index = 0;

            Dictionary<int, Entry3D> indexes = new Dictionary<int, Entry3D>();

            using ( FileStream reader = File.Open( Path.Combine( _dataPath, "artidx.mul" ), FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite ) )
            {
                int bytesRead;

                do
                {
                    bytesRead = reader.Read( buffer, 0, entrySize );
                    indexes[index++] =
                        (Entry3D) Marshal.PtrToStructure( pinnedBuffer.AddrOfPinnedObject(), typeof( Entry3D ) );
                }
                while ( bytesRead > 0 );
            }

            return indexes;
        }

        private static Dictionary<int, Entry3D> LoadUOPIndex()
        {
            Dictionary<ulong, int> hashes = new Dictionary<ulong, int>();
            Dictionary<int, Entry3D> indexes = new Dictionary<int, Entry3D>();

            using ( FileStream reader = File.Open( Path.Combine( _dataPath, "artLegacyMUL.uop" ), FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite ) )
            {
                using ( BinaryReader bin = new BinaryReader( reader ) )
                {
                    reader.Seek( 12, SeekOrigin.Current );
                    int firstAddress = bin.ReadInt32();

                    for ( int i = 0; i < 0x10000 /*formatHeader.NumberOfFiles*/; i++ )
                    {
                        string entryName = $"build/artlegacymul/{i:D8}.tga";
                        ulong hash = HashFileName( entryName );

                        if ( !hashes.ContainsKey( hash ) )
                        {
                            hashes.Add( hash, i );
                        }
                    }

                    long nextAddress = firstAddress;

                    do
                    {
                        reader.Seek( nextAddress, SeekOrigin.Begin );
                        UOPBlockHeader blockHeader = reader.ReadStruct<UOPBlockHeader>();

                        nextAddress = blockHeader.NextAddress;

                        for ( int i = 0; i < blockHeader.NumberOfFiles; i++ )
                        {
                            UOPFileHeader fileHeader = reader.ReadStruct<UOPFileHeader>();

                            if ( fileHeader.DataHeaderAddress == 0 )
                            {
                                continue;
                            }

                            if ( hashes.ContainsKey( fileHeader.Hash ) )
                            {
                                int index = hashes[fileHeader.Hash];

                                indexes[index] = new Entry3D( (int) fileHeader.DataHeaderAddress + fileHeader.Length,
                                    fileHeader.IsCompressed == 1
                                        ? fileHeader.CompressedSize
                                        : fileHeader.DecompressedSize, 0 );
                            }
                        }
                    }
                    while ( nextAddress > 0 );
                }
            }

            return indexes;
        }

        public static Bitmap GetStatic( int itemID, int hue )
        {
            Bitmap bmp = GetStatic( itemID );

            if ( hue == 0 || bmp == null )
            {
                return bmp;
            }

            StaticTile tileData = TileData.GetStaticTile( itemID );

            Hues.ApplyHue( hue, bmp, tileData.Flags.HasFlag( TileFlags.PartialHue ) );

            return bmp;
        }

        public static unsafe Bitmap GetStatic( int itemId )
        {
            itemId += 0x4000;

            string fileName = Path.Combine( _dataPath, "art.mul" );

            if ( _isUOPFormat )
            {
                fileName = Path.Combine( _dataPath, "artLegacyMUL.uop" );
            }

            FileStream artFile = File.Open( fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );

            Entry3D entry;

            try
            {
                entry = _lazyIndex.Value[itemId];
            }
            catch ( KeyNotFoundException )
            {
                entry = _lazyIndex.Value[0x4000];
            }

            artFile.Seek( entry.Lookup, SeekOrigin.Begin );

            using ( BinaryReader reader = new BinaryReader( artFile ) )
            {
                reader.ReadInt32();

                int width = reader.ReadInt16();
                int height = reader.ReadInt16();

                if ( width <= 0 || height <= 0 )
                {
                    return null;
                }

                int[] lookups = new int[height];

                int start = (int) reader.BaseStream.Position + height * 2;

                for ( int i = 0; i < height; i++ )
                {
                    lookups[i] = start + reader.ReadUInt16() * 2;
                }

                Bitmap bmp = new Bitmap( width, height, PixelFormat.Format16bppArgb1555 );

                BitmapData bd = bmp.LockBits( new Rectangle( 0, 0, width, height ), ImageLockMode.WriteOnly,
                    PixelFormat.Format16bppArgb1555 );

                ushort* line = (ushort*) bd.Scan0;
                int delta = bd.Stride >> 1;

                int largestX = 0;
                int smallestX = width;
                int largestY = 0;
                int smallestY = height;

                for ( int y = 0; y < height; y++, line += delta )
                {
                    reader.BaseStream.Seek( lookups[y], SeekOrigin.Begin );

                    ushort* cur = line;

                    int xOffset, xRun;
                    int x = 0;

                    if ( y > largestY )
                    {
                        largestY = y + 1;
                    }

                    if ( y < smallestY )
                    {
                        smallestY = y;
                    }

                    while ( ( xOffset = reader.ReadUInt16() ) + ( xRun = reader.ReadUInt16() ) != 0 )
                    {
                        cur += xOffset;
                        ushort* end = cur + xRun;
                        x += xOffset;

                        if ( x < smallestX )
                        {
                            smallestX = x;
                        }

                        while ( cur < end )
                        {
                            *cur++ = (ushort) ( reader.ReadUInt16() ^ 0x8000 );
                            x++;
                        }

                        if ( x > largestX )
                        {
                            largestX = x;
                        }
                    }
                }

                bmp.UnlockBits( bd );

                return bmp;
            }
        }

        private static ulong HashFileName( string s )
        {
            uint ecx, edx, ebx, esi, edi;
            uint eax = ecx = edx = ebx = esi = edi = 0;
            ebx = edi = esi = (uint) s.Length + 0xDEADBEEF;
            int i = 0;

            for ( i = 0; i + 12 < s.Length; i += 12 )
            {
                edi = (uint) ( ( s[i + 7] << 24 ) | ( s[i + 6] << 16 ) | ( s[i + 5] << 8 ) | s[i + 4] ) + edi;
                esi = (uint) ( ( s[i + 11] << 24 ) | ( s[i + 10] << 16 ) | ( s[i + 9] << 8 ) | s[i + 8] ) + esi;
                edx = (uint) ( ( s[i + 3] << 24 ) | ( s[i + 2] << 16 ) | ( s[i + 1] << 8 ) | s[i] ) - esi;
                edx = ( edx + ebx ) ^ ( esi >> 28 ) ^ ( esi << 4 );
                esi += edi;
                edi = ( edi - edx ) ^ ( edx >> 26 ) ^ ( edx << 6 );
                edx += esi;
                esi = ( esi - edi ) ^ ( edi >> 24 ) ^ ( edi << 8 );
                edi += edx;
                ebx = ( edx - esi ) ^ ( esi >> 16 ) ^ ( esi << 16 );
                esi += edi;
                edi = ( edi - ebx ) ^ ( ebx >> 13 ) ^ ( ebx << 19 );
                ebx += esi;
                esi = ( esi - edi ) ^ ( edi >> 28 ) ^ ( edi << 4 );
                edi += ebx;
            }

            if ( s.Length - i > 0 )
            {
                switch ( s.Length - i )
                {
                    case 12:
                        esi += (uint) s[i + 11] << 24;
                        goto case 11;
                    case 11:
                        esi += (uint) s[i + 10] << 16;
                        goto case 10;
                    case 10:
                        esi += (uint) s[i + 9] << 8;
                        goto case 9;
                    case 9:
                        esi += s[i + 8];
                        goto case 8;
                    case 8:
                        edi += (uint) s[i + 7] << 24;
                        goto case 7;
                    case 7:
                        edi += (uint) s[i + 6] << 16;
                        goto case 6;
                    case 6:
                        edi += (uint) s[i + 5] << 8;
                        goto case 5;
                    case 5:
                        edi += s[i + 4];
                        goto case 4;
                    case 4:
                        ebx += (uint) s[i + 3] << 24;
                        goto case 3;
                    case 3:
                        ebx += (uint) s[i + 2] << 16;
                        goto case 2;
                    case 2:
                        ebx += (uint) s[i + 1] << 8;
                        goto case 1;
                    case 1:
                        ebx += s[i];
                        break;
                }

                esi = ( esi ^ edi ) - ( ( edi >> 18 ) ^ ( edi << 14 ) );
                ecx = ( esi ^ ebx ) - ( ( esi >> 21 ) ^ ( esi << 11 ) );
                edi = ( edi ^ ecx ) - ( ( ecx >> 7 ) ^ ( ecx << 25 ) );
                esi = ( esi ^ edi ) - ( ( edi >> 16 ) ^ ( edi << 16 ) );
                edx = ( esi ^ ecx ) - ( ( esi >> 28 ) ^ ( esi << 4 ) );
                edi = ( edi ^ edx ) - ( ( edx >> 18 ) ^ ( edx << 14 ) );
                eax = ( esi ^ edi ) - ( ( edi >> 8 ) ^ ( edi << 24 ) );
                return ( (ulong) edi << 32 ) | eax;
            }

            return ( (ulong) esi << 32 ) | eax;
        }

        #region Structures

        [StructLayout( LayoutKind.Explicit )]
        private struct Entry3D
        {
            [FieldOffset( 0 )] public readonly int Lookup;
            [FieldOffset( 4 )] public readonly int Length;
            [FieldOffset( 8 )] public readonly int Extra;

            public Entry3D( int lookup, int length, int extra )
            {
                Lookup = lookup;
                Length = length;
                Extra = extra;
            }
        }

        [StructLayout( LayoutKind.Explicit, Size = 12 )]
        private struct UOPBlockHeader
        {
            [FieldOffset( 0 )] public readonly int NumberOfFiles;
            [FieldOffset( 4 )] public readonly long NextAddress;
        }

        [StructLayout( LayoutKind.Explicit, Pack = 0, Size = 34 )]
        private struct UOPFileHeader
        {
            [FieldOffset( 0 )] public readonly long DataHeaderAddress;
            [FieldOffset( 8 )] public readonly int Length;
            [FieldOffset( 12 )] public readonly int CompressedSize;
            [FieldOffset( 16 )] public readonly int DecompressedSize;
            [FieldOffset( 20 )] public readonly ulong Hash;
            [FieldOffset( 28 )] public readonly int Unknown;
            [FieldOffset( 32 )] public readonly short IsCompressed;
        }

        #endregion
    }
}