using System;
using System.Collections;
using System.Collections.Generic;

namespace Alex.Common.Utils.Collections
{
	/// <summary>
	///     A list that only holds weak references to various objects
	/// </summary>
	/// <typeparam name="T">The type of class object held in the list</typeparam>
	class WeakList<T> : IList, IList<T>, IReadOnlyList<T>, ICollection, ICollection<T>, IReadOnlyCollection<T>,
		IEnumerable, IEnumerable<T>, IEnumerator<T> where T : class
	{
		/// <summary>
		///     The backing object is just a list of weak references
		/// </summary>
		private List<WeakReference<T>> _list = new List<WeakReference<T>>();

		#region IList

		/// <summary>
		///     True if the list is fixed in size (it is not)
		/// </summary>
		public bool IsFixedSize => false;

		/// <summary>
		///     True if the list is ReadOnly (it is not)
		/// </summary>
		public bool IsReadOnly => false;

		/// <summary>
		///     Adds an object to the list
		/// </summary>
		/// <param name="value">The object to add to the list</param>
		/// <returns>The number of objects added</returns>
		public int Add(object value)
		{
			var v = value as T;

			if (v == null)
			{
				return 0;
			}

			lock (SyncRoot)
			{
				int i = 0;

				while (i < _list.Count)
				{
					if (this[i] == v)
					{
						return 0;
					}

					i++;
				}

				_list.Add(v);

				return 1;
			}
		}

		/// <summary>
		///     Gets or sets an object by index
		/// </summary>
		/// <param name="index">A non-negative integer index</param>
		/// <returns>A value, null otherwise</returns>
		object IList.this[int index]
		{
			get
			{
				if (index < 0 || index >= _list.Count)
				{
					return null;
				}

				return _list[index].GetValue();
			}
			set
			{
				if (index < 0 || index >= _list.Count)
				{
					return;
				}

				var v = value as T;

				if (v != null)
				{
					_list[index].SetTarget(v);
				}
			}
		}


		/// <summary>
		///     Clears the list
		/// </summary>
		public void Clear()
		{
			lock (SyncRoot)
			{
				_list.Clear();
			}
		}

		/// <summary>
		///     Checks if the list contains an item
		/// </summary>
		/// <param name="item">The item to check</param>
		/// <returns>true if the list contains the item</returns>
		public bool Contains(object value)
		{
			var v = value as T;

			if (v == null)
			{
				return false;
			}

			return Contains(v);
		}

		/// <summary>
		///     Finds the index of an item
		/// </summary>
		/// <param name="item">The item to find</param>
		/// <returns>The index or -1</returns>
		public int IndexOf(object value)
		{
			var v = value as T;

			if (v == null)
			{
				return -1;
			}

			return IndexOf(v);
		}

		/// <summary>
		///     Inserts an item at a given index
		/// </summary>
		/// <param name="index">The index to insert at</param>
		/// <param name="value">The value to insert</param>
		public void Insert(int index, object value)
		{
			var v = value as T;

			if (v == null)
			{
				return;
			}

			lock (SyncRoot)
			{
				_list.Insert(index, new WeakReference<T>(v));
			}
		}


		/// <summary>
		///     Removes a value from the list
		/// </summary>
		/// <param name="value"></param>
		public void Remove(object value)
		{
			Remove(value as T);
		}

		/// <summary>
		///     Removes an item at an index
		/// </summary>
		/// <param name="index">A non-negative integer giving the index of the item to remove</param>
		public void RemoveAt(int index)
		{
			if (index < 0 || index >= _list.Count)
			{
				return;
			}

			lock (SyncRoot)
			{
				_list.RemoveAt(index);
			}
		}

		#endregion IList

		#region IList<T>

		/// <summary>
		///     Gets or sets an object by index
		/// </summary>
		/// <param name="index">A non-negative integer index</param>
		/// <returns>A value, null otherwise</returns>
		public T this[int index]
		{
			get
			{
				if (index < 0 || index >= _list.Count)
				{
					return null;
				}

				return _list[index].GetValue();
			}
			set
			{
				if (index < 0 || index >= _list.Count)
				{
					return;
				}

				_list[index].SetTarget(value);
			}
		}


		/// <summary>
		///     Finds the index of an item
		/// </summary>
		/// <param name="item">The item to find</param>
		/// <returns>The index or -1</returns>
		public int IndexOf(T item)
		{
			if (item == null)
			{
				return -1;
			}

			lock (SyncRoot)
			{
				int i = 0;

				while (i < _list.Count)
				{
					T current = this[i];

					if (current == item)
					{
						return i;
					}

					if (current == null)
					{
						_list.RemoveAt(i);
						i--;
					}

					i++;
				}
			}

			return -1;
		}



		/// <summary>
		///     Inserts an item at a given index
		/// </summary>
		/// <param name="index">The index to insert at</param>
		/// <param name="item">The item to insert</param>
		public void Insert(int index, T item)
		{
			var v = item as T;

			if (v == null)
			{
				return;
			}

			lock (SyncRoot)
			{
				_list.Insert(index, new WeakReference<T>(v));
			}
		}

		#endregion

		#region ICollection

		/// <summary>
		///     Gets the number of items currently in the list
		/// </summary>
		public int Count => _list.Count;

		/// <summary>
		///     True if the list uses locks to synchronize itself in multi-threaded environments
		/// </summary>
		public bool IsSynchronized => true;

		/// <summary>
		///     The object to lock when performing multi-threaded operations
		/// </summary>
		public object SyncRoot { get; private set; } = new object();

		/// <summary>
		///     Copies the contents into an array
		/// </summary>
		/// <param name="array">The array to copy to</param>
		/// <param name="index">The index in the array to start copying at</param>
		public void CopyTo(Array array, int index)
		{
			lock (SyncRoot)
			{
				lock (array.SyncRoot)
				{
					int i = 0;

					while (i < _list.Count && i + index < array.Length)
					{
						var v = this[i];

						while (v == null && i < _list.Count)
						{
							_list.RemoveAt(i);
							v = this[i];
						}

						if (v != null)
						{
							array.SetValue(v, i + index);
						}

						i++;
					}
				}
			}
		}

		#endregion

		#region ICollection<T>

		/// <summary>
		///     Adds an object to the list
		/// </summary>
		/// <param name="value">The object to add to the list</param>
		public void Add(T item)
		{
			var v = item as T;

			if (v == null)
			{
				return;
			}

			lock (SyncRoot)
			{
				int i = 0;

				while (i < _list.Count)
				{
					if (this[i] == v)
					{
						return;
					}

					i++;
				}

				_list.Add(v);
			}
		}

		/// <summary>
		///     Checks if the list contains an item
		/// </summary>
		/// <param name="item">The item to check</param>
		/// <returns>true if the list contains the item</returns>
		public bool Contains(T item)
		{
			lock (SyncRoot)
			{
				int i = 0;

				while (i < _list.Count)
				{
					T current = this[i];

					if (current == item)
					{
						return true;
					}
					else if (current == null)
					{
						_list.RemoveAt(i);
						i--;
					}

					i++;
				}
			}

			return false;
		}


		/// <summary>
		///     Copies the contents into an array
		/// </summary>
		/// <param name="array">The array to copy to</param>
		/// <param name="index">The index in the array at which to start copying</param>
		public void CopyTo(T[] array, int arrayIndex)
		{
			lock (SyncRoot)
			{
				int i = 0;

				while (i < _list.Count && i + arrayIndex < array.Length)
				{
					var v = this[i];

					while (v == null && i < _list.Count)
					{
						_list.RemoveAt(i);
						v = this[i];
					}

					if (v != null)
					{
						array[i + arrayIndex] = v;
					}

					i++;
				}
			}
		}

		/// <summary>
		///     Removes an item
		/// </summary>
		/// <param name="item">An item to remove</param>
		/// <returns>True if item removed</returns>
		public bool TryRemove(T item)
		{
			return Remove(item);
		}
		
		/// <summary>
		///     Removes an item
		/// </summary>
		/// <param name="item">An item to remove</param>
		/// <returns>True if tiem removed</returns>
		public bool Remove(T item)
		{
			var ret = false;

			lock (SyncRoot)
			{
				int i = 0;

				while (i < _list.Count)
				{
					var current = this[i];

					if (current == item || current == null)
					{
						_list.RemoveAt(i);
						ret = ret || current != null;
						i--;
					}

					i++;
				}
			}

			return false;
		}

		#endregion

		#region IEnumerator

		private int _position = -1;

		/// <summary>
		///     Gets the current object of the enumerator
		/// </summary>
		public object Current
		{
			get
			{
				try
				{
					return _list[_position].GetValue();
				}
				catch (IndexOutOfRangeException)
				{
					throw new InvalidOperationException();
				}
			}
		}

		/// <summary>
		///     Gets the current value of the enumerator
		/// </summary>
		T IEnumerator<T>.Current
		{
			get
			{
				try
				{
					return _list[_position].GetValue();
				}
				catch (IndexOutOfRangeException)
				{
					throw new InvalidOperationException();
				}
			}
		}

		/// <summary>
		///     Moves the enumerator to the next item
		/// </summary>
		/// <returns>True if the enumerator can move</returns>
		public bool MoveNext()
		{
			if (_list == null)
			{
				return false;
			}

			_position++;

			return _position < _list.Count;
		}

		/// <summary>
		///     Resets the enumerator
		/// </summary>
		public void Reset()
		{
			lock (SyncRoot)
			{
				int i = 0;

				while (i < _list.Count)
				{
					if (this[i] == null)
					{
						_list.RemoveAt(i);
						i--;
					}

					i++;
				}

				_position = -1;
			}

			(_list.GetEnumerator() as IEnumerator)?.Reset();
		}

		#endregion

		#region IEnumerable

		/// <summary>
		///     This list is an enumerator
		/// </summary>
		/// <returns>The enumerator</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this;
		}

		#endregion

		#region IEnumerable<T>

		/// <summary>
		///     Gets this object as an enumerator
		/// </summary>
		/// <returns>The enumerator</returns>
		public IEnumerator<T> GetEnumerator()
		{
			return this;
		}

		#endregion

		#region IDisposable

		/// <summary>
		///     The dispose method of the list
		/// </summary>
		public void Dispose()
		{
			_position = -1;
		}

		#endregion
	}

	internal static class WeakReferenceExtensions
	{
		/// <summary>
		///     Gets the value of a weak reference (what the reference it to)
		/// </summary>
		/// <typeparam name="T">The type of object the weak reference holds</typeparam>
		/// <param name="wr">The weak reference</param>
		/// <returns>The value or null</returns>
		public static T GetValue<T>(this WeakReference<T> wr) where T : class
		{
			T val;
			if (wr.TryGetTarget(out val)) { return val; }
			return null;
		}
		
		/// <summary>
		///     Adds an object to a collection of weak references
		///     (will not add if object is null or not of right type)
		/// </summary>
		/// <typeparam name="T">The type of object ot add</typeparam>
		/// <param name="l">The weak reference list</param>
		/// <param name="val">The object o add</param>
		public static void Add<T>(this ICollection<WeakReference<T>> l, object val) where T : class
		{
			var v = val as T;
			if (v != null) { l.Add(new WeakReference<T>(v)); }
		}
	}
}