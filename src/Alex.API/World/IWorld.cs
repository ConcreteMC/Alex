using System.Collections.Generic;
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
		long Vertices { get; }

        int ChunkCount { get; }
		int ConcurrentChunkUpdates { get; }
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

		void SetBlockState(int x, int y, int z, IBlockState block);

		IEnumerable<(IBlockState state, int storage)> GetBlockStates(int x, int y, int z);
		IBlockState GetBlockState(int x, int y, int z);
		IBlockState GetBlockState(int x, int y, int z, int storage);
		IBlockState GetBlockState(BlockCoordinates coordinates);
		int GetBiome(int x, int y, int z);
		bool HasBlock(int x, int y, int z);
		BlockCoordinates FindBlockPosition(BlockCoordinates coords, out IChunkColumn chunk);
		IChunkColumn GetChunkColumn(int x, int z);

		bool IsTransparent(int posX, int posY, int posZ);
		bool IsSolid(int posX, int posY, int posZ);
	    bool IsScheduled(int posX, int posY, int posZ);
        void GetBlockData(int posX, int posY, int posZ, out bool transparent, out bool solid);
    }
}
