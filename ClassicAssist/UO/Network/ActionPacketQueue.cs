#region License

// Copyright (C) 2020 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Misc;
using ClassicAssist.UO.Network.Packets;

namespace ClassicAssist.UO.Network
{
    public static class ActionPacketQueue
    {
        private static readonly ThreadPriorityQueue<ActionPacketQueueItem> _actionPacketQueue =
            new ThreadPriorityQueue<ActionPacketQueueItem>( ProcessActionPacketQueue );

        private static readonly object _actionPacketQueueLock = new object();

        private static void ProcessActionPacketQueue( ActionPacketQueueItem queueItem )
        {
            // Lock here so wait don't set the WaitHandle before we've called ToTask(?)
            lock ( _actionPacketQueueLock )
            {
                if ( queueItem.DelaySend )
                {
                    while ( Engine.LastActionPacket +
                            TimeSpan.FromMilliseconds( Options.CurrentOptions.ActionDelayMS ) > DateTime.Now )
                    {
                        Thread.Sleep( 1 );
                    }
                }

                byte[] data = queueItem.Packet;
                int length = queueItem.Length;

                Engine.LastActionPacket = DateTime.Now;
                Engine.SendPacketToServer( data, length );

                queueItem.WaitHandle.Set();
            }
        }

        // ReSharper disable once UnusedMember.Global
        public static Task EnqueueActionPacket( byte[] packet, int length, QueuePriority priority, bool delaySend )
        {
            return EnqueueActionPacket( new ActionPacketQueueItem( packet, length, delaySend ), priority );
        }

        public static Task EnqueueActionPacket( BasePacket packet, QueuePriority priority = QueuePriority.Low,
            bool delaySend = true )
        {
            byte[] data = packet.ToArray();

            return EnqueueActionPacket( new ActionPacketQueueItem( data, data.Length, delaySend ), priority );
        }

        public static Task EnqueueActionPacket( ActionPacketQueueItem queueItem, QueuePriority priority )
        {
            lock ( _actionPacketQueueLock )
            {
                _actionPacketQueue.Enqueue( queueItem, priority );

                return queueItem.WaitHandle.ToTask();
            }
        }

        public static Task EnqueueDragDropGround( int serial, int amount, int x, int y, int z,
            QueuePriority priority = QueuePriority.Low, bool delaySend = true )
        {
            lock ( _actionPacketQueueLock )
            {
                byte[] dragPacket = new DragItem( serial, amount ).ToArray();
                byte[] dropItem = new DropItem( serial, -1, x, y, z ).ToArray();

                ActionPacketQueueItem dragQueueItem =
                    new ActionPacketQueueItem( dragPacket, dragPacket.Length, delaySend );
                _actionPacketQueue.Enqueue( dragQueueItem, priority );

                ActionPacketQueueItem dropQueueItem = new ActionPacketQueueItem( dropItem, dropItem.Length, delaySend );
                _actionPacketQueue.Enqueue( dropQueueItem, priority );

                return new[] { dragQueueItem.WaitHandle, dropQueueItem.WaitHandle }.ToTask();
            }
        }

        public static Task EnqueueActionPackets( IEnumerable<BasePacket> packets,
            QueuePriority priority = QueuePriority.Low, bool delaySend = true )
        {
            lock ( _actionPacketQueueLock )
            {
                List<EventWaitHandle> handles = new List<EventWaitHandle>();

                foreach ( BasePacket packet in packets )
                {
                    byte[] data = packet.ToArray();

                    ActionPacketQueueItem queueItem = new ActionPacketQueueItem( data, data.Length, delaySend );
                    handles.Add( queueItem.WaitHandle );

                    _actionPacketQueue.Enqueue( queueItem, priority );
                }

                return handles.ToTask();
            }
        }

        public static Task EnqueueDragDrop( int serial, int amount, int containerSerial,
            QueuePriority priority = QueuePriority.Low, bool delaySend = true, int x = -1, int y = -1 )
        {
            return EnqueueActionPackets(
                new BasePacket[] { new DragItem( serial, amount ), new DropItem( serial, containerSerial, -1, -1, 0 ) },
                priority, delaySend );
        }

        public static int Count()
        {
            lock ( _actionPacketQueueLock )
            {
                return _actionPacketQueue.Count();
            }
        }

        public static void Clear()
        {
            lock ( _actionPacketQueueLock )
            {
                _actionPacketQueue.Clear();
            }
        }
    }
}