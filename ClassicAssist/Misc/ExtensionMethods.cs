using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        public static void AddRangeSorted<T>( this IList<T> list, IEnumerable<T> items )
        {
            foreach ( T item in items )
            {
                list.AddSorted( item );
            }
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

        // https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/interop-with-other-asynchronous-patterns-and-types?redirectedfrom=MSDN#WHToTap
        public static Task<bool> ToTask( this EventWaitHandle waitHandle, Func<bool> resultAction = null )
        {
            if ( waitHandle == null )
            {
                throw new ArgumentNullException( nameof( waitHandle ) );
            }

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            RegisteredWaitHandle rwh = ThreadPool.RegisterWaitForSingleObject( waitHandle,
                delegate { tcs.TrySetResult( resultAction?.Invoke() ?? true ); }, null, -1, true );

            Task<bool> t = tcs.Task;

            t.ContinueWith( antecedent => rwh.Unregister( null ) );

            return t;
        }

        public static Task ToTask( this IEnumerable<EventWaitHandle> waitHandles )
        {
            List<Task<bool>> tasks = waitHandles.Select( waitHandle => waitHandle.ToTask() ).ToList();

            return Task.WhenAll( tasks );
        }

        public static string SHA1( this string str )
        {
            using ( SHA1Managed sha1 = new SHA1Managed() )
            {
                byte[] hash = sha1.ComputeHash( Encoding.UTF8.GetBytes( str ) );
                StringBuilder formatted = new StringBuilder( 2 * hash.Length );

                foreach ( byte b in hash )
                {
                    formatted.AppendFormat( "{0:X2}", b );
                }

                return formatted.ToString();
            }
        }
    }
}