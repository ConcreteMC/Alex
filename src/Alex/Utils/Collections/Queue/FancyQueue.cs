using System;
using System.Collections;
using System.Collections.Generic;

namespace Alex.Utils.Collections.Queue
{
	public class FancyQueue<T> : IEnumerable<T>
	{
		private readonly LinkedList<T> _items;
		private object _lock = new object();

		private Func<T, T, int> _ordering = null;
		public FancyQueue(Func<T, T, int> ordering = null)
		{
			_ordering = ordering;
			_items = new LinkedList<T>();
		}

		public int Count
		{
			get
			{
				lock (_items)
				{
					return _items.Count;
				}
			}
		}
		
		public bool IsEmpty
		{
			get
			{
				lock (_items)
				{
					return _items.Count == 0;
				}
			}
		}

		public void Enqueue(T item)
		{
			lock (_lock)
			{
				_items.AddLast(item);
			}
		}

		public bool TryDequeue(out T item, Func<T, bool> predicate = null)
		{
			item = default;
			lock (_lock)
			{
				if (_items.Count == 0)
					return false;

				var first = _items.First;
				
				if (first != null)
				{
					if (predicate != null)
					{
						//List<T> candidates = 
						double lowest = double.MaxValue;
						var    currentItem   = first;
						while (currentItem.Next != null)
						{
							if (predicate(currentItem.Value))
							{
								if (_ordering != null)
								{
									var res = _ordering(first.Value, currentItem.Value);

									if (res > 0)
									{
										first = currentItem;
									}
								}
								else
								{
									first = currentItem;

									break;
								}
							}
							
							currentItem = currentItem.Next;
						}
					}
					
					_items.Remove(first);
					item = first.Value;
					return true;
				}
			}

			return false;
		}

		public void Remove(T item)
		{
			lock (_lock)
			{
				_items.Remove(item);
			}
		}

		public bool Contains(T item)
		{
			lock (_lock)
			{
				return _items.Contains(item);
			}
		}

		public void Clear()
		{
			lock (_lock)
			{
				_items.Clear();
			}
		}
		
		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator()
		{
			lock (_lock)
			{
				foreach (var item in _items)
				{
					yield return item;
				}
			}
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}