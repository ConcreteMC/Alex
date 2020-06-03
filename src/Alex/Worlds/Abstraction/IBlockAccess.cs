using System.Collections.Generic;
using Alex.API.Utils;
using Alex.Blocks.State;
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
        
        int GetHeight(BlockCoordinates coordinates);
        //Block GetBlock(BlockCoordinates coord, ChunkColumn tryChunk = null);
       // void SetBlock(Block block, bool broadcast = true, bool applyPhysics = true, bool calculateLight = true, ChunkColumn possibleChunk = null);
        IEnumerable<ChunkSection.BlockEntry> GetBlockStates(int positionX, in int positionY, int positionZ);
        BlockState GetBlockState(BlockCoordinates position);
    }
}