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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Macros;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UO.Network
{
    public static class ActionPacketQueue
    {
        private const int DRAG_DROP_DISTANCE = 3;
        private const int DROP_DELAY = 50;
        private const int MAX_ATTEMPTS = 5;

        private static readonly ThreadPriorityQueue<BaseQueueItem> _actionPacketQueue =
            new ThreadPriorityQueue<BaseQueueItem>( ProcessActionPacketQueue );

        private static readonly object _actionPacketQueueLock = new object();

        private static void ProcessActionPacketQueue( BaseQueueItem queueItem )
        {
            // Lock here so wait don't set the WaitHandle before we've called ToTask(?)
            lock ( _actionPacketQueueLock )
            {
                if ( queueItem is PacketQueueItem actionQueueItem )
                {
                    if ( actionQueueItem.DelaySend )
                    {
                        while ( Engine.LastActionPacket +
                                TimeSpan.FromMilliseconds( Options.CurrentOptions.ActionDelayMS ) > DateTime.Now )
                        {
                            Thread.Sleep( 1 );
                        }
                    }

                    if ( actionQueueItem.Token.IsCancellationRequested )
                    {
                        actionQueueItem.Result = false;
                        actionQueueItem.WaitHandle.Set();
                        return;
                    }

                    byte[] data = actionQueueItem.Packet;
                    int length = actionQueueItem.Length;

                    Engine.LastActionPacket = DateTime.Now;
                    Engine.SendPacketToServer( data, length );

                    actionQueueItem.Result = true;
                    actionQueueItem.WaitHandle.Set();
                }
                else if ( queueItem is ActionQueueItem actionItem )
                {
                    if ( actionItem.DelaySend )
                    {
                        while ( Engine.LastActionPacket +
                                TimeSpan.FromMilliseconds( Options.CurrentOptions.ActionDelayMS ) > DateTime.Now )
                        {
                            Thread.Sleep( 1 );
                        }
                    }

                    bool? result = actionItem.Action?.Invoke( actionItem.Arguments );

                    if ( result.HasValue && result.Value )
                    {
                        Engine.LastActionPacket = DateTime.Now;
                    }

                    actionItem.Result = result ?? true;
                    actionItem.WaitHandle.Set();
                }
            }
        }

        // ReSharper disable once UnusedMember.Global
        public static Task EnqueuePacket( byte[] packet, int length, QueuePriority priority, bool delaySend,
            CancellationToken cancellationToken = default )
        {
            return EnqueuePacket( new PacketQueueItem( packet, length, delaySend ) { Token = cancellationToken },
                priority );
        }

        public static Task EnqueuePacket( BasePacket packet, QueuePriority priority = QueuePriority.Low,
            bool delaySend = true, CancellationToken cancellationToken = default )
        {
            byte[] data = packet.ToArray();

            return EnqueuePacket( new PacketQueueItem( data, data.Length, delaySend ) { Token = cancellationToken },
                priority );
        }

        public static Task EnqueuePacket( PacketQueueItem packetQueueItem, QueuePriority priority )
        {
            lock ( _actionPacketQueueLock )
            {
                if ( _actionPacketQueue.Count() > Options.CurrentOptions.UseObjectQueueAmount )
                {
                    if ( !MacroManager.QuietMode )
                    {
                        Commands.SystemMessage( Strings.Object_queue_full, 61, true );
                    }

                    return Task.CompletedTask;
                }

                _actionPacketQueue.Enqueue( packetQueueItem, priority );

                return packetQueueItem.WaitHandle.ToTask();
            }
        }

        //TODO Change to ActionQueueItem with CancellationToken
        public static Task EnqueueDragDropGround( int serial, int amount, int x, int y, int z,
            QueuePriority priority = QueuePriority.Low, bool delaySend = true )
        {
            lock ( _actionPacketQueueLock )
            {
                byte[] dragPacket = new DragItem( serial, amount ).ToArray();
                byte[] dropItem = new DropItem( serial, -1, x, y, z ).ToArray();

                PacketQueueItem dragPacketQueueItem = new PacketQueueItem( dragPacket, dragPacket.Length, delaySend );
                _actionPacketQueue.Enqueue( dragPacketQueueItem, priority );

                PacketQueueItem dropPacketQueueItem = new PacketQueueItem( dropItem, dropItem.Length, delaySend );
                _actionPacketQueue.Enqueue( dropPacketQueueItem, priority );

                return new[] { dragPacketQueueItem.WaitHandle, dropPacketQueueItem.WaitHandle }.ToTask();
            }
        }

        public static Task EnqueuePackets( IEnumerable<BasePacket> packets, QueuePriority priority = QueuePriority.Low,
            bool delaySend = true, CancellationToken cancellationToken = default )
        {
            lock ( _actionPacketQueueLock )
            {
                if ( _actionPacketQueue.Count() > Options.CurrentOptions.UseObjectQueueAmount )
                {
                    if ( !MacroManager.QuietMode )
                    {
                        Commands.SystemMessage( Strings.Object_queue_full, 61, true );
                    }

                    return Task.CompletedTask;
                }

                List<EventWaitHandle> handles = new List<EventWaitHandle>();

                foreach ( BasePacket packet in packets )
                {
                    byte[] data = packet.ToArray();

                    PacketQueueItem packetQueueItem =
                        new PacketQueueItem( data, data.Length, delaySend ) { Token = cancellationToken };
                    handles.Add( packetQueueItem.WaitHandle );

                    _actionPacketQueue.Enqueue( packetQueueItem, priority );
                }

                return handles.ToTask();
            }
        }

        public static Task<bool> EnqueueDragDrop( int serial, int amount, int containerSerial,
            QueuePriority priority = QueuePriority.Low, bool checkRange = false, bool checkExisting = false,
            bool delaySend = true, int x = -1, int y = -1, CancellationToken cancellationToken = default,
            bool requeueOnFailure = false, Func<int, int, bool> successPredicate = null, int attempt = 0 )
        {
            lock ( _actionPacketQueueLock )
            {
                if ( checkExisting &&
                     _actionPacketQueue.Contains( e => e is ActionQueueItem aqi && aqi.Serial == serial ) )
                {
                    return Task.FromResult( true );
                }

                ActionQueueItem actionQueueItem = new ActionQueueItem( param =>
                {
                    if ( cancellationToken.IsCancellationRequested )
                    {
                        return false;
                    }

                    if ( param is bool check && check )
                    {
                        Item item = Engine.Items.GetItem( serial );

                        if ( item == null || item.Distance >= DRAG_DROP_DISTANCE )
                        {
                            if ( !MacroManager.QuietMode )
                            {
                                Commands.SystemMessage( Strings.Item_out_of_range___ );
                            }

                            return false;
                        }
                    }

                    Engine.SendPacketToServer( new DragItem( serial, amount ) );
                    Thread.Sleep( DROP_DELAY );
                    Engine.SendPacketToServer( new DropItem( serial, containerSerial, x, y, 0 ) );

                    if ( !requeueOnFailure || successPredicate == null || attempt >= MAX_ATTEMPTS )
                    {
                        return true;
                    }

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    Commands.WaitForContainerContents( containerSerial, Options.CurrentOptions.ActionDelayMS );

                    bool result = successPredicate.Invoke( serial, containerSerial );

                    if ( result )
                    {
                        return true;
                    }

#if DEBUG
                    Commands.SystemMessage( $"Requeue: 0x{serial:x8}" );
#endif
                    EnqueueDragDrop( serial, amount, containerSerial, priority, checkRange, checkExisting, delaySend, x,
                        y, cancellationToken, true, successPredicate, attempt++ );

                    stopWatch.Stop();

                    int delayRemaining = Options.CurrentOptions.ActionDelayMS - (int) stopWatch.ElapsedMilliseconds;

                    if ( delayRemaining > 0 )
                    {
                        Thread.Sleep( delayRemaining );
                    }

                    // Return false so we don't rewait the action delay
                    return false;
                } ) { CheckRange = checkRange, DelaySend = delaySend, Serial = serial, Arguments = checkRange};

                _actionPacketQueue.Enqueue( actionQueueItem, priority );

                return actionQueueItem.WaitHandle.ToTask( () => actionQueueItem.Result );
            }
        }

        public static Task<bool> EnqueueAction<T>( T arguments, Func<T, bool> action, QueuePriority priority = QueuePriority.Low,
            bool delaySend = true, CancellationToken cancellationToken = default )
        {
            lock ( _actionPacketQueueLock )
            {
                ActionQueueItem actionQueueItem =
                    new ActionQueueItem( e => !cancellationToken.IsCancellationRequested && action( (T) e ) )
                    {
                        DelaySend = delaySend, Token = cancellationToken, Arguments = arguments
                    };

                _actionPacketQueue.Enqueue( actionQueueItem, priority );

                return actionQueueItem.WaitHandle.ToTask( () => actionQueueItem.Result );
            }
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