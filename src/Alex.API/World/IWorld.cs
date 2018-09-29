using Alex.API.Blocks.State;
using Alex.API.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework;

namespace Alex.API.World
{
	public interface IWorld
	{
		TickManager Ticker { get; }
		LevelInfo WorldInfo { get; }
		int Vertices { get; }
		int ChunkCount { get; }
		int ChunkUpdates { get; }
		void ResetChunks();
		void RebuildChunks();
		void Render(IRenderArgs args);
		Vector3 GetSpawnPoint();
		byte GetSkyLight(Vector3 position);
		byte GetSkyLight(float x, float y, float z);
		byte GetSkyLight(int x, int y, int z);
		byte GetBlockLight(Vector3 position);
		byte GetBlockLight(float x, float y, float z);
		byte GetBlockLight(int x, int y, int z);
		IBlock GetBlock(BlockCoordinates position);
		IBlock GetBlock(Vector3 position);
		IBlock GetBlock(float x, float y, float z);
		IBlock GetBlock(int x, int y, int z);
		void SetBlock(float x, float y, float z, IBlock block);
		void SetBlock(int x, int y, int z, IBlock block);
		void SetBlockState(int x, int y, int z, IBlockState block);
		IBlockState GetBlockState(int x, int y, int z);
		int GetBiome(int x, int y, int z);
	}
}
