using Alex.API.Utils;
using Alex.API.World;
using Alex.Worlds.Chunks;
using Microsoft.Xna.Framework;

namespace Alex.Worlds.Abstraction
{
	public interface IWorldGenerator
	{
		ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates);
		Vector3 GetSpawnPoint();

		void Initialize();
		LevelInfo GetInfo();
	}
}
