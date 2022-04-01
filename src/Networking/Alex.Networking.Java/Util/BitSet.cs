using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace Alex.Networking.Java.Util
{

	public class BitSet
	{
		private readonly long[] _data;

		/// <summary>
		///		The size of the backing storage
		/// </summary>
		public int Length => _data.Length;

		/// <summary>
		///		The total amount of bits available
		/// </summary>
		public int Count => _data.Length * 64;

		public BitSet(long[] data)
		{
			_data = data;
		}

		static int CountSetBits(long[] input)
		{
			int count = 0;

			for (int i = 0; i < input.Length; i++)
				count += CountSetBits(input[i]);

			return count;
		}

		static int CountSetBits(long n)
		{
			int count = 0;

			while (n > 0)
			{
				n &= (n - 1);
				count++;
			}

			return count;
		}

		public bool IsSet(int bit)
		{
			if ((bit / 64) >= _data.Length) return false;

			// bit >> 6
			return (_data[bit / 64] & (1L << (bit % 64))) != 0;
		}

		public static async Task<BitSet> ReadAsync(MinecraftStream ms)
		{
			var length = await ms.ReadVarIntAsync();
			long[] data = new long[length];

			for (int i = 0; i < data.Length; i++)
			{
				data[i] = await ms.ReadLongAsync();
			}

			return new BitSet(data);
		}
	}
}