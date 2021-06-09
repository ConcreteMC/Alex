using System;

namespace Alex.Common.Utils
{
	public class NibbleArray : ICloneable
	{
		public readonly byte[] Data;

		public NibbleArray()
		{
		}

		public NibbleArray(byte[] data)
		{
			Data = data;
		}

		public NibbleArray(int length, byte initialValue = 0)
		{
			Data = new byte[length / 2];
			for (int i = 0; i < Data.Length; i++)
			{
				Data[i] = initialValue;
			}
		}

		public int Length
		{
			get { return Data.Length * 2; }
		}

		public byte this[int index]
		{
			get { return (byte)(Data[index >> 1] >> ((index & 1) * 4) & 0xF); }
			set
			{
				value &= 0xF;
				var idx = index >> 1;
				Data[idx] &= (byte)(0xF << (((index + 1) & 1) * 4));
				Data[idx] |= (byte)(value << ((index & 1) * 4));
			}
		}

		public object Clone()
		{
			return new NibbleArray((byte[]) Data.Clone());
		}
	}
}
