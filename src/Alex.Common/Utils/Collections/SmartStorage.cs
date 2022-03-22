using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Alex.Common.Utils.Collections
{
	public class SmartStorage<T>
	{
		private T[] Data { get; set; }

		//private int[]                           References { get; set; }
		private ConcurrentDictionary<int, int> Indexer { get; set; }

		private IEqualityComparer<T> EqualityComparer { get; }

		public SmartStorage(IEqualityComparer<T> equalityComparer)
		{
			EqualityComparer = equalityComparer;

			Data = new T[0];
			//	References = new int[0];
			Indexer = new ConcurrentDictionary<int, int>();
		}

		public SmartStorage() : this(EqualityComparer<T>.Default) { }

		private object _writeLock = new object();

		public int GetIndex(T data)
		{
			if (Indexer.TryGetValue(data.GetHashCode(), out var index))
				return index;

			return -1;
		}

		public int Add(T data)
		{
			lock (_writeLock)
			{
				var items = Data;
				//var references = References;

				Array.Resize(ref items, items.Length + 1);
				//Array.Resize(ref references, references.Length + 1);

				items[^1] = data;
				Data = items;
				//References = references;

				Indexer.TryAdd(data.GetHashCode(), items.Length - 1);

				return items.Length - 1;
			}
		}

		public void IncreaseUsage(int index)
		{
			
		}

		public void DecrementUsage(int index)
		{
			
		}

		public T this[int index]
		{
			get
			{
				return Data[index];
			}
		}
	}
}