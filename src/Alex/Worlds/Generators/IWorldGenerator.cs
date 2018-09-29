using Alex.API.Utils;
using Alex.API.World;
using Microsoft.Xna.Framework;

namespace Alex.Worlds.Generators
{
	public interface IWorldGenerator
	{
		IChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates);
		Vector3 GetSpawnPoint();

		void Initialize();
		LevelInfo GetInfo();
	}
}
