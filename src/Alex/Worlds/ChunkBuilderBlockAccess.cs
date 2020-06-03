using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Utils;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Blocks.Storage;
using Alex.Utils;
using Collections.Pooled;

namespace Alex.Worlds
{
	public class ChunkBuilderBlockAccess : PooledObject, IBlockAccess
	{
		private static BlockState AirState = BlockFactory.GetBlockState("minecraft:air");
		
		private IBlockAccess Target { get; }
	//	private PooledDictionary<ChunkCoordinates, ChunkColumn> _columns = new PooledDictionary<ChunkCoordinates, ChunkColumn>();
		private PooledDictionary<BlockCoordinates, ChunkSection.BlockEntry[]> _blockEntries = new PooledDictionary<BlockCoordinates, ChunkSection.BlockEntry[]>();
		private PooledDictionary<BlockCoordinates, byte> _skyLight = new PooledDictionary<BlockCoordinates, byte>();
		private PooledDictionary<BlockCoordinates, byte> _blockLight = new PooledDictionary<BlockCoordinates, byte>();
		
		public ChunkBuilderBlockAccess(IBlockAccess target)
		{
			Target = target;
		}
		
		/// <inheritdoc />
		public ChunkColumn GetChunk(BlockCoordinates coordinates, bool cacheOnly = false)
		{
			return Target.GetChunk(coordinates, cacheOnly);
			//	return _columns.GetOrAdd((ChunkCoordinates) coordinates, (cc) => Target.GetChunk(cc, cacheOnly));
		}

		/// <inheritdoc />
		public ChunkColumn GetChunk(ChunkCoordinates coordinates, bool cacheOnly = false)
		{
			return Target.GetChunk(coordinates, cacheOnly);
			//return _columns.GetOrAdd(coordinates, (c) => Target.GetChunk(c, cacheOnly));
		}

		/// <inheritdoc />
		public void SetSkyLight(BlockCoordinates coordinates, byte skyLight)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public byte GetSkyLight(BlockCoordinates coordinates)
		{
			return _skyLight.GetOrAdd(coordinates, (cc) => Target.GetSkyLight(cc));
		}

		/// <inheritdoc />
		public byte GetBlockLight(BlockCoordinates coordinates)
		{
			return _blockLight.GetOrAdd(coordinates, (cc) => Target.GetBlockLight(cc));
		}

		/// <inheritdoc />
		public int GetHeight(BlockCoordinates coordinates)
		{
			return Target.GetHeight(coordinates);
		}

		/// <inheritdoc />
		public IEnumerable<ChunkSection.BlockEntry> GetBlockStates(int positionX, in int positionY, int positionZ)
		{
			return _blockEntries.GetOrAdd(new BlockCoordinates(positionX, positionY, positionZ), (cc) => Target.GetBlockStates(cc.X, cc.Y, cc.Z).ToArray());
		}

		/// <inheritdoc />
		public BlockState GetBlockState(BlockCoordinates position)
		{
			var r = _blockEntries.GetOrAdd(position, (cc) => Target.GetBlockStates(cc.X, cc.Y, cc.Z).ToArray()).FirstOrDefault(x => x.Storage == 0)?.State;

			if (r != null)
			{
				return r;
			}

			return AirState;
		}

		/// <inheritdoc />
		protected override void Reset()
		{
		//	_columns.Clear();
			_blockEntries.Clear();
			_blockLight.Clear();
			_skyLight.Clear();
		}
	}
}