using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ClassicAssist.UO.Data
{
    public static class Hues
    {
        private const int BlockCount = 375;
        private static string _dataPath;

        public static Lazy<HueEntry[]> _lazyHueEntries;

        public static bool Initialize( string dataPath )
        {
            _dataPath = dataPath;

            _lazyHueEntries = new Lazy<HueEntry[]>( LoadHueIndex );

            return true;
        }

        private static HueEntry[] LoadHueIndex()
        {
            HueEntry[] entries = new HueEntry[3000];

            if ( !File.Exists( Path.Combine( _dataPath, "hues.mul" ) ) )
            {
                throw new FileNotFoundException( "File not found", "hues.mul" );
            }

            using ( FileStream reader = File.Open( Path.Combine( _dataPath, "hues.mul" ), FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite ) )
            {
                BinaryReader binaryReader = new BinaryReader( reader );
                int total = 0;

                for ( int i = 0; i < BlockCount; i++ )
                {
                    binaryReader.ReadInt32();

                    for ( int j = 0; j < 8; ++j, ++total )
                    {
                        entries[total] = HueEntry.Read( binaryReader );
                    }
                }
            }

            return entries;
        }

        public static unsafe void ApplyHue( int hue, Bitmap bmp, bool onlyHueGrayPixels )
        {
            BitmapData bd = bmp.LockBits( new Rectangle( 0, 0, bmp.Width, bmp.Height ), ImageLockMode.ReadWrite,
                PixelFormat.Format16bppArgb1555 );

            hue = ( hue & 0x3FFF ) - 1;

            int stride = bd.Stride >> 1;
            int width = bd.Width;
            int height = bd.Height;
            int delta = stride - width;

            ushort* pBuffer = (ushort*) bd.Scan0;
            ushort* pLineEnd = pBuffer + width;
            ushort* pImageEnd = pBuffer + stride * height;

            if ( onlyHueGrayPixels )
            {
                while ( pBuffer < pImageEnd )
                {
                    while ( pBuffer < pLineEnd )
                    {
                        int c = *pBuffer;

                        if ( c != 0 )
                        {
                            int r = ( c >> 10 ) & 0x1F;
                            int g = ( c >> 5 ) & 0x1F;
                            int b = c & 0x1F;

                            if ( r == g && r == b )
                            {
                                *pBuffer = (ushort) _lazyHueEntries.Value[hue].Colors[( c >> 10 ) & 0x1F];
                            }
                        }

                        ++pBuffer;
                    }

                    pBuffer += delta;
                    pLineEnd += stride;
                }
            }
            else
            {
                while ( pBuffer < pImageEnd )
                {
                    while ( pBuffer < pLineEnd )
                    {
                        if ( *pBuffer != 0 )
                        {
                            int index = ( *pBuffer >> 10 ) & 0x1F;
                            HueEntry hueEntry = _lazyHueEntries.Value[hue];
                            *pBuffer = (ushort) hueEntry.Colors[index];
                        }

                        ++pBuffer;
                    }

                    pBuffer += delta;
                    pLineEnd += stride;
                }
            }

            bmp.UnlockBits( bd );
        }
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public struct HueEntry
    {
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 32 )]
        public short[] Colors;

        public short TableStart;
        public short TableEnd;

        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 20 )]
        public string Name;

        public static HueEntry Read( BinaryReader reader )
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            HueEntry entry = new HueEntry();

            entry.Colors = new short[32];

            for ( int i = 0; i < 32; ++i )
            {
                entry.Colors[i] = (short) ( reader.ReadUInt16() | 0x8000 );
            }

            entry.TableStart = (short) ( reader.ReadUInt16() | 0x8000 );
            entry.TableEnd = (short) ( reader.ReadUInt16() | 0x8000 );
            entry.Name = Encoding.ASCII.GetString( reader.ReadBytes( 20 ) ).TrimEnd( '\0' );

            return entry;
        }
    }
}