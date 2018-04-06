using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using Alex.API.Blocks.State;
using Alex.API.Entities;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Entities;
using Alex.Gamestates;
using Alex.Graphics.Models;
using Alex.Rendering;
using Alex.Rendering.Camera;
using Alex.Utils;
using Alex.Worlds.Lighting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using Block = Alex.Blocks.Block;
using Color = Microsoft.Xna.Framework.Color;
using EntityManager = Alex.Rendering.EntityManager;
using MathF = System.MathF;

namespace Alex.Worlds
{
	public class World : IWorld, IWorldReceiver
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(World));

		private GraphicsDevice Graphics { get; }
		public Rendering.Camera.Camera Camera { get; set; }

		public LevelInfo WorldInfo;

		public Player Player { get; set; }
		public World(Alex alex, GraphicsDevice graphics, Rendering.Camera.Camera camera)
        {
            Graphics = graphics;
	        Camera = camera;

			ChunkManager = new ChunkManager(alex, graphics, this);
			EntityManager = new EntityManager(graphics, this);
			Ticker = new TickManager(this);
			 
			ChunkManager.Start();

	        alex.Resources.BedrockResourcePack.TryGetTexture("textures/entity/alex", out Bitmap rawTexture);
	        var t = TextureUtils.BitmapToTexture2D(graphics, rawTexture);

			Player = new Player(graphics, alex, alex.GameSettings.Username, this, t);
	        Player.KnownPosition = new PlayerLocation(GetSpawnPoint());
	        Camera.MoveTo(Player.KnownPosition, Vector3.Zero);
		}

		private long LastLightningBolt = 0;
		private long Tick = 0;
		public long WorldTime { get; private set; } = 6000;
		public bool FreezeWorldTime { get; set; } = false;

		public TickManager Ticker { get; }
		public EntityManager EntityManager { get; }
		public ChunkManager ChunkManager { get; private set; }
	//	private WorldProvider WorldProvider { get; set; }

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

		public int LowPriorityUpdates
		{
			get { return ChunkManager.LowPriortiyUpdates; }
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
			EntityManager.Render(args);

	        if (Camera is ThirdPersonCamera)
	        {
		        Player.Render(args);
	        }
        }

		public void Render2D(IRenderArgs args)
		{
			args.Camera = Camera;

			EntityManager.Render2D(args);
		}
		
		public void Update(UpdateArgs args, SkyboxModel skyRenderer)
		{
			args.Camera = Camera;
			Camera.Update(args, Player);

			ChunkManager.Update(args, skyRenderer);
			EntityManager.Update(args, skyRenderer);

			Player.ModelRenderer.DiffuseColor = Color.White.ToVector3() * new Vector3(skyRenderer.BrightnessModifier);
			Player.Update(args);

			if (Ticker.Update(args))
			{
				if (!FreezeWorldTime)
				{
					WorldTime++;
				}

				//if (Player.IsSpawned)
			}
		}

		public Vector3 SpawnPoint { get; set; } = Vector3.Zero;
        public Vector3 GetSpawnPoint()
        {
	        return SpawnPoint;
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
            return BlockFactory.GetBlock(0);
        }

		public void SetBlock(Block block)
		{
			var x = block.Coordinates.X;
			var y = block.Coordinates.Y;
			var z = block.Coordinates.Z;

			IChunkColumn chunk;
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
			{
				chunk.SetBlock(x & 0xf, y & 0xff, z & 0xf, block);
				ChunkManager.ScheduleChunkUpdate(new ChunkCoordinates(x >> 4, z >> 4), ScheduleType.Full);

				UpdateNeighbors(x, y, z);
			}
		}

		public void SetBlock(float x, float y, float z, IBlock block)
	    {
		    SetBlock((int) Math.Floor(x), (int)Math.Floor(y), (int)Math.Floor(z), block);
	    }

	    public void SetBlock(int x, int y, int z, IBlock block)
	    {
		    var chunkCoords = new ChunkCoordinates(x >> 4, z >> 4);

			IChunkColumn chunk;
		    if (ChunkManager.TryGetChunk(chunkCoords, out chunk))
		    {
				chunk.SetBlock(x & 0xf, y & 0xff, z & 0xf, block);
				ChunkManager.ScheduleChunkUpdate(chunkCoords, ScheduleType.Full);

			    UpdateNeighbors(x, y, z);
			} 
	    }

		public void SetBlockState(int x, int y, int z, IBlockState block)
		{
			var chunkCoords = new ChunkCoordinates(x >> 4, z >> 4);

			IChunkColumn chunk;
			if (ChunkManager.TryGetChunk(chunkCoords, out chunk))
			{
				var cx = x & 0xf;
				var cy = y & 0xff;
				var cz = z & 0xf;

				chunk.SetBlockState(cx, cy, cz, block);
				
				ChunkManager.ScheduleChunkUpdate(chunkCoords, ScheduleType.Full);

				UpdateNeighbors(x,y,z);
			}
		}

		private void UpdateNeighbors(int x, int y, int z)
		{
			var source = new BlockCoordinates(x, y, z);

			ScheduleBlockUpdate(source, new BlockCoordinates(x + 1, y, z));
			ScheduleBlockUpdate(source, new BlockCoordinates(x - 1, y, z));

			ScheduleBlockUpdate(source, new BlockCoordinates(x, y, z + 1));
			ScheduleBlockUpdate(source, new BlockCoordinates(x, y, z - 1));

			ScheduleBlockUpdate(source, new BlockCoordinates(x, y + 1, z));
			ScheduleBlockUpdate(source, new BlockCoordinates(x, y - 1, z));
		}

		private void ScheduleBlockUpdate(BlockCoordinates updatedBlock, BlockCoordinates block)
		{
			Ticker.ScheduleTick(() =>
			{
				GetBlock(block).BlockUpdate(this, block, updatedBlock);
			}, 1);
		}

		public IBlockState GetBlockState(int x, int y, int z)
		{
			IChunkColumn chunk;
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
			{
				return chunk.GetBlockState(x & 0xf, y & 0xff, z & 0xf);
			}

			return BlockFactory.GetBlockState(0);
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
			ChunkManager.Dispose();
		}

		#region IWorldReceiver (Handle WorldProvider callbacks)

		public IEntity GetPlayerEntity()
		{
			return Player;
		}

		public void ChunkReceived(IChunkColumn chunkColumn, int x, int z, bool update)
		{
			ChunkManager.AddChunk(chunkColumn, new ChunkCoordinates(x, z), update);
			//InitiateChunk(chunkColumn as ChunkColumn);
		}

		public void ChunkUnload(int x, int z)
		{
			var chunkCoordinates = new ChunkCoordinates(x, z);
			ChunkManager.RemoveChunk(chunkCoordinates);

			EntityManager.UnloadEntities(chunkCoordinates);
		}

		public void SpawnEntity(long entityId, IEntity entity)
		{
			EntityManager.AddEntity(entityId, (Entity)entity);
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

		public void UpdatePlayerPosition(PlayerLocation location)
		{
			Player.KnownPosition = location;
		}

		public void UpdateEntityPosition(long entityId, PlayerLocation position, bool relative = false, bool updateLook = false)
		{
			if (EntityManager.TryGet(entityId, out IEntity entity))
			{
				entity.KnownPosition.OnGround = position.OnGround;
				if (!relative)
				{
					entity.KnownPosition = position;
				}
				else
				{
					entity.KnownPosition.Move(position);
					
					if (updateLook)
					{
						entity.KnownPosition.Yaw = position.Yaw;
						entity.KnownPosition.Pitch = position.Pitch;
						entity.KnownPosition.HeadYaw = position.HeadYaw;
					}
				}
			}
		}

		public bool TryGetEntity(long entityId, out IEntity entity)
		{
			return EntityManager.TryGet(entityId, out entity);
		}

		public void SetTime(long worldTime)
		{
			WorldTime = worldTime;
		}

		#endregion
	}
}
