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
using System.Linq;
using System.Threading;
using ClassicAssist.UO;
using Priority_Queue;

namespace ClassicAssist.Misc
{
    public enum QueuePriority
    {
        Immediate,
        High,
        Medium,
        Low
    }

    public class ThreadPriorityQueue<T> : IDisposable
    {
        private readonly object _lock = new object();
        private readonly Action<T> _onAction;
        private readonly SimplePriorityQueue<T> _queue = new SimplePriorityQueue<T>();
        private readonly EventWaitHandle _wh = new AutoResetEvent( false );
        private readonly Thread _workerThread;

        public ThreadPriorityQueue( Action<T> onAction )
        {
            _onAction = onAction;
            _workerThread = new Thread( ProcessQueue ) { IsBackground = true };
            _workerThread.Start();
        }

        public void Dispose()
        {
            StopThread();
        }

        public int Count()
        {
            lock ( _lock )
            {
                return _queue.Count;
            }
        }

        public int Count( Predicate<T> predicate )
        {
            lock ( _lock )
            {
                return _queue.Count( predicate.Invoke );
            }
        }

        public void Clear()
        {
            lock ( _lock )
            {
                _queue.Clear();
            }
        }

        private void ProcessQueue()
        {
            while ( _workerThread.IsAlive )
            {
                bool result;
                T queueItem;

                lock ( _lock )
                {
                    result = _queue.TryDequeue( out queueItem );
                }

                if ( result )
                {
                    if ( queueItem == null )
                    {
                        return;
                    }

                    _onAction( queueItem );
                }
                else
                {
                    _wh.WaitOne();
                }
            }
        }

        public void Enqueue( T queueItem, QueuePriority priority )
        {
            lock ( _lock )
            {
                _queue.Enqueue( queueItem, (float) priority );
            }

            try
            {
                _wh.Set();
            }
            catch ( ObjectDisposedException )
            {
            }
        }

        public bool Contains( Predicate<T> predicate )
        {
            lock ( _lock )
            {
                return _queue.Any( predicate.Invoke );
            }
        }

        private void StopThread()
        {
            lock ( _lock )
            {
                _queue.Enqueue( default, (float) QueuePriority.Immediate );
            }

            try
            {
                _wh.Set();
            }
            catch ( ObjectDisposedException )
            {
            }

            _workerThread.Join();
            _wh.Close();
        }
    }
}