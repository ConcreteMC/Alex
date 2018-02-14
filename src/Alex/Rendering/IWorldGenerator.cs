using Microsoft.Xna.Framework;
using MiNET.Utils;

namespace Alex.Rendering
{
	public interface IWorldGenerator
	{
		Chunk GenerateChunkColumn(ChunkCoordinates chunkCoordinates);
		Vector3 GetSpawnPoint();

		void Initialize();
	}
}
