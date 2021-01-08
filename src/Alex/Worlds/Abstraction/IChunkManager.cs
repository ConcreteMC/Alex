using System.Collections.Generic;
using Alex.API.Utils;
using Alex.Worlds.Chunks;
using Microsoft.Xna.Framework;

namespace Alex.Worlds.Abstraction
{
	public interface IChunkManager
	{
		int     ChunkCount        { get; }
		int     RenderedChunks    { get; }
		bool    FogEnabled        { get; set; }
		Vector3 FogColor          { get; set; }
		float   FogDistance       { get; set; }
		Vector3 AmbientLightColor { get; set; }
	//	float   BrightnessModifier { get; set; }

		void Start();

		void AddChunk(ChunkColumn chunk, ChunkCoordinates position, bool doUpdates = false);

		void RemoveChunk(ChunkCoordinates position, bool dispose = true);

		bool TryGetChunk(ChunkCoordinates coordinates, out ChunkColumn chunk);

		KeyValuePair<ChunkCoordinates, ChunkColumn>[]GetAllChunks();

		void ClearChunks();
	}
}