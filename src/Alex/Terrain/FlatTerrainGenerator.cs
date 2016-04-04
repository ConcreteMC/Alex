using Alex.Blocks;
using Alex.Rendering;
using Microsoft.Xna.Framework;

namespace Alex.Terrain
{
    public class FlatTerrainGenerator : TerrainGenerator
    {
        public FlatTerrainGenerator(string seed) : base(seed)
        {
        }

        public override Chunk GenerateChunk(Vector3 position)
        {
            var chunk = new Chunk(position);
            PopulateChunk(chunk);
            return chunk;
        }

        private void PopulateChunk(Chunk chunk)
        {
            for (var x = 0; x < Chunk.ChunkSize.X; x++)
            {
                for (var z = 0; z < Chunk.ChunkSize.Z; z++)
                {
                    for (var y = 0; y < 4; y++)
                    {
                        switch (y)
                        {
                            case 0:
                                chunk.SetBlock(x, y, z, new Bedrock());
                                break;
                            case 1:
                            case 2:
                                chunk.SetBlock(x, y, z, new Dirt());
                                break;
                            default:
                                chunk.SetBlock(x, y, z, new Grass());
                                break;
                        }
                    }
                }
            }

            chunk.SetBlock(1, 5, 0, new Torch());
            chunk.SetBlock(1, 4, 1, new Stone());
            chunk.SetBlock(1, 5, 1, new Stone());
            chunk.SetBlock(1, 4, 0, new Stone());
            chunk.SetBlock(2, 4, 0, new StoneSlab());
			chunk.SetBlock(2, 4, 1, new Leaves(0));
			chunk.SetBlock(2, 4, 2, new Glass());

	        chunk.SetBlock(2, 4, 3, new StainedGlass(1));
			chunk.SetBlock(2, 4, 4, new StainedGlass(2));
			chunk.SetBlock(2, 4, 5, new StainedGlass(3));
			chunk.SetBlock(2, 4, 6, new StainedGlass(4));

			chunk.SetBlock(2, 5, 3, new StainedGlass(5));
			chunk.SetBlock(2, 5, 4, new StainedGlass(6));
	        chunk.SetBlock(2, 5, 5, new StainedGlass(7));

			chunk.SetBlock(2, 4, 9, new Wood(0));
			chunk.SetBlock(2, 5, 9, new Wood(0));
			chunk.SetBlock(2, 6, 9, new Wood(0));
			chunk.SetBlock(2, 7, 9, new Wood(0));

			chunk.SetBlock(2, 7, 10, new Leaves(0));
			chunk.SetBlock(3, 7, 9, new Leaves(0));

			chunk.SetBlock(2, 7, 8, new Leaves(0));
			chunk.SetBlock(1, 7, 9, new Leaves(0));

			chunk.SetBlock(2, 8, 9, new Leaves(0));

			chunk.UpdateHeightMap();
        }
    }
}
