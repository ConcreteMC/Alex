using Alex.CoreRT.API.World;
using Microsoft.Xna.Framework;
using MiNET.Utils;

namespace Alex.CoreRT.Worlds.Generators
{
	public interface IWorldGenerator
	{
		IChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates);
		Vector3 GetSpawnPoint();

		void Initialize();
	}
}
