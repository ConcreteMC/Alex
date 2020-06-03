using System;
using System.Collections.Generic;
using Alex.API.Utils;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Chunks;

namespace Alex.Worlds.Lighting
{
	public class SkyLightBlockAccess : IBlockAccess
	{
		private readonly ChunkManager     _worldProvider;
		private readonly int              _heightForUnloadedChunk;
		private readonly ChunkCoordinates _coord = ChunkCoordinates.None;
		private readonly ChunkColumn      _chunk = null;

		public SkyLightBlockAccess(ChunkManager worldProvider, int heightForUnloadedChunk = 255)
		{
			_worldProvider = worldProvider;
			_heightForUnloadedChunk = heightForUnloadedChunk;
		}

		public SkyLightBlockAccess(ChunkManager worldProvider, ChunkColumn chunk) : this(worldProvider, -1)
		{
			_chunk = chunk;
			_coord = new ChunkCoordinates(chunk.X, chunk.Z);
		}

		public ChunkColumn GetChunk(BlockCoordinates coordinates, bool cacheOnly = false)
		{
			return GetChunk((ChunkCoordinates) coordinates, cacheOnly);
		}

		public ChunkColumn GetChunk(ChunkCoordinates coordinates, bool cacheOnly = false)
		{
			if (coordinates == _coord) return _chunk;

			if (_coord != ChunkCoordinates.None)
				if (coordinates != _coord)
					if (coordinates != _coord + ChunkCoordinates.Backward)
						if (coordinates != _coord + ChunkCoordinates.Forward)
							if (coordinates != _coord + ChunkCoordinates.Left)
								if (coordinates != _coord + ChunkCoordinates.Right)
									if (coordinates != _coord + ChunkCoordinates.Backward + ChunkCoordinates.Left)
										if (coordinates != _coord + ChunkCoordinates.Backward + ChunkCoordinates.Right)
											if (coordinates != _coord + ChunkCoordinates.Forward + ChunkCoordinates.Left)
												if (coordinates != _coord + ChunkCoordinates.Forward + ChunkCoordinates.Right)
													return null;

			if (_worldProvider.TryGetChunk(coordinates, out ChunkColumn column))
			{
				return (ChunkColumn) column;
			}

			return null;
			//return _worldProvider.GenerateChunkColumn(coordinates, true);
		}

		public void SetSkyLight(BlockCoordinates coordinates, byte skyLight)
		{
			ChunkColumn chunk = GetChunk(coordinates, true);
			chunk?.SetSkyLight(coordinates.X & 0x0f, coordinates.Y & 0xff, coordinates.Z & 0x0f, skyLight);
		}

		public byte GetSkyLight(BlockCoordinates coordinates)
		{
			return 15;
		}

		public byte GetBlockLight(BlockCoordinates coordinates)
		{
			return 0;
		}

		public int GetHeight(BlockCoordinates coordinates)
		{
			ChunkColumn chunk = GetChunk(coordinates, true);
			if (chunk == null) return _heightForUnloadedChunk;

			return chunk.GetHeight(coordinates.X & 0x0f, coordinates.Z & 0x0f);
		}

		public Block GetBlock(BlockCoordinates coord, ChunkColumn tryChunk = null)
		{
			return null;
		}

		public void SetBlock(int x, int y, int z, int blockId, int metadata = 0, bool broadcast = true, bool applyPhysics = true, bool calculateLight = true)
		{
		}

		public void SetBlock(Block block, bool broadcast = true, bool applyPhysics = true, bool calculateLight = true, ChunkColumn possibleChunk = null)
		{
		}

		public IEnumerable<ChunkSection.BlockEntry> GetBlockStates(int positionX, in int positionY, int positionZ)
		{
			throw new NotImplementedException();
		}

		public BlockState GetBlockState(BlockCoordinates position)
		{
			return null;
			//return air
		}
	}
}