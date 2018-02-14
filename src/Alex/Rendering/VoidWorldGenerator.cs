using System;
using Microsoft.Xna.Framework;
using MiNET.Utils;

namespace Alex.Rendering
{
	public class VoidWorldGenerator : IWorldGenerator
	{
		public Chunk GenerateChunkColumn(ChunkCoordinates chunkCoordinates)
		{
			Chunk c = new Chunk(chunkCoordinates.X, 0, chunkCoordinates.Z);
			for (int x = 0; x < Chunk.ChunkWidth; x++)
			{
				for (int y = 0; y < Chunk.ChunkHeight; y++)
				{
					for (int z = 0; z < Chunk.ChunkDepth; z++)
					{
						c.SetSkylight(x,y,z, 15);
					}
				}
			}
			return c;
		}

		public Vector3 GetSpawnPoint()
		{
			return Vector3.Zero;
		}

		public void Initialize()
		{
			
		}
	}
}
