using System.Collections.Generic;
using Alex.API.Blocks.State;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.World;
using Microsoft.Xna.Framework;

namespace Alex.Worlds
{
    public class ItemRenderingWorld : IWorld
    {
        private static IBlockState Air { get; } = BlockFactory.GetBlockState("minecraft:air");

        private IBlock Block { get; }
        public ItemRenderingWorld(IBlock block)
        {
            Block = block;
        }
        
        public TickManager Ticker { get; }
        public LevelInfo WorldInfo { get; }
        public long Vertices { get; }
        public int ChunkCount { get; }
        public int ConcurrentChunkUpdates { get; }
        public void ResetChunks()
        {
            throw new System.NotImplementedException();
        }

        public void RebuildChunks()
        {
            throw new System.NotImplementedException();
        }

        public void Render(IRenderArgs args)
        {
            throw new System.NotImplementedException();
        }

        public Vector3 GetSpawnPoint()
        {
            throw new System.NotImplementedException();
        }

        public byte GetSkyLight(Vector3 position)
        {
            return 0xf;
        }

        public byte GetSkyLight(float x, float y, float z)
        {
            return 0xf;
        }

        public byte GetSkyLight(int x, int y, int z)
        {
            return 0xf;
        }

        public byte GetBlockLight(Vector3 position)
        {
            return 0;
        }

        public byte GetBlockLight(float x, float y, float z)
        {
            return 0;
        }

        public byte GetBlockLight(int x, int y, int z)
        {
            return 0;
        }

        public IBlock GetBlock(BlockCoordinates position)
        {
            return Air.Block;
        }

        public IBlock GetBlock(Vector3 position)
        {
            return Air.Block;
        }

        public IBlock GetBlock(float x, float y, float z)
        {
            return Air.Block;
        }

        public IBlock GetBlock(int x, int y, int z)
        {
            return Air.Block;
        }

        public void SetBlock(float x, float y, float z, IBlock block)
        {
            throw new System.NotImplementedException();
        }

        public void SetBlock(int x, int y, int z, IBlock block)
        {
            throw new System.NotImplementedException();
        }

        public void SetBlockState(int x, int y, int z, IBlockState block)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<(IBlockState state, int storage)> GetBlockStates(int x, int y, int z)
        {
            yield return (Air, 0);
        }

        public IBlockState GetBlockState(int x, int y, int z)
        {
            return Air;
        }

        public IBlockState GetBlockState(int x, int y, int z, int storage)
        {
            return Air;
        }

        public IBlockState GetBlockState(BlockCoordinates coordinates)
        {
            return Air;
        }

        public int GetBiome(int x, int y, int z)
        {
            return 0;
        }

        public bool HasBlock(int x, int y, int z)
        {
            return true;
        }

        public BlockCoordinates FindBlockPosition(BlockCoordinates coords, out IChunkColumn chunk)
        {
            throw new System.NotImplementedException();
        }

        public IChunkColumn GetChunkColumn(int x, int z)
        {
            throw new System.NotImplementedException();
        }

        public bool IsTransparent(int posX, int posY, int posZ)
        {
            return !Block.Transparent;
        }

        public bool IsSolid(int posX, int posY, int posZ)
        {
            return !Block.Solid;
        }

        public bool IsScheduled(int posX, int posY, int posZ)
        {
            return false;
        }

        public void GetBlockData(int posX, int posY, int posZ, out bool transparent, out bool solid)
        {
            transparent = !Block.Transparent;
            solid = !Block.Solid;
        }
    }
}