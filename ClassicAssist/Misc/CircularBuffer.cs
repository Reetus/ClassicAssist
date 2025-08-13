/* Copyright (C) 2009 Matthew Geyer
 * 
 * This file is part of UO Machine.
 * 
 * UO Machine is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * UO Machine is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with UO Machine.  If not, see <http://www.gnu.org/licenses/>. */

using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassicAssist.Misc
{
    /* NOTE: This class forces thread safety on every access to make it more user-friendly
     * in scripts at the expense of a bit of performance.  Obviously if you use this code
     * elsewhere you would want to implement the thread-safety yourself. */

    /// <summary>
    ///     Thread-safe generic circular buffer (not a queue).  Read position is remembered.
    /// </summary>
    public sealed class CircularBuffer<T>
    {
        private readonly object _syncRoot;
        private T[] _buffer;
        private int _capacity, _head, _tail;

        private readonly Dictionary<string, int> _readOffsets = new Dictionary<string, int>();

        public CircularBuffer( int capacity )
        {
            _capacity = capacity;
            Count = 0;
            _head = 0;
            _tail = 0;
            _buffer = new T[capacity + 1];
            _syncRoot = new object();
        }

        /// <summary>
        ///     Get or set buffer capacity. Buffer can only grow.
        /// </summary>
        public int Capacity
        {
            get => _capacity;
            set
            {
                lock ( _syncRoot )
                {
                    if ( value <= _capacity )
                    {
                        return;
                    }

                    T[] newBuffer = new T[value + 1];
                    T[] oldBuffer = GetEntireBuffer();
                    Array.Copy( oldBuffer, 0, newBuffer, 0, oldBuffer.Length );
                    _buffer = newBuffer;
                    _capacity = value;
                    _head = 0;
                    _tail = Count;
                }
            }
        }

        /// <summary>
        ///     Get number of elements actually contained in buffer.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        ///     Reset buffer. Does not actually clear it but sets the position to the end of the buffer.
        /// </summary>
        public void Clear( string key )
        {
            lock ( _syncRoot )
            {
                if (_head <= _tail )
                {
                    _readOffsets[key] = _tail - _head;
                }
                else
                {
                    _readOffsets[key] = _buffer.Length - _head + _tail;
                }
            }
        }

        /// <summary>
        ///     Change read position to specified number of indices from the beginning.
        /// </summary>
        /// <param name="count"></param>
        public void Seek( int count, string key )
        {
            lock ( _syncRoot )
            {
                _readOffsets[key] = Math.Min( count, Count );
            }
        }

        /// <summary>
        ///     Write item to buffer.
        /// </summary>
        public void Write( T item )
        {
            lock ( _syncRoot )
            {
                _buffer[_tail] = item;
                _tail = ++_tail % _buffer.Length;

                if ( _tail == _head )
                {
                    _head = ++_head % _buffer.Length;

                    foreach ( var readOffset in _readOffsets.ToArray() )
                    {
                        _readOffsets[readOffset.Key] = Math.Max( 0, readOffset.Value - 1 );
                    }
                }

                Count = Math.Min( Count + 1, _capacity );
            }
        }

        /// <summary>
        ///     Read item from buffer and increment read position.  Item remains in buffer until overwritten.
        /// </summary>
        /// <returns>True on success, false if nothing new to read.</returns>
        public bool Read( out T item, string key )
        {
            lock ( _syncRoot )
            {
                EnsureReadOffset( key );

                int readPos = ( _head + _readOffsets[key] ) % _buffer.Length;

                if ( readPos == _tail )
                {
                    item = default;
                    return false;
                }

                _readOffsets[key] = Math.Min( _readOffsets[key] + 1, Count );
                item = _buffer[readPos];
                return true;
            }
        }

        /// <summary>
        ///     Get array of elements contained in buffer.
        /// </summary>
        public T[] GetEntireBuffer()
        {
            lock ( _syncRoot )
            {
                T[] buffer = new T[Count];

                for ( int x = 0; x < Count; x++ )
                {
                    buffer[x] = _buffer[( _head + x ) % _buffer.Length];
                }

                return buffer;
            }
        }

        /// <summary>
        ///     Determines whether any elements in the named buffer associated with the specified key satisfy the given
        ///     predicate.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition. Cannot be <see langword="null"/>.</param>
        /// <param name="key">The key identifying the named buffer to search. Cannot be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if any elements in the named buffer satisfy the predicate; otherwise, <see
        /// langword="false"/>.</returns>
        public bool FindAny( Func<T, bool> predicate, string key )
        {
            lock ( _syncRoot )
            {
                EnsureReadOffset( key );

                return EnumerateNamedBuffer( key ).Any(predicate);
            }
        }

        private IEnumerable<T> EnumerateNamedBuffer( string key )
        {
            int readPos = ( _head + _readOffsets[key] );

            while ( readPos % _buffer.Length != _tail )
            {
                yield return _buffer[readPos++ % _buffer.Length];
            }
        }

        private void EnsureReadOffset( string key )
        {
           if ( !_readOffsets.ContainsKey( key ) )
                {
                    _readOffsets[key] = 0;
                }
        }
    }
}