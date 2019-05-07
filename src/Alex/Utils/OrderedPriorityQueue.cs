using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Alex.Utils
{
	public class QueueCollection<TItem> : IEnumerable<TItem>
	{
		private List<TItem> Items { get; }
		private ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
		public int Count
		{
			get
			{
				_readWriteLock.EnterReadLock();
				try
				{
					return Items.Count;
				}
				finally
				{
					_readWriteLock.ExitReadLock();
				}
			}
		}

        public QueueCollection()
		{
            Items = new List<TItem>();
		}

		public bool Remove(TItem item)
		{
			_readWriteLock.EnterWriteLock();
			try
			{
				return Items.Remove(item);
			}
			finally
			{
				_readWriteLock.ExitWriteLock();
			}
        }

		public void RemoveMany(IEnumerable<TItem> items)
		{
			_readWriteLock.EnterWriteLock();
			try
			{
				foreach (var item in items)
				{
					Items.Remove(item);
				}
				//return Items.Remove(item);
			}
			finally
			{
				_readWriteLock.ExitWriteLock();
			}
		}

        public void Add(TItem item)
		{
			//_readWriteLock.EnterWriteLock();
			try
			{
				Items.Add(item);
            }
			finally
			{
			//	_readWriteLock.ExitWriteLock();
			}
		}

		public bool TryTake(out TItem item)
		{
            _readWriteLock.EnterUpgradeableReadLock();
			try
			{
				if (Items.Count == 0)
				{
					item = default(TItem);
					return false;
				}

				_readWriteLock.EnterWriteLock();
				try
				{
					item = Items[0];
					Items.RemoveAt(0);
				}
				finally
				{
					_readWriteLock.ExitWriteLock();
				}
			}
			finally
			{
                _readWriteLock.ExitUpgradeableReadLock();
			}

			return true;
		}

		public IEnumerator<TItem> GetEnumerator()
		{
			_readWriteLock.EnterReadLock();
			try
			{
				foreach (var item in Items)
				{
					yield return item;
				}
			}
			finally
			{
				_readWriteLock.ExitReadLock();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

    public class OrderedPriorityQueue<TItem, TPriority> where TPriority : IComparable
    {
	    private SortedList<TPriority, QueueCollection<TItem>> pq = new SortedList<TPriority, QueueCollection<TItem>>();
        private ConcurrentDictionary<TItem, TPriority> LocationHelper { get; } = new ConcurrentDictionary<TItem, TPriority>();
	    private ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        private int _count = 0;
	    public int Count
	    {
		    get
		    {
			    _readWriteLock.EnterReadLock();
			    try
			    {
				    return _count;
			    }
			    finally
			    {
				    _readWriteLock.ExitReadLock();
			    }
		    }
	    }

	    public void UpdatePriority(TItem item, TPriority newPriority)
	    {
		    if (LocationHelper.TryGetValue(item, out TPriority currentPriority))
		    {
                _readWriteLock.EnterUpgradeableReadLock();
			    try
			    {
				    var queue = pq[currentPriority];
				    if (queue.Count == 1)
				    {
                        _readWriteLock.EnterWriteLock();
					    try
					    {
						    pq.Remove(currentPriority);
					    }
					    finally
					    {
                            _readWriteLock.ExitWriteLock();
					    }
				    }

                    if (queue.Remove(item))
                    {
						Enqueue(item, newPriority);
                    }
			    }
			    finally
			    {
                    _readWriteLock.ExitUpgradeableReadLock();
			    }
		    }
	    }

        public void Enqueue(TItem item, TPriority priority)
	    {
		    _readWriteLock.EnterUpgradeableReadLock();
		    try
		    {
			    Interlocked.Increment(ref _count);
			    if (!pq.ContainsKey(priority))
			    {
                    _readWriteLock.EnterWriteLock();
				    try
				    {
					    pq[priority] = new QueueCollection<TItem>();
				    }
				    finally
				    {
					    _readWriteLock.ExitWriteLock();
				    }
			    }
			    pq[priority].Add(item);

			    LocationHelper.AddOrUpdate(item, priority, (item1, priority1) => priority);
		    }
		    finally
		    {
                _readWriteLock.ExitUpgradeableReadLock();
		    }
	    }

	    public bool TryDequeue(out TItem result)
	    {
		    bool success;
            _readWriteLock.EnterWriteLock();
		    try
		    {
			    if (Interlocked.Decrement(ref _count) > 0)
			    {
				    var queue = pq.ElementAt(0).Value;
				    if (queue.Count == 1) pq.RemoveAt(0);
				    success = queue.TryTake(out result);
			    }
			    else
			    {
				    result = default(TItem);
				    success = false;
			    }
		    }
		    finally
		    {
                _readWriteLock.ExitWriteLock();
		    }

		    if (success)
		    {
			    LocationHelper.TryRemove(result, out _);
		    }

		    return success;
	    }
    }
}
