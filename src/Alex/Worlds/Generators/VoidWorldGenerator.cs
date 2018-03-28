using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks;
using Microsoft.Xna.Framework;


namespace Alex.Worlds.Generators
{
	public class VoidWorldGenerator : IWorldGenerator
	{
		public IChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates)
		{
			IChunkColumn c = new ChunkColumn()
			{
				X = chunkCoordinates.X,
				Z = chunkCoordinates.Z
			};
			for (int x = 0; x < ChunkColumn.ChunkWidth; x++)
			{
				for (int z = 0; z < ChunkColumn.ChunkDepth; z++)
				{
					for (int y = 0; y < ChunkColumn.ChunkHeight; y++)
					{
						c.SetBlock(x,y,z, new Air());
						c.SetSkyLight(x,y,z, 15);
					}
					c.SetHeight(x,z, 0);
				}
			}
			return c;
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
