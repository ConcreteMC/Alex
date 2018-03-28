using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Alex.Utils;

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
			initialCapacity = (int)(initialCapacity / 0.8F);
			_keys = new TK[initialCapacity];
			_values = new uint[initialCapacity];
			_byId = new TK[initialCapacity];
		}

		public uint GetId(TK p1868151)
		{
			if (p1868151 == null) return uint.MaxValue;
			return GetValue(GetIndex(p1868151, HashObject(p1868151)));
		}

		public TK Get(uint idIn)
		{
			return idIn >= 0 && idIn < _byId.Length ? _byId[idIn] : default(TK);
		}

		private uint GetValue(uint index)
		{
			return index == uint.MaxValue ? uint.MaxValue : _values[index];
		}

		/**
	     * Adds the given object while expanding this map
	     */
		public uint Add(TK objectIn)
		{
			uint i = NextId();
			Put(objectIn, i);
			return i;
		}

		private uint NextId()
		{
			while (_nextFreeIndex < _byId.Length && _byId[_nextFreeIndex] != null)
			{
				++_nextFreeIndex;
			}

			return _nextFreeIndex;
		}

		/**
	     * Rehashes the map to the new capacity
	     */
		private void Grow(int capacity)
		{
			TK[] ak = _keys;
			uint[] aint = _values;
			_keys = new TK[capacity];
			_values = new uint[capacity];
			_byId = new TK[capacity];
			_nextFreeIndex = 0;
			_mapSize = 0;

			for (int i = 0; i < ak.Length; ++i)
			{
				if (ak[i] != null)
				{
					Put(ak[i], aint[i]);
				}
			}
		}

		/**
	     * Puts the provided object value with the integer key.
	     */
		public void Put(TK objectIn, uint intKey)
		{
			uint i = (uint)Math.Max(intKey, _mapSize + 1);

			if (i >= _keys.Length * 0.8F)
			{
				int j;

				for (j = _keys.Length << 1; j < intKey; j <<= 1)
				{
				}

				Grow(j);
			}

			uint k = FindEmpty(HashObject(objectIn));
			_keys[k] = objectIn;
			_values[k] = intKey;
			_byId[intKey] = objectIn;
			++_mapSize;

			if (intKey == _nextFreeIndex)
			{
				++_nextFreeIndex;
			}
		}

		private uint HashObject(TK obectIn)
		{
			return (uint)(MathUtils.Hash((uint)(RuntimeHelpers.GetHashCode(obectIn) & uint.MaxValue)) % _keys.Length);
		}

		private uint GetIndex(TK objectIn, uint index)
		{
			for (uint i = index; i < _keys.Length; ++i)
			{
				if (_keys[i] == objectIn)
				{
					return i;
				}

				if (_keys[i] == _empty)
				{
					return uint.MaxValue;
				}
			}

			for (uint j = 0; j < index; ++j)
			{
				if (_keys[j] == objectIn)
				{
					return j;
				}

				if (_keys[j] == _empty)
				{
					return uint.MaxValue;
				}
			}

			return uint.MaxValue;
		}

		private uint FindEmpty(uint index)
		{
			for (uint i = index; i < _keys.Length; ++i)
			{
				if (_keys[i] == _empty)
				{
					return i;
				}
			}

			for (uint j = 0; j < index; ++j)
			{
				if (_keys[j] == _empty)
				{
					return j;
				}
			}

			throw new OverflowException();
		}

		public void Clear()
		{
			Array.Fill(_keys, default(TK));
			Array.Fill(_byId, default(TK));

			_nextFreeIndex = 0;
			_mapSize = 0;
		}

		public int Size()
		{
			return _mapSize;
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