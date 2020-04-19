using System;
using System.Collections;
using System.Collections.Generic;

namespace Alex.Utils.Queue
{
     /// <summary>
    /// Heap-based implementation of priority queue.
    /// </summary>
    public abstract class AbstractPriorityQueue<TElement, TPriority> : IPriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
    {
        internal sealed class Node
        {
            public TPriority Priority { get; internal set; }
            public readonly TElement Element;

            public Node(TElement element, TPriority priority)
            {
                Priority = priority;
                Element = element;
            }
        }

        internal Node[] _nodes;
        internal int _count;
        internal readonly NodeComparer _comparer;
        private readonly bool _dataIsValueType;

        /// <summary>
        /// Create an empty max priority queue of given capacity.
        /// </summary>
        /// <param name="capacity">Queue capacity. Greater than 0.</param>
        /// <param name="comparer">Priority comparer. Default type comparer will be used unless custom is provided.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal AbstractPriorityQueue(int capacity, IComparer<TPriority> comparer = null)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException("capacity", "Expected capacity greater than zero.");

            _nodes = new Node[capacity + 1];        // first element at 1
            _count = 0;
            _comparer = new NodeComparer(comparer ?? Comparer<TPriority>.Default);
            _dataIsValueType = typeof (TElement).IsValueType;
        }

        /// <summary>
        /// Create a new priority queue from the given nodes storage and comparer.
        /// Used to create existing queue copies. 
        /// </summary>
        /// <param name="nodes">Heap with data.</param>
        /// <param name="count">Count of items in the heap.</param>
        /// <param name="comparer">Node comparer for nodes in the queue.</param>
        internal AbstractPriorityQueue(Node[] nodes, int count, NodeComparer comparer)
        {
            _nodes = nodes;
            _count = count;
            _comparer = comparer;
            _dataIsValueType = typeof(TElement).IsValueType;
        }

        public int Capacity { get { return _nodes.Length - 1; } }

        public int Count { get { return _count; } }

        /// <summary>
        /// Returns true if there is at least one item, which is equal to given.
        /// TElement.Equals is used to compare equality.
        /// </summary>
        public virtual bool Contains(TElement item)
        {
            return GetItemIndex(item) > 0;
        }

        /// <summary>
        /// Returns index of the first occurrence of the given item or 0.
        /// TElement.Equals is used to compare equality.
        /// </summary>
        private int GetItemIndex(TElement item)
        {
            for (int i = 1; i <= _count; i++)
            {
                if (Equals(_nodes[i].Element, item)) return i;
            }
            return 0;            
        }

        /// <summary>
        /// Check if given data items are equal using TD.Equals.
        /// Handles null values for object types.
        /// </summary>
        internal bool Equals(TElement a, TElement b)
        {
            if (_dataIsValueType)
            {
                return a.Equals(b);
            }

            var objA = a as object;
            var objB = b as object;
            if (objA == null && objB == null) return true;      // null == null because equality should be symmetric
            if (objA == null || objB == null) return false;
            return objA.Equals(objB);
        }

        public virtual void Enqueue(TElement item, TPriority priority)
        {
            int index = _count + 1;
            _nodes[index] = new Node(item, priority);

            _count = index;             // update count after the element is really added but before Sift

            Sift(index);                // move item "up" while heap principles are not met
        }

        public virtual TElement Dequeue()
        {
            if (_count == 0) throw new InvalidOperationException("Unable to dequeue from empty queue.");

            TElement item = _nodes[1].Element;   // first element at 1
            Swap(1, _count);            // last element at _count
            _nodes[_count] = null;      // release hold on the object

            _count--;                   // update count after the element is really gone but before Sink

            Sink(1);                    // move item "down" while heap principles are not met

            return item;
        }

        /// <summary>
        /// Returns the first element in the queue (element with max priority) without removing it from the queue.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual TElement Peek()
        {
            if (_count == 0) throw new InvalidOperationException("Unable to peek from empty queue.");

            return _nodes[1].Element;   // first element at 1
        }

        /// <summary>
        /// Remove all items from the queue. Capacity is not changed.
        /// </summary>
        public virtual void Clear()
        {
            for (int i = 1; i <= _count; i++)
            {
                _nodes[i] = null;
            }
            _count = 0;
        }

