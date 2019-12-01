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
        private int _capacity, _head, _tail, _readOffset;

        public CircularBuffer( int capacity )
        {
            _capacity = capacity;
            Count = 0;
            _head = 0;
            _tail = 0;
            _readOffset = 0;
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
                    T[] oldBuffer = GetBuffer();
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
        ///     Reset buffer.
        /// </summary>
        public void Clear()
        {
            lock ( _syncRoot )
            {
                Count = 0;
                _tail = 0;
                _head = 0;
                _readOffset = 0;
            }
        }

        /// <summary>
        ///     Change read position to specified number of indices from the beginning.
        /// </summary>
        /// <param name="count"></param>
        public void Seek( int count )
        {
            lock ( _syncRoot )
            {
                _readOffset = Math.Min( count, Count );
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
                    _readOffset = Math.Max( 0, _readOffset - 1 );
                }

                Count++;
                Count = Math.Min( Count, _capacity );
            }
        }

        /// <summary>
        ///     Read item from buffer and increment read position.  Item remains in buffer until overwritten.
        /// </summary>
        /// <returns>True on success, false if nothing new to read.</returns>
        public bool Read( out T item )
        {
            lock ( _syncRoot )
            {
                int readPos = ( _head + _readOffset ) % _buffer.Length;

                if ( readPos == _tail )
                {
                    item = default;
                    return false;
                }

                _readOffset = Math.Min( _readOffset + 1, Count );
                item = _buffer[readPos];
                return true;
            }
        }

        /// <summary>
        ///     Get array of elements contained in buffer.
        /// </summary>
        public T[] GetBuffer()
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
    }
}