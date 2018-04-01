using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
using EntityManager = Alex.Rendering.EntityManager;
using MathF = System.MathF;

namespace Alex.Worlds
{
	public class World : IWorld, IWorldReceiver
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(World));

		private GraphicsDevice Graphics { get; }
		public Rendering.Camera.Camera Camera { get; set; }
		private SkyboxModel SkyRenderer { get; }

		public readonly LevelInfo WorldInfo;

		public Player Player { get; set; }
		public World(Alex alex, GraphicsDevice graphics, Rendering.Camera.Camera camera, WorldProvider worldProvider)
        {
            Graphics = graphics;
	        Camera = camera;

			SkyRenderer = new SkyboxModel(alex, graphics, this);

			ChunkManager = new ChunkManager(alex, graphics, this);
			EntityManager = new EntityManager(graphics, this);
			Ticker = new TickManager(this);

	        WorldProvider = worldProvider;
			WorldProvider.Init(this, out WorldInfo);

			ChunkManager.Start();

	        alex.Resources.BedrockResourcePack.TryGetTexture("textures/entity/alex", out Bitmap rawTexture);
	        var t = TextureUtils.BitmapToTexture2D(graphics, rawTexture);

			Player = new Player(alex.GameSettings.Username, this, t);
	        Player.KnownPosition = new PlayerLocation(GetSpawnPoint());
	        Camera.MoveTo(Player.KnownPosition, Vector3.Zero);
		}

		private long LastLightningBolt = 0;
		private long Tick = 0;
		public long WorldTime { get; private set; } = 6000;
		public Vector3 GetSkyColor(float celestialAngle)
		{
			var position = Camera.Position;

			float f1 = MathF.Cos(celestialAngle * ((float)Math.PI * 2F)) * 2.0F + 0.5F;
			f1 = MathHelper.Clamp(f1, 0.0F, 1.0F);

			int x = (int)MathF.Floor(position.X);
			int y = (int)MathF.Floor(position.Y);
			int z = (int) MathF.Floor(position.Z);

			Biome biome = BiomeUtils.GetBiomeById(this.GetBiome(x,y,z));
			float f2 = biome.Temperature;//.getTemperature(blockpos);
			int l = GetSkyColorByTemp(f2);
			float r = (l >> 16 & 255) / 255.0F;
			float g = (l >> 8 & 255) / 255.0F;
			float b = (l & 255) / 255.0F;
			r = r * f1;
			g = g * f1;
			b = b * f1;
			float f6 = 0;//RainStrength

			if (f6 > 0.0F)
			{
				float f7 = (r * 0.3F + g * 0.59F + b * 0.11F) * 0.6F;
				float f8 = 1.0F - f6 * 0.75F;
				r = r * f8 + f7 * (1.0F - f8);
				g = g * f8 + f7 * (1.0F - f8);
				b = b * f8 + f7 * (1.0F - f8);
			}

			float f10 = 0f; //Thunder

			if (f10 > 0.0F)
			{
				float f11 = (r * 0.3F + g * 0.59F + b * 0.11F) * 0.2F;
				float f9 = 1.0F - f10 * 0.75F;
				r = r * f9 + f11 * (1.0F - f9);
				g = g * f9 + f11 * (1.0F - f9);
				b = b * f9 + f11 * (1.0F - f9);
			}

			if (LastLightningBolt > 0)
			{
				float f12 = (float)this.LastLightningBolt - Tick;

				if (f12 > 1.0F)
				{
					f12 = 1.0F;
				}

				f12 = f12 * 0.45F;
				r = r * (1.0F - f12) + 0.8F * f12;
				g = g * (1.0F - f12) + 0.8F * f12;
				b = b * (1.0F - f12) + 1.0F * f12;
			}

			return new Vector3(r, g, b);
		}

		public int GetSkyColorByTemp(float currentTemperature)
		{
			currentTemperature = currentTemperature / 3.0F;
			currentTemperature = MathHelper.Clamp(currentTemperature, -1.0F, 1.0F);
			return MathUtils.HsvToRGB(0.62222224F - currentTemperature * 0.05F, 0.5F + currentTemperature * 0.1F, 1.0F);
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
	        SkyRenderer.Draw(args, Camera);

			Graphics.DepthStencilState = DepthStencilState.Default;
            Graphics.SamplerStates[0] = SamplerState.PointWrap;
            
            ChunkManager.Draw(args, Camera);
			EntityManager.Render(args, Camera);

	        if (Camera is ThirdPersonCamera)
	        {
		        Player.ModelRenderer.Render(args, Camera, Player.KnownPosition);
	        }
        }

		public void Render2D(IRenderArgs args)
		{
			EntityManager.Render2D(args, Camera);
		}

		private Stopwatch testWatch = Stopwatch.StartNew();
		public void Update(GameTime gameTime)
		{
			Camera.Update(gameTime, Player);

			if (testWatch.ElapsedMilliseconds >= 50)
			{
				testWatch.Restart();
			//	WorldTime++;

			 	SkyRenderer.Update(gameTime);
			}
			
			ChunkManager.Update(gameTime, SkyRenderer, Camera);
			EntityManager.Update(gameTime, SkyRenderer);

			Player.ModelRenderer.Update(Graphics, gameTime, Player.KnownPosition, SkyRenderer);

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

		#endregion
	}
}
