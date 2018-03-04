using System;
using Alex.API.Blocks.State;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Gamestates;
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

			ChunkManager = new ChunkManager(alex, graphics, camera, this);
			EntityManager = new EntityManager(graphics);

	        WorldProvider = worldProvider;
			WorldProvider.Init(OnChunkReceived, Unload, PlayerPositionProvider);
        }

		public EntityManager EntityManager { get; }
		public ChunkManager ChunkManager { get; private set; }
		private WorldProvider WorldProvider { get; set; }

		public int Vertices
        {
            get { return ChunkManager.Vertices; }
        }

		public int ChunkCount
        {
            get { return ChunkManager.ChunkCount; }
        }

		public int ChunkUpdates
        {
            get { return ChunkManager.ChunkUpdates; }
        }

		private Vector3 PlayerPositionProvider()
		{
			return Camera.Position;
		}

		private void Unload(int x, int z)
		{
			ChunkManager.RemoveChunk(new ChunkCoordinates(x, z));
		}

		private void OnChunkReceived(IChunkColumn chunkColumn, int x, int z)
		{
			ChunkManager.AddChunk(chunkColumn, new ChunkCoordinates(x, z), true);
		}

		public void ResetChunks()
        {
            ChunkManager.ClearChunks();
        }

        public void RebuildChunks()
        {
            ChunkManager.RebuildAll();
        }

        public void Render(IRenderArgs args)
        {
            Graphics.DepthStencilState = DepthStencilState.Default;
            Graphics.SamplerStates[0] = SamplerState.PointWrap;
            
            ChunkManager.Draw(args);
			EntityManager.Render(args, Camera);
        }

		public void Update(GameTime gameTime)
		{
			ChunkManager.Update();
			EntityManager.Update(gameTime);
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
	        if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
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
	        if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
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
            if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
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
		    if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
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
		//	if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
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
			ChunkManager.Dispose();
		}
	}
}
