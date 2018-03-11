using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Blocks.State;
using Alex.API.World;
using Alex.Entities;
using Alex.Utils;
using fNbt;
using fNbt.Tags;
using Microsoft.Xna.Framework;
using MiNET.Utils;
using MiNET.Worlds;

namespace Alex.Worlds.Generators
{
    public class DebugWorldGenerator : IWorldGenerator
    {
	    private static List<IBlockState> ALL_VALID_STATES = new List<IBlockState>();
	    private static IBlockState AIR = BlockFactory.GetBlockState(0);
	    private static IBlockState BARRIER = BlockFactory.GetBlockState(7, 0);//.getDefaultState();
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
				    int k = chunkCoordinates.X * 16 + x;
				    int l = chunkCoordinates.Z * 16 + z;
				//    chunk.SetBlockState(x, 60, z, BARRIER);
				    IBlockState iblockstate = GetBlockStateFor(k, l);

				    if (iblockstate != null)
				    {
					    chunk.SetBlockState(x, 70, z, iblockstate);
					}

				    chunk.SetSkyLight(x, 70, z, 15);
				    chunk.SetSkyLight(x, 71, z, 15);
				    chunk.SetSkyLight(x, 69, z, 15);
				}
			}

			//chunk.generateSkylightMap();
			/*  Biome[] abiome = BiomeUtils.Biomes;//((Biome[])null, chunkCoordinates.X * 16, chunkCoordinates.Z * 16, 16, 16);
			  byte[] abyte = chunk.BiomeId;

			  for (int i1 = 0; i1 < abyte.length; ++i1)
			  {
				  abyte[i1] = (byte)BiomeUtils.Get(abiome[i1]);
			  }
			  */
			chunk.CalculateHeight();
			//chunk.CalculateSkylight();
		    return chunk;
		}

	    public static IBlockState GetBlockStateFor(int p_177461_0_, int p_177461_1_)
	    {
		    IBlockState iblockstate = AIR;

		    if (p_177461_0_ > 0 && p_177461_1_ > 0 && p_177461_0_ % 2 != 0 && p_177461_1_ % 2 != 0)
		    {
			    p_177461_0_ = p_177461_0_ / 2;
			    p_177461_1_ = p_177461_1_ / 2;

			    if (p_177461_0_ <= GRID_WIDTH && p_177461_1_ <= GRID_HEIGHT)
			    {
				    int i = (int) MathHelpers.Abs(p_177461_0_ * GRID_WIDTH + p_177461_1_);

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
		    foreach (var state in BlockFactory.AllBlockstates.OrderBy(x => x.Value.Name))
		    {
			    ALL_VALID_STATES.Add(state.Value);
			}

		    GRID_WIDTH = (int)MathHelpers.Ceiling(MathHelpers.Sqrt((float)ALL_VALID_STATES.Count));
		    GRID_HEIGHT = (int)MathHelpers.Ceiling((float)ALL_VALID_STATES.Count / (float)GRID_WIDTH);
		}
    }
}
