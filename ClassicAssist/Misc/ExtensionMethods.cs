using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Misc
{
    public static class ExtensionMethods
    {
        public static T ReadStruct<T>( this Stream stream ) where T : struct
        {
            int size = Marshal.SizeOf( typeof( T ) );

            byte[] buffer = new byte[size];

            stream.Read( buffer, 0, size );

            GCHandle pinnedBuffer = GCHandle.Alloc( buffer, GCHandleType.Pinned );

            T structure = (T) Marshal.PtrToStructure( pinnedBuffer.AddrOfPinnedObject(), typeof( T ) );

            pinnedBuffer.Free();

            return structure;
        }

        public static T GetPropertyAttribute<T>( this Type type, string propertyName )
        {
            if ( type == null )
            {
                return default;
            }

            T attr = default;

            PropertyInfo pi = type.GetProperty( propertyName );

            if ( pi != null )
            {
                attr = pi.GetCustomAttributes( false ).OfType<T>().SingleOrDefault();
            }

            return attr != null ? attr : default;
        }

        [DllImport( "gdi32.dll", EntryPoint = "DeleteObject" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        private static extern bool DeleteObject( [In] IntPtr hObject );

        public static ImageSource ToImageSource( this Bitmap bmp )
        {
            IntPtr handle = bmp.GetHbitmap();

            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap( handle, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions() );
            }
            finally
            {
                DeleteObject( handle );
            }
        }

        public static BitmapSource ToBitmapSource( this Bitmap bmp )
        {
            return Imaging.CreateBitmapSourceFromHBitmap( bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight( bmp.Width, bmp.Height ) );
        }

        public static void AddSorted<T>( this IList<T> list, T item, IComparer<T> comparer = null )
        {
            if ( comparer == null )
            {
                comparer = Comparer<T>.Default;
            }

            int i = 0;

            while ( i < list.Count && comparer.Compare( list[i], item ) < 0 )
            {
                i++;
            }

            list.Insert( i, item );
        }

        public static JArray ToJArray( this int[] arr )
        {
            JArray jArray = new JArray();

            foreach ( int i in arr )
            {
                jArray.Add( i );
            }

            return jArray;
        }

        public static int[] ToIntArray( this JToken jToken )
        {
            return jToken.Select( token => token.ToObject<int>() ).ToArray();
        }
    }
}