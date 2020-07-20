using System;
using System.Runtime.InteropServices;

namespace ClassicAssist.UO
{
    public static class Compression
    {
        [DllImport( "zlib32.dll", EntryPoint = "uncompress" )]
        private static extern int Uncompress32( byte[] dest, ref int destLen, byte[] source, int sourceLen );

        [DllImport( "zlib64.dll", EntryPoint = "uncompress" )]
        private static extern int Uncompress64( byte[] dest, ref int destLen, byte[] source, int sourceLen );

        [DllImport( "zlib32.dll", EntryPoint = "compress" )]
        private static extern int Compress32( byte[] dest, ref int destLen, byte[] source, int sourceLen );

        [DllImport( "zlib64.dll", EntryPoint = "compress" )]
        private static extern int Compress64( byte[] dest, ref int destLen, byte[] source, int sourceLen );

        public static bool Uncompress( ref byte[] destBuffer, ref int destLength, byte[] sourceBuffer, int sourceLen )
        {
            int success = Environment.Is64BitProcess
                ? Uncompress64( destBuffer, ref destLength, sourceBuffer, sourceLen )
                : Uncompress32( destBuffer, ref destLength, sourceBuffer, sourceLen );

            return success == 0;
        }

        public static byte[] Compress( byte[] bytes )
        {
            byte[] compressBytes = new byte[(int) ( bytes.Length * 1.001 ) + 12];

            int length = compressBytes.Length;

            if ( Environment.Is64BitProcess )
            {
                Compress64( compressBytes, ref length, bytes, bytes.Length );
            }
            else
            {
                Compress32( compressBytes, ref length, bytes, bytes.Length );
            }

            Array.Resize( ref compressBytes, length );

            return compressBytes;
        }
    }
}