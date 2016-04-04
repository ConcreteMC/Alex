using System;
using Alex.Rendering;
using Alex.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Terrain
{
    public class DefaultTerrainGenerator : TerrainGenerator
    {
        public DefaultTerrainGenerator(string seed) : base(seed)
        {
            Bottom = new SimplexOctaveGenerator(Seed, 8);
            Overhang = new SimplexOctaveGenerator(Seed, 8);
            Overhang.SetScale(1 / 64.0);
            Bottom.SetScale(1 / 128.0);
        }

        public override Chunk GenerateChunk(Vector3 position)
        {
            Chunk chunk = new Chunk(position);
            PopulateChunk(chunk);
            return chunk;
        }

	    private int _spawnY;
	    public override Vector3 GetSpawnPoint()
	    {
		    return new Vector3(1, _spawnY , 1);
	    }

	    private SimplexOctaveGenerator Bottom { get; }
        private SimplexOctaveGenerator Overhang { get; }

        private void PopulateChunk(Chunk chunk)
        {
            double overhangsMagnitude = 16;
            double bottomsMagnitude = 32;

			ushort[] blocks = new ushort[Chunk.ChunkHeight * Chunk.ChunkDepth * Chunk.ChunkWidth];
			byte[] metadata = new byte[Chunk.ChunkHeight * Chunk.ChunkDepth * Chunk.ChunkWidth];
			int[] heightMap = new int[Chunk.ChunkDepth * Chunk.ChunkWidth];

			for (int i = 0; i < blocks.Length; i++)
	        {
		        blocks[i] = 0;
		        metadata[i] = 0;
	        }

			for (var x = 0; x < Chunk.ChunkWidth; x++)
            {
                for (var z = 0; z < Chunk.ChunkDepth; z++)
                {
                    var ox = x + chunk.Position.X;
                    var oz = z + chunk.Position.Z;
                    
                    var bottomHeight = (int) (Bottom.Noise(ox, oz, 0.5, 0.5)*bottomsMagnitude + 64);
                    var maxHeight = (int) (Overhang.Noise(ox, oz, 0.5, 0.5)*overhangsMagnitude + bottomHeight + 32.0);

                    var threshold = 0.0;
                    maxHeight = Math.Max(1, maxHeight);

                    for (var y = 0; y < maxHeight && y < Chunk.ChunkHeight; y++)
                    {
	                    var index = GetIndex(x, y, z);

						if (y < 1)
						{
							blocks[index] = 7; //Bedrock
                            continue;
                        }

                        if (y > bottomHeight)
                        {
                            //part where we do the overhangs
                            var density = Overhang.Noise(ox, y, oz, 0.5, 0.5);
	                        if (density > threshold)
	                        {
		                        blocks[index] = 1; //Stone
	                        }
                        }
                        else
                        {
							blocks[index] = 1;
						}
                    }
                }
            }

	        for (var x = 0; x < Chunk.ChunkWidth; x++)
	        {
		        for (var z = 0; z < Chunk.ChunkDepth; z++)
		        {
			        for (int y = Chunk.ChunkHeight - 1; y > 1; y--)
			        {
				        var index = GetIndex(x,y,z);
				        if (index < blocks.Length)
				        {
							var blockId = blocks[index];
							if (blockId != 0)
							{
								heightMap[(x << 4) + z] = y;

								blocks[index] = 2;
								blocks[GetIndex(x, y - 1, z)] = 3;
								blocks[GetIndex(x, y - 2, z)] = 3;
								break;
							}
						}
			        }
		        }
	        }

	        chunk.Blocks = blocks;
	        chunk.Metadata = metadata;
	        chunk.HeightMap = heightMap;

	        _spawnY = chunk.GetHeight(1, 1);
        }

	    private int GetIndex(int x, int y, int z)
	    {
			return x * Chunk.ChunkHeight * Chunk.ChunkWidth + y * Chunk.ChunkDepth + z;
		}
    }
}
