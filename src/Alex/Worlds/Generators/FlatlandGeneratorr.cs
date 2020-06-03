using Alex.API.Blocks.State;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks;
using Alex.Blocks.State;
using Microsoft.Xna.Framework;

namespace Alex.Worlds.Generators
{
    public class FlatlandGenerator : IWorldGenerator
    {
	    private BlockState Bedrock = BlockFactory.GetBlockState("minecraft:bedrock");
	    private BlockState Dirt = BlockFactory.GetBlockState("minecraft:dirt");
	    private BlockState Grass = BlockFactory.GetBlockState("minecraft:grass_block");
	    private BlockState Water = BlockFactory.GetBlockState("minecraft:water");
	    private BlockState Slab = BlockFactory.GetBlockState("minecraft:oak_slab");
	    
		public FlatlandGenerator()
	    {

	    }

	    public ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates)
	    {
		    ChunkColumn column = new ChunkColumn();
		    column.X = chunkCoordinates.X;
		    column.Z = chunkCoordinates.Z;

		    for (int x = 0; x < 16; x++)
		    {
			    for (int z = 0; z < 16; z++)
			    {
				    column.SetBlockState(x, 0, z, Bedrock);
				    if (column.X == 1 && column.Z == 1)
				    {
					    for (int y = 1; y < 2; y++)
					    {
						    column.SetBlockState(x, y, z, Water.WithProperty("level", "8"));
					    }
					    column.SetBlockState(x, 3, z, Water.WithProperty("level", "3"));
				    }
				    else
				    {
					    column.SetBlockState(x, 1, z, Dirt);
					    column.SetBlockState(x, 2, z, Dirt);
					    column.SetBlockState(x, 3, z, Grass);

					    if (x == 8 && z == 8)
					    {
						    column.SetBlockState(x, 5, z, Slab.WithProperty("type", "bottom"));
					    }
					    else if (x == 8 && z == 7)
					    {
						    column.SetBlockState(x, 5, z, Slab.WithProperty("type", "top"));
					    }
				    }

				    column.SetSkyLight(x, 0, z, 15);
				    column.SetSkyLight(x, 1, z, 15);
				    column.SetSkyLight(x, 2, z, 15);
				    column.SetSkyLight(x, 3, z, 15);
				    column.SetSkyLight(x, 4, z, 15);
				}
		    }

		    return column;
	    }

	    public Vector3 GetSpawnPoint()
	    {
		    return new Vector3(0, 16, 0);
	    }

	    public void Initialize()
	    {
		    
	    }

	    public LevelInfo GetInfo()
	    {
		    return new LevelInfo();
	    }
    }
}
