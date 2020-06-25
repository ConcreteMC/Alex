using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Alex.API.Utils;
using Alex.Blocks.State;
using Alex.Utils;

namespace Alex.Blocks.Storage
{
	public class IntIdentityHashBiMap : IEnumerable<BlockState>, IPallete
	{
		private static BlockState _empty = null;
		private BlockState[] _values;
		private uint[] _keys;
		private BlockState[] _byId;
		private uint _nextFreeIndex;
		private int _mapSize;

		public IntIdentityHashBiMap(int initialCapacity)
		{
			_values = new BlockState[initialCapacity];
			_keys = new uint[initialCapacity];
			_byId = new BlockState[initialCapacity];
		}

		public uint GetId(BlockState value)
		{
			if (value == null) throw new Exception("NULL");
			return GetValue(GetIndex(value, HashObject(value)));
		}

		public BlockState Get(uint idIn)
		{
			return idIn >= 0 && idIn < _byId.Length ? _byId[idIn] : null;
		}

		private uint GetValue(uint index)
		{
			return index == uint.MaxValue ? uint.MaxValue : _keys[index];
		}

		public uint Add(BlockState objectIn)
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
			BlockState[] ak = _values;
			uint[] aint = _keys;
			_values = new BlockState[capacity];
			_keys = new uint[capacity];
			_byId = new BlockState[capacity];
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
		
		public void Put(BlockState objectIn, uint intKey)
		{
			uint i = (uint)Math.Max(intKey, _mapSize + 1);

			if (i >= _values.Length * 0.8F)
			{
				int j;

				for (j = _values.Length << 1; j < intKey; j <<= 1)
				{
				}

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

		private uint HashObject(BlockState obectIn)
		{
			return (uint)((uint)(RuntimeHelpers.GetHashCode(obectIn) & uint.MaxValue) % _values.Length);
		}

		private uint GetIndex(BlockState objectIn, uint index)
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

		public int Size()
		{
			return _mapSize;
		}

		public IEnumerator<BlockState> GetEnumerator()
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
	}
}