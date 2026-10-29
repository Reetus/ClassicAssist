using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Mcp;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Tests.Mcp
{
    [TestClass]
    public class McpAutoStartTests
    {
        private const int SERIAL = 0x0000ABCD;
        private const string SERIAL_KEY = "0x0000ABCD";
        private const string OTHER_KEY = "0x00001234";
        private bool _originalAutoStart;
        private Options _originalCurrentOptions;
        private Dispatcher _originalDispatcher;
        private PlayerMobile _originalPlayer;
        private string _originalProfileDirectory;
        private string _originalToken;
        private string _tempDir;

        [TestInitialize]
        public void SetUp()
        {
            _originalProfileDirectory = AssistantOptions.ProfileDirectory;
            _originalCurrentOptions = Options.CurrentOptions;
            _originalPlayer = Engine.Player;
            _originalDispatcher = Engine.Dispatcher;
            _originalAutoStart = AssistantOptions.McpAutoStart;
            _originalToken = AssistantOptions.McpToken;

            // Consent given, with a token already set so nothing writes Assistant.json.
            AssistantOptions.McpAutoStart = true;
            AssistantOptions.McpToken = "test-token";

            // No dispatcher: the login handler runs inline.
            Engine.Dispatcher = null;

            _tempDir = Path.Combine( Path.GetTempPath(), "ClassicAssistMcpAutoStart", Guid.NewGuid().ToString( "N" ) );
            AssistantOptions.ProfileDirectory = Path.Combine( _tempDir, "Profiles" );
            Directory.CreateDirectory( AssistantOptions.ProfileDirectory );

            Options.CurrentOptions = new Options { Name = "test.json" };
            Engine.Player = new PlayerMobile( SERIAL );

            McpAutoStart.Initialize();
        }

        [TestCleanup]
        public void TearDown()
        {
            McpServer.Shutdown();

            Engine.Player = _originalPlayer;
            Engine.Dispatcher = _originalDispatcher;
            AssistantOptions.McpAutoStart = _originalAutoStart;
            AssistantOptions.McpToken = _originalToken;
            McpServer.Token = null;
            Options.CurrentOptions = _originalCurrentOptions;
            AssistantOptions.ProfileDirectory = _originalProfileDirectory;

            try
            {
                Directory.Delete( _tempDir, true );
            }
            catch ( IOException )
            {
                // best effort
            }
            catch ( UnauthorizedAccessException )
            {
                // best effort
            }
        }

        [TestMethod]
        public void LoadRestartsOnTheSavedPort()
        {
            int port = FreePort();
            WriteProfile( new JObject { [SERIAL_KEY] = port } );

            Options.Load( "test.json", Options.CurrentOptions );

            Assert.IsTrue( McpServer.IsRunning );
            Assert.AreEqual( port, McpServer.Port );
            Assert.IsTrue( Options.CurrentOptions.McpEnabled );
            Assert.AreEqual( port, Options.CurrentOptions.McpPort );
        }

        [TestMethod]
        public void LoadTakesTheNextFreePortWhenTheSavedOneIsTaken()
        {
            TcpListener taken = new TcpListener( IPAddress.Loopback, 0 );
            taken.Start();

            try
            {
                int port = ( (IPEndPoint) taken.LocalEndpoint ).Port;

                WriteProfile( new JObject { [SERIAL_KEY] = port } );

                Options.Load( "test.json", Options.CurrentOptions );

                Assert.IsTrue( McpServer.IsRunning );
                Assert.IsTrue( McpServer.Port > port, $"Expected a port above {port}, got {McpServer.Port}." );
                Assert.AreEqual( McpServer.Port, Options.CurrentOptions.McpPort );
            }
            finally
            {
                taken.Stop();
            }
        }

        [TestMethod]
        public void LoadDoesNothingForACharacterWithoutAnEntry()
        {
            WriteProfile( new JObject { [OTHER_KEY] = FreePort() } );

            Options.Load( "test.json", Options.CurrentOptions );

            Assert.IsFalse( McpServer.IsRunning );
            Assert.IsFalse( Options.CurrentOptions.McpEnabled );
        }

        [TestMethod]
        public void SaveRecordsTheRunningPortAndKeepsOtherCharacters()
        {
            WriteProfile( new JObject { [OTHER_KEY] = 5000 } );
            Options.Load( "test.json", Options.CurrentOptions );

            int port = FreePort();
            McpServer.Initialize( port );

            Options.Save( Options.CurrentOptions );

            JObject ports = ReadPorts();

            Assert.AreEqual( port, ports[SERIAL_KEY]?.ToObject<int>() );
            Assert.AreEqual( 5000, ports[OTHER_KEY]?.ToObject<int>() );
        }

        [TestMethod]
        public void SaveKeepsAnEntryWrittenByAnotherInstanceSinceLoad()
        {
            Options.Load( "test.json", Options.CurrentOptions );

            // Another instance sharing the profile saves its character in the meantime.
            WriteProfile( new JObject { [OTHER_KEY] = 5001 } );

            McpServer.Initialize( FreePort() );
            Options.Save( Options.CurrentOptions );

            Assert.AreEqual( 5001, ReadPorts()[OTHER_KEY]?.ToObject<int>() );
        }

        [TestMethod]
        public void SaveDropsTheCharacterWhenMcpIsOff()
        {
            int port = FreePort();
            WriteProfile( new JObject { [SERIAL_KEY] = port, [OTHER_KEY] = 5000 } );
            Options.Load( "test.json", Options.CurrentOptions );

            McpServer.Shutdown();
            Options.Save( Options.CurrentOptions );

            JObject ports = ReadPorts();

            Assert.IsNull( ports[SERIAL_KEY] );
            Assert.AreEqual( 5000, ports[OTHER_KEY]?.ToObject<int>() );
        }

        [TestMethod]
        public void LoadBeforeLoginStartsOnTheLowestSavedPort()
        {
            ( int low, int high ) = TwoFreePorts();
            WriteProfile( new JObject { [SERIAL_KEY] = high, [OTHER_KEY] = low } );
            Engine.Player = null;

            Options.Load( "test.json", Options.CurrentOptions );

            Assert.IsTrue( McpServer.IsRunning );
            Assert.AreEqual( low, McpServer.Port );
            Assert.IsTrue( Options.CurrentOptions.McpEnabled );
        }

        [TestMethod]
        public void LoadBeforeLoginDoesNothingWithoutSavedPorts()
        {
            WriteProfile( new JObject() );
            Engine.Player = null;

            Options.Load( "test.json", Options.CurrentOptions );

            Assert.IsFalse( McpServer.IsRunning );
        }

        [TestMethod]
        public void LoginMovesToTheCharactersOwnPort()
        {
            ( int low, int high ) = TwoFreePorts();
            WriteProfile( new JObject { [SERIAL_KEY] = high, [OTHER_KEY] = low } );
            Engine.Player = null;
            Options.Load( "test.json", Options.CurrentOptions );

            Engine.SetPlayer( new PlayerMobile( SERIAL ) );

            Assert.IsTrue( McpServer.IsRunning );
            Assert.AreEqual( high, McpServer.Port );
            Assert.AreEqual( high, Options.CurrentOptions.McpPort );
        }

        [TestMethod]
        public void LoginKeepsThePortWhenTheCharactersOwnIsTaken()
        {
            ( int low, int high ) = TwoFreePorts();
            WriteProfile( new JObject { [SERIAL_KEY] = high, [OTHER_KEY] = low } );
            Engine.Player = null;
            Options.Load( "test.json", Options.CurrentOptions );

            TcpListener taken = new TcpListener( IPAddress.Loopback, high );
            taken.Start();

            try
            {
                Engine.SetPlayer( new PlayerMobile( SERIAL ) );

                Assert.IsTrue( McpServer.IsRunning );
                Assert.AreEqual( low, McpServer.Port );
            }
            finally
            {
                taken.Stop();
            }
        }

        [TestMethod]
        public void LoginStopsAServerTheCharacterDoesNotUse()
        {
            WriteProfile( new JObject { [OTHER_KEY] = FreePort() } );
            Engine.Player = null;
            Options.Load( "test.json", Options.CurrentOptions );

            Assert.IsTrue( McpServer.IsRunning );

            Engine.SetPlayer( new PlayerMobile( SERIAL ) );

            Assert.IsFalse( McpServer.IsRunning );
            Assert.IsFalse( Options.CurrentOptions.McpEnabled );
        }

        [TestMethod]
        public void LoadRequiresTheTokenWithConsent()
        {
            Options.Load( "test.json", Options.CurrentOptions );

            Assert.AreEqual( "test-token", McpServer.Token );
        }

        [TestMethod]
        public void NothingStartsWithoutConsent()
        {
            AssistantOptions.McpAutoStart = false;
            WriteProfile( new JObject { [SERIAL_KEY] = FreePort() } );

            Options.Load( "test.json", Options.CurrentOptions );

            Assert.IsFalse( McpServer.IsRunning );
            Assert.IsNull( McpServer.Token );

            Engine.Player = null;
            Options.Load( "test.json", Options.CurrentOptions );

            Assert.IsFalse( McpServer.IsRunning );
        }

        [TestMethod]
        public void SaveLeavesPortsAloneWithoutConsent()
        {
            AssistantOptions.McpAutoStart = false;
            WriteProfile( new JObject { [SERIAL_KEY] = 5000 } );
            Options.Load( "test.json", Options.CurrentOptions );

            Options.Save( Options.CurrentOptions );

            Assert.AreEqual( 5000, ReadPorts()[SERIAL_KEY]?.ToObject<int>() );

            McpServer.Initialize( FreePort() );
            Options.Save( Options.CurrentOptions );

            Assert.AreEqual( 5000, ReadPorts()[SERIAL_KEY]?.ToObject<int>() );
        }

        [TestMethod]
        public void LoginLeavesAServerStartedByHandAlone()
        {
            ( int byHand, int saved ) = TwoFreePorts();
            McpServer.Initialize( byHand );

            WriteProfile( new JObject { [SERIAL_KEY] = saved } );
            Engine.Player = null;
            Options.Load( "test.json", Options.CurrentOptions );

            Engine.SetPlayer( new PlayerMobile( SERIAL ) );

            Assert.IsTrue( McpServer.IsRunning );
            Assert.AreEqual( byHand, McpServer.Port );
        }

        private static ( int Low, int High ) TwoFreePorts()
        {
            TcpListener first = new TcpListener( IPAddress.Loopback, 0 );
            TcpListener second = new TcpListener( IPAddress.Loopback, 0 );

            first.Start();
            second.Start();

            int a = ( (IPEndPoint) first.LocalEndpoint ).Port;
            int b = ( (IPEndPoint) second.LocalEndpoint ).Port;

            first.Stop();
            second.Stop();

            return ( Math.Min( a, b ), Math.Max( a, b ) );
        }

        private static void WriteProfile( JObject ports )
        {
            JObject profile = new JObject { ["Name"] = "test.json", ["McpAutoStart"] = new JObject { ["Ports"] = ports } };

            File.WriteAllText( Path.Combine( AssistantOptions.ProfileDirectory, "test.json" ), profile.ToString() );
        }

        private static JObject ReadPorts()
        {
            JObject profile = JObject.Parse( File.ReadAllText( Path.Combine( AssistantOptions.ProfileDirectory, "test.json" ) ) );

            return profile["McpAutoStart"]?["Ports"] as JObject ?? new JObject();
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
