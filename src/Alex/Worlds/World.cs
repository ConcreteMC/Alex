using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using Alex.API.Blocks.State;
using Alex.API.Data.Options;
using Alex.API.Entities;
using Alex.API.Graphics;
using Alex.API.Network;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.Minecraft;
using Alex.Entities;
using Alex.GameStates;
using Alex.Graphics.Camera;
using Alex.Graphics.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Utils;
using NLog;
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;
using ChunkCoordinates = Alex.API.Utils.ChunkCoordinates;
using Color = Microsoft.Xna.Framework.Color;
using PlayerLocation = Alex.API.Utils.PlayerLocation;

//using System.Reflection.Metadata.Ecma335;

namespace Alex.Worlds
{
	public class World : IWorld, IWorldReceiver
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(World));

		private GraphicsDevice Graphics { get; }
		public Camera Camera { get; set; }

		public LevelInfo WorldInfo { get; set; }

		public Player Player { get; set; }
		private Alex Alex { get; }
		private AlexOptions Options { get; }

		public World(Alex alex, GraphicsDevice graphics, AlexOptions options, Camera camera,
			INetworkProvider networkProvider)
		{
			Alex = alex;
			Graphics = graphics;
			Camera = camera;
			Options = options;

			PhysicsEngine = new PhysicsManager(alex, this);
			ChunkManager = new ChunkManager(alex, graphics, options, this);
			EntityManager = new EntityManager(graphics, this, networkProvider);
			Ticker = new TickManager(this);
			PlayerList = new PlayerList();

			ChunkManager.Start();
			var profileService = alex.Services.GetService<IPlayerProfileService>();
			string username = string.Empty;
			Skin skin = profileService?.CurrentProfile?.Skin;
			if (skin == null)
			{
				alex.Resources.ResourcePack.TryGetBitmap("entity/alex", out Bitmap rawTexture);
				var t = TextureUtils.BitmapToTexture2D(graphics, rawTexture);
				skin = new Skin()
				{
					Texture = t,
					Slim = true
				};
			}

			if (!string.IsNullOrWhiteSpace(profileService?.CurrentProfile?.Username))
			{
				username = profileService.CurrentProfile.Username;
			}

			Player = new Player(graphics, alex, username, this, skin, networkProvider, PlayerIndex.One);

			Player.KnownPosition = new PlayerLocation(GetSpawnPoint());
			Camera.MoveTo(Player.KnownPosition, Vector3.Zero);

			Options.FieldOfVision.ValueChanged += FieldOfVisionOnValueChanged;
			Camera.FOV = Options.FieldOfVision.Value;

			PhysicsEngine.AddTickable(Player);
		}

		private void FieldOfVisionOnValueChanged(int oldvalue, int newvalue)
		{
			Camera.FOV = newvalue;
		}

		//public long WorldTime { get; private set; } = 6000;
		public bool FreezeWorldTime { get; set; } = false;

		public PlayerList PlayerList { get; }
		public TickManager Ticker { get; }
		public EntityManager EntityManager { get; }
		public ChunkManager ChunkManager { get; private set; }
		public PhysicsManager PhysicsEngine { get; set; }

		public long Vertices
        {
            get { return ChunkManager.Vertices; }
        }

		public int IndexBufferSize
		{
			get { return ChunkManager.IndexBufferSize; }
		}

		public int ChunkCount
        {
            get { return ChunkManager.ChunkCount; }
        }

		public int ConcurrentChunkUpdates
        {
            get { return ChunkManager.ConcurrentChunkUpdates; }
        }

		public int EnqueuedChunkUpdates
		{
			get { return ChunkManager.EnqueuedChunkUpdates; }
		}

		public void ResetChunks()
        {
            ChunkManager.ClearChunks();
        }

        public void RebuildChunks()
        {
            ChunkManager.RebuildAll();
        }

        public void ToggleWireFrame()
        {
	        ChunkManager.UseWireFrames = !ChunkManager.UseWireFrames;
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

		private float _fovModifier = -1;
		private bool UpdatingPriorities = false;
		private float BrightnessMod = 0f;
        public void Update(UpdateArgs args, SkyBox skyRenderer)
		{
			args.Camera = Camera;
			if (Player.FOVModifier != _fovModifier)
			{
				_fovModifier = Player.FOVModifier;

				Camera.FOV += _fovModifier;
				Camera.UpdateProjectionMatrix();
				Camera.FOV -= _fovModifier;
			}
			Camera.Update(args, Player);

			BrightnessMod = skyRenderer.BrightnessModifier;
			ChunkManager.Update(args);
			EntityManager.Update(args, skyRenderer);
			PhysicsEngine.Update(args.GameTime);
			
			var diffuseColor = Color.White.ToVector3() * BrightnessModifier;
			ChunkManager.AmbientLightColor = diffuseColor;

			Player.ModelRenderer.DiffuseColor = diffuseColor;
			Player.Update(args);

			if (Player.IsInWater)
			{
				ChunkManager.FogColor = new Vector3(0.2666667F, 0.6862745F, 0.9607844F) * BrightnessModifier;
				ChunkManager.FogDistance = (float)Math.Pow(Options.VideoOptions.RenderDistance, 2) * 0.15f;
			}
			else
			{
				ChunkManager.FogColor = skyRenderer.WorldFogColor.ToVector3();
				ChunkManager.FogDistance = (float) Math.Pow(Options.VideoOptions.RenderDistance, 2) * 0.8f;
			}
			
			if (Ticker.Update(args))
			{
				if (!FreezeWorldTime)
				{
					WorldInfo.Time++;
				}
			}
		}
        
        public Vector3 SpawnPoint { get; set; } = Vector3.Zero;

        public float BrightnessModifier
        {
	        get { return (BrightnessMod + ((Options.VideoOptions.Brightness - 50f) * (0.5f / 100f))); }
        }

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
            if (y < 0 || y > ChunkColumn.ChunkHeight) return 16;

			IChunkColumn chunk;
	        if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
	        {
				return chunk.GetSkylight(x & 0xf, y & 0xff, z & 0xf);
            }
            return 16;
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
	      //  try
	      //  {
		        IChunkColumn chunk;
		        if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
		        {
			        return chunk.GetBlock(x & 0xf, y & 0xff, z & 0xf);
		        }
			//}
		//	catch { }

	        return BlockFactory.GetBlock(0);
        }

		public void SetBlock(Block block)
		{
			var x = block.Coordinates.X;
			var y = block.Coordinates.Y;
			var z = block.Coordinates.Z;

			var chunkCoords = new ChunkCoordinates(x >> 4, z >> 4);

            IChunkColumn chunk;
			if (ChunkManager.TryGetChunk(chunkCoords, out chunk))
			{
				var cx = x & 0xf;
				var cy = y & 0xff;
				var cz = z & 0xf;

                chunk.SetBlock(cx, cy, cz, block);
                ChunkManager.ScheduleChunkUpdate(new ChunkCoordinates(x >> 4, z >> 4), ScheduleType.Scheduled | ScheduleType.Lighting, true);

				UpdateNeighbors(x, y, z);
				CheckForUpdate(chunkCoords, cx, cz);
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
			    var cx = x & 0xf;
			    var cy = y & 0xff;
			    var cz = z & 0xf;

                chunk.SetBlock(cx, cy, cz, block);

                UpdateNeighbors(x, y, z);
			    CheckForUpdate(chunkCoords, cx, cz);
			    
			    ChunkManager.ScheduleChunkUpdate(chunkCoords, ScheduleType.Scheduled | ScheduleType.Lighting, true);
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

                UpdateNeighbors(x,y,z);
				CheckForUpdate(chunkCoords, cx, cz);
				
				ChunkManager.ScheduleChunkUpdate(chunkCoords, ScheduleType.Scheduled | ScheduleType.Lighting, true);
            }
		}

		private void CheckForUpdate(ChunkCoordinates chunkCoords, int cx, int cz)
		{
			if (cx == 0)
			{
				ChunkManager.ScheduleChunkUpdate(chunkCoords - new ChunkCoordinates(1, 0), ScheduleType.Border | ScheduleType.Lighting, true);
			}
			else if (cx == 0xf)
			{
				ChunkManager.ScheduleChunkUpdate(chunkCoords + new ChunkCoordinates(1, 0), ScheduleType.Border | ScheduleType.Lighting, true);
			}

			if (cz == 0)
			{
				ChunkManager.ScheduleChunkUpdate(chunkCoords - new ChunkCoordinates(0, 1), ScheduleType.Border | ScheduleType.Lighting, true);
			}
			else if (cz == 0xf)
			{
				ChunkManager.ScheduleChunkUpdate(chunkCoords + new ChunkCoordinates(0, 1), ScheduleType.Border | ScheduleType.Lighting, true);
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
				GetBlockState(block).Block.BlockUpdate(this, block, updatedBlock);
				//GetBlock(block).BlockUpdate(this, block, updatedBlock);
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
		
		public IBlockState GetBlockState(BlockCoordinates coords)
		{
			return GetBlockState(coords.X, coords.Y, coords.Z);
		}

		public int GetBiome(int x, int y, int z)
		{
			IChunkColumn chunk;
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
			{
				Worlds.ChunkColumn realColumn = (Worlds.ChunkColumn) chunk;
				return	realColumn.GetBiome(x & 0xf, z & 0xf);
			}

			//Log.Debug($"Failed getting biome: {x} | {y} | {z}");
			return -1;
		}

		public bool HasBlock(int x, int y, int z)
		{
			
			IChunkColumn chunk;
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
			{
				//Worlds.ChunkColumn realColumn = (Worlds.ChunkColumn)chunk;
				return true;
			}

			return false;
		}


		public bool IsTransparent(int x, int y, int z)
		{
			IChunkColumn chunk;
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
			{
				return chunk.IsTransparent(x & 0xf, y & 0xff, z & 0xf);
              //  return true;
			}

			return true;
        }

		public bool IsSolid(int x, int y, int z)
		{
			IChunkColumn chunk;
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
			{
				return chunk.IsSolid(x & 0xf, y & 0xff, z & 0xf);
				//  return true;
			}

			return false;
		}

	    public bool IsScheduled(int x, int y, int z)
	    {
	        IChunkColumn chunk;
	        if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
	        {
	            return chunk.IsScheduled(x & 0xf, y & 0xff, z & 0xf);
	            //  return true;
	        }

	        return false;
        }

	    public void GetBlockData(int x, int y, int z, out bool transparent, out bool solid)
		{
			IChunkColumn chunk;
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
			{
				chunk.GetBlockData(x & 0xf, y & 0xff, z & 0xf, out transparent, out solid);
				//return chunk.IsSolid(x & 0xf, y & 0xff, z & 0xf);
				//  return true;
				return;
			}

			transparent = false;
			solid = false;
			//return false;
		}

        public BlockCoordinates FindBlockPosition(BlockCoordinates coords, out IChunkColumn chunk)
		{
			ChunkManager.TryGetChunk(new ChunkCoordinates(coords.X >> 4, coords.Z >> 4), out chunk);
			return new BlockCoordinates(coords.X & 0xf, coords.Y & 0xff, coords.Z & 0xf);
		}
        private bool _destroyed = false;
		public void Destroy()
		{
			if (_destroyed) return;
			_destroyed = true;

			PhysicsEngine.Stop();
			EntityManager.Dispose();
			ChunkManager.Dispose();

			PhysicsEngine.Dispose();
			
		}

		#region IWorldReceiver (Handle WorldProvider callbacks)

		public IEntity GetPlayerEntity()
		{
			return Player;
		}

		public IChunkColumn GetChunkColumn(int x, int z)
		{
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x, z), out IChunkColumn val))
			{
				return val;
			}
			else
			{
				return null;
			}
		}

		public void ChunkReceived(IChunkColumn chunkColumn, int x, int z, bool update)
		{
			var c = new ChunkCoordinates(x, z);

			ChunkManager.AddChunk(chunkColumn, c, update);
            //InitiateChunk(chunkColumn as ChunkColumn);
        }

		public void ChunkUnload(int x, int z)
		{
			var chunkCoordinates = new ChunkCoordinates(x, z);
			ChunkManager.RemoveChunk(chunkCoordinates);

			EntityManager.UnloadEntities(chunkCoordinates);
		}

		public void ChunkUpdate(IChunkColumn chunkColumn, ScheduleType type = ScheduleType.Lighting)
		{
			ChunkManager.ScheduleChunkUpdate(new ChunkCoordinates(chunkColumn.X, chunkColumn.Z), type);
		}

		public void SpawnEntity(long entityId, IEntity entity)
		{
			if (EntityManager.AddEntity(entityId, (Entity) entity))
			{
				PhysicsEngine.AddTickable((Entity) entity);
			}
		}

		public void SpawnEntity(long entityId, Entity entity)
		{
			if (EntityManager.AddEntity(entityId, entity))
			{
				PhysicsEngine.AddTickable(entity);
			}
			//Log.Info($"Spawned entity {entityId} : {entity} at {entity.KnownPosition} with renderer {entity.GetModelRenderer()}");
		}

		public void DespawnEntity(long entityId)
		{
			if (EntityManager.TryGet(entityId, out IEntity entity))
			{
				PhysicsEngine.Remove(entity);
				entity.Dispose();
			}

			EntityManager.Remove(entityId);
			//	Log.Info($"Despawned entity {entityId}");
		}

		public void UpdatePlayerPosition(PlayerLocation location)
		{
			Player.KnownPosition = location;
		}

		public void UpdateEntityPosition(long entityId, PlayerLocation position, bool relative = false, bool updateLook = false, bool updatePitch = false)
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
					entity.KnownPosition.X += position.X;
					entity.KnownPosition.Y += position.Y;
					entity.KnownPosition.Z += position.Z;	
					//entity.KnownPosition.Move(position);
				}

				if (updateLook)
				{
					//entity.KnownPosition.Yaw = position.Yaw;
					if (updatePitch)
					{
						entity.KnownPosition.Pitch = position.Pitch;
					}

					entity.KnownPosition.HeadYaw = position.HeadYaw;
					//	entity.UpdateHeadYaw(position.HeadYaw);
				}
            }
		}

		public void UpdateEntityLook(long entityId, float yaw, float pitch, bool onGround)
		{
			if (EntityManager.TryGet(entityId, out IEntity entity))
			{
				entity.KnownPosition.OnGround = onGround;
				entity.KnownPosition.Pitch = pitch;
				entity.KnownPosition.HeadYaw = yaw;
			}
		}

		public bool TryGetEntity(long entityId, out IEntity entity)
		{
			return EntityManager.TryGet(entityId, out entity);
		}

		public void SetTime(long worldTime)
		{
			WorldInfo.Time = worldTime;
		}

		public void SetRain(bool raining)
		{
			WorldInfo.Raining = raining;
		}

		public void SetBlockState(BlockCoordinates coordinates, IBlockState blockState)
		{
			SetBlockState(coordinates.X, coordinates.Y, coordinates.Z, blockState);
		}

		public void AddPlayerListItem(PlayerListItem item)
		{
			PlayerList.Entries.Add(item.UUID, item);
		}

		public void RemovePlayerListItem(UUID item)
		{
			PlayerList.Entries.Remove(item);
		}

		#endregion
	}
}
