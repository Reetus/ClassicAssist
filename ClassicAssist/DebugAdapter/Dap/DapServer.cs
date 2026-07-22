using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ClassicAssist.DebugAdapter.Dap
{
    public static class DapServer
    {
        private static TcpListener _listener;
        private static CancellationTokenSource _cts;
        private static DapSession _currentSession;

        public static bool IsRunning => _listener != null;

        public static int Port { get; private set; }

        public static void Initialize( int port )
        {
            if ( _listener != null )
            {
                return;
            }

            Port = port;

            // Create DebugManager early so macro start/stop events are tracked
            // before any DAP client connects
            DebugManager.GetInstance();

            _cts = new CancellationTokenSource();

            try
            {
                // Bind to loopback only — the debug port must never be exposed on the network
                _listener = new TcpListener( IPAddress.Loopback, port );
                _listener.Start();
            }
            catch
            {
                // Binding failed — tear down partial state so IsRunning stays false and a later
                // Initialize attempt isn't short-circuited by a stale, unusable listener.
                _cts.Dispose();
                _cts = null;
                _listener = null;
                throw;
            }

            CancellationToken token = _cts.Token;
            Task.Run( () => AcceptLoop( token ) );
        }

        public static void Shutdown()
        {
            _cts?.Cancel();
            _currentSession?.Close();

            try
            {
                _listener?.Stop();
            }
            catch
            {
                // ignored
            }

            _listener = null;
            _currentSession = null;
        }

        private static async Task AcceptLoop( CancellationToken token )
        {
            while ( !token.IsCancellationRequested && _listener != null )
            {
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();

                    _currentSession?.Close();
                    _currentSession = new DapSession( client, DebugManager.GetInstance() );

                    await _currentSession.RunAsync( token );
                }
                catch ( ObjectDisposedException )
                {
                    break;
                }
                catch ( SocketException )
                {
                    break;
                }
                catch ( Exception )
                {
                    // Listener stopped or transient error — exit if we've been shut down
                    if ( token.IsCancellationRequested )
                    {
                        break;
                    }
                }
                finally
                {
                    _currentSession = null;
                }
            }
        }

        public static void SendEvent( DapEvent evt )
        {
            _currentSession?.SendMessage( evt );
        }
    }
}
