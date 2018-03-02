using Alex.API.Blocks.State;
using Microsoft.Xna.Framework;

namespace Alex.API.World
{
	public interface IWorld
	{
		int Vertices { get; }
		int ChunkCount { get; }
		int ChunkUpdates { get; }
		void ResetChunks();
		void RebuildChunks();
		void Render();
		Vector3 GetSpawnPoint();
		bool IsSolid(Vector3 location);
		bool IsSolid(float x, float y, float z);
		bool IsTransparent(Vector3 location);
		bool IsTransparent(float x, float y, float z);
		byte GetSkyLight(Vector3 position);
		byte GetSkyLight(float x, float y, float z);
		byte GetSkyLight(int x, int y, int z);
		byte GetBlockLight(Vector3 position);
		byte GetBlockLight(float x, float y, float z);
		byte GetBlockLight(int x, int y, int z);
		IBlock GetBlock(Vector3 position);
		IBlock GetBlock(float x, float y, float z);
		IBlock GetBlock(int x, int y, int z);
		void SetBlock(float x, float y, float z, IBlock block);
		void SetBlock(int x, int y, int z, IBlock block);

		void SetBlockState(float x, float y, float z, IBlockState blockState);
		IBlockState GetBlockState(float x, float y, float z);
	}
}
