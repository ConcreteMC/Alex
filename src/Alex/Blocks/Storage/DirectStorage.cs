namespace Alex.Blocks.Storage;

public class DirectStorage : IStorage
{
	private readonly uint _index;

	public DirectStorage(uint index)
	{
		_index = index;
	}
	
	/// <inheritdoc />
	public uint this[int index]
	{
		get => _index;
		set => throw new System.NotImplementedException();
	}

	/// <inheritdoc />
	public int Length => 1;
	
	/// <inheritdoc />
	public void Dispose()
	{
		
	}

}