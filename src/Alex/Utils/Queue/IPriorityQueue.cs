using System;
using System.Collections.Generic;

namespace Alex.Utils.Queue
{
    public interface IPriorityQueue<TElement, in TPriority> : IEnumerable<TElement> where TPriority : IComparable<TPriority>
    {
        int Capacity { get; }
        int Count { get; }
        bool Contains(TElement item);
        void Enqueue(TElement item, TPriority priority);
        TElement Dequeue();
        TElement Peek();
        void Clear();
        void UpdatePriority(TElement item, TPriority priority);
    }
}