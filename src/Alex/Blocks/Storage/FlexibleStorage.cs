using System;
using System.Buffers;

namespace Alex.Blocks.Storage
{
	public class FlexibleStorage : IStorage
	{
		public long[] _data;
		private int _bitsPerEntry;
		private int _size;
		private uint _valueMask;

		public FlexibleStorage(int bitsPerEntry, int size) : this(
			bitsPerEntry, new long[RoundUp(size * bitsPerEntry, 64) / 64])
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
			this._valueMask = (uint)((1L << this._bitsPerEntry) - 1);
		}
		
		public uint this[int index]
		{
			get
			{
				if (index < 0 || index >= this._size)
				{
					throw new IndexOutOfRangeException();
				}


				int bitIndex = index * this._bitsPerEntry;
				int startIndex = bitIndex >> 6;
				int i1 = bitIndex & 0x3f;

				long value = (long)((ulong)_data[startIndex] >> i1);
				int i2 = i1 + _bitsPerEntry;
				// The value is divided over two long values
				if (i2 > 64) {
					value |= _data[++startIndex] << 64 - i1;
				}

				return (uint) (value & _valueMask);
			}
			set
			{
				if (index < 0 || index >= this._size)
				{
					throw new IndexOutOfRangeException($"{index} falls outside of our current range (0 - {this._size - 1}) (BPE: {_bitsPerEntry} | Size: {_data.Length})");
				}

				if (value > this._valueMask)
				{
					throw new Exception($"Value cannot be outside of accepted range: Value: {value} RangeLimit: {this._valueMask}  (BPE: {_bitsPerEntry} | Size: {_data.Length})");
				}

				int bitIndex = index * this._bitsPerEntry;
				int i0 = bitIndex >> 6;
				int i1 = bitIndex & 0x3f;

				_data[i0] = this._data[i0] & ~(this._valueMask << i1) | (value & _valueMask) << i1;
				int i2 = i1 + _bitsPerEntry;
				// The value is divided over two long values
				if (i2 > 64) {
					i0++;
					_data[i0] = _data[i0] & ~((1L << i2 - 64) - 1L) | value >> 64 - i1;
				}
			}
		}

		public int Length => _size - 1;
		private static int RoundUp(int value, int roundTo)
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
				return remainder == 0 ? value : value + roundTo - remainder;
			}
		}
	}
}