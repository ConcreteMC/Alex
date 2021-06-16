using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alex.Blocks;
using Alex.Blocks.State;
using Alex.Common.Utils.Vectors;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Chunks;
using Microsoft.Xna.Framework;
using MathF = System.MathF;


namespace Alex.Worlds.Singleplayer.Generators
{
    public class DebugWorldGenerator : IWorldGenerator
    {
	    private List<BlockState> ALL_VALID_STATES = new List<BlockState>();
	   // private BlockState AIR = BlockFactory.GetBlockState("minecraft:air");

		private readonly int GRID_WIDTH;
	    private readonly int GRID_HEIGHT;

	    public DebugWorldGenerator()
	    {
		    foreach (var state in BlockFactory.AllBlockstates)
		    {
			    ALL_VALID_STATES.Add(state.Value);
		    }

		    GRID_WIDTH = (int)MathF.Ceiling(MathF.Sqrt(ALL_VALID_STATES.Count));
		    GRID_HEIGHT = (int)MathF.Ceiling((float)ALL_VALID_STATES.Count / GRID_WIDTH);
	    }
	    
	    private ConcurrentDictionary<ChunkCoordinates, ChunkColumn> _cache = new ConcurrentDictionary<ChunkCoordinates, ChunkColumn>();
	    
		public ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates)
		{
			return _cache.GetOrAdd(
				chunkCoordinates, (cc) =>
				{
					ChunkColumn chunk = new ChunkColumn(cc.X, cc.Z);
					for (int x = 0; x < 16; x++)
					{
						int rx = (cc.X * 16) + x;
						for (int z = 0; z < 16; z++)
						{
							int rz = (cc.Z * 16) + z;

							BlockState iblockstate = GetBlockStateFor(rx, rz);

							if (iblockstate != null && iblockstate.Block.Renderable)
							{
								chunk.SetBlockState(x, 1, z, iblockstate);
								//chunk.Height[((z << 4) + (x))] = 70;
							}

							chunk.SetSkyLight(x, 2, z, 15);
							//  chunk.SetSkyLight(x, 71, z, 15);
							//  chunk.SetSkyLight(x, 69, z, 15);
						}
					}
					
					//chunk.CalculateHeight();
					return chunk;
				});
		}

	    private BlockState GetBlockStateFor(int x, int z)
	    {
		    if (x > 0 && z > 0 && x % 2 != 0 && z % 2 != 0)
		    {
			    x = x / 2;
			    z = z / 2;

			    if (x <= GRID_WIDTH && z <= GRID_HEIGHT)
			    {
				    int i = Math.Abs(x * GRID_WIDTH + z);

				    if (i < ALL_VALID_STATES.Count)
				    {
					    return ALL_VALID_STATES[i];
				    }
			    }
		    }

		    return null;
	    }

		public Vector3 GetSpawnPoint()
	    {
		    return new Vector3(0, 1 ,0);
	    }

	    public void Initialize()
	    {
		    
	    }
    }
}
