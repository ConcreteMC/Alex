using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Alex.Utils.Collections
{
    public class ConcurrentDeck<T> : IReadOnlyCollection<T>
    {
        private readonly int _size;
        private T[] _buffer;
        private int _position = 0;

        private T[] _backBuffer;
        private int _items = 0;
        public ConcurrentDeck(int size)
        {
            _size = size;
            _buffer = new T[size];
            _backBuffer = new T[0];
        }

        public void Resize(int size)
        {
            var buffer = _backBuffer;
            lock (this)
            {
                _buffer = new T[size];
                _position = 0;
                _items = 0;
                
                foreach (var item in buffer)
                {
                    PushInternal(item);
                }
            }
        }

        private void PushInternal(T item)
        {
            _buffer[_position] = item;
            _position++;
                
            if (_items < _size) _items++;
                
            if (_position == _size) _position = 0;

            _backBuffer = _buffer.Skip(_position).Union(_buffer.Take(_position)).TakeLast(_items).ToArray();
        }
        
        public void Push(T item)
        {
            lock (this)
            {
                PushInternal(item);
            }
        }

        public T[] ReadDeck()
        {
            return _backBuffer;
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            var deck = ReadDeck();

            foreach (var d in deck)
            {
                yield return d;
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public int Count => _items;
    }
}