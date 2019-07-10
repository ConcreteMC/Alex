using Alex.API.Blocks.State;
using Alex.API.Utils;
using Alex.API.World;
using Microsoft.Xna.Framework;

namespace Alex.Worlds.Generators
{
    public class ChunkDebugWorldGenerator: IWorldGenerator
    {
        IBlockState _air = BlockFactory.GetBlockState("minecraft:air");
        IBlockState _dirt = BlockFactory.GetBlockState("minecraft:dirt");
        IBlockState _stone = BlockFactory.GetBlockState("minecraft:stone");
        IBlockState _cobble = BlockFactory.GetBlockState("minecraft:cobblestone");
        IBlockState _grass = BlockFactory.GetBlockState("minecraft:grass");
        
        public ChunkDebugWorldGenerator()
        {
            
        }
        
        public IChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates)
        {
            IChunkColumn c = new ChunkColumn()
            {
                X = chunkCoordinates.X,
                Z = chunkCoordinates.Z
            };
            
            for (int x = 0; x < ChunkColumn.ChunkWidth; x++)
            {
                for (int z = 0; z < ChunkColumn.ChunkDepth; z++)
                {
                    for (int y = 0; y < ChunkColumn.ChunkHeight; y++)
                    {
                        if (c.X == 0 && c.Z == 0)
                        {
                            IBlockState block;
                            switch (y >> 4)
                            {
                                case 0:
                                    block = _dirt;
                                    break;
                                case 1:
                                    block = _stone;
                                    break;
                                case 2:
                                    block = _cobble;
                                    break;
                                case 3:
                                    block = _grass;
                                    break;
                                //case 4:
                                //    break;
                                default:
                                    continue;
                            }
                            
                            c.SetBlockState(x, y, z, block);
                        }
                        else
                        {
                            //c.SetBlockState(x, y, z, _air);
                        }

                        c.SetSkyLight(x,y,z, 15);
                    }
                    c.SetHeight(x,z, 0);
                }
            }
            return c;
        }

        public Vector3 GetSpawnPoint()
        {
            return new Vector3(0, 4 * 16, 0);
        }

        public LevelInfo GetInfo()
        {
            return new LevelInfo();
        }
        public void Initialize()
        {
			
        }
    }
}