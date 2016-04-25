using System;
using System.Collections.Generic;
using System.Linq;
using Alex.Graphics.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Rendering
{
    public class World
    {
        public World()
        {
            ChunkManager = new ObjectManager();
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

        public void ResetChunks()
        {
            ChunkManager.Chunks.Clear();
        }

        public void Render()
        {
            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Game.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

            ChunkManager.Draw(Game.GraphicsDevice);
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

        public Block GetBlock(Vector3 position)
        {
            return GetBlock(position.X, position.Y, position.Z);
        }

		public Block GetBlock(float x, float y, float z)
	    {
		    return GetBlock((int) x, (int) y, (int) z);
	    }

		public Block GetBlock(int x, int y, int z)
        {
            var key = new Vector3(x >> 4, 0, z >> 4);
            if (ChunkManager.Chunks.ContainsKey(key))
            {
                return ChunkManager.Chunks[key].GetBlock(x & 0xf, y & 0xff, z & 0xf);
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
		    if (ChunkManager.Chunks.ContainsKey(key))
		    {
			    ChunkManager.Chunks[key].SetBlock(x & 0xf, y & 0xff, z & 0xf, block);
		    }
	    }
    }
}