        /// <summary>
        /// Update priority of the first occurrence of the given item
        /// </summary>
        /// <param name="item">Item, which priority should be updated.</param>
        /// <param name="priority">New priority</param>
        /// <exception cref="ArgumentException"></exception>
        public virtual void UpdatePriority(TElement item, TPriority priority)
        {
            var index = GetItemIndex(item);
            if (index == 0) throw new ArgumentException("Item is not found in the queue.");

            var priorityCompare = _comparer.Compare(_nodes[index].Priority, priority);
            if (priorityCompare < 0)
            {
                _nodes[index].Priority = priority;
                Sift(index);            // priority is increased, so item should go "up" the heap
            }
            else if (priorityCompare > 0)
            {
                _nodes[index].Priority = priority;
                Sink(index);            // priority is decreased, so item should go "down" the heap
            }
        }

        /// <summary>
        /// Returns a copy of internal heap array. Number of elements is _count + 1;
        /// </summary>
        /// <returns></returns>
        internal Node[] CopyNodes()
        {
            var nodesCopy = new Node[_count + 1];
            Array.Copy(_nodes, 0, nodesCopy, 0, _count + 1);
            return nodesCopy;
        }

        private bool GreaterOrEqual(Node i, Node j)
        {
            return _comparer.Compare(i, j) >= 0;
        }

        /// <summary>
        /// Moves the item with given index "down" the heap while heap principles are not met.
        /// </summary>
        private void Sink(int i)
        {
            while (true)
            {
                int leftChildIndex = 2 * i;
                int rightChildIndex = 2 * i + 1;
                if (leftChildIndex > _count) return; // reached last item

                var item = _nodes[i];
                var left = _nodes[leftChildIndex];
                var right = rightChildIndex > _count ? null : _nodes[rightChildIndex];

                // if item is greater than children - exit
                if (GreaterOrEqual(item, left) && (right == null || GreaterOrEqual(item, right))) return;

                // else exchange with greater of children
                int greaterChild = right == null || GreaterOrEqual(left, right) ? leftChildIndex : rightChildIndex;
                Swap(i, greaterChild);

                // continue at new position
                i = greaterChild;
            }
        }

        /// <summary>
        /// Moves the item with given index "up" the heap while heap principles are not met.
        /// </summary>
        private void Sift(int i)
        {
            while (true)
            {
                if (i <= 1) return;         // reached root
                int parent = i / 2;         // get parent

                // if root is greater or equal - exit
                if (GreaterOrEqual(_nodes[parent], _nodes[i])) return;

                Swap(parent, i);
                i = parent;
            }
        }

        private void Swap(int i, int j)
        {
            var tmp = _nodes[i];
            _nodes[i] = _nodes[j];
            _nodes[j] = tmp;
        }

        public abstract IEnumerator<TElement> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Compare nodes based on priority or just priorities
        /// </summary>
        internal sealed class NodeComparer : IComparer<Node>, IComparer<TPriority>
        {
            private readonly IComparer<TPriority> _comparer;

            public NodeComparer(IComparer<TPriority> comparer)
            {
                _comparer = comparer;
            }

            public int Compare(Node x, Node y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                return _comparer.Compare(y.Priority, x.Priority);
            }

            public int Compare(TPriority x, TPriority y)
            {
                return _comparer.Compare(y,x);
            }
        }

        /// <summary>
        /// Queue items enumerator. Returns items in the oder of queue priority.
        /// </summary>
        internal sealed class PriorityQueueEnumerator : IEnumerator<TElement>
        {
            private readonly TElement[] _items;
            private int _currentIndex;

            internal PriorityQueueEnumerator(AbstractPriorityQueue<TElement, TPriority> queueCopy)
            {
                _items = new TElement[queueCopy.Count];

                // dequeue the given queue copy to extract items in order of priority
                // enumerator is based on the new array to allow reset and multiple enumerations
                for (int i = 0; i < _items.Length; i++)
                {
                    _items[i] = queueCopy.Dequeue();
                }

                Reset();
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                _currentIndex++;
                return _currentIndex < _items.Length;
            }

            public void Reset()
            {
                _currentIndex = -1;
            }

            public TElement Current { get { return _items[_currentIndex]; } }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }
    }
}