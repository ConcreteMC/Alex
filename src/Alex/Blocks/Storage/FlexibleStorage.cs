using System;

namespace Alex.Blocks.Storage
{
	public class FlexibleStorage
	{
		private long[] _data;
		private int _bitsPerEntry;
		private int _size;
		private long _maxEntryValue;

		public FlexibleStorage(int bitsPerEntry, int size) : this(bitsPerEntry, new long[RoundToNearest(size * bitsPerEntry, 64) / 64])
		{
		}

		public FlexibleStorage(int bitsPerEntry, long[] data)
		{
			if (bitsPerEntry < 1 || bitsPerEntry > 32)
			{
				throw new Exception("BitsPerEntry cannot be outside of accepted range.");
			}

			this._bitsPerEntry = bitsPerEntry;
			this._data = data;

			this._size = this._data.Length * 64 / this._bitsPerEntry;
			this._maxEntryValue = (1L << this._bitsPerEntry) - 1;
		}

		public uint this[int index]
		{
			get
			{
				if (index < 0 || index > this._size - 1)
				{
					throw new IndexOutOfRangeException();
				}

				int bitIndex = index * this._bitsPerEntry;
				int startIndex = bitIndex / 64;
				int endIndex = ((index + 1) * this._bitsPerEntry - 1) / 64;
				int startBitSubIndex = bitIndex % 64;
				if (startIndex == endIndex)
				{
					return (uint)(this._data[startIndex] >> startBitSubIndex & this._maxEntryValue);
				}
				else
				{
					int endBitSubIndex = 64 - startBitSubIndex;
					return (uint)((this._data[startIndex] >> startBitSubIndex | this._data[endIndex] << endBitSubIndex) & this._maxEntryValue);
				}
			}
			set
			{
				if (index < 0 || index > this._size - 1)
				{
					throw new IndexOutOfRangeException();
				}

				if (value < 0 || value > this._maxEntryValue)
				{
					throw new Exception("Value cannot be outside of accepted range.");
				}

				int bitIndex = index * this._bitsPerEntry;
				int startIndex = bitIndex / 64;
				int endIndex = ((index + 1) * this._bitsPerEntry - 1) / 64;
				int startBitSubIndex = bitIndex % 64;
				this._data[startIndex] = this._data[startIndex] & ~(this._maxEntryValue << startBitSubIndex) | ((long)value & this._maxEntryValue) << startBitSubIndex;
				if (startIndex != endIndex)
				{
					int endBitSubIndex = 64 - startBitSubIndex;
					this._data[endIndex] = this._data[endIndex] >> endBitSubIndex << endBitSubIndex | ((long)value & this._maxEntryValue) >> endBitSubIndex;
				}
			}
		}

		public long[] GetBackingLongArray()
		{
			return _data;
		}

		public int Size()
		{
			return _size;
		}


		private static int RoundToNearest(int value, int roundTo)
		{
			if (roundTo == 0)
			{
				return 0;
			}
			else if (value == 0)
			{
				return roundTo;
			}
			else
			{
				if (value < 0)
				{
					roundTo *= -1;
				}

				int remainder = value % roundTo;
				return remainder != 0 ? value + roundTo - remainder : value;
			}
		}
	}
}