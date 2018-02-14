using System.Collections.Generic;
using System.Threading;
using Alex.Blocks;
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

        private byte[] Blocks = new byte[ChunkWidth * ChunkHeight * ChunkDepth];
        private NibbleArray Metadata = new NibbleArray(ChunkWidth * ChunkHeight * ChunkDepth);
        private NibbleArray Blocklight = new NibbleArray(ChunkWidth * ChunkHeight * ChunkDepth);
        private NibbleArray Skylight = new NibbleArray(ChunkWidth * ChunkHeight * ChunkDepth);

        internal object VertexLock = new object();

		public Chunk(Vector3 position)
        {
            Position = position * ChunkSize;
            IsDirty = true;
        }

        public Chunk(float x, float y, float z) : this(new Vector3(x, y, z))
        {
        }
        
        internal object UpdateLock { get; set; } = new object();
	    internal bool Scheduled { get; set; } = false;

        public bool IsDirty { get; set; }

	    private VertexBuffer _vertex = null;

        public VertexBuffer VertexBuffer
        {
            get { return _vertex; }
            set { _vertex = value; }
        }

	    private VertexBuffer _transparentBuffer = null;

	    public VertexBuffer TransparentVertexBuffer
	    {
		    get { return _transparentBuffer; }
		    set { _transparentBuffer = value; }
	    }

        public Vector3 Position { get; set; }
        public Mesh Mesh { get; set; }
		public Mesh TransparentMesh { get; set; }
        public Mesh GenerateSolidMesh(World world)
        {
            var vertices = new List<VertexPositionNormalTextureColor>();
            for (var x = 0; x < ChunkWidth; x++)
                for (var z = 0; z < ChunkDepth; z++)
                    for (var y = 0; y < ChunkHeight; y++)
                    {
                        var index = GetIndex(x, y, z);
                        if (Blocks[index] == 0) continue;

                        //TODO: Do lighting in here?

                        var block = BlockFactory.GetBlock(Blocks[index], Metadata[index]);
	                    if (!block.Renderable || block.Transparent) continue;

                        var vert = block.GetVertices(new Vector3(x, y, z) + Position, world);
                        vertices.AddRange(vert);
                    }

            return new Mesh(vertices.ToArray());
        }

	    public Mesh GenerateTransparentMesh(World world)
	    {
		    var vertices = new List<VertexPositionNormalTextureColor>();
		    for (var x = 0; x < ChunkWidth; x++)
		    for (var z = 0; z < ChunkDepth; z++)
		    for (var y = 0; y < ChunkHeight; y++)
		    {
			    var index = GetIndex(x, y, z);
			    if (Blocks[index] == 0) continue;

			    //TODO: Do lighting in here?

			    var block = BlockFactory.GetBlock(Blocks[index], Metadata[index]);
			    if (!block.Renderable || !block.Transparent) continue;

			    var vert = block.GetVertices(new Vector3(x, y, z) + Position, world);
			    vertices.AddRange(vert);
		    }

		    return new Mesh(vertices.ToArray());
	    }

		public void SetBlock(int x, int y, int z, Block block)
        {
            var index = GetIndex(x, y, z);
            Blocks[index] = block.BlockId;
            Metadata[index] = block.Metadata;
            IsDirty = true;
        }

        public Block GetBlock(int x, int y, int z)
        {
            if (y < 0 || y > ChunkHeight)
            {
                return BlockFactory.GetBlock(0, 0);
            }

            var index = GetIndex(x, y, z);
            return BlockFactory.GetBlock(Blocks[index], Metadata[index]);
        }

        public void SetBlocklight(int x, int y, int z, byte value)
        {
            var index = GetIndex(x, y, z);
            Blocklight[index] = value;
        }

        public byte GetBlocklight(int x, int y, int z)
		{
            var index = GetIndex(x, y, z);
            return Blocklight[index];
		}

        public void SetSkylight(int x, int y, int z, byte value)
        {
            var index = GetIndex(x, y, z);
            Skylight[index] = value;
        }

        public byte GetSkylight(int x, int y, int z)
        {
            var index = GetIndex(x, y, z);
            return Skylight[index];
        }

        private int GetIndex(int x, int y, int z)
        {
            return y + (z * ChunkHeight) + (x * ChunkHeight * ChunkWidth);
            // return (x * ChunkHeight * ChunkWidth + y * ChunkDepth + z);
        }
    }
}