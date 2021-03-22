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

namespace ClassicAssist.Data.Targeting
{
    public class TargetQueue<T>
    {
        private readonly object _lock = new object();
        private Queue<T> _queue = new Queue<T>();

        public int Count
        {
            get
            {
                lock ( _lock )
                {
                    RemoveExpired();

                    return _queue.Count;
                }
            }
        }

        public void Enqueue( T obj )
        {
            lock ( _lock )
            {
                // If queue is full, pop n-1 entries off the queue and make a new queue
                if ( _queue.Count >= Options.CurrentOptions.MaxTargetQueueLength )
                {
                    List<T> list = new List<T>();

                    for ( int i = 0; i < Options.CurrentOptions.MaxTargetQueueLength - 1; i++ )
                    {
                        list.Add( _queue.Dequeue() );
                    }

                    _queue = new Queue<T>( list );
                }

                _queue.Enqueue( obj );
            }
        }

        public T Peek()
        {
            lock ( _lock )
            {
                try
                {
                    RemoveExpired();

                    return _queue.Peek();
                }
                catch ( InvalidOperationException )
                {
                    return default;
                }
            }
        }

        public T Dequeue()
        {
            lock ( _lock )
            {
                try
                {
                    RemoveExpired();

                    return _queue.Dequeue();
                }
                catch ( InvalidOperationException )
                {
                    return default;
                }
            }
        }

        public void Clear()
        {
            lock ( _lock )
            {
                _queue.Clear();
            }
        }

        private void RemoveExpired()
        {
            if ( Options.CurrentOptions.ExpireTargetsMS < 0 )
            {
                return;
            }

            lock ( _lock )
            {
                try
                {
                    bool expired;

                    do
                    {
                        T obj = _queue.Peek();

                        expired = Expired( obj );

                        if ( expired )
                        {
                            _queue.Dequeue();
                        }
                    }
                    while ( expired );
                }
                catch ( InvalidOperationException )
                {
                }
            }
        }

        private static bool Expired( T obj )
        {
            bool expired = false;

            if ( obj is TargetQueueObject targetQueueObject )
            {
                expired = targetQueueObject.DateTime +
                    TimeSpan.FromMilliseconds( Options.CurrentOptions.ExpireTargetsMS ) < DateTime.Now;
            }

            return expired;
        }
    }
}