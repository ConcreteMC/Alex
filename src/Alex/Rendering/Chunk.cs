using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alex.Blocks;
using Alex.Graphics;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Worlds;

namespace Alex.Rendering
{
    public class Chunk : IDisposable
    {
        public const int ChunkDepth = 16;
        public const int ChunkWidth = 16;
        public const int ChunkHeight = 256;

        public static Vector3 ChunkSize => new Vector3(ChunkWidth, ChunkHeight, ChunkDepth);
	    public Vector3 Size => new Vector3(ChunkWidth, 16 * ((Height.Max() >> 4) + 1), ChunkDepth);

		public byte[] BiomeId = new byte[ChunkWidth * ChunkDepth];
		public byte[] Height = new byte[ChunkWidth * ChunkDepth];
        private uint[] Blocks = new uint[ChunkWidth * ChunkHeight * ChunkDepth];

        private NibbleArray Blocklight = new NibbleArray(ChunkWidth * ChunkHeight * ChunkDepth);
        private NibbleArray Skylight = new NibbleArray(ChunkWidth * ChunkHeight * ChunkDepth);

        internal object VertexLock = new object();

		public Chunk(Vector3 position)
        {
            Position = position * ChunkSize;
            IsDirty = true;

	        for (int i = 0; i < BiomeId.Length; i++)
	        {
		        BiomeId[i] = 1;
	        }
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

                        var block = BlockFactory.GetBlock(Blocks[index]);
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

			    var block = BlockFactory.GetBlock(Blocks[index]);
			    if (!block.Renderable || !block.Transparent) continue;

			    var vert = block.GetVertices(new Vector3(x, y, z) + Position, world);
			    vertices.AddRange(vert);
		    }

		    return new Mesh(vertices.ToArray());
	    }

		public void SetBlock(int x, int y, int z, Block block)
        {
            var index = GetIndex(x, y, z);
            Blocks[index] = block.BlockStateID;
            IsDirty = true;
        }

        public Block GetBlock(int x, int y, int z)
        {
            if (y < 0 || y > ChunkHeight)
            {
                return BlockFactory.GetBlock(0, 0);
            }

            var index = GetIndex(x, y, z);
            return BlockFactory.GetBlock(Blocks[index]);
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
	       // if (y > GetHeight(x, z)) return 15;

			var index = GetIndex(x, y, z);
            return Skylight[index];
        }

		public void SetHeight(int bx, int bz, byte h)
	    {
		    Height[((bz << 4) + (bx))] = h;
	    }

	    public byte GetHeight(int bx, int bz)
	    {
		    return Height[((bz << 4) + (bx))];
	    }

	    public void SetBiome(int bx, int bz, byte biome)
	    {
		    BiomeId[(bz << 4) + (bx)] = biome;
		    IsDirty = true;
	    }

	    public byte GetBiome(int bx, int bz)
	    {
		    return BiomeId[(bz << 4) + (bx)];
	    }

		private int GetIndex(int x, int y, int z)
        {
            return y + (z * ChunkHeight) + (x * ChunkHeight * ChunkWidth);
        }

	    private Color GetBiomeColor(int bx, int bz)
	    {
		    if (bx < 0) bx = 0;
		    if (bz < 0) bz = 0;
		    if (bx > 15) bx = 15;
		    if (bz > 15) bz = 15;

		    BiomeUtils utils = new BiomeUtils();
		    var biome = GetBiome(bx, bz);
		    int color = utils.ComputeBiomeColor(biome, 0, true);

		   /* if (random.Next(30) == 0)
		    {
			    Color col = Color.FromArgb(color);
			    color = Color.FromArgb(0, Math.Max(0, col.R - 160), Math.Max(0, col.G - 160), Math.Max(0, col.B - 160)).ToArgb();
		    }*/

		    return new Color((uint) color);
	    }

	    public void CalculateHeight()
	    {
		    for (int x = 0; x < ChunkWidth; x++)
		    {
			    for (int z = 0; z < ChunkDepth; z++)
			    {
				    for (int y = ChunkHeight - 1; y > 0; --y)
				    {
					    if (GetBlock(x, y, z).Renderable)
					    {
							SetHeight(x,z, (byte) y);
							break;
					    }
				    }
			    }
		    }
	    }

	    public void Dispose()
	    {
		    if (_vertex != null)
		    {
			    if (!_vertex.IsDisposed)
			    {
					_vertex.Dispose();
			    }

			    _vertex = null;
		    }

		    if (_transparentBuffer != null)
		    {
			    if (!_transparentBuffer.IsDisposed)
			    {
				    _transparentBuffer.Dispose();
			    }

			    _transparentBuffer = null;
		    }
		}
	}
}