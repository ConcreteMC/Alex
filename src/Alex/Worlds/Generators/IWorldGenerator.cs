using Alex.API.World;
using Microsoft.Xna.Framework;
using MiNET.Utils;

namespace Alex.Worlds.Generators
{
	public interface IWorldGenerator
	{
		IChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates);
		Vector3 GetSpawnPoint();

		void Initialize();
	}
}
