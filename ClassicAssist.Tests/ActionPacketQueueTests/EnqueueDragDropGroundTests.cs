#region License

// Copyright (C) 2024 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System.Threading;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.UO.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.ActionPacketQueueTests
{
    [TestClass]
    public class EnqueueDragDropGroundTests
    {
        [TestMethod]
        public async Task WillEnqueueDragDropGroundSendDragDropPacket()
        {
            bool dragSent = false;

            ActionPacketQueue.Clear();
            Task task = ActionPacketQueue.EnqueueDragDropGround( 0x1234, 0x5678, 0x9ABC, 0xDEF0, -80 );

            Engine.InternalPacketSentEvent += OnPacketSentEvent;

            bool result = task.Wait( 5000 );

            Assert.IsTrue( result );

            Engine.InternalPacketSentEvent -= OnPacketSentEvent;
            return;

            void OnPacketSentEvent( byte[] data, int length )
            {
                switch ( data[0] )
                {
                    case 0x07:
                    {
                        int serial = ( data[1] << 24 ) | ( data[2] << 16 ) | ( data[3] << 8 ) | data[4];
                        int amount = ( data[5] << 8 ) | data[6];
                        Assert.AreEqual( serial, 0x1234 );
                        Assert.AreEqual( amount, 0x5678 );
                        dragSent = true;
                        break;
                    }
                    case 0x08 when dragSent:
                    {
                        int serial = ( data[1] << 24 ) | ( data[2] << 16 ) | ( data[3] << 8 ) | data[4];
                        int x = ( data[5] << 8 ) | data[6];
                        int y = ( data[7] << 8 ) | data[8];
                        int z = (sbyte) data[9];
                        int location = ( data[10] << 24 ) | ( data[11] << 16 ) | ( data[12] << 8 ) | data[13];
                        Assert.AreEqual( serial, 0x1234 );
                        Assert.AreEqual( x, 0x9ABC );
                        Assert.AreEqual( y, 0xDEF0 );
                        Assert.AreEqual( z, -80 );
                        Assert.AreEqual( location, -1 );
                        break;
                    }
                }
            }
        }

        [TestMethod]
        public void WillCancelEnqueueDragDropGround()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            Task<bool> task = ActionPacketQueue.EnqueueDragDropGround( 0x1234, 0x5678, 0x9ABC, 0xDEF0, -80, cancellationToken: cancellationTokenSource.Token );

            Engine.InternalPacketSentEvent += OnPacketSentEvent;

            task.Wait( 1000 );
            bool result = task.Result;

            Assert.IsFalse( result );

            Engine.InternalPacketSentEvent -= OnPacketSentEvent;
            return;

            void OnPacketSentEvent( byte[] data, int length )
            {
                Assert.Fail();
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            ActionPacketQueue.Clear();
        }
    }
}