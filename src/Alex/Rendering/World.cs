using System;
using Alex.Entities;
using Alex.Gamestates;
using Alex.Graphics.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Rendering
{
    public class World
    {
        private GraphicsDevice Graphics { get; }
        public World(GraphicsDevice graphics, Camera.Camera camera)
        {
            Graphics = graphics;
            ChunkManager = new ObjectManager(graphics, camera, this);
        }

        public ObjectManager ChunkManager { get; private set; }

        internal int Vertices
        {
            get { return ChunkManager.Vertices; }
        }

        internal int ChunkCount
        {
            get { return ChunkManager.Chunks.Count; }
        }

        internal int ChunkUpdates
        {
            get { return ChunkManager.ChunkUpdates; }
        }

        public void ResetChunks()
        {
            ChunkManager.Chunks.Clear();
        }

        public void RebuildChunks()
        {
            ChunkManager.RebuildAll();
        }

        public void Render()
        {
            Graphics.DepthStencilState = DepthStencilState.Default;
            Graphics.SamplerStates[0] = SamplerState.PointWrap;
            
            ChunkManager.Draw(Graphics);
        }

        public Vector3 GetSpawnPoint()
        {
            return Vector3.Zero;
        }

	    public bool IsSolid(Vector3 location)
	    {
		    return IsSolid(location.X, location.Y, location.Z);
	    }

	    public bool IsSolid(float x, float y, float z)
	    {
		    return GetBlock(x, y, z).Solid;
	    }

		public bool IsTransparent(Vector3 location)
		{
			return IsTransparent(location.X, location.Y, location.Z);
		}

		public bool IsTransparent(float x, float y, float z)
		{
			return GetBlock(x, y, z).Transparent;
		}

        public byte GetSkyLight(Vector3 position)
        {
            return GetSkyLight(position.X, position.Y, position.Z);
        }

        public byte GetSkyLight(float x, float y, float z)
        {
            return GetSkyLight((int)Math.Floor(x), (int)Math.Floor(y), (int)Math.Floor(z)); // Fix. xd
        }

        public byte GetSkyLight(int x, int y, int z)
        {
            if (y < 0 || y > Chunk.ChunkHeight) return 15;
            var key = new Vector3(x >> 4, 0, z >> 4);

            Chunk chunk;
            if (ChunkManager.Chunks.TryGetValue(key, out chunk))
            {
                return chunk.GetSkylight(x & 0xf, y & 0xff, z & 0xf);
            }
            return 15;
        }

        public byte GetBlockLight(Vector3 position)
        {
            return GetBlockLight(position.X, position.Y, position.Z);
        }
        public byte GetBlockLight(float x, float y, float z)
        {
            return GetBlockLight((int)Math.Floor(x), (int)Math.Floor(y), (int)Math.Floor(z)); // Fix. xd
        }

        public byte GetBlockLight(int x, int y, int z)
        {
            if (y < 0 || y > Chunk.ChunkHeight) return 15;

            var key = new Vector3(x >> 4, 0, z >> 4);

            Chunk chunk;
            if (ChunkManager.Chunks.TryGetValue(key, out chunk))
            {
                return chunk.GetBlocklight(x & 0xf, y & 0xff, z & 0xf);
            }
            return 15;
        }

        public Block GetBlock(Vector3 position)
        {
            return GetBlock(position.X, position.Y, position.Z);
        }

		public Block GetBlock(float x, float y, float z)
	    {
		    return GetBlock((int) Math.Floor(x), (int) Math.Floor(y), (int) Math.Floor(z)); // Fix. xd
	    }

		public Block GetBlock(int x, int y, int z)
        {
            var key = new Vector3(x >> 4, 0, z >> 4);

		    Chunk chunk;
            if (ChunkManager.Chunks.TryGetValue(key, out chunk))
            {
                return chunk.GetBlock(x & 0xf, y & 0xff, z & 0xf);
            }
            return BlockFactory.GetBlock(0, 0);
        }

	    public void SetBlock(float x, float y, float z, Block block)
	    {
		    SetBlock((int) x, (int) y, (int) z, block);
	    }

	    public void SetBlock(int x, int y, int z, Block block)
	    {
			var key = new Vector3(x >> 4, 0, z >> 4);

            Chunk chunk;
            if (ChunkManager.Chunks.TryGetValue(key, out chunk))
            {
                chunk.SetBlock(x & 0xf, y & 0xff, z & 0xf, block);
		    }
	    }
    }
}
