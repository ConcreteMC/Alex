using System.Collections.Generic;
using Alex.Graphics;
using Alex.Graphics.Items;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Rendering
{
    public class Chunk
    {
        public const int ChunkDepth = 16;
        public const int ChunkWidth = 16;
        public const int ChunkHeight = 256;

        public static Vector3 ChunkSize => new Vector3(ChunkWidth, ChunkHeight, ChunkDepth);

        public ushort[] Blocks = new ushort[ChunkHeight * ChunkDepth * ChunkWidth];
        public byte[] Metadata = new byte[ChunkHeight * ChunkDepth * ChunkWidth];
		public int[] HeightMap = new int[ChunkDepth * ChunkWidth];
		public NibbleArray Blocklight = new NibbleArray(ChunkHeight * ChunkDepth * ChunkWidth);
        internal object ChunkLock = new object();

		public Chunk(Vector3 position)
        {
            Position = position * ChunkSize;
            IsDirty = true;

	        for (int i = 0; i < HeightMap.Length; i++)
	        {
		        HeightMap[i] = 0;
	        }
		    IsBeingUpdated = false;
        }

        public Chunk(float x, float y, float z) : this(new Vector3(x, y, z))
        {
        }
        
        internal bool IsBeingUpdated { get; set; }
        public bool IsDirty { get; set; }
        public VertexBuffer VertexBuffer { get; set; }
        public Vector3 Position { get; set; }
        public Mesh Mesh { get; set; }

        public Mesh GenerateMesh()
        {
            var vertices = new List<VertexPositionNormalTextureColor>();
            for (var x = 0; x < ChunkWidth; x++)
                for (var z = 0; z < ChunkDepth; z++)
                    for (var y = 0; y < ChunkHeight; y++)
                    {
                        var index = x * ChunkHeight * ChunkWidth + y * ChunkDepth + z;
                        if (Blocks[index] == 0) continue;

                        var block = BlockFactory.GetBlock(Blocks[index], Metadata[index]);

                        var vert = block.GetVertices(new Vector3(x, y, z) + Position);
                        vertices.AddRange(vert);
                    }

            return new Mesh(vertices.ToArray());
        }

        public void SetBlock(int x, int y, int z, Block block)
        {
            var index = x * ChunkHeight * ChunkWidth + y * ChunkDepth + z;
            Blocks[index] = block.BlockId;
            Metadata[index] = block.Metadata;
            IsDirty = true;

	        if (y > GetHeight(x, z))
	        {
				index = (x << 4) + (z);
		        HeightMap[index] = y;
	        }
        }

        public Block GetBlock(int x, int y, int z)
        {
            var index = x * ChunkHeight * ChunkWidth + y * ChunkDepth + z;
            return BlockFactory.GetBlock(Blocks[index], Metadata[index]);
        }

	    public void UpdateHeightMap()
	    {
			for (var x = 0; x < Chunk.ChunkWidth; x++)
			{
				for (var z = 0; z < Chunk.ChunkDepth; z++)
				{
					for (int y = Chunk.ChunkHeight - 1; y > 1; y--)
					{
						var index = x * ChunkHeight * ChunkWidth + y * ChunkDepth + z;
						if (index < Blocks.Length)
						{
							var blockId = Blocks[index];
							if (blockId != 0)
							{
								HeightMap[(x << 4) + z] = y;
								break;
							}
						}
					}
				}
			}
		}

	    public int GetHeight(int x, int z)
	    {
		    var index = (x << 4) + z;
		    return HeightMap[index];
	    }
    }
}