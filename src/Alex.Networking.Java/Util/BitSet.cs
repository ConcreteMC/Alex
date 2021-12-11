using System.Threading.Tasks;

namespace Alex.Networking.Java.Util;

public class BitSet
{
	private readonly long[] _data;
	public BitSet(long[] data)
	{
		_data = data;
	}

	public bool IsSet(int bit)
	{
		return (_data[bit / 64] & (1L << (bit % 64))) != 0;
	}

	public void Set(int bit, bool value)
	{
		
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