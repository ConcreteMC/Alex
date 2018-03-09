using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Alex.Blocks.Storage
{
	public class IntIdentityHashBiMap<TK> : IEnumerable<TK> where TK : class 
	{
		private static TK _empty = default(TK);
		private TK[] _keys;
		private int[] _values;
		private TK[] _byId;
		private int _nextFreeIndex;
		private int _mapSize;

		public IntIdentityHashBiMap(int initialCapacity)
		{
			initialCapacity = (int) ((float) initialCapacity / 0.8F);
			this._keys = new TK[initialCapacity];
			this._values = new int[initialCapacity];
			this._byId = new TK[initialCapacity];
		}

		public int GetId(TK p1868151)
		{
			if (p1868151 == null) return -1;
			return this.GetValue(this.GetIndex(p1868151, this.HashObject(p1868151)));
		}

		public TK Get(int idIn)
		{
			return (TK) (idIn >= 0 && idIn < this._byId.Length ? this._byId[idIn] : default(TK));
		}

		private int GetValue(int p1868051)
		{
			return p1868051 == -1 ? -1 : this._values[p1868051];
		}

		/**
	     * Adds the given object while expanding this map
	     */
		public int Add(TK objectIn)
		{
			int i = this.NextId();
			this.Put(objectIn, i);
			return i;
		}

		private int NextId()
		{
			while (this._nextFreeIndex < this._byId.Length && this._byId[this._nextFreeIndex] != null)
			{
				++this._nextFreeIndex;
			}

			return this._nextFreeIndex;
		}

		/**
	     * Rehashes the map to the new capacity
	     */
		private void Grow(int capacity)
		{
			TK[] ak = this._keys;
			int[] aint = this._values;
			this._keys = new TK[capacity];
			this._values = new int[capacity];
			this._byId = new TK[capacity];
			this._nextFreeIndex = 0;
			this._mapSize = 0;

			for (int i = 0; i < ak.Length; ++i)
			{
				if (ak[i] != null)
				{
					this.Put(ak[i], aint[i]);
				}
			}
		}

		/**
	     * Puts the provided object value with the integer key.
	     */
		public void Put(TK objectIn, int intKey)
		{
			int i = Math.Max(intKey, this._mapSize + 1);

			if ((float) i >= (float) this._keys.Length * 0.8F)
			{
				int j;

				for (j = this._keys.Length << 1; j < intKey; j <<= 1)
				{
					;
				}

				this.Grow(j);
			}

			int k = this.FindEmpty(this.HashObject(objectIn));
			this._keys[k] = objectIn;
			this._values[k] = intKey;
			this._byId[intKey] = objectIn;
			++this._mapSize;

			if (intKey == this._nextFreeIndex)
			{
				++this._nextFreeIndex;
			}
		}

		private int HashObject(TK obectIn)
		{
			//if (obectIn == null) return -1;
			return obectIn.GetHashCode() % this._keys.Length;
			//	return (MathHelper.getHash(System.identityHashCode(obectIn)) & Integer.MAX_VALUE) % this.keys.length;
		}

		private int GetIndex(TK objectIn, int p1868162)
		{
			for (int i = p1868162; i < this._keys.Length; ++i)
			{
				if (this._keys[i] == objectIn)
				{
					return i;
				}

				if (this._keys[i] == _empty)
				{
					return -1;
				}
			}

			for (int j = 0; j < p1868162; ++j)
			{
				if (this._keys[j] == objectIn)
				{
					return j;
				}

				if (this._keys[j] == _empty)
				{
					return -1;
				}
			}

			return -1;
		}

		private int FindEmpty(int p1868061)
		{
			for (int i = p1868061; i < this._keys.Length; ++i)
			{
				if (this._keys[i] == _empty)
				{
					return i;
				}
			}

			for (int j = 0; j < p1868061; ++j)
			{
				if (this._keys[j] == _empty)
				{
					return j;
				}
			}

			throw new OverflowException();
		}

		//public Iterator<K> iterator()
		//{
		//	return Iterators.filter(Iterators.forArray(this.byId), Predicates.notNull());
		//}

		public void Clear()
		{
			Array.Fill<TK>(this._keys, default(TK));
			Array.Fill<TK>(this._byId, default(TK));
			//Array.fill(this.keys, (Object) null);
			//Arrays.fill(this.byId, (Object) null);
			this._nextFreeIndex = 0;
			this._mapSize = 0;
		}

		public int Size()
		{
			return this._mapSize;
		}

		public IEnumerator<TK> GetEnumerator()
		{
			foreach (var i in _byId.Where(x => !x.Equals(default(TK))))
			{
				yield return i;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}