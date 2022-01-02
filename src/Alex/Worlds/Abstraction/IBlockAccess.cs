using System.Collections.Generic;
using Alex.Blocks.State;
using Alex.Common.Utils.Vectors;
using Alex.Worlds.Chunks;

namespace Alex.Worlds.Abstraction
{
    public interface IBlockAccess
    {
        ChunkColumn GetChunk(BlockCoordinates coordinates, bool cacheOnly = false);
        ChunkColumn GetChunk(ChunkCoordinates coordinates, bool cacheOnly = false);
        void SetSkyLight(BlockCoordinates coordinates, byte skyLight);
        byte GetSkyLight(BlockCoordinates coordinates);
        byte GetBlockLight(BlockCoordinates coordinates);
        void SetBlockLight(BlockCoordinates coordinates, byte blockLight);
        bool TryGetBlockLight(BlockCoordinates coordinates, out byte blockLight);
        
        void GetLight(BlockCoordinates coordinates, out byte blockLight, out byte skyLight);
        
        int GetHeight(BlockCoordinates coordinates);
        IEnumerable<ChunkSection.BlockEntry> GetBlockStates(int positionX, int positionY, int positionZ);
        BlockState GetBlockState(BlockCoordinates position);

        void SetBlockState(int x,
	        int y,
	        int z,
	        BlockState block,
	        int storage,
	        BlockUpdatePriority priority = BlockUpdatePriority.High | BlockUpdatePriority.Neighbors);

        Biome GetBiome(BlockCoordinates coordinates);

        BlockState GetBlockState(int x, int y, int z) => GetBlockState(new BlockCoordinates(x, y, z));
    }
}