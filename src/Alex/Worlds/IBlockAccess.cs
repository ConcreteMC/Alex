using System.Collections;
using System.Collections.Generic;
using Alex.API.Blocks.State;
using Alex.API.Utils;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;

namespace Alex.Worlds
{
    public interface IBlockAccess
    {
        ChunkColumn GetChunk(BlockCoordinates coordinates, bool cacheOnly = false);
        ChunkColumn GetChunk(ChunkCoordinates coordinates, bool cacheOnly = false);
        void SetSkyLight(BlockCoordinates coordinates, byte skyLight);
        byte GetSkyLight(BlockCoordinates coordinates);
        byte GetBlockLight(BlockCoordinates coordinates);
        
        int GetHeight(BlockCoordinates coordinates);
        Block GetBlock(BlockCoordinates coord, ChunkColumn tryChunk = null);
        void SetBlock(Block block, bool broadcast = true, bool applyPhysics = true, bool calculateLight = true, ChunkColumn possibleChunk = null);
        IEnumerable<(BlockState state, int storage)> GetBlockStates(int positionX, in int positionY, int positionZ);
        BlockState GetBlockState(BlockCoordinates position);
    }
}