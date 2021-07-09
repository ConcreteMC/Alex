using Alex.Common.Utils;
using Alex.Worlds.Chunks;

namespace Alex.Worlds.Lighting
{
	public class LightArray
	{
		private NibbleArray Data { get; }

		public LightArray()
		{
			Data = new NibbleArray(new byte[2048]);
		}
		
		public LightArray(byte[] data)
		{
			Data = new NibbleArray(data);
		}

		public byte Get(int x, int y, int z)
		{
			return Data[ChunkSection.GetCoordinateIndex(x, y, z)];
		}
		
		public void Set(int x, int y, int z, byte value)
		{
			Data[ChunkSection.GetCoordinateIndex(x, y, z)] = value;
		}

		public byte this[int index]
		{
			get
			{
				return Data[index];
			}
			set
			{
				Data[index] = value;
			}
		}

		public byte this[int x, int y, int z]
		{
			get
			{
				return Get(x, y, z);
			}
			set
			{
				Set(x, y, z, value);
			}
		}

		public void Reset(byte value)
		{
			MiNET.Worlds.ChunkColumn.Fill<byte>(Data.Data, value);
		}
	}
}