using Alex.API.Utils;
using Alex.Blocks.Minecraft;

namespace Alex.Worlds
{
    public interface IBlockAccess
    {
        ChunkColumn GetChunk(BlockCoordinates coordinates, bool cacheOnly = false);
        ChunkColumn GetChunk(ChunkCoordinates coordinates, bool cacheOnly = false);
        void SetSkyLight(BlockCoordinates coordinates, byte skyLight);
        int GetHeight(BlockCoordinates coordinates);
        Block GetBlock(BlockCoordinates coord, ChunkColumn tryChunk = null);
        void SetBlock(Block block, bool broadcast = true, bool applyPhysics = true, bool calculateLight = true, ChunkColumn possibleChunk = null);
    }
}