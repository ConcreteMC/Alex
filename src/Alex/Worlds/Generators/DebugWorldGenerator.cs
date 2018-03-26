using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Blocks.State;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.State;
using Alex.Entities;
using Alex.Utils;
using fNbt;
using fNbt.Tags;
using Microsoft.Xna.Framework;
using MathF = System.MathF;


namespace Alex.Worlds.Generators
{
    public class DebugWorldGenerator : IWorldGenerator
    {
	    private static List<IBlockState> ALL_VALID_STATES = new List<IBlockState>();
	    private static IBlockState AIR = BlockFactory.GetBlockState(0);

		private static int GRID_WIDTH;
	    private static int GRID_HEIGHT;

		public IChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates)
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

				    IBlockState iblockstate = GetBlockStateFor(rx, rz);

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

			//chunk.CalculateHeight();
		    return chunk;
		}

	    public static IBlockState GetBlockStateFor(int x, int z)
	    {
		    IBlockState iblockstate = AIR;

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
		    return new Vector3(0, 75, 0);
	    }

	    public void Initialize()
	    {
		    
	    }

	    static DebugWorldGenerator()
	    {
		    foreach (var state in BlockFactory.AllBlockstatesByName.OrderBy(x => x.Key))
		    {
			    foreach (var variant in ((BlockState) state.Value).Variants)
			    {
					if (variant == state.Value) continue;
					ALL_VALID_STATES.Add(variant);
			    }
			    ALL_VALID_STATES.Add(state.Value);
			}

		    GRID_WIDTH = (int)MathF.Ceiling(MathF.Sqrt(ALL_VALID_STATES.Count));
		    GRID_HEIGHT = (int)MathF.Ceiling((float)ALL_VALID_STATES.Count / GRID_WIDTH);
		}
    }
}
