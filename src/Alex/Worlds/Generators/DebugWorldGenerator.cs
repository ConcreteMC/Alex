using System;
using System.Collections.Generic;
using Alex.API.Blocks.State;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks;
using Alex.Blocks.State;
using Microsoft.Xna.Framework;
using MathF = System.MathF;


namespace Alex.Worlds.Generators
{
    public class DebugWorldGenerator : IWorldGenerator
    {
	    private static List<BlockState> ALL_VALID_STATES = new List<BlockState>();
	    private static BlockState AIR = BlockFactory.GetBlockState("minecraft:air");

		private static int GRID_WIDTH;
	    private static int GRID_HEIGHT;

		public ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates)
	    {
		    int cc = 0;

			ChunkColumn chunk = new ChunkColumn();
		    chunk.X = chunkCoordinates.X;
		    chunk.Z = chunkCoordinates.Z;

			for (int x = 0; x < 16; ++x)
		    {
			    for (int z = 0; z < 16; ++z)
			    {
				    int rx = chunkCoordinates.X * 16 + x;
				    int rz = chunkCoordinates.Z * 16 + z;

				    BlockState iblockstate = GetBlockStateFor(rx, rz);

				    if (iblockstate != null)
				    {
					    chunk.SetBlockState(x, 70, z, iblockstate);
					    chunk.Height[((z << 4) + (x))] = 70;
				    }

				    chunk.SetSkyLight(x, 70, z, 15);
				    chunk.SetSkyLight(x, 71, z, 15);
				    chunk.SetSkyLight(x, 69, z, 15);
				}
			}

			chunk.CalculateHeight();
		    return chunk;
		}

	    public static BlockState GetBlockStateFor(int x, int z)
	    {
		    BlockState iblockstate = AIR;

		    if (x > 0 && z > 0 && x % 2 != 0 && z % 2 != 0)
		    {
			    x = x / 2;
			    z = z / 2;

			    if (x <= GRID_WIDTH && z <= GRID_HEIGHT)
			    {
				    int i = Math.Abs(x * GRID_WIDTH + z);

				    if (i < ALL_VALID_STATES.Count)
				    {
					    iblockstate = ALL_VALID_STATES[i];
				    }
			    }
		    }

		    return iblockstate;
	    }

		public Vector3 GetSpawnPoint()
	    {
		    return new Vector3(_levelInfo.SpawnX, _levelInfo.SpawnY, _levelInfo.SpawnZ);
	    }

		private LevelInfo _levelInfo = new LevelInfo()
		{
			LevelName = "Internal Debug",
			AllowCommands = true,
			GeneratorVersion = 1,
			GeneratorName = "debug",
			Raining = false,
			Initialized = true,
			SpawnX = 0,
			SpawnY = 75,
			SpawnZ = 0,
			Hardcore = false,
			Thundering = false,
			ThunderTime = 0,
			DayTime = 1200,
			Time = 1200,
			MapFeatures = true,
			RainTime = 0
		};
	    public void Initialize()
	    {
		    
	    }

	    public LevelInfo GetInfo()
	    {
		    return _levelInfo;
	    }

	    static DebugWorldGenerator()
	    {
		    foreach (var state in BlockFactory.AllBlockstatesByName/*.OrderBy(x => x.Key)*/)
		    {
			    foreach (var variant in state.Value.GetVariants())
			    {
					if (variant == state.Value.GetDefaultState()) continue;
					ALL_VALID_STATES.Add(variant);
			    }
			    ALL_VALID_STATES.Add(state.Value.GetDefaultState());
			}

		    GRID_WIDTH = (int)MathF.Ceiling(MathF.Sqrt(ALL_VALID_STATES.Count));
		    GRID_HEIGHT = (int)MathF.Ceiling((float)ALL_VALID_STATES.Count / GRID_WIDTH);
		}
    }
}
