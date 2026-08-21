using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Mcp
{
    public static class McpServer
    {
        private const string PROTOCOL_VERSION = "2025-06-18";
        private const string SERVER_VERSION = "1.0.0";

        private static TcpListener _listener;
        private static CancellationTokenSource _cts;

        public static bool IsRunning => _listener != null;

        public static int Port { get; private set; }

        public static void Initialize( int port )
        {
            if ( _listener != null )
            {
                return;
            }

            Port = port;

            _cts = new CancellationTokenSource();

            try
            {
                // Bind to loopback only - the MCP port must never be exposed on the network.
                _listener = new TcpListener( IPAddress.Loopback, port );
                _listener.Start();
            }
            catch
            {
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

            try
            {
                _listener?.Stop();
            }
            catch
            {
                // ignored
            }

            _listener = null;
            _cts = null;
        }

        private static async Task AcceptLoop( CancellationToken token )
        {
            while ( !token.IsCancellationRequested && _listener != null )
            {
                TcpClient client;

                try
                {
                    client = await _listener.AcceptTcpClientAsync();
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
                    if ( token.IsCancellationRequested )
                    {
                        break;
                    }

                    continue;
                }

                _ = Task.Run( () => HandleClient( client, token ) );
            }
        }

        private static async Task HandleClient( TcpClient client, CancellationToken token )
        {
            using ( client )
            {
                using ( CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource( token ) )
                {
                    timeoutCts.CancelAfter( TimeSpan.FromSeconds( 30 ) );

                    try
                    {
                        using ( NetworkStream stream = client.GetStream() )
                        {
                            byte[] request = await ReadHttpRequestAsync( stream, timeoutCts.Token );

                            if ( request == null )
                            {
                                return;
                            }

                            byte[] response = Process( request );

                            if ( response != null )
                            {
                                await stream.WriteAsync( response, 0, response.Length, timeoutCts.Token );
                                await stream.FlushAsync( timeoutCts.Token );
                            }
                        }
                    }
                    catch
                    {
                        // ignored - client disconnected or timed out
                    }
                }
            }
        }

        private static async Task<byte[]> ReadHttpRequestAsync( NetworkStream stream, CancellationToken token )
        {
            byte[] buffer = new byte[8192];
            MemoryStream pending = new MemoryStream();
            int headerEnd = -1;
            int contentLength = 0;

            while ( true )
            {
                int read = await stream.ReadAsync( buffer, 0, buffer.Length, token );

                if ( read == 0 )
                {
                    return null;
                }

                pending.Write( buffer, 0, read );

                byte[] data = pending.GetBuffer();
                int length = (int) pending.Length;

                if ( headerEnd < 0 )
                {
                    headerEnd = IndexOfHeaderTerminator( data, length );

                    if ( headerEnd >= 0 )
                    {
                        contentLength = ParseContentLength( data, headerEnd );
                    }
                    else if ( length > 64 * 1024 )
                    {
                        return null;
                    }
                }

                if ( headerEnd >= 0 && length >= headerEnd + 4 + contentLength )
                {
                    byte[] result = new byte[headerEnd + 4 + contentLength];
                    Buffer.BlockCopy( data, 0, result, 0, result.Length );
                    return result;
                }
            }
        }

        private static byte[] Process( byte[] request )
        {
            int headerEnd = IndexOfHeaderTerminator( request, request.Length );

            if ( headerEnd < 0 )
            {
                return HttpResponse( 400, "Bad Request", Array.Empty<byte>() );
            }

            string headerText = Encoding.ASCII.GetString( request, 0, headerEnd );
            string[] lines = headerText.Split( new[] { "\r\n" }, StringSplitOptions.None );
            string requestLine = lines.Length > 0 ? lines[0] : string.Empty;
            string[] requestParts = requestLine.Split( new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );

            string method = requestParts.Length > 0 ? requestParts[0] : string.Empty;
            string path = requestParts.Length > 1 ? requestParts[1] : "/";

            if ( method.Equals( "OPTIONS", StringComparison.OrdinalIgnoreCase ) )
            {
                return HttpResponse( 204, "No Content", Array.Empty<byte>() );
            }

            if ( !method.Equals( "POST", StringComparison.OrdinalIgnoreCase ) )
            {
                return HttpResponse( 405, "Method Not Allowed", Array.Empty<byte>(), "Allow: POST, OPTIONS\r\n" );
            }

            if ( !path.Equals( "/", StringComparison.OrdinalIgnoreCase ) &&
                 !path.Equals( "/mcp", StringComparison.OrdinalIgnoreCase ) )
            {
                return HttpResponse( 404, "Not Found", Array.Empty<byte>() );
            }

            int bodyStart = headerEnd + 4;
            int bodyLength = request.Length - bodyStart;

            if ( bodyLength <= 0 )
            {
                return RpcError( null, -32600, "Invalid Request: empty body" );
            }

            string body = Encoding.UTF8.GetString( request, bodyStart, bodyLength );

            JsonRpcRequest rpcRequest;

            try
            {
                rpcRequest = JsonConvert.DeserializeObject<JsonRpcRequest>( body );
            }
            catch
            {
                return RpcError( null, -32700, "Parse error" );
            }

            if ( rpcRequest == null || string.IsNullOrEmpty( rpcRequest.Method ) )
            {
                return RpcError( null, -32600, "Invalid Request" );
            }

            JsonRpcResponse response = Handle( rpcRequest );

            if ( response == null )
            {
                // Notification (or unknown notification) - no JSON-RPC response, but the HTTP
                // layer must still answer so the client doesn't see the connection drop.
                return HttpResponse( 202, "Accepted", Array.Empty<byte>() );
            }

            byte[] responseBody = Encoding.UTF8.GetBytes( JsonConvert.SerializeObject( response ) );

            return HttpResponse( 200, "OK", responseBody );
        }

        private static JsonRpcResponse Handle( JsonRpcRequest request )
        {
            switch ( request.Method )
            {
                case "initialize":
                    return Result( request, Initialize() );
                case "ping":
                    return Result( request, new JObject() );
                case "tools/list":
                    return Result( request, new JObject { ["tools"] = JToken.FromObject( McpTools.GetTools() ) } );
                case "tools/call":
                    return HandleToolsCall( request );
                case "notifications/initialized":
                case "notifications/cancelled":
                    return null;
                default:
                    // Unknown notification - no response. Unknown request - method not found.
                    return request.Id == null ? null : Error( request, -32601, "Method not found" );
            }
        }

        private static JsonRpcResponse HandleToolsCall( JsonRpcRequest request )
        {
            CallToolParams callParams;

            try
            {
                callParams = request.Params?.ToObject<CallToolParams>();
            }
            catch
            {
                return Error( request, -32602, "Invalid params" );
            }

            if ( callParams == null || string.IsNullOrEmpty( callParams.Name ) )
            {
                return Error( request, -32602, "Invalid params: missing tool name" );
            }

            try
            {
                CallToolResult result = McpTools.Invoke( callParams.Name, callParams.Arguments );

                return Result( request, result );
            }
            catch ( Exception e )
            {
                return Result( request, new CallToolResult
                {
                    IsError = true,
                    Content = new List<McpContent> { new McpContent { Text = e.Message } }
                } );
            }
        }

        private static JObject Initialize()
        {
            return new JObject
            {
                ["protocolVersion"] = PROTOCOL_VERSION,
                ["capabilities"] = new JObject { ["tools"] = new JObject() },
                ["serverInfo"] = new JObject { ["name"] = "ClassicAssist", ["version"] = SERVER_VERSION },
                ["instructions"] = "Tools for inspecting, creating, editing and running ClassicAssist macros."
            };
        }

        private static JsonRpcResponse Result( JsonRpcRequest request, object result )
        {
            return new JsonRpcResponse { Id = request.Id, Result = result };
        }

        private static byte[] RpcError( object id, int code, string message )
        {
            JsonRpcResponse response = new JsonRpcResponse
            {
                Id = id,
                Error = new JsonRpcError { Code = code, Message = message }
            };

            return HttpResponse( 200, "OK", Encoding.UTF8.GetBytes( JsonConvert.SerializeObject( response ) ) );
        }

        private static JsonRpcResponse Error( JsonRpcRequest request, int code, string message )
        {
            return new JsonRpcResponse
            {
                Id = request.Id,
                Error = new JsonRpcError { Code = code, Message = message }
            };
        }

        private static byte[] HttpResponse( int statusCode, string statusText, byte[] body, string extraHeaders = null )
        {
            StringBuilder header = new StringBuilder();

            header.Append( $"HTTP/1.1 {statusCode} {statusText}\r\n" );
            header.Append( "Content-Type: application/json\r\n" );
            header.Append( $"MCP-Protocol-Version: {PROTOCOL_VERSION}\r\n" );
            header.Append( "Access-Control-Allow-Origin: *\r\n" );
            header.Append( "Access-Control-Allow-Methods: POST, OPTIONS\r\n" );
            header.Append( "Access-Control-Allow-Headers: content-type, mcp-protocol-version, mcp-session-id, accept\r\n" );

            if ( extraHeaders != null )
            {
                header.Append( extraHeaders );
            }

            header.Append( $"Content-Length: {body.Length}\r\n" );
            header.Append( "Connection: close\r\n" );
            header.Append( "\r\n" );

            byte[] headerBytes = Encoding.ASCII.GetBytes( header.ToString() );
            byte[] result = new byte[headerBytes.Length + body.Length];
            headerBytes.CopyTo( result, 0 );
            body.CopyTo( result, headerBytes.Length );

            return result;
        }

        private static int IndexOfHeaderTerminator( byte[] data, int length )
        {
            for ( int i = 0; i + 3 < length; i++ )
            {
                if ( data[i] == (byte) '\r' && data[i + 1] == (byte) '\n' &&
                     data[i + 2] == (byte) '\r' && data[i + 3] == (byte) '\n' )
                {
                    return i;
                }
            }

            return -1;
        }

        private static int ParseContentLength( byte[] data, int headerEnd )
        {
            string header = Encoding.ASCII.GetString( data, 0, headerEnd );

            foreach ( string line in header.Split( new[] { "\r\n" }, StringSplitOptions.None ) )
            {
                if ( line.StartsWith( "Content-Length:", StringComparison.OrdinalIgnoreCase ) )
                {
                    int contentLength;

                    if ( int.TryParse( line.Substring( 15 ).Trim(), out contentLength ) )
                    {
                        return contentLength;
                    }
                }
            }

            return 0;
        }
    }
}
