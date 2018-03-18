using System;
using System.Collections.Generic;
using Alex.API.Blocks.State;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Gamestates;
using Alex.Rendering;
using Alex.Utils;
using Alex.Worlds.Lighting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Blocks;
using MiNET.Entities;
using MiNET.Utils;
using MiNET.Worlds;
using NLog;
using Block = Alex.Blocks.Block;
using EntityManager = Alex.Rendering.EntityManager;

namespace Alex.Worlds
{
	public class World : IWorld, IWorldReceiver
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(World));

		private GraphicsDevice Graphics { get; }
		private Rendering.Camera.Camera Camera { get; }
        public World(Alex alex, GraphicsDevice graphics, Rendering.Camera.Camera camera, WorldProvider worldProvider)
        {
            Graphics = graphics;
	        Camera = camera;

			ChunkManager = new ChunkManager(alex, graphics, camera, this);
			EntityManager = new EntityManager(graphics, this);
			Ticker = new TickManager(this);

	        WorldProvider = worldProvider;
			WorldProvider.Init(this);

			ChunkManager.Start();
        }

		public TickManager Ticker { get; }
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

		public void Render2D(IRenderArgs args)
		{
			EntityManager.Render2D(args, Camera);
		}

		public void Update(GameTime gameTime)
		{
			ChunkManager.Update();
			EntityManager.Update(gameTime);
			Ticker.Update(gameTime);
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

		public IBlock GetBlock(BlockCoordinates position)
		{
			return GetBlock(position.X, position.Y, position.Z);
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
		    SetBlock((int) Math.Floor(x), (int)Math.Floor(y), (int)Math.Floor(z), block);
	    }

	    public void SetBlock(int x, int y, int z, IBlock block)
	    {
			IChunkColumn chunk;
		    if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
		    {
				chunk.SetBlock(x & 0xf, y & 0xff, z & 0xf, block);
				ChunkManager.ScheduleChunkUpdate(new ChunkCoordinates(x >> 4, z >> 4), ScheduleType.Full);
		    }
	    }

		public void SetBlockState(int x, int y, int z, IBlockState block)
		{
			IChunkColumn chunk;
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
			{
				chunk.SetBlockState(x & 0xf, y & 0xff, z & 0xf, block);
				ChunkManager.ScheduleChunkUpdate(new ChunkCoordinates(x >> 4, z >> 4), ScheduleType.Full);
			}
		}

		public IBlockState GetBlockState(int x, int y, int z)
		{
			IChunkColumn chunk;
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
			{
				return chunk.GetBlockState(x & 0xf, y & 0xff, z & 0xf);
			}

			return BlockFactory.GetBlockState(0,0);
		}

		public int GetBiome(int x, int y, int z)
		{
			IChunkColumn chunk;
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
			{
				Worlds.ChunkColumn realColumn = (Worlds.ChunkColumn) chunk;
				return	realColumn.GetBiome((int) x & 0xf, (int) z & 0xf);
			}

			return -1;
		}

		public void TickChunk(ChunkColumn chunkColumn)
		{
			var chunkCoords = new Vector3(chunkColumn.X >> 4, 0, chunkColumn.Z >> 4);

			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					for (int y = chunkColumn.GetHeight(x, z); y > 1; y--)
					{
						var block = chunkColumn.GetBlock(x, y, z);
						if (block.Tick(this, chunkCoords + new Vector3(x, y, z)))
						{

						}
					}
				}
			}
		}

		private void InitiateChunk(ChunkColumn chunkColumn)
		{
			var chunkCoords = new BlockCoordinates(chunkColumn.X >> 4, 0, chunkColumn.Z >> 4);

			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					for (int y = 255; y > 0; y--)
					{
						var block = (Block)chunkColumn.GetBlock(x, y, z);
						block.BlockPlaced(this, chunkCoords + new BlockCoordinates(x, y, z));
					}
				}
			}
		}

		private bool _destroyed = false;
		public void Destroy()
		{
			if (_destroyed) return;
			_destroyed = true;

			EntityManager.Dispose();
			WorldProvider.Dispose();
			ChunkManager.Dispose();
		}

		#region IWorldReceiver (Handle WorldProvider callbacks)

		public Vector3 RequestPlayerPosition()
		{
			return Camera.Position;
		}

		public void ChunkReceived(IChunkColumn chunkColumn, int x, int z, bool update)
		{
			ChunkManager.AddChunk(chunkColumn, new ChunkCoordinates(x, z), update);
			InitiateChunk(chunkColumn as ChunkColumn);
		}

		public void ChunkUnload(int x, int z)
		{
			var chunkCoordinates = new ChunkCoordinates(x, z);
			ChunkManager.RemoveChunk(chunkCoordinates);

			EntityManager.UnloadEntities(chunkCoordinates);
		}

		public void SpawnEntity(long entityId, Entity entity)
		{
			EntityManager.AddEntity(entityId, entity);
			//Log.Info($"Spawned entity {entityId} : {entity} at {entity.KnownPosition} with renderer {entity.GetModelRenderer()}");
		}

		public void DespawnEntity(long entityId)
		{
			EntityManager.Remove(entityId);
		//	Log.Info($"Despawned entity {entityId}");
		}

		#endregion
	}
}
