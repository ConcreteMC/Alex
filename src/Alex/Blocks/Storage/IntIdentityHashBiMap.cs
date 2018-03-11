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
		private uint[] _values;
		private TK[] _byId;
		private uint _nextFreeIndex;
		private int _mapSize;

		public IntIdentityHashBiMap(int initialCapacity)
		{
			initialCapacity = (int) ((float) initialCapacity / 0.8F);
			this._keys = new TK[initialCapacity];
			this._values = new uint[initialCapacity];
			this._byId = new TK[initialCapacity];
		}

		public uint GetId(TK p1868151)
		{
			if (p1868151 == null) return uint.MaxValue;
			return this.GetValue(this.GetIndex(p1868151, this.HashObject(p1868151)));
		}

		public TK Get(uint idIn)
		{
			return (TK) (idIn >= 0 && idIn < this._byId.Length ? this._byId[idIn] : default(TK));
		}

		private uint GetValue(uint p1868051)
		{
			return p1868051 == uint.MaxValue ? uint.MaxValue : this._values[p1868051];
		}

		/**
	     * Adds the given object while expanding this map
	     */
		public uint Add(TK objectIn)
		{
			uint i = this.NextId();
			this.Put(objectIn, i);
			return i;
		}

		private uint NextId()
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
			uint[] aint = this._values;
			this._keys = new TK[capacity];
			this._values = new uint[capacity];
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
		public void Put(TK objectIn, uint intKey)
		{
			uint i = (uint) Math.Max(intKey, this._mapSize + 1);

			if ((float) i >= (float) this._keys.Length * 0.8F)
			{
				int j;

				for (j = this._keys.Length << 1; j < intKey; j <<= 1)
				{
					;
				}

				this.Grow(j);
			}

			uint k = this.FindEmpty(this.HashObject(objectIn));
			this._keys[k] = objectIn;
			this._values[k] = intKey;
			this._byId[intKey] = objectIn;
			++this._mapSize;

			if (intKey == this._nextFreeIndex)
			{
				++this._nextFreeIndex;
			}
		}

		private uint HashObject(TK obectIn)
		{
			//if (obectIn == null) return -1;
			return (uint) ((obectIn.GetHashCode() & uint.MaxValue) % this._keys.Length);
			//	return (MathHelper.getHash(System.identityHashCode(obectIn)) & Integer.MAX_VALUE) % this.keys.length;
		}

		private uint GetIndex(TK objectIn, uint p1868162)
		{
			for (uint i = p1868162; i < this._keys.Length; ++i)
			{
				if (this._keys[i] == objectIn)
				{
					return i;
				}

				if (this._keys[i] == _empty)
				{
					return uint.MaxValue;
				}
			}

			for (uint j = 0; j < p1868162; ++j)
			{
				if (this._keys[j] == objectIn)
				{
					return j;
				}

				if (this._keys[j] == _empty)
				{
					return uint.MaxValue;
				}
			}

			return uint.MaxValue;
		}

		private uint FindEmpty(uint p1868061)
		{
			for (uint i = p1868061; i < this._keys.Length; ++i)
			{
				if (this._keys[i] == _empty)
				{
					return i;
				}
			}

			for (uint j = 0; j < p1868061; ++j)
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