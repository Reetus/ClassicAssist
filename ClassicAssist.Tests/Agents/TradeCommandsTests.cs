#region License

// Copyright (C) 2022 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System;
using System.Threading;
using Assistant;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.Agents
{
    [TestClass]
    public class TradeCommandsTests
    {
        [TestMethod]
        public void TradeWillSetRemoteAmount()
        {
            byte[] packet =
            {
                0x6F, 0x00, 0x11, 0x03, 0x41, 0x65, 0x78, 0xDE, 0x00, 0x09, 0xFB, 0xF1, 0x00, 0x00, 0x00, 0x01, 0x00
            };

            IncomingPacketHandlers.Initialize();

            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x6F );

            if ( handler == null )
            {
                Assert.Fail();
            }

            handler.OnReceive( new PacketReader( packet, packet.Length, false ) );

            Assert.AreEqual( 654321, Engine.Trade.GoldRemote );
            Assert.AreEqual( 1, Engine.Trade.PlatinumRemote );
        }

        [TestMethod]
        public void TradeWillSetLocalAmount()
        {
            byte[] packet =
            {
                0x6F, 0x00, 0x10, 0x03, 0x41, 0x65, 0x76, 0x0A, 0x00, 0x09, 0xFB, 0xF1, 0x00, 0x00, 0x00, 0x01
            };

            OutgoingPacketHandlers.Initialize();

            PacketHandler handler = OutgoingPacketHandlers.GetHandler( 0x6F );

            if ( handler == null )
            {
                Assert.Fail();
            }

            handler.OnReceive( new PacketReader( packet, packet.Length, false ) );

            Assert.AreEqual( 654321, Engine.Trade.GoldLocal );
            Assert.AreEqual( 1, Engine.Trade.PlatinumLocal );
        }

        [TestMethod]
        public void TradeStartWillSetContainers()
        {
            byte[] packet =
            {
                0x6F, 0x00, 0x2F, 0x00, 0x01, 0x5D, 0xA3, 0xC3, 0x41, 0x65, 0x73, 0x8D, 0x41, 0x65, 0x73, 0x8E
            };

            IncomingPacketHandlers.Initialize();

            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x6F );

            if ( handler == null )
            {
                Assert.Fail();
            }

            handler.OnReceive( new PacketReader( packet, packet.Length, false ) );

            Assert.AreEqual( 0x4165738D, Engine.Trade.ContainerLocal );
            Assert.AreEqual( 0x4165738E, Engine.Trade.ContainerRemote );
        }

        [TestMethod]
        public void TradeAcceptWillSendAcceptPacket()
        {
            AutoResetEvent are = new AutoResetEvent( false );

            void OnPacket( byte[] data, int len )
            {
                if ( data[0] == 0x6F && data[11] == 1 )
                {
                    are.Set();
                    Engine.InternalPacketSentEvent -= OnPacket;
                }
            }

            Engine.InternalPacketSentEvent += OnPacket;

            AgentCommands.TradeAccept();

            bool result = are.WaitOne( TimeSpan.FromSeconds( 5 ) );

            Engine.InternalPacketSentEvent -= OnPacket;

            Assert.IsTrue( result );
        }

        [TestMethod]
        public void TradeRejectWillSendRejectPacket()
        {
            AutoResetEvent are = new AutoResetEvent( false );

            void OnPacket( byte[] data, int len )
            {
                if ( data[0] == 0x6F && data[11] == 0 )
                {
                    are.Set();
                    Engine.InternalPacketSentEvent -= OnPacket;
                }
            }

            Engine.InternalPacketSentEvent += OnPacket;

            AgentCommands.TradeReject();

            bool result = are.WaitOne( TimeSpan.FromSeconds( 5 ) );

            Engine.InternalPacketSentEvent -= OnPacket;

            Assert.IsTrue( result );
        }

        [TestMethod]
        public void TradeCancelWillSendCancelPacket()
        {
            AutoResetEvent are = new AutoResetEvent( false );

            void OnPacket( byte[] data, int len )
            {
                if ( data[0] == 0x6F && data[3] == 1 )
                {
                    are.Set();
                    Engine.InternalPacketSentEvent -= OnPacket;
                }
            }

            Engine.InternalPacketSentEvent += OnPacket;

            AgentCommands.TradeClose();

            bool result = are.WaitOne( TimeSpan.FromSeconds( 5 ) );

            Engine.InternalPacketSentEvent -= OnPacket;

            Assert.IsTrue( result );
        }
    }
}