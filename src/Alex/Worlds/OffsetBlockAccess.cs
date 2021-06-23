using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Alex.Blocks.State;
using Alex.Common.Utils.Vectors;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Chunks;

namespace Alex.Worlds
{
	public class OffsetBlockAccess : IBlockAccess
	{
		private BlockCoordinates _offset;
		private IBlockAccess _world;
		public OffsetBlockAccess(BlockCoordinates offset, IBlockAccess world)
		{
			_offset = offset;
			_world = world;
		}

		private BlockCoordinates GetOffset(BlockCoordinates coordinates)
		{
			return new BlockCoordinates(
				coordinates.X + _offset.X, coordinates.Y + _offset.Y, coordinates.Z + _offset.Z);
		}

		private ChunkCoordinates GetOffset(ChunkCoordinates coordinates)
		{
			return new ChunkCoordinates((coordinates.X >> 4) + _offset.X, (coordinates.Z >> 4) + _offset.Z);
			return coordinates + new ChunkCoordinates(_offset);
		}

		
		/// <inheritdoc />
		public ChunkColumn GetChunk(BlockCoordinates coordinates, bool cacheOnly = false)
		{
			return _world.GetChunk(GetOffset(coordinates), cacheOnly);
		}

		/// <inheritdoc />
		public ChunkColumn GetChunk(ChunkCoordinates coordinates, bool cacheOnly = false)
		{
			return _world.GetChunk(GetOffset(coordinates), cacheOnly);
		}

		/// <inheritdoc />
		public void SetSkyLight(BlockCoordinates coordinates, byte skyLight)
		{
			_world.SetSkyLight(GetOffset(coordinates), skyLight);
		}

		/// <inheritdoc />
		public byte GetSkyLight(BlockCoordinates coordinates)
		{
			return _world.GetSkyLight(GetOffset(coordinates));
		}

		/// <inheritdoc />
		public byte GetBlockLight(BlockCoordinates coordinates)
		{
			return _world.GetBlockLight(GetOffset(coordinates));
		}

		/// <inheritdoc />
		public void SetBlockLight(BlockCoordinates coordinates, byte blockLight)
		{
			_world.SetBlockLight(GetOffset(coordinates), blockLight);
		}

		/// <inheritdoc />
		public bool TryGetBlockLight(BlockCoordinates coordinates, out byte blockLight)
		{
			return _world.TryGetBlockLight(coordinates, out blockLight);
		}

		/// <inheritdoc />
		public void GetLight(BlockCoordinates coordinates, out byte blockLight, out byte skyLight)
		{
			_world.GetLight(GetOffset(coordinates), out blockLight, out skyLight);
		}

		/// <inheritdoc />
		public int GetHeight(BlockCoordinates coordinates)
		{
			return _world.GetHeight(GetOffset(coordinates));
		}

		/// <inheritdoc />
		public IEnumerable<ChunkSection.BlockEntry> GetBlockStates(int positionX, int positionY, int positionZ)
		{
			return _world.GetBlockStates(positionX + _offset.X, positionY + _offset.Y, positionZ + _offset.Z);
		}

		/// <inheritdoc />
		public BlockState GetBlockState(BlockCoordinates position)
		{
			return _world.GetBlockState(GetOffset(position));
		}

		/// <inheritdoc />
		public Biome GetBiome(BlockCoordinates coordinates)
		{
			return _world.GetBiome(GetOffset(coordinates));
		}
	}
}