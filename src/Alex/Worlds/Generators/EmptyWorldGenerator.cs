using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.Minecraft;
using Microsoft.Xna.Framework;

namespace Alex.Worlds.Generators
{
	public class EmptyWorldGenerator : IWorldGenerator
	{
		private ChunkColumn _sharedChunk;
		public EmptyWorldGenerator()
		{
			_sharedChunk = new ChunkColumn();
			_sharedChunk.IsAllAir = true;
			for (int x = 0; x < ChunkColumn.ChunkWidth; x++)
			{
				for (int z = 0; z < ChunkColumn.ChunkDepth; z++)
				{
					for (int y = 0; y < ChunkColumn.ChunkHeight; y++)
					{
						_sharedChunk.SetBlock(x, y, z, new Air());
						_sharedChunk.SetSkyLight(x, y, z, 15);
					}

					_sharedChunk.SetHeight(x, z, 0);
				}
			}
		}

		public IChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates)
		{
			return _sharedChunk;
		}

		public Vector3 GetSpawnPoint()
		{
			return Vector3.Zero;
		}

		public LevelInfo GetInfo()
		{
			return new LevelInfo();
		}

		public void Initialize()
		{

		}
	}
}
