using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Alex.Blocks.State;
using Alex.Common.Utils.Vectors;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Chunks;

namespace Alex.Worlds;

public class CollisionBlockAccess : IBlockAccess, IDisposable
{
	private readonly IBlockAccess _source;

	private ConcurrentDictionary<BlockCoordinates, BlockState> _cached = new ConcurrentDictionary<BlockCoordinates, BlockState>();
	public CollisionBlockAccess(IBlockAccess source)
	{
		_source = source;
	}

	/// <inheritdoc />
	public ChunkColumn GetChunk(BlockCoordinates coordinates, bool cacheOnly = false)
	{
		throw new System.NotImplementedException();
	}

	/// <inheritdoc />
	public ChunkColumn GetChunk(ChunkCoordinates coordinates, bool cacheOnly = false)
	{
		throw new System.NotImplementedException();
	}

	/// <inheritdoc />
	public void SetSkyLight(BlockCoordinates coordinates, byte skyLight)
	{
		throw new System.NotImplementedException();
	}

	/// <inheritdoc />
	public byte GetSkyLight(BlockCoordinates coordinates)
	{
		throw new System.NotImplementedException();
	}

	/// <inheritdoc />
	public byte GetBlockLight(BlockCoordinates coordinates)
	{
		throw new System.NotImplementedException();
	}

	/// <inheritdoc />
	public void SetBlockLight(BlockCoordinates coordinates, byte blockLight)
	{
		throw new System.NotImplementedException();
	}

	/// <inheritdoc />
	public bool TryGetBlockLight(BlockCoordinates coordinates, out byte blockLight)
	{
		throw new System.NotImplementedException();
	}

	/// <inheritdoc />
	public void GetLight(BlockCoordinates coordinates, out byte blockLight, out byte skyLight)
	{
		throw new System.NotImplementedException();
	}

	/// <inheritdoc />
	public int GetHeight(BlockCoordinates coordinates)
	{
		throw new System.NotImplementedException();
	}

	/// <inheritdoc />
	public IEnumerable<ChunkSection.BlockEntry> GetBlockStates(int positionX, int positionY, int positionZ)
	{
		throw new System.NotImplementedException();
	}

	/// <inheritdoc />
	public BlockState GetBlockState(BlockCoordinates position)
	{
		return _cached.GetOrAdd(position, v => _source.GetBlockState(v));
	}

	/// <inheritdoc />
	public void SetBlockState(int x,
		int y,
		int z,
		BlockState block,
		int storage,
		BlockUpdatePriority priority = BlockUpdatePriority.High)
	{
		throw new System.NotImplementedException();
	}

	/// <inheritdoc />
	public Biome GetBiome(BlockCoordinates coordinates)
	{
		throw new System.NotImplementedException();
	}

	/// <inheritdoc />
	public void Dispose()
	{
		
	}
}