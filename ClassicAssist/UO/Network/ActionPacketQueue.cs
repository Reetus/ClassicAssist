﻿#region License

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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
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

        private static void ProcessActionPacketQueue( BaseQueueItem queueItem )
        {
            switch ( queueItem )
            {
                case PacketQueueItem actionQueueItem:
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

                    Commands.SystemMessage( $"OnAction {actionQueueItem.Caller}", Commands.SystemMessageHues.Yellow );

                    Engine.LastActionPacket = DateTime.Now;
                    Engine.SendPacketToServer( data, length );

                    actionQueueItem.Result = true;
                    actionQueueItem.WaitHandle.Set();
                    break;
                }
                case ActionQueueItem actionItem:
                {
                    if ( actionItem.DelaySend )
                    {
                        while ( Engine.LastActionPacket +
                            TimeSpan.FromMilliseconds( Options.CurrentOptions.ActionDelayMS ) > DateTime.Now )
                        {
                            Thread.Sleep( 1 );
                        }
                    }

                    Commands.SystemMessage( $"OnAction {actionItem.Caller}", Commands.SystemMessageHues.Yellow );

                    bool? result = actionItem.Action?.Invoke( actionItem.Arguments );

                    if ( result.HasValue && result.Value )
                    {
                        Engine.LastActionPacket = DateTime.Now;
                    }

                    actionItem.Result = result ?? true;
                    actionItem.WaitHandle.Set();
                    break;
                }
            }
        }

        // ReSharper disable once UnusedMember.Global
        public static Task EnqueuePacket( byte[] packet, int length, QueuePriority priority, bool delaySend,
            CancellationToken cancellationToken = default, [CallerMemberName] string caller = "" )
        {
            return EnqueuePacket( new PacketQueueItem( packet, length, delaySend, caller ) { Token = cancellationToken },
                priority );
        }

        public static Task EnqueuePacket( BasePacket packet, QueuePriority priority = QueuePriority.Low,
            bool delaySend = true, CancellationToken cancellationToken = default, [CallerMemberName] string caller = "" )
        {
            byte[] data = packet.ToArray();

            return EnqueuePacket( new PacketQueueItem( data, data.Length, delaySend, caller ) { Token = cancellationToken },
                priority );
        }

        public static Task EnqueuePacket( PacketQueueItem packetQueueItem, QueuePriority priority )
        {
            if ( !CheckUseObjectQueueLength() )
            {
                return Task.CompletedTask;
            }

            _actionPacketQueue.Enqueue( packetQueueItem, priority );

            return packetQueueItem.WaitHandle.ToTask();
        }

        //TODO Change to ActionQueueItem with CancellationToken
        public static Task EnqueueDragDropGround( int serial, int amount, int x, int y, int z,
            QueuePriority priority = QueuePriority.Low, bool delaySend = true, [CallerMemberName] string caller = "" )
        {
            byte[] dragPacket = new DragItem( serial, amount ).ToArray();
            byte[] dropItem = new DropItem( serial, -1, x, y, z ).ToArray();

            PacketQueueItem dragPacketQueueItem = new PacketQueueItem( dragPacket, dragPacket.Length, delaySend, caller );
            _actionPacketQueue.Enqueue( dragPacketQueueItem, priority );

            PacketQueueItem dropPacketQueueItem = new PacketQueueItem( dropItem, dropItem.Length, delaySend, caller );
            _actionPacketQueue.Enqueue( dropPacketQueueItem, priority );

            return new[] { dragPacketQueueItem.WaitHandle, dropPacketQueueItem.WaitHandle }.ToTask();
        }

        public static Task EnqueuePackets( IEnumerable<BasePacket> packets, QueuePriority priority = QueuePriority.Low,
            bool delaySend = true, CancellationToken cancellationToken = default, [CallerMemberName] string caller = "" )
        {
            if ( !CheckUseObjectQueueLength() )
            {
                return Task.CompletedTask;
            }

            List<EventWaitHandle> handles = new List<EventWaitHandle>();

            foreach ( BasePacket packet in packets )
            {
                byte[] data = packet.ToArray();

                PacketQueueItem packetQueueItem =
                    new PacketQueueItem( data, data.Length, delaySend, caller ) { Token = cancellationToken };
                handles.Add( packetQueueItem.WaitHandle );

                _actionPacketQueue.Enqueue( packetQueueItem, priority );
            }

            return handles.ToTask();
        }

        public static Task<bool> EnqueueDragDrop( int serial, int amount, int containerSerial,
            QueuePriority priority = QueuePriority.Low, bool checkRange = false, bool checkExisting = false,
            bool delaySend = true, int x = -1, int y = -1, CancellationToken cancellationToken = default,
            bool requeueOnFailure = false, Func<int, int, bool> successPredicate = null, int attempt = 0, [CallerMemberName] string caller = "" )
        {
            if ( checkExisting && _actionPacketQueue.Contains( e => e is ActionQueueItem aqi && aqi.Serial == serial ) )
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
                        Commands.SystemMessage( Strings.Item_out_of_range___, true );

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
                EnqueueDragDrop( serial, amount, containerSerial, priority, checkRange, checkExisting, delaySend, x, y,
                    cancellationToken, true, successPredicate, attempt++ );

                stopWatch.Stop();

                int delayRemaining = Options.CurrentOptions.ActionDelayMS - (int) stopWatch.ElapsedMilliseconds;

                if ( delayRemaining > 0 )
                {
                    Thread.Sleep( delayRemaining );
                }

                // Return false so we don't rewait the action delay
                return false;
            } ) { CheckRange = checkRange, DelaySend = delaySend, Serial = serial, Arguments = checkRange };
            
            actionQueueItem.Caller = caller;

            _actionPacketQueue.Enqueue( actionQueueItem, priority );

            return actionQueueItem.WaitHandle.ToTask( () => actionQueueItem.Result );
        }

        public static Task<bool> EnqueueAction<T>( T arguments, Func<T, bool> action,
            QueuePriority priority = QueuePriority.Low, bool delaySend = true,
            CancellationToken cancellationToken = default, bool checkUseObjectQueue = false, [CallerMemberName] string caller = "" )
        {
            if ( checkUseObjectQueue && !CheckUseObjectQueueLength() )
            {
                return Task.FromResult( false );
            }

            ActionQueueItem actionQueueItem =
                new ActionQueueItem( e => !cancellationToken.IsCancellationRequested && action( (T) e ) )
                {
                    DelaySend = delaySend, Token = cancellationToken, Arguments = arguments, Caller = caller
                };

            _actionPacketQueue.Enqueue( actionQueueItem, priority );

            return actionQueueItem.WaitHandle.ToTask( () => actionQueueItem.Result );
        }

        public static int Count()
        {
            return _actionPacketQueue.Count();
        }

        public static void Clear()
        {
            _actionPacketQueue.Clear();
        }

        public static bool CheckUseObjectQueueLength()
        {
            if ( _actionPacketQueue.Count() > Options.CurrentOptions.UseObjectQueueAmount )
            {
                Commands.SystemMessage( Strings.Object_queue_full, (int) Commands.SystemMessageHues.Yellow );

                return false;
            }

            return true;
        }
    }
}