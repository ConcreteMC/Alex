using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Alex.Utils
{
	#region License & Information

	// This notice must be kept visible in the source.
	//
	// This section of source code belongs to Rick@AIBrain.Org unless otherwise specified,
	// or the original license has been overwritten by the automatic formatting of this code.
	// Any unmodified sections of source code borrowed from other projects retain their original license and thanks goes to the Authors.
	//
	// Donations and Royalties can be paid via
	// PayPal: paypal@aibrain.org
	// bitcoin:1Mad8TxTqxKnMiHuZxArFvX8BuFEB9nqX2
	// bitcoin:1NzEsF7eegeEWDr5Vr9sSSgtUC4aL6axJu
	// litecoin:LeUxdU2w3o6pLZGVys5xpDZvvo8DUrjBp9
	//
	// Usage of the source code or compiled binaries is AS-IS.
	// I am not responsible for Anything You Do.
	//
	// Contact me by email if you have any questions or helpful criticism.
	//
	// "Librainian/ThreadSafeList.cs" was last cleaned by Rick on 2014/08/13 at 10:37 PM
	//
	// Taken from: https://github.com/AIBrain/Librainian
	// Modified by: Kenny van Vulpen (https://github.com/kennyvv/)

	#endregion

	[DebuggerDisplay("Count={Count}")]
	public sealed class ThreadSafeList<T> : IList<T>
	{
		private IList<T> Items { get; }
		private FastRandom Random { get; } = new FastRandom();
		private ReaderWriterLockSlim RwLock { get; } = new ReaderWriterLockSlim();

		public ThreadSafeList(IList<T> dataHolder)
		{
			Items = dataHolder;
		}

		public ThreadSafeList() : this(new List<T>()) { }

		public long LongCount
		{
			get
			{
				using (RwLock.Read())
				{
					return Items.LongCount();
				}
			}
		}

		public int Count
		{
			get
			{
				using (RwLock.Read())
				{
					return Items.Count;
				}
			}
		}

		public bool IsReadOnly => false;

		public T this[int index]
		{
			get
			{
				using (RwLock.Read())
				{
					return Items[index];
				}
			}

			set
			{
				using (RwLock.Write())
				{
					Items[index] = value;
				}
			}
		}

		public void Add(T item)
		{
			using (RwLock.Write())
			{
				Items.Add(item);
			}
		}

		public void Clear()
		{
			using (RwLock.Write())
			{
				Items.Clear();
			}
		}

		public bool Contains(T item)
		{
			using (RwLock.Read())
			{
				return Items.Contains(item);
			}
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			using (RwLock.Read())
			{
				Items.CopyTo(array, arrayIndex);
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			return Clone().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public int IndexOf(T item)
		{
			using (RwLock.Read())
			{
				return Items.IndexOf(item);
			}
		}

		public void Insert(int index, T item)
		{
			using (RwLock.Write())
			{
				Items.Insert(index, item);
			}
		}

		public bool Remove(T item)
		{
			using (RwLock.Write())
			{
				return Items.Remove(item);
			}
		}

		public void RemoveAt(int index)
		{
			using (RwLock.Write())
			{
				Items.RemoveAt(index);
			}
		}

		public T[] ToArray()
		{
			using (RwLock.Write())
			{
				return Items.ToArray();
			}
		}

		public Task AddAsync(T item)
		{
			return Task.Run(() => { TryAdd(item); });
		}

		/// <summary>
		///     Add in an enumerable of items.
		/// </summary>
		/// <param name="collection"></param>
		/// <param name="asParallel"></param>
		public void AddRange(IEnumerable<T> collection, bool asParallel = true)
		{
			if (null == collection)
			{
				return;
			}
			using (RwLock.Write())
			{
				Items.AddRange(asParallel ? collection.AsParallel() : collection);
			}
		}

		/// <summary>
		///     Returns a new copy of all items in the <see cref="List{T}" />.
		/// </summary>
		/// <returns></returns>
		public List<T> Clone(bool asParallel = true)
		{
			using (RwLock.Read())
			{
				return asParallel ? new List<T>(Items.AsParallel()) : new List<T>(Items);
			}
		}

		/// <summary>
		///     Perform the <paramref name="action" /> on each item in the list.
		/// </summary>
		/// <param name="action"><paramref name="action" /> to perform on each item.</param>
		/// <param name="performActionOnClones">
		///     If true, the <paramref name="action" /> will be performed on a <see cref="Clone" /> of
		///     the items.
		/// </param>
		/// <param name="asParallel">Use the <see cref="ParallelQuery{TSource}" /> method.</param>
		/// <param name="inParallel">
		///     Use the
		///     <see
		///         cref="Parallel.ForEach{TSource}(System.Collections.Generic.IEnumerable{TSource},System.Action{TSource})" />
		///     method.
		/// </param>
		public void ForAll(Action<T> action, bool performActionOnClones = true, bool asParallel = true,
			bool inParallel = false)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			Action<T> wrapper = obj =>
			{
				try
				{
					action(obj);
				}
				catch (ArgumentNullException)
				{
					//if a null gets into the list then swallow an ArgumentNullException so we can continue adding
				}
			};
			if (performActionOnClones)
			{
				List<T> clones = Clone(asParallel);
				if (asParallel)
				{
					clones.AsParallel().ForAll(wrapper);
				}
				else if (inParallel)
				{
					Parallel.ForEach(clones, wrapper);
				}
				else
				{
					clones.ForEach(wrapper);
				}
			}
			else
			{
				using (RwLock.Read())
				{
					if (asParallel)
					{
						Items.AsParallel().ForAll(wrapper);
					}
					else if (inParallel)
					{
						Parallel.ForEach(Items, wrapper);
					}
					else
					{
						Items.ForEach(wrapper);
					}
				}
			}
		}

		/// <summary>
		///     Perform the <paramref name="action" /> on each item in the list.
		/// </summary>
		/// <param name="action">
		///     <paramref name="action" /> to perform on each item.
		/// </param>
		/// <param name="performActionOnClones">
		///     If true, the <paramref name="action" /> will be performed on a <see cref="Clone" /> of the items.
		/// </param>
		/// <param name="asParallel">
		///     Use the <see cref="ParallelQuery{TSource}" /> method.
		/// </param>
		/// <param name="inParallel">
		///     Use the
		///     <see
		///         cref="Parallel.ForEach{TSource}(System.Collections.Generic.IEnumerable{TSource},System.Action{TSource})" />
		///     method.
		/// </param>
		public void ForEach(Action<T> action, bool performActionOnClones = true, bool asParallel = true,
			bool inParallel = false)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			Action<T> wrapper = obj =>
			{
				try
				{
					action(obj);
				}
				catch (ArgumentNullException)
				{
					//if a null gets into the list then swallow an ArgumentNullException so we can continue adding
				}
			};
			if (performActionOnClones)
			{
				List<T> clones = Clone(asParallel);
				if (asParallel)
				{
					clones.AsParallel().ForAll(wrapper);
				}
				else if (inParallel)
				{
					Parallel.ForEach(clones, wrapper);
				}
				else
				{
					clones.ForEach(wrapper);
				}
			}
			else
			{
				using (RwLock.Read())
				{
					if (asParallel)
					{
						Items.AsParallel().ForAll(wrapper);
					}
					else if (inParallel)
					{
						Parallel.ForEach(Items, wrapper);
					}
					else
					{
						Items.ForEach(wrapper);
					}
				}
			}
		}

		public bool TryAdd(T item)
		{
			bool written = false;
			try
			{
				RwLock.EnterUpgradeableReadLock();
				if (!Items.Contains(item))
				{
					try
					{
						RwLock.EnterWriteLock();
						Items.Add(item);
						written = true;
					}
					finally
					{
						RwLock.ExitWriteLock();
					}
				}
			}
			catch (NullReferenceException)
			{
			}
			catch (ObjectDisposedException)
			{
			}
			catch (ArgumentNullException)
			{
			}
			catch (ArgumentOutOfRangeException)
			{
			}
			catch (ArgumentException)
			{
			}
			finally
			{
				RwLock.ExitUpgradeableReadLock();
			}

			return written;
		}

		public bool TryTake(out T item)
		{
			using (RwLock.Read())
			{
				int count = Items.Count;
				if (count >= 1)
				{
					int idx = Random.Next(0, count);
					item = Items[idx];
					Items.RemoveAt(idx);
					return true;
				}
			}
			item = default(T);
			return false;
		}

		/// <summary>
		///     Remove one item, and return a list-copy of the rest.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="rest"></param>
		/// <returns></returns>
		public bool TryTakeOneCopyRest(out T item, out List<T> rest)
		{
			using (RwLock.Write())
			{
				int count = Items.Count;
				if (count >= 1)
				{
					item = Items[0];
					Items.RemoveAt(0);
					rest = new List<T>(Items);
					return true;
				}
			}
			item = default(T);
			rest = default(List<T>);
			return false;
		}

		public T[] TakeAndClear()
		{
			T[] result;
			using (RwLock.Write())
			{
				result = Items.ToArray();
				Items.Clear();
			}

			return result;
		}

		public void Add(IEnumerable<T> items)
		{
			try
			{
				RwLock.EnterUpgradeableReadLock();
				foreach (var item in items)
				{
					if (Items.Contains(item))
					{
						continue;
					}
					else
					{
						RwLock.EnterWriteLock();
						try
						{
							Items.Add(item);
						}
						finally
						{
							RwLock.ExitWriteLock();
						}
					}
				}
			}
			finally
			{
				RwLock.ExitUpgradeableReadLock();
			}
		}
	}

	internal static class ListExtensions
	{
		public static void AddRange<T>(this IList<T> list, IEnumerable<T> collection)
		{
			foreach (T i in collection)
			{
				list.Add(i);
			}
		}

		public static void ForEach<T>(this IList<T> list, Action<T> action)
		{
			foreach (T i in list)
			{
				action.Invoke(i);
			}
		}
	}

	internal static class ReaderWriterLockExtensions
	{
		public static ReadLock Read(this ReaderWriterLockSlim rwl)
		{
			return new ReadLock(rwl);
		}

		public static WriteLock Write(this ReaderWriterLockSlim rwl)
		{
			return new WriteLock(rwl);
		}

		public class ReadLock : IDisposable
		{
			private ReaderWriterLockSlim TheLock { get; }

			internal ReadLock(ReaderWriterLockSlim rwl)
			{
				TheLock = rwl;
				TheLock.EnterReadLock();
			}

			public void Dispose()
			{
				TheLock.ExitReadLock();
			}
		}

		public class WriteLock : IDisposable
		{
			private ReaderWriterLockSlim TheLock { get; }

			internal WriteLock(ReaderWriterLockSlim rwl)
			{
				TheLock = rwl;
				TheLock.EnterWriteLock();
			}

			public void Dispose()
			{
				TheLock.ExitWriteLock();
			}
		}
	}
}
