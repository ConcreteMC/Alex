using System;
using System.Collections.Generic;

namespace Alex.Utils.Queue
{
   /// <summary>
    /// Heap-based implementation of concurrent priority queue. Max priority is on top of the heap.
    /// </summary>
    public class ConcurrentPriorityQueue<TElement, TPriority> : AbstractPriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
    {
        private readonly object _sync = new object();
        private const int _defaultCapacity = 10;
        private const int _shrinkRatio = 4;
        internal const int _resizeFactor = 2;

        private int _shrinkBound;

        /// <summary>
        /// Create a new instance of priority queue with given initial capacity.
        /// </summary>
        /// <param name="capacity">Initial queue capacity. Should be greater than 0.</param>
        /// <param name="comparer">Priority comparer. Default for type will be used unless custom is provided.</param>
        public ConcurrentPriorityQueue(int capacity, IComparer<TPriority> comparer = null):base(capacity, comparer)
        {
            _shrinkBound = Capacity / _shrinkRatio;
        }

        /// <summary>
        /// Create a new instance of priority queue with default initial capacity.
        /// </summary>
        /// <param name="comparer">Priority comparer. Default for type will be used unless custom is provided.</param>
        public ConcurrentPriorityQueue(IComparer<TPriority> comparer = null): this(_defaultCapacity, comparer)
        {
        }

        private ConcurrentPriorityQueue(Node[] nodes, int count, NodeComparer comparer):base(nodes, count, comparer)
        { }

        /// <summary>
        /// Add new item to the queue.
        /// </summary>
        public override void Enqueue(TElement item, TPriority priority)
        {
            lock (_sync)
            {
                if (_count == Capacity) GrowCapacity();

                base.Enqueue(item, priority);
            }
        }

        /// <summary>
        /// Remove and return the item with max priority from the queue.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public override TElement Dequeue()
        {
            TElement item;
            lock (_sync)
            {
                item = base.Dequeue();

                if (_count <= _shrinkBound && _count > _defaultCapacity) ShrinkCapacity();
            }

            return item;
        }

        public bool TryDequeue(out TElement item)
        {
            lock (_sync)
            {
                if (_count == 0)
                {
                    item = default;
                    return false;
                }

                item = Dequeue();
                return true;
            }
        }

        /// <summary>
        /// Trim queue capacity to count of items in the queue
        /// </summary>
        public void Trim()
        {
            lock (_sync)
            {
                int newCapacity = _count;
                Array.Resize(ref _nodes, newCapacity + 1);  // first element is at position 1
                _shrinkBound = newCapacity / _shrinkRatio;
            }
        }

        /// <summary>
        /// Remove all items from the queue. Capacity is not changed.
        /// </summary>
        public override void Clear()
        {
            lock (_sync)
            {
                base.Clear();
            }
        }

        /// <summary>
        /// Returns true if there is at least one item, which is equal to given.
        /// TD.Equals is used to compare equality.
        /// </summary>
        public override bool Contains(TElement item)
        {
            lock (_sync)
            {
                return base.Contains(item);
            }
        }

        /// <summary>
        /// Returns the first element in the queue (element with max priority) without removing it from the queue.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public override TElement Peek()
        {
            lock (_sync)
            {
                return base.Peek();
            }
        }

        /// <summary>
        /// Update priority of the first occurrence of the given item
        /// </summary>
        /// <param name="item">Item, which priority should be updated.</param>
        /// <param name="priority">New priority</param>
        /// <exception cref="ArgumentException"></exception>
        public override void UpdatePriority(TElement item, TPriority priority)
        {
            lock (_sync)
            {
                base.UpdatePriority(item, priority);
            }
        }

        public override IEnumerator<TElement> GetEnumerator()
        {
            Node[] nodesCopy;
            lock (_sync)
            {
                nodesCopy = CopyNodes();
            }
            // queue copy is created to be able to extract the items in the priority order
            // using the already existing dequeue method
            // (because they are not exactly in priority order in the underlying array)
            var queueCopy = new ConcurrentPriorityQueue<TElement, TPriority>(nodesCopy, nodesCopy.Length - 1, _comparer);

            return new PriorityQueueEnumerator(queueCopy);
        }

        private void GrowCapacity()
        {
            int newCapacity = Capacity * _resizeFactor;
            Array.Resize(ref _nodes, newCapacity + 1);  // first element is at position 1
            _shrinkBound = newCapacity / _shrinkRatio;
        }

        private void ShrinkCapacity()
        {
            int newCapacity = Capacity / _resizeFactor;
            Array.Resize(ref _nodes, newCapacity + 1);  // first element is at position 1
            _shrinkBound = newCapacity / _shrinkRatio;
        }
    }
}