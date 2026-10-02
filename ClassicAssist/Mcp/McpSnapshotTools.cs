using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Assistant;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Mcp
{
    public static class McpSnapshotTools
    {
        private const int SRCCOPY = 0x00CC0020;
        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;

        [DllImport( "user32.dll" )]
        private static extern IntPtr GetDC( IntPtr hWnd );

        [DllImport( "user32.dll" )]
        private static extern int ReleaseDC( IntPtr hWnd, IntPtr hDC );

        [DllImport( "user32.dll" )]
        private static extern bool GetClientRect( IntPtr hWnd, out RECT lpRect );

        [DllImport( "user32.dll" )]
        private static extern int GetSystemMetrics( int nIndex );

        [DllImport( "gdi32.dll" )]
        private static extern bool BitBlt( IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
            IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop );

        [StructLayout( LayoutKind.Sequential )]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public static IReadOnlyList<McpTool> GetTools()
        {
            return new List<McpTool>
            {
                new McpTool
                {
                    Name = "getSnapshot",
                    Description =
                        "Capture the game client (or the whole desktop with fullscreen:true) and return it as an image the " +
                        "model can see, plus basic metadata. The capture is kept in memory and returned as base64 - no file is written.",
                    InputSchema = McpTools.ObjectSchema(
                        new JObject
                        {
                            ["fullscreen"] = new JObject
                            {
                                ["type"] = "boolean",
                                ["description"] = "Capture the entire desktop instead of the game client window (default false)."
                            },
                            ["maxWidth"] = McpTools.IntegerProperty(
                                "Optional maximum image width; wider captures are downscaled (default 0 = native)." ),
                            ["format"] = new JObject
                            {
                                ["type"] = "string",
                                ["enum"] = new JArray( "png", "jpeg" ),
                                ["description"] = "Image format: png (default) or jpeg (smaller)."
                            }
                        } )
                }
            };
        }

        public static CallToolResult Invoke( string name, JObject args )
        {
            try
            {
                switch ( name )
                {
                    case "getSnapshot":
                        return GetSnapshot( McpTools.GetBool( args, "fullscreen" ), McpTools.GetInt( args, "maxWidth" ),
                            McpTools.GetString( args, "format" ) );
                    default:
                        return null;
                }
            }
            catch ( Exception e )
            {
                return McpTools.Error( e.Message );
            }
        }

        private static CallToolResult GetSnapshot( bool fullscreen, int? maxWidth, string format )
        {
            ImageFormat imageFormat;
            string mimeType;
            string formatName;

            switch ( format?.ToLowerInvariant() )
            {
                case "jpeg":
                case "jpg":
                    imageFormat = ImageFormat.Jpeg;
                    mimeType = "image/jpeg";
                    formatName = "jpeg";
                    break;
                default:
                    imageFormat = ImageFormat.Png;
                    mimeType = "image/png";
                    formatName = "png";
                    break;
            }

            byte[] bytes;
            int width;
            int height;

            using ( Bitmap bitmap = Capture( fullscreen, maxWidth, out width, out height ) )
            {
                using ( MemoryStream stream = new MemoryStream() )
                {
                    bitmap.Save( stream, imageFormat );
                    bytes = stream.ToArray();
                }
            }

            return new CallToolResult
            {
                Content = new List<McpContent>
                {
                    new McpContent { Type = "image", Data = Convert.ToBase64String( bytes ), MimeType = mimeType },
                    new McpContent
                    {
                        Type = "text",
                        Text = new JObject
                        {
                            ["fullscreen"] = fullscreen,
                            ["width"] = width,
                            ["height"] = height,
                            ["format"] = formatName,
                            ["capturedAt"] = DateTime.Now.ToString( "o" ),
                            ["byteLength"] = bytes.Length
                        }.ToString( Formatting.Indented )
                    }
                }
            };
        }

        private static Bitmap Capture( bool fullscreen, int? maxWidth, out int width, out int height )
        {
            IntPtr dcOwner;
            IntPtr sourceDC;

            if ( fullscreen )
            {
                dcOwner = IntPtr.Zero;
                sourceDC = GetDC( IntPtr.Zero );
                width = GetSystemMetrics( SM_CXSCREEN );
                height = GetSystemMetrics( SM_CYSCREEN );
            }
            else
            {
                IntPtr window = Engine.WindowHandle;

                if ( window == IntPtr.Zero )
                {
                    throw new InvalidOperationException( "Game window handle is not available." );
                }

                dcOwner = window;
                GetClientRect( window, out RECT rect );
                width = rect.Right - rect.Left;
                height = rect.Bottom - rect.Top;
                sourceDC = GetDC( window );
            }

            try
            {
                if ( width <= 0 || height <= 0 )
                {
                    throw new InvalidOperationException( "Captured region is empty." );
                }

                // 24bpp so the PNG has no alpha channel (BitBlt leaves alpha undefined, which would
                // otherwise produce a transparent/black image).
                Bitmap bitmap = new Bitmap( width, height, PixelFormat.Format24bppRgb );

                using ( Graphics graphics = Graphics.FromImage( bitmap ) )
                {
                    IntPtr destinationDC = graphics.GetHdc();

                    try
                    {
                        BitBlt( destinationDC, 0, 0, width, height, sourceDC, 0, 0, SRCCOPY );
                    }
                    finally
                    {
                        graphics.ReleaseHdc( destinationDC );
                    }
                }

                if ( maxWidth != null && maxWidth.Value > 0 && width > maxWidth.Value )
                {
                    Bitmap resized = Resize( bitmap, maxWidth.Value );
                    bitmap.Dispose();

                    return resized;
                }

                return bitmap;
            }
            finally
            {
                ReleaseDC( dcOwner, sourceDC );
            }
        }

        private static Bitmap Resize( Bitmap source, int maxWidth )
        {
            int width = maxWidth;
            int height = Math.Max( 1, (int) Math.Round( source.Height * ( width / (double) source.Width ) ) );

            Bitmap resized = new Bitmap( width, height, PixelFormat.Format24bppRgb );

            using ( Graphics graphics = Graphics.FromImage( resized ) )
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage( source, 0, 0, width, height );
            }

            return resized;
        }
    }
}
