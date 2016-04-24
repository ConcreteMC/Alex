using System;
using System.Collections.Generic;
using System.Linq;
using Alex.Graphics.Items;
using Alex.Terrain;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Rendering
{
    public class World
    {
        public World()
        {
            ChunkManager = new ObjectManager();
            TerrainGenerator = new FlatTerrainGenerator("Flatlands");
		    //TerrainGenerator = new DefaultTerrainGenerator("lol");
            Logging.Info("Generating chunks...");
			GenerateChunks(GetSpawnPoint(), (int)Game.RenderDistance);
        }

		private TerrainGenerator TerrainGenerator { get; set; }
        private ObjectManager ChunkManager { get; }

        internal int Vertices
        {
            get { return ChunkManager.Vertices; }
        }

        internal int ChunkCount
        {
            get { return ChunkManager.Chunks.Count; }
        }

        public void Render()
        {
            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Game.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Game.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;


            ChunkManager.Draw(Game.GraphicsDevice);
        }

        private readonly object _chunkLock = new object();
        public bool GenerateChunks(Vector3 chunkPosition, int viewDistance)
        {
            lock (_chunkLock)
            {
                var newOrders = new Dictionary<Vector3, double>();
                var radiusSquared = viewDistance / Math.PI;
                var radius = Math.Ceiling(Math.Sqrt(radiusSquared));
                var centerX = (int)chunkPosition.X;
                var centerZ = (int)chunkPosition.Z;

                for (var x = -radius; x <= radius; ++x)
                {
                    for (var z = -radius; z <= radius; ++z)
                    {
                        var distance = (x*x) + (z*z);
                        if (distance > radiusSquared)
                        {
                            continue;
                        }
                        var chunkX = (int) (x + centerX);
                        var chunkZ = (int) (z + centerZ);

                        var index = new Vector3(chunkX, 0, chunkZ);
                        newOrders[index] = distance;
                    }
                }

                if (newOrders.Count > viewDistance)
                {
                    foreach (var pair in newOrders.OrderByDescending(pair => pair.Value))
                    {
                        if (newOrders.Count <= viewDistance) break;
                        newOrders.Remove(pair.Key);
                    }
                }

                var renderChunks = ChunkManager.Chunks;
                foreach (var chunkKey in renderChunks.Keys)
                {
                    if (!newOrders.ContainsKey(chunkKey))
                    {
                        ChunkManager.RemoveChunk(chunkKey);
                    }
                }

                foreach (var pair in newOrders.OrderBy(pair => pair.Value))
                {
                    if (renderChunks.ContainsKey(pair.Key)) continue;

                    var chunk = TerrainGenerator.GenerateChunk(pair.Key);

                    ChunkManager.AddChunk(chunk, pair.Key);
                }
            }

            return true;
        }

        public Vector3 GetSpawnPoint()
        {
            return TerrainGenerator.GetSpawnPoint();
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
