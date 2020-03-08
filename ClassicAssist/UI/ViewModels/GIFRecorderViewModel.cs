using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using AnimatedGif;
using Assistant;
using ClassicAssist.UI.Views;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace ClassicAssist.UI.ViewModels
{
    public class GIFRecorderViewModel : BaseViewModel
    {
        private readonly GIFRecorderWindow _window;
        private bool _isRecording;
        private bool _isWritingStream;
        private MemoryStream _lastStream;
        private ICommand _recordCommand;
        private ICommand _saveCommand;
        private CancellationTokenSource _token;

        public GIFRecorderViewModel( GIFRecorderWindow window ) : this()
        {
            _window = window;
        }

        public GIFRecorderViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public bool IsRecording
        {
            get => _isRecording;
            set => SetProperty( ref _isRecording, value );
        }

        public MemoryStream LastStream
        {
            get => _lastStream;
            set => SetProperty( ref _lastStream, value );
        }

        public ICommand RecordCommand => _recordCommand ?? ( _recordCommand = new RelayCommand( Record, o => true ) );

        public ICommand SaveCommand => _saveCommand ?? ( _saveCommand = new RelayCommand( Save, o => !IsRecording ) );

        private void Save( object obj )
        {
            if ( LastStream == null )
            {
                return;
            }

            string directory = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Screenshots" );

            if ( !Directory.Exists( directory ) )
            {
                Directory.CreateDirectory( directory );
            }

            DateTime now = DateTime.Now;

            string fileName =
                $"ClassicAssist-{now.Year}-{now.Month}-{now.Day}-{now.Hour}-{now.Minute}-{now.Second}.gif";

            string fullPath = Path.Combine( directory, fileName );

            using ( FileStream fs = new FileStream( fullPath, FileMode.Create ) )
            {
                LastStream.WriteTo( fs );
            }

            string args = $"/e, /select, \"{fullPath}\"";

            ProcessStartInfo info = new ProcessStartInfo { FileName = "explorer", Arguments = args };
            Process.Start( info );
        }

        private void Record( object obj )
        {
            if ( IsRecording )
            {
                _token.Cancel();
            }
            else
            {
                IsRecording = true;
                _token = new CancellationTokenSource();

                Window w = Window.GetWindow( _window );
                WindowInteropHelper wih = new WindowInteropHelper( w ?? throw new InvalidOperationException() );

                Task.Run( async () =>
                {
                    try
                    {
                        TimeSpan frameInterval = TimeSpan.FromSeconds( 1.0 / 5 );
                        MemoryStream ms = new MemoryStream();

                        using ( AnimatedGifCreator gif = new AnimatedGifCreator( ms, frameInterval.Milliseconds ) )
                        {
                            Rectangle rect = new Rectangle();
                            _dispatcher.Invoke( () =>
                                GetWindowRect( wih.Handle, ref rect ) );

                            Bitmap bmp = new Bitmap( rect.Width - rect.Left - 10, rect.Height - rect.Top - 70 );

                            Stopwatch sw = new Stopwatch();
                            sw.Start();

                            while ( true )
                            {
                                _dispatcher.Invoke( () =>
                                    GetWindowRect( wih.Handle, ref rect ) );

                                using ( Graphics g = Graphics.FromImage( bmp ) )
                                {
                                    g.CopyFromScreen( new Point( rect.Left + 5, rect.Top + 50 ), Point.Empty,
                                        new Size( bmp.Width, bmp.Height ) );
                                }

                                await gif.AddFrameAsync( bmp, -1, GifQuality.Bit8 );

                                sw.Stop();

                                int wait = frameInterval.Milliseconds - (int) sw.ElapsedMilliseconds;

                                if ( wait > 0 )
                                {
                                    await Task.Delay( wait );
                                }

                                if ( _token.IsCancellationRequested )
                                {
                                    return ms;
                                }
                            }
                        }
                    }
                    finally
                    {
                        _dispatcher.Invoke( () => IsRecording = false );
                    }
                } ).ContinueWith( t =>
                {
                    if ( !( t.Result is MemoryStream ms ) )
                    {
                        return;
                    }

                    if ( LastStream != null )
                    {
                        LastStream.Dispose();
                        LastStream = null;
                    }

                    LastStream = ms;
                    CommandManager.InvalidateRequerySuggested();
                } );
            }
        }

        [DllImport( "user32.dll" )]
        private static extern IntPtr GetWindowRect( IntPtr hWnd, ref Rectangle rect );
    }
}