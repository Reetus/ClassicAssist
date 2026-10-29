using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ClassicAssist.Mcp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.Mcp
{
    [TestClass]
    public class McpServerTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            McpServer.Shutdown();
            McpServer.Token = null;
        }

        [TestMethod]
        public void LoopbackJsonRequestIsAccepted()
        {
            int port = FreePort();
            McpServer.Initialize( port );

            Assert.AreEqual( 200, Send( port, Request( "Host: localhost:" + port ) ) );
            Assert.AreEqual( 200, Send( port, Request( "Host: [::1]:" + port ) ) );
        }

        [TestMethod]
        public void BrowserOriginIsRejected()
        {
            int port = FreePort();
            McpServer.Initialize( port );

            Assert.AreEqual( 403, Send( port, Request( "Host: localhost", "Origin: https://example.com" ) ) );
        }

        [TestMethod]
        public void NonLoopbackHostIsRejected()
        {
            int port = FreePort();
            McpServer.Initialize( port );

            Assert.AreEqual( 403, Send( port, Request( "Host: attacker.example:" + port ) ) );
        }

        [TestMethod]
        public void NonJsonContentTypeIsRejected()
        {
            int port = FreePort();
            McpServer.Initialize( port );

            Assert.AreEqual( 415, Send( port, Request( "Host: localhost", "Content-Type: text/plain" ) ) );
        }

        [TestMethod]
        public void TokenIsRequiredWhenSet()
        {
            int port = FreePort();
            McpServer.Initialize( port );
            McpServer.Token = "secret";

            Assert.AreEqual( 401, Send( port, Request( "Host: localhost" ) ) );
            Assert.AreEqual( 401, Send( port, Request( "Host: localhost", "Authorization: Bearer wrong" ) ) );
            Assert.AreEqual( 200, Send( port, Request( "Host: 127.0.0.1:" + port, "Authorization: Bearer secret" ) ) );
        }

        [TestMethod]
        public void OptionsNeedsTheTokenWhenSet()
        {
            int port = FreePort();
            McpServer.Initialize( port );
            McpServer.Token = "secret";

            Assert.AreEqual( 401, Send( port, "OPTIONS /mcp HTTP/1.1\r\nHost: localhost\r\nConnection: close\r\n\r\n" ) );
            Assert.AreEqual( 204,
                Send( port, "OPTIONS /mcp HTTP/1.1\r\nHost: localhost\r\nAuthorization: Bearer secret\r\nConnection: close\r\n\r\n" ) );
        }

        [TestMethod]
        public void OversizedBodyIsDropped()
        {
            int port = FreePort();
            McpServer.Initialize( port );

            string request = "POST /mcp HTTP/1.1\r\nHost: localhost\r\nContent-Type: application/json\r\n" +
                             $"Content-Length: {64 * 1024 * 1024}\r\nConnection: close\r\n\r\n";

            Assert.AreEqual( -1, Send( port, request ) );
        }

        private static string Request( params string[] headers )
        {
            byte[] body = Encoding.UTF8.GetBytes( @"{""jsonrpc"":""2.0"",""id"":1,""method"":""ping""}" );

            bool hasContentType = headers.Any( h => h.StartsWith( "Content-Type:", StringComparison.OrdinalIgnoreCase ) );

            return "POST /mcp HTTP/1.1\r\n" + string.Join( "", headers.Select( h => h + "\r\n" ) ) +
                   ( hasContentType ? "" : "Content-Type: application/json\r\n" ) +
                   $"Content-Length: {body.Length}\r\nConnection: close\r\n\r\n" + Encoding.UTF8.GetString( body );
        }

        private static int Send( int port, string request )
        {
            using ( TcpClient client = new TcpClient() )
            {
                client.Connect( IPAddress.Loopback, port );

                using ( NetworkStream stream = client.GetStream() )
                {
                    byte[] bytes = Encoding.UTF8.GetBytes( request );
                    stream.Write( bytes, 0, bytes.Length );

                    using ( StreamReader reader = new StreamReader( stream, Encoding.UTF8, false, 1024, true ) )
                    {
                        string[] statusLine = ( reader.ReadLine() ?? string.Empty ).Split( ' ' );

                        return statusLine.Length > 1 && int.TryParse( statusLine[1], out int status ) ? status : -1;
                    }
                }
            }
        }

        private static int FreePort()
        {
            TcpListener listener = new TcpListener( IPAddress.Loopback, 0 );

            listener.Start();

            int port = ( (IPEndPoint) listener.LocalEndpoint ).Port;

            listener.Stop();

            return port;
        }
    }
}
