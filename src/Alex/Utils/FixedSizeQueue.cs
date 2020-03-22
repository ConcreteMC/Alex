using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Alex.Utils
{
    public class FixedSizedQueue<T> : ConcurrentQueue<T>
    {
        private readonly object syncObject = new object();

        public int Size { get; private set; }

        public FixedSizedQueue(int size)
        {
            Size = size;
        }

        public new void Enqueue(T obj)
        {
            base.Enqueue(obj);
            lock (syncObject)
            {
                while (base.Count > Size)
                {
                    T outObj;
                    base.TryDequeue(out outObj);
                }
            }
        }
    }

    public class FixedSizeQueue<T> : IReadOnlyCollection<T>
    {
        private ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private int _count;

        public int Limit { get; private set; }

        public FixedSizeQueue(int limit)
        {
            this.Limit = limit;
        }

        public void Enqueue(T obj)
        {
            _queue.Enqueue(obj);
            Interlocked.Increment(ref _count);

            // Calculate the number of items to be removed by this thread in a thread safe manner
            int currentCount;
            int finalCount;
            do
            {
                currentCount = _count;
                finalCount = Math.Min(currentCount, this.Limit);
            } while (currentCount != 
                     Interlocked.CompareExchange(ref _count, finalCount, currentCount));

            T overflow;
            while (currentCount > finalCount && _queue.TryDequeue(out overflow))
                currentCount--;
        }

        public int Count
        {
            get { return _count; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _queue.GetEnumerator();
        }
    }
    
    public class CircularBuffer<T> : IEnumerable<T>
    {
        readonly int size;
        readonly object locker;

        int count;
        int head;
        int rear;
        T[] values;

        public CircularBuffer(int max)
        {
            this.size = max;
            locker = new object();
            count = 0;
            head = 0;
            rear = 0;
            values = new T[size];
        }

        static int Incr(int index, int size)
        {
            return (index + 1) % size;
        }

        private void UnsafeEnsureQueueNotEmpty()
        {
            if (count == 0)
                throw new Exception("Empty queue");
        }

        public int Size
        {
            get { return size; }
        }

        public object SyncRoot
        {
            get { return locker; }
        }

        #region Count

        public int Count
        {
            get { return UnsafeCount; }
        }

        public int SafeCount
        {
            get
            {
                lock (locker)
                {
                    return UnsafeCount;
                }
            }
        }

        public int UnsafeCount
        {
            get { return count; }
        }

        #endregion

        #region Enqueue

        public void Enqueue(T obj)
        {
            UnsafeEnqueue(obj);
        }

        public void SafeEnqueue(T obj)
        {
            lock (locker)
            {
                UnsafeEnqueue(obj);
            }
        }

        public void UnsafeEnqueue(T obj)
        {
            values[rear] = obj;

            if (Count == Size)
                head = Incr(head, Size);
            rear = Incr(rear, Size);
            count = Math.Min(count + 1, Size);
        }

        #endregion

        #region Dequeue

        public T Dequeue()
        {
            return UnsafeDequeue();
        }

        public T SafeDequeue()
        {
            lock (locker)
            {
                return UnsafeDequeue();
            }
        }

        public T UnsafeDequeue()
        {
            UnsafeEnsureQueueNotEmpty();

            T res = values[head];
            values[head] = default(T);
            head = Incr(head, Size);
            count--;

            return res;
        }

        #endregion

        #region Peek

        public T Peek()
        {
            return UnsafePeek();
        }

        public T SafePeek()
        {
            lock (locker)
            {
                return UnsafePeek();
            }
        }

        public T UnsafePeek()
        {
            UnsafeEnsureQueueNotEmpty();

            return values[head];
        }

        #endregion


        #region GetEnumerator

        public IEnumerator<T> GetEnumerator()
        {
            return UnsafeGetEnumerator();
        }

        public IEnumerator<T> SafeGetEnumerator()
        {
            lock (locker)
            {
                List<T> res = new List<T>(count);
                var enumerator = UnsafeGetEnumerator();
                while (enumerator.MoveNext())
                    res.Add(enumerator.Current);
                return res.GetEnumerator();
            }
        }

        public IEnumerator<T> UnsafeGetEnumerator()
        {
            int index = head;
            for (int i = 0; i < count; i++)
            {
                yield return values[index];
                index = Incr(index, size);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
    
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