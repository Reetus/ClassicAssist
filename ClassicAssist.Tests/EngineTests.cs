using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Assistant;
using ClassicAssist.UO.Network.PacketFilter;
using CUO_API;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Tests
{
    [TestClass]
    [TestCategory( "Engine" )]
    public class EngineTests
    {
        private const string CLIENT_PATH = @"C:\Users\johns\Desktop\KvG Client 2.0";
        private OnGetPacketLength _getPacketLength;
        private OnGetUOFilePath _getUOFilePath;
        private int[] _packetLengths;
        private PluginHeader _pluginHeader;
        private OnPacketSendRecv _receivePacket;
        private OnPacketSendRecv _sendPacket;
        private string _startupPath;

        [TestInitialize]
        public void Initialize()
        {
            _startupPath = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );

            _getUOFilePath = GetUOFilePath;
            _getPacketLength = GetPacketLength;
            _sendPacket = SendPacket;
            _receivePacket = ReceivePacket;

            _pluginHeader = new PluginHeader
            {
                GetPacketLength = Marshal.GetFunctionPointerForDelegate( _getPacketLength ),
                GetUOFilePath = Marshal.GetFunctionPointerForDelegate( _getUOFilePath ),
                Recv = Marshal.GetFunctionPointerForDelegate( _receivePacket ),
                Send = Marshal.GetFunctionPointerForDelegate( _sendPacket )
            };

            Engine.StartupPath = _startupPath;

            _packetLengths = new int[byte.MaxValue];

            string json = File.ReadAllText( Path.Combine( _startupPath ?? throw new InvalidOperationException(),
                "packetlengths.json" ) );

            JObject jsonObj = JObject.Parse( json );

            foreach ( JProperty token in jsonObj["PacketLengths"].Children<JProperty>() )
            {
                int key = int.Parse( token.Name );
                int val = token.Value.ToObject<int>();

                if ( val == 0x8000 )
                {
                    val = 0;
                }

                _packetLengths[key] = val;
            }
        }

        [TestMethod]
        public void WillGetPacketLength()
        {
            // Fixed Length
            int length = GetPacketLength( 0x1B );

            Assert.AreEqual( 37, length );

            // Dynamic Length
            length = GetPacketLength( 0x1C );

            Assert.AreEqual( 0, length );
        }

        [TestMethod]
        public void WillInitializePlugin()
        {
            InitializePlugin();

            Assert.AreEqual( CLIENT_PATH, Engine.ClientPath );
        }

        private unsafe void InitializePlugin()
        {
            fixed ( void* func = &_pluginHeader )
            {
                Engine.InitializePlugin( (PluginHeader*) func );
            }
        }

        [TestMethod]
        public unsafe void WillSendPacket()
        {
            fixed ( void* func = &_pluginHeader )
            {
                Engine.Install( (PluginHeader*) func );
            }

            using ( AutoResetEvent are = new AutoResetEvent( false ) )
            {
                void OnPacketSentEvent( byte[] data, int len )
                {
                    Engine.PacketSentEvent -= OnPacketSentEvent;

                    int serial = ( data[1] << 24 ) | ( data[2] << 16 ) | ( data[3] << 8 ) | data[4];

                    if ( (uint) serial != 0xaabbccdd )
                    {
                        Assert.Fail();
                    }

                    // ReSharper disable once AccessToDisposedClosure
                    are.Set();
                }

                Engine.PacketSentEvent += OnPacketSentEvent;

                byte[] packet = { 0x06, 0xAA, 0xBB, 0xCC, 0xDD };
                int length = packet.Length;

                Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv>( _pluginHeader.OnSend )
                    .Invoke( ref packet, ref length );

                bool result = are.WaitOne( 5000 );

                Assert.IsTrue( result );
            }
        }

        [TestMethod]
        public unsafe void WillFilterSendPacket()
        {
            fixed ( void* func = &_pluginHeader )
            {
                Engine.Install( (PluginHeader*) func );
            }

            byte[] packet = { 0x06, 0xAA, 0xBB, 0xCC, 0xDD };
            int length = packet.Length;

            try
            {
                Engine.AddSendFilter( new PacketFilterInfo( 0x06 ) );

                using ( AutoResetEvent are = new AutoResetEvent( false ) )
                {
                    void OnSentPacketFilteredEvent( byte[] data, int len )
                    {
                        Engine.SentPacketFilteredEvent -= OnSentPacketFilteredEvent;

                        int serial = ( data[1] << 24 ) | ( data[2] << 16 ) | ( data[3] << 8 ) | data[4];

                        if ( (uint) serial != 0xaabbccdd )
                        {
                            Assert.Fail();
                        }

                        // ReSharper disable once AccessToDisposedClosure
                        are.Set();
                    }

                    Engine.SentPacketFilteredEvent += OnSentPacketFilteredEvent;

                    Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv>( _pluginHeader.OnSend )
                        .Invoke( ref packet, ref length );

                    bool result = are.WaitOne( 5000 );

                    Assert.IsTrue( result );
                }
            }
            finally
            {
                Engine.ClearSendFilter();
            }
        }

        [TestMethod]
        public unsafe void WillFilterReceivedPacket()
        {
            fixed ( void* func = &_pluginHeader )
            {
                Engine.Install( (PluginHeader*) func );
            }

            byte[] packet = { 0x2F, 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88 };
            int length = packet.Length;

            try
            {
                Engine.AddReceiveFilter( new PacketFilterInfo( 0x2F ) );

                using ( AutoResetEvent are = new AutoResetEvent( false ) )
                {
                    void OnSentPacketFilteredEvent( byte[] data, int len )
                    {
                        Engine.ReceivedPacketFilteredEvent -= OnSentPacketFilteredEvent;

                        int attacker = ( data[2] << 24 ) | ( data[3] << 16 ) | ( data[4] << 8 ) | data[5];
                        int defender = ( data[6] << 24 ) | ( data[7] << 16 ) | ( data[8] << 8 ) | data[9];

                        if ( (uint) attacker != 0x11223344 || defender != 0x55667788 )
                        {
                            Assert.Fail();
                        }

                        // ReSharper disable once AccessToDisposedClosure
                        are.Set();
                    }

                    Engine.ReceivedPacketFilteredEvent += OnSentPacketFilteredEvent;

                    Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv>( _pluginHeader.OnRecv )
                        .Invoke( ref packet, ref length );

                    bool result = are.WaitOne( 5000 );

                    Assert.IsTrue( result );
                }
            }
            finally
            {
                Engine.ClearReceiveFilter();
            }
        }

        [TestMethod]
        public unsafe void WillReceivePacket()
        {
            fixed ( void* func = &_pluginHeader )
            {
                Engine.Install( (PluginHeader*) func );
            }

            byte[] packet = { 0x2F, 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88 };
            int length = packet.Length;

            AutoResetEvent are = new AutoResetEvent( false );

            void OnPacketReceivedEvent( byte[] data, int len )
            {
                Engine.PacketReceivedEvent -= OnPacketReceivedEvent;

                int attacker = ( data[2] << 24 ) | ( data[3] << 16 ) | ( data[4] << 8 ) | data[5];
                int defender = ( data[6] << 24 ) | ( data[7] << 16 ) | ( data[8] << 8 ) | data[9];

                if ( (uint) attacker != 0x11223344 || defender != 0x55667788 )
                {
                    Assert.Fail();
                }

                are.Set();
            }

            Engine.PacketReceivedEvent += OnPacketReceivedEvent;

            Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv>( _pluginHeader.OnRecv )
                .Invoke( ref packet, ref length );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );
        }

        [TestMethod]
        public void OnConnectedWillSetEngineConnected()
        {
            InitializePlugin();

            Marshal.GetDelegateForFunctionPointer<OnConnected>( _pluginHeader.OnConnected )();

            Assert.IsTrue( Engine.Connected );
        }

        [TestMethod]
        public void OnDisconnectedWillSetEngineDisconnected()
        {
            InitializePlugin();

            Marshal.GetDelegateForFunctionPointer<OnDisconnected>( _pluginHeader.OnDisconnected )();

            Assert.IsFalse( Engine.Connected );
        }

        [TestMethod]
        public void OnDisconnectedWillInvokeDisconnectedEvent()
        {
            InitializePlugin();

            using ( AutoResetEvent are = new AutoResetEvent( false ) )
            {
                void OnDisconnectedEvent()
                {
                    // ReSharper disable once AccessToDisposedClosure
                    are.Set();
                }

                Engine.DisconnectedEvent += OnDisconnectedEvent;

                Marshal.GetDelegateForFunctionPointer<OnDisconnected>( _pluginHeader.OnDisconnected )();

                bool result = are.WaitOne( 5000 );

                Assert.IsTrue( result );

                Engine.DisconnectedEvent -= OnDisconnectedEvent;

                are.Dispose();
            }
        }

        [TestMethod]
        public void OnConnectedWillInvokeDisconnectedEvent()
        {
            InitializePlugin();

            using ( AutoResetEvent are = new AutoResetEvent( false ) )
            {
                void OnConnectedEvent()
                {
                    // ReSharper disable once AccessToDisposedClosure
                    are.Set();
                }

                Engine.ConnectedEvent += OnConnectedEvent;

                Marshal.GetDelegateForFunctionPointer<OnConnected>( _pluginHeader.OnConnected )();

                bool result = are.WaitOne( 5000 );

                Assert.IsTrue( result );

                Engine.ConnectedEvent -= OnConnectedEvent;

                are.Dispose();
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if ( _pluginHeader.OnClientClosing != IntPtr.Zero )
            {
                Marshal.GetDelegateForFunctionPointer<OnClientClose>( _pluginHeader.OnClientClosing )();
            }
        }

        #region Plugin Functions

        private short GetPacketLength( int id )
        {
            return (short) _packetLengths[id];
        }

        private static string GetUOFilePath()
        {
            return CLIENT_PATH;
        }

        private static bool ReceivePacket( ref byte[] data, ref int length )
        {
            return true;
        }

        private static bool SendPacket( ref byte[] data, ref int length )
        {
            return true;
        }

        #endregion
    }
}