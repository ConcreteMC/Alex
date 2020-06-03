using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Alex.Utils
{
    public class ConcurrentDeck<T>
    {
        private readonly int _size;
        private readonly T[] _buffer;
        private int _position = 0;

        private  T[] _backBuffer;
        private int _items = 0;
        public ConcurrentDeck(int size)
        {
            _size = size;
            _buffer = new T[size];
            _backBuffer = new T[0];
        }

        public void Push(T item)
        {
            lock (this)
            {
                _buffer[_position] = item;
                _position++;
                
                if (_items < _size) _items++;
                
                if (_position == _size) _position = 0;

                _backBuffer = _buffer.Skip(_position).Union(_buffer.Take(_position)).TakeLast(_items).ToArray();
            }
        }

        public T[] ReadDeck()
        {
            return _backBuffer;
        }
    }
}