using System;
using Alex.API.Blocks.State;
using Alex.API.World;
using Alex.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Utils;

namespace Alex.Worlds
{
	public class World : IWorld
	{
        private GraphicsDevice Graphics { get; }
		private Rendering.Camera.Camera Camera { get; }
        public World(Alex alex, GraphicsDevice graphics, Rendering.Camera.Camera camera, WorldProvider worldProvider)
        {
            Graphics = graphics;
	        Camera = camera;

			RenderingManager = new RenderingManager(alex, graphics, camera, this);
	        WorldProvider = worldProvider;
			WorldProvider.Init(OnChunkReceived, Unload, PlayerPositionProvider);
        }

		public RenderingManager RenderingManager { get; private set; }
		private WorldProvider WorldProvider { get; set; }

		public int Vertices
        {
            get { return RenderingManager.Vertices; }
        }

		public int ChunkCount
        {
            get { return RenderingManager.ChunkCount; }
        }

		public int ChunkUpdates
        {
            get { return RenderingManager.ChunkUpdates; }
        }

		private Vector3 PlayerPositionProvider()
		{
			return Camera.Position;
		}

		private void Unload(int x, int z)
		{
			RenderingManager.RemoveChunk(new ChunkCoordinates(x, z));
		}

		private void OnChunkReceived(IChunkColumn chunkColumn, int x, int z)
		{
			RenderingManager.AddChunk(chunkColumn, new ChunkCoordinates(x, z), true);
		}

		public void ResetChunks()
        {
            RenderingManager.ClearChunks();
        }

        public void RebuildChunks()
        {
            RenderingManager.RebuildAll();
        }

        public void Render()
        {
            Graphics.DepthStencilState = DepthStencilState.Default;
            Graphics.SamplerStates[0] = SamplerState.PointWrap;
            
            RenderingManager.Draw(Graphics);
        }

		public void Update()
		{
			RenderingManager.Update();
		}

        public Vector3 GetSpawnPoint()
        {
	        if (WorldProvider != null)
	        {
		        return WorldProvider.GetSpawnPoint();
	        }
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
            if (y < 0 || y > ChunkColumn.ChunkHeight) return 15;

			IChunkColumn chunk;
	        if (RenderingManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
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
            if (y < 0 || y > ChunkColumn.ChunkHeight) return 15;

			IChunkColumn chunk;
	        if (RenderingManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
	        {
                return chunk.GetBlocklight(x & 0xf, y & 0xff, z & 0xf);
            }
            return 15;
        }

        public IBlock GetBlock(Vector3 position)
        {
            return GetBlock(position.X, position.Y, position.Z);
        }

		public IBlock GetBlock(float x, float y, float z)
	    {
		    return GetBlock((int) Math.Floor(x), (int) Math.Floor(y), (int) Math.Floor(z)); // Fix. xd
	    }

		public IBlock GetBlock(int x, int y, int z)
        {
		    IChunkColumn chunk;
            if (RenderingManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
            {
                return chunk.GetBlock(x & 0xf, y & 0xff, z & 0xf);
            }
            return BlockFactory.GetBlock(0, 0);
        }

	    public void SetBlock(float x, float y, float z, IBlock block)
	    {
		    SetBlock((int) x, (int) y, (int) z, block);
	    }

	    public void SetBlock(int x, int y, int z, IBlock block)
	    {
			IChunkColumn chunk;
		    if (RenderingManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
		    {
				chunk.SetBlock(x & 0xf, y & 0xff, z & 0xf, block);
		    }
	    }

		public void SetBlockState(float x, float y, float z, IBlockState blockState)
		{
			SetBlockState((int)x, (int)y, (int)z, blockState);
		}

		public void SetBlockState(int x, int y, int z, IBlockState blockState)
		{
		//	IChunkColumn chunk;
		//	if (RenderingManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
			//{
		//		chunk.SetBlockState(x & 0xf, y & 0xff, z & 0xf, blockState);
			//}
		}

		public IBlockState GetBlockState(float x, float y, float z)
		{
			throw new NotImplementedException();
		}

		private bool _destroyed = false;
		public void Destroy()
		{
			if (_destroyed) return;
			_destroyed = true;

			WorldProvider.Dispose();
			RenderingManager.Dispose();
		}
	}
}
