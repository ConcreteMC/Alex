using System;
using Alex.API.Blocks.State;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks;
using Alex.Rendering;
using Microsoft.Xna.Framework;


namespace Alex.Worlds
{
	public class CachedWorld : IWorld, IDisposable
	{
		public TickManager Ticker { get; }
		public long WorldTime { get; } = 0;

		public int Vertices => 0;

		public int ChunkCount => 0;

		public int ChunkUpdates => 0;

		internal ChunkManager ChunkManager { get; }
		public CachedWorld(Alex alex)
		{
			ChunkManager = new ChunkManager(alex, alex.GraphicsDevice, this);
			Ticker = new TickManager(this);
		}

		public byte GetSkyLight(Vector3 position)
		{
			return GetSkyLight(position.X, position.Y, position.Z);
		}

		public byte GetSkyLight(float x, float y, float z)
		{
			return GetSkyLight((int)Math.Floor(x), (int)Math.Floor(y), (int)Math.Floor(z)); // Fix. xd
		}

		public byte GetSkyLight(int x, int y, int z)
		{
			if (y < 0 || y > ChunkColumn.ChunkHeight) return 15;

			IChunkColumn chunk;
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
			{
				return chunk.GetSkylight(x & 0xf, y & 0xff, z & 0xf);
			}
			return 15;
		}

		public byte GetBlockLight(Vector3 position)
		{
			return GetBlockLight(position.X, position.Y, position.Z);
		}
		public byte GetBlockLight(float x, float y, float z)
		{
			return GetBlockLight((int)Math.Floor(x), (int)Math.Floor(y), (int)Math.Floor(z)); // Fix. xd
		}

		public byte GetBlockLight(int x, int y, int z)
		{
			if (y < 0 || y > ChunkColumn.ChunkHeight) return 15;

			IChunkColumn chunk;
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
			{
				return chunk.GetBlocklight(x & 0xf, y & 0xff, z & 0xf);
			}
			return 15;
		}

		public IBlock GetBlock(BlockCoordinates position)
		{
			return GetBlock(position.X, position.Y, position.Z);
		}

		public IBlock GetBlock(Vector3 position)
		{
			return GetBlock(position.X, position.Y, position.Z);
		}

		public IBlock GetBlock(float x, float y, float z)
		{
			return GetBlock((int)Math.Floor(x), (int)Math.Floor(y), (int)Math.Floor(z)); // Fix. xd
		}

		public IBlock GetBlock(int x, int y, int z)
		{
			IChunkColumn chunk;
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
			{
				return chunk.GetBlock(x & 0xf, y & 0xff, z & 0xf);
			}
			return BlockFactory.GetBlock(0);
		}

		public void SetBlock(float x, float y, float z, IBlock block)
		{
			SetBlock((int)x, (int)y, (int)z, block);
		}

		public void SetBlock(int x, int y, int z, IBlock block)
		{
			IChunkColumn chunk;
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
			{
				chunk.SetBlock(x & 0xf, y & 0xff, z & 0xf, block);
			}
		}

		public void SetBlockState(int x, int y, int z, IBlockState block)
		{
			IChunkColumn chunk;
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
			{
				chunk.SetBlockState(x & 0xf, y & 0xff, z & 0xf, block);
			}
		}

		public IBlockState GetBlockState(int x, int y, int z)
		{
			IChunkColumn chunk;
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
			{
				return chunk.GetBlockState(x & 0xf, y & 0xff, z & 0xf);
			}

			return new Air().GetDefaultState();
		}

		public int GetBiome(int x, int y, int z)
		{
			IChunkColumn chunk;
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
			{
				Worlds.ChunkColumn realColumn = (Worlds.ChunkColumn)chunk;
				return realColumn.GetBiome((int)x & 0xf, (int)z & 0xf);
			}

			return -1;
		}


		public void RebuildChunks()
		{
			
		}

		public void Render(IRenderArgs args)
		{
			
		}

		public void ResetChunks()
		{
			
		}

		public Vector3 GetSpawnPoint()
		{
			return Vector3.Zero;
		}

		public void Dispose()
		{
			ChunkManager.Dispose();
		}
	}
}
