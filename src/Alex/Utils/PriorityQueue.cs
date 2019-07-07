using System;
using System.Collections.Generic;
using System.Linq;

namespace Alex.Utils
{
    public class PriorityQueue<TElement, TKey>
    {
        private SortedDictionary<TKey, Queue<TElement>> dictionary = new SortedDictionary<TKey, Queue<TElement>>();
        
        public PriorityQueue()
        {

        }

        public void Enqueue(TElement item, TKey key)
        {
           // TKey key = selector(item);
            Queue<TElement> queue;
            if (!dictionary.TryGetValue(key, out queue))
            {
                queue = new Queue<TElement>();
                dictionary.Add(key, queue);
            }

            queue.Enqueue(item);
        }

        public bool TryDequeue(out TElement element, out TKey key)
        {
            if (dictionary.Count == 0)
            {
                element = default;
                key = default;
                
                return false;
            }
            
            key = dictionary.Keys.First();

            var queue = dictionary[key];
            if (queue.TryDequeue(out element))
            {
                if (queue.Count == 0)
                    dictionary.Remove(key);
                
                return true;
            }

            return false;
        }
    }
}