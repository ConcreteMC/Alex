using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Alex.Blocks.State;

namespace Alex.Blocks.Storage.Palette
{
	public class IntIdentityHashBiMap<TValue> : IEnumerable<TValue>, IPalette<TValue> where TValue : class, IHasKey
	{
		private static object _empty = null;
		private TValue[] _values;
		private uint[] _keys;
		private TValue[] _byId;
		private uint _nextFreeIndex;
		private uint _mapSize;

		public IntIdentityHashBiMap(int initialCapacity)
		{
			_values = new TValue[initialCapacity];
			_keys = new uint[initialCapacity];
			_byId = new TValue[initialCapacity];
		}

		public uint GetId(TValue value)
		{
			if (value == null) throw new Exception("NULL");

			return GetValue(GetIndex(value, HashObject(value)));
		}

		public TValue Get(uint idIn)
		{
			return idIn < _byId.Length ? _byId[idIn] : null;
		}

		private uint GetValue(uint index)
		{
			return index == uint.MaxValue ? uint.MaxValue : _keys[index];
		}

		public uint Add(TValue objectIn)
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

		private void Grow(int capacity)
		{
			TValue[] ak = _values;
			uint[] aint = _keys;
			_values = new TValue[capacity];
			_keys = new uint[capacity];
			_byId = new TValue[capacity];
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

		public void Put(TValue objectIn, uint intKey)
		{
			uint i = Math.Max(intKey, _mapSize + 1);

			if (i >= _values.Length * 0.8F)
			{
				int j;

				for (j = _values.Length << 1; j < intKey; j <<= 1) { }

				Grow(j);
			}

			uint k = FindEmpty(HashObject(objectIn));
			_values[k] = objectIn;
			_keys[k] = intKey;
			_byId[intKey] = objectIn;
			++_mapSize;

			if (intKey == _nextFreeIndex)
			{
				++_nextFreeIndex;
			}
		}

		private uint HashObject(TValue state)
		{
			return (uint)((state.GetHashCode() & uint.MaxValue) % _values.Length);
		}

		private uint GetIndex(TValue objectIn, uint index)
		{
			for (uint i = index; i < _values.Length; i++)
			{
				if (_values[i] == objectIn)
				{
					return i;
				}

				if (_values[i] == _empty)
				{
					return uint.MaxValue;
				}
			}

			for (uint j = 0; j < index; j++)
			{
				if (_values[j] == objectIn)
				{
					return j;
				}

				if (_values[j] == _empty)
				{
					return uint.MaxValue;
				}
			}

			return uint.MaxValue;
		}

		private uint FindEmpty(uint index)
		{
			for (uint i = index; i < _values.Length; ++i)
			{
				if (_values[i] == _empty)
				{
					return i;
				}
			}

			for (uint j = 0; j < index; ++j)
			{
				if (_values[j] == _empty)
				{
					return j;
				}
			}

			throw new OverflowException();
		}

		public void Clear()
		{
			Array.Fill(_values, null);
			Array.Fill(_byId, null);

			_nextFreeIndex = 0;
			_mapSize = 0;
		}

		public uint Size()
		{
			return _mapSize;
		}

		public IEnumerator<TValue> GetEnumerator()
		{
			foreach (var i in _byId.Where(x => !x.Equals(null)))
			{
				yield return i;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <inheritdoc />
		public void Dispose()
		{
			//	_values = null;
			//	_byId = null;
		}
	}
}