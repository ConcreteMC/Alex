namespace Alex.Blocks.Storage.Palette;

public class SinglePalette<TValue> : IPalette<TValue> where TValue : class, IHasKey
{
	private readonly TValue _value;

	public SinglePalette(TValue value)
	{
		_value = value;
	}

	public uint GetId(TValue state)
	{
		return state.Id;
	}

	public uint Add(TValue state)
	{
		throw new System.NotImplementedException();
	}

	public TValue Get(uint id)
	{
		return _value; // BlockFactory.GetBlockState(id);
	}

	public void Put(TValue objectIn, uint intKey) { }

	/// <inheritdoc />
	public void Dispose() { }
}