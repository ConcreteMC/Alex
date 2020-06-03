using System;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks;
using Alex.Blocks.State;
using Microsoft.Xna.Framework;
using MiNET.Utils;
using ChunkCoordinates = Alex.API.Utils.ChunkCoordinates;
using IBlockState = Alex.API.Blocks.State.IBlockState;

namespace Alex.Worlds.Generators
{
    public class DemoGenerator : IWorldGenerator
    {
        private OpenSimplexNoise SimplexNoise { get; }
        
        private FastRandom Random { get; }
        private bool IsNether { get; }
        
        private BlockState MainBlock { get; }
        private BlockState LiquidBlock { get; }
        public DemoGenerator()
        {
            Random = new FastRandom();
            SimplexNoise = new OpenSimplexNoise(Random.Next());

            IsNether = Random.NextBool();

            if (IsNether)
            {
                MainBlock = BlockFactory.GetBlockState("minecraft:netherrack");
                LiquidBlock = BlockFactory.GetBlockState("minecraft:lava");
            }
            else
            {
                MainBlock = BlockFactory.GetBlockState("minecraft:dirt");
                LiquidBlock = BlockFactory.GetBlockState("minecraft:water");
            }
        }

        private const int Height = 16;
        public ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates)
        {
          //  IBlockState stone = BlockFactory.GetBlockState("minecraft:stone");
          //  IBlockState grass = BlockFactory.GetBlockState("minecraft:grass");
            
            
            ChunkColumn column = new ChunkColumn();
            for (int x = 0; x < 16; x++)
            {
                var rx = (chunkCoordinates.X * 16) + x;
                for (int z = 0; z < 16; z++)
                {
                    var rz = (chunkCoordinates.Z * 16) + z;

                    int height = GetHeight(rx, rz);

                    bool placeWater = false;
                    //if (height <= 3)
                    {
                        //placeWater = SimplexNoise.Evaluate(rz / 8f, rx / 8f) > 0f;
                        //height = 3;
                    }
                    
                    for (int y = 0; y < Height; y++)
                    {
                        if (y > height - 1 && y <= 5)
                        {
                            column.SetBlockState(x,y,z, LiquidBlock);
                        }
                        else if (y < height)
                        {
                            column.SetBlockState(x, y, z, MainBlock);
                        }
                    }
                }
            }

            column.X = chunkCoordinates.X;
            column.Z = chunkCoordinates.Z;

            return column;
        }

        private int GetHeight(int x, int z)
        {
            var noise = Math.Abs(SimplexNoise.Evaluate(x / 256f, z / 256f));
            return 1 + (int) Math.Ceiling(noise * Height);
        }
        
        public Vector3 GetSpawnPoint()
        {
            return new Vector3(0, GetHeight(0, 0) + 2, 0);
        }

        public void Initialize()
        {
            
        }

        public LevelInfo GetInfo()
        {
            return new LevelInfo()
            {
                Time = 12000,
                Initialized = true,
                LevelName = "Demo",
                SpawnX = 0,
                SpawnY = GetHeight(0, 0) + 2,
                SpawnZ = 0
            };
        }
    }
}