using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Alex.Blocks.State;
using Alex.Common.Utils.Vectors;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Chunks;

namespace Alex.Worlds;

public class CachedBlockAccess : IBlockAccess, IDisposable
{
	private readonly IBlockAccess _source;

	private record CachedEntry(BlockState BlockState, byte SkyLight, byte BlockLight, Biome Biome);

	private ConcurrentDictionary<BlockCoordinates, CachedEntry> _cached =
		new ConcurrentDictionary<BlockCoordinates, CachedEntry>();

	public CachedBlockAccess(IBlockAccess source)
	{
		_source = source;
	}

	/// <inheritdoc />
	public ChunkColumn GetChunk(BlockCoordinates coordinates, bool cacheOnly = false)
	{
		return _source.GetChunk(coordinates, cacheOnly);
	}

	/// <inheritdoc />
	public ChunkColumn GetChunk(ChunkCoordinates coordinates, bool cacheOnly = false)
	{
		return _source.GetChunk(coordinates, cacheOnly);
	}

	/// <inheritdoc />
	public void SetSkyLight(BlockCoordinates coordinates, byte skyLight)
	{
		_source.SetSkyLight(coordinates, skyLight);
	}

	/// <inheritdoc />
	public byte GetSkyLight(BlockCoordinates coordinates)
	{
		return GetCachedEntry(coordinates).SkyLight;
	}

	/// <inheritdoc />
	public byte GetBlockLight(BlockCoordinates coordinates)
	{
		return GetCachedEntry(coordinates).BlockLight;
	}

	/// <inheritdoc />
	public void SetBlockLight(BlockCoordinates coordinates, byte blockLight)
	{
		_source.SetBlockLight(coordinates, blockLight);
	}

	/// <inheritdoc />
	public bool TryGetBlockLight(BlockCoordinates coordinates, out byte blockLight)
	{
		blockLight = GetCachedEntry(coordinates).BlockLight;

		return true;
	}

	/// <inheritdoc />
	public void GetLight(BlockCoordinates coordinates, out byte blockLight, out byte skyLight)
	{
		var entry = GetCachedEntry(coordinates);
		blockLight = entry.BlockLight;
		skyLight = entry.SkyLight;
	}

	/// <inheritdoc />
	public int GetHeight(BlockCoordinates coordinates)
	{
		return _source.GetHeight(coordinates);
	}

	/// <inheritdoc />
	public IEnumerable<ChunkSection.BlockEntry> GetBlockStates(int positionX, int positionY, int positionZ)
	{
		return _source.GetBlockStates(positionX, positionY, positionZ);
	}

	/// <inheritdoc />
	public BlockState GetBlockState(BlockCoordinates position)
	{
		return GetCachedEntry(position).BlockState;
	}

	/// <inheritdoc />
	public void SetBlockState(int x,
		int y,
		int z,
		BlockState block,
		int storage,
		BlockUpdatePriority priority = BlockUpdatePriority.High)
	{
		_source.SetBlockState(x, y, z, block, storage, priority);
	}

	/// <inheritdoc />
	public Biome GetBiome(BlockCoordinates coordinates)
	{
		return GetCachedEntry(coordinates).Biome;
	}

	private CachedEntry GetCachedEntry(BlockCoordinates coordinates)
	{
		return _cached.GetOrAdd(
			coordinates, v =>
			{
				_source.GetLight(v, out var blockLight, out var skyLight);

				return new CachedEntry(_source.GetBlockState(v), skyLight, blockLight, _source.GetBiome(v));
			});
	}

	/// <inheritdoc />
	public void Dispose()
	{
		_cached.Clear();
	}
}