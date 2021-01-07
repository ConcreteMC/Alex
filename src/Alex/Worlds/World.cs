using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using Alex.API;
using Alex.API.Data.Options;
using Alex.API.Data.Servers;
using Alex.API.Graphics;
using Alex.API.Gui;
using Alex.API.Network;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Blocks.Storage;
using Alex.Entities;
using Alex.Entities.BlockEntities;
using Alex.Gamestates;
using Alex.Graphics.Camera;
using Alex.Graphics.Models;
using Alex.Graphics.Models.Entity;
using Alex.Graphics.Models.Items;
using Alex.Net;
using Alex.Utils;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Chunks;
using Alex.Worlds.Lighting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET;
using MiNET.Utils;
using NLog;
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;
using ChunkCoordinates = Alex.API.Utils.ChunkCoordinates;
using Color = Microsoft.Xna.Framework.Color;
using Inventory = MiNET.Inventory;
using MathF = System.MathF;
using Player = Alex.Entities.Player;
using PlayerLocation = Alex.API.Utils.PlayerLocation;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Skin = Alex.API.Utils.Skin;
using UUID = Alex.API.Utils.UUID;

//using System.Reflection.Metadata.Ecma335;

namespace Alex.Worlds
{
	public enum Dimension
	{
		Overworld,
		Nether,
		TheEnd,
	}
	
	public class World : IBlockAccess, ITicked
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(World));

		private GraphicsDevice Graphics { get; }
		public EntityCamera Camera { get; }

		public Player Player { get; set; }
		private AlexOptions Options { get; }
		
		public InventoryManager InventoryManager { get; }
		private SkyBox SkyRenderer { get; }

		public long Time      { get; set; } = 1;
		public long TimeOfDay { get; set; } = 1;
		public bool Raining   { get; set; } = false;
		
		public bool DrowningDamage      { get; set; } = true;
		public bool CommandblockOutput  { get; set; } = true;
		public bool DoTiledrops         { get; set; } = true;
		public bool DoMobloot           { get; set; } = true;
		public bool KeepInventory       { get; set; } = true;
		public bool DoDaylightcycle     { get; set; } = true;
		public bool DoMobspawning       { get; set; } = true;
		public bool DoEntitydrops       { get; set; } = true;
		public bool DoFiretick          { get; set; } = true;
		public bool DoWeathercycle      { get; set; } = true;
		public bool Pvp                 { get; set; } = true;
		public bool Falldamage          { get; set; } = true;
		public bool Firedamage          { get; set; } = true;
		public bool Mobgriefing         { get; set; } = true;
		public bool ShowCoordinates     { get; set; } = true;
		public bool NaturalRegeneration { get; set; } = true;
		public bool TntExplodes         { get; set; } = true;
		public bool SendCommandfeedback { get; set; } = true;
		public bool InstantRespawn      { get; set; } = false;
		public int  RandomTickSpeed     { get; set; } = 3;

		private Dimension _dimension = Dimension.Overworld;

		public Dimension Dimension
		{
			get
			{
				return _dimension;
			}
			set
			{
				_dimension = value;
			}
		}

		public BackgroundWorker BackgroundWorker { get; }
		public World(IServiceProvider serviceProvider, GraphicsDevice graphics, AlexOptions options,
			NetworkProvider networkProvider)
		{
			Graphics = graphics;
			Options = options;

			PhysicsEngine = new PhysicsManager(this);
			ChunkManager = new ChunkManager(serviceProvider, graphics, this);
			EntityManager = new EntityManager(graphics, this, networkProvider);
			Ticker = new TickManager();
			PlayerList = new PlayerList();

			Ticker.RegisterTicked(this);
			Ticker.RegisterTicked(EntityManager);
			Ticker.RegisterTicked(PhysicsEngine);
			Ticker.RegisterTicked(ChunkManager);
			
			ChunkManager.Start();
			var profileService = serviceProvider.GetRequiredService<IPlayerProfileService>();
			var resources = serviceProvider.GetRequiredService<ResourceManager>();
			
			string username = string.Empty;
			PooledTexture2D texture;
			
			if (Alex.PlayerTexture != null)
			{
				texture = TextureUtils.BitmapToTexture2D(graphics, Alex.PlayerTexture);
			}
			else
			{
				resources.TryGetBitmap("entity/alex", out var rawTexture);
				texture = TextureUtils.BitmapToTexture2D(graphics, rawTexture);
			}
			
			Skin skin = profileService?.CurrentProfile?.Skin;
			if (skin == null)
			{
				skin = new Skin()
				{
					Texture = texture,
					Slim = true
				};
			}

			if (!string.IsNullOrWhiteSpace(profileService?.CurrentProfile?.Username))
			{
				username = profileService.CurrentProfile.Username;
			}

			Player = new Player(graphics, serviceProvider.GetRequiredService<Alex>().InputManager, username, this, skin, networkProvider, PlayerIndex.One);
			Camera = new EntityCamera(Player);
			
			if (Alex.PlayerModel != null)
			{
				EntityModelRenderer modelRenderer = new EntityModelRenderer(Alex.PlayerModel, texture);

				if (modelRenderer.Valid)
				{
					Player.ModelRenderer = modelRenderer;
				}
			}
			
			Player.KnownPosition = new PlayerLocation(GetSpawnPoint());

			Options.FieldOfVision.ValueChanged += FieldOfVisionOnValueChanged;
			Camera.FOV = Options.FieldOfVision.Value;

			PhysicsEngine.AddTickable(Player);

			var guiManager = serviceProvider.GetRequiredService<GuiManager>();
			InventoryManager = new InventoryManager(guiManager);
				
			SkyRenderer = new SkyBox(serviceProvider, graphics, this);
			
			options.VideoOptions.RenderDistance.Bind(
				(old, newValue) =>
				{
					Camera.SetRenderDistance(newValue);
				});
			Camera.SetRenderDistance(options.VideoOptions.RenderDistance);

			BackgroundWorker = new BackgroundWorker();
		}

		private void FieldOfVisionOnValueChanged(int oldvalue, int newvalue)
		{
			Camera.FOV = newvalue;
		}

		//public long WorldTime { get; private set; } = 6000;

		public PlayerList     PlayerList    { get; }
		public TickManager    Ticker        { get; }
		public EntityManager  EntityManager { get; set; }
		public ChunkManager   ChunkManager  { get; private set; }
		public PhysicsManager PhysicsEngine { get; set; }

		public long Vertices
        {
            get { return ChunkManager.Vertices + EntityManager.VertexCount; }
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
          //  ChunkManager.RebuildAll();
        }

        public void ToggleWireFrame()
        {
	        ChunkManager.UseWireFrames = !ChunkManager.UseWireFrames;
        }
        
        public void Render(IRenderArgs args)
        {
	        if (_destroyed)
		        return;
	        
	        Graphics.DepthStencilState = DepthStencilState.Default;
	        Graphics.SamplerStates[0] = SamplerState.PointWrap;

            SkyRenderer.Draw(args);
            
            ChunkManager.Draw(args,
	            RenderStage.OpaqueFullCube,
	            RenderStage.Opaque);
            
            EntityManager.Render(args);
            
            ChunkManager.Draw(args, 
	            RenderStage.Transparent,
	            RenderStage.Translucent,
	            RenderStage.Animated,
	        //    RenderStage.AnimatedTranslucent,
	            RenderStage.Liquid);

	        Player.Render(args);
        }

        public void Render2D(IRenderArgs args)
        {
	        if (_destroyed)
		        return;
	        
	        EntityManager.Render2D(args);
        }
        
	//	private float _fovModifier  = -1;
		private float _brightnessMod = 0f;
		public void Update(UpdateArgs args)
		{
			if (_destroyed)
				return;
			
			var camera = Camera;
			
			args.Camera = camera;
			/*if (Math.Abs(Player.FOVModifier - _fovModifier) > 0f)
			{
				_fovModifier = Player.FOVModifier;

				camera.FOV += _fovModifier;
				camera.UpdateProjectionMatrix();
				camera.FOV -= _fovModifier;
			}*/
			camera.Update(args);

			//_brightnessMod = SkyRenderer.BrightnessModifier;
			
			SkyRenderer.Update(args);
			ChunkManager.Update(args);
			
			EntityManager.Update(args);
			PhysicsEngine.Update(args.GameTime);

			if (Math.Abs(_brightnessMod - SkyRenderer.BrightnessModifier) > 0f)
			{
				_brightnessMod = SkyRenderer.BrightnessModifier;
				
				var diffuseColor = Color.White.ToVector3() * SkyRenderer.BrightnessModifier;
				ChunkManager.AmbientLightColor = diffuseColor;

				if (Math.Abs(ChunkManager.Shaders.BrightnessModifier - SkyRenderer.BrightnessModifier) > 0f)
				{
					ChunkManager.Shaders.BrightnessModifier = SkyRenderer.BrightnessModifier;
				}
				
				var modelRenderer = Player?.ModelRenderer;

				if (modelRenderer != null)
				{
					modelRenderer.DiffuseColor = diffuseColor;
				}
			}

			Player.Update(args);
		}

		public void OnTick()
		{
			Player?.OnTick();

			Time++;
			
			if (DoDaylightcycle)
			{
				var tod = TimeOfDay;
				TimeOfDay = ((tod + 1) % 24000);
			}
		}
		
        public Vector3 SpawnPoint { get; set; } = Vector3.Zero;

        public float BrightnessModifier
        {
	        get { return (_brightnessMod + ((Options.VideoOptions.Brightness - 50f) * (0.5f / 100f))); }
        }

        public Vector3 GetSpawnPoint()
        {
	        return SpawnPoint;
        }

        public ChunkColumn GetChunk(BlockCoordinates coordinates, bool cacheOnly = false)
        {
	        if (ChunkManager.TryGetChunk(new ChunkCoordinates(coordinates), out var c))
	        {
		        return (ChunkColumn) c;
	        }

	        return null;
        }

        public ChunkColumn GetChunk(ChunkCoordinates coordinates, bool cacheOnly = false)
        {
	        if (ChunkManager.TryGetChunk(coordinates, out var c))
	        {
		        return (ChunkColumn) c;
	        }

	        return null;
        }

        public void SetSkyLight(BlockCoordinates coordinates, byte p1)
        {
	        var         chunkCoords = new ChunkCoordinates(coordinates);
	        ChunkColumn chunk;
	        if (ChunkManager.TryGetChunk(chunkCoords, out chunk))
	        {
		        if (chunk.SetSkyLight(coordinates.X & 0xf, coordinates.Y & 0xff, coordinates.Z & 0xf, p1))
		        {
			       // if ((chunk.Scheduled & ScheduleType.Lighting) != ScheduleType.Lighting)
			        {
				       // ChunkManager.ScheduleChunkUpdate(chunkCoords, ScheduleType.Lighting);
			        }
			       // else
			        {
				       // chunk.Scheduled = chunk.Scheduled | ScheduleType.Lighting;
			        }
		        }
	        }
        }
        
        public byte GetSkyLight(BlockCoordinates position)
        {
	        return GetSkyLight(position.X, position.Y, position.Z);
        }

        public byte GetBlockLight(BlockCoordinates coordinates)
        {
	        return GetBlockLight(coordinates.X, coordinates.Y, coordinates.Z);
        }
        
        public byte GetSkyLight(int x, int y, int z)
        {
            if (y < 0 || y > ChunkColumn.ChunkHeight) return 15;
            
            ChunkColumn chunk;
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
            if (y < 0 || y > ChunkColumn.ChunkHeight) return 0;

			ChunkColumn chunk;
	        if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
	        {
                return chunk.GetBlocklight(x & 0xf, y & 0xff, z & 0xf);
            }
            return 0;
        }

        public void SetBlockEntity(int x, int y, int z, BlockEntity blockEntity)
		{
			var coords      = new BlockCoordinates(x, y, z);
			var chunkCoords = new ChunkCoordinates(x >> 4, z >> 4);

			ChunkColumn chunk;

			if (ChunkManager.TryGetChunk(chunkCoords, out chunk))
			{
				var cx       = x & 0xf;
				var cy       = y & 0xff;
				var cz       = z & 0xf;
				
				var chunkPos   = new BlockCoordinates(cx, cy, cz);
			//	var blockAtPos = chunk.GetBlockState(cx, cy, cz);

				//if (blockAtPos.Block.BlockMaterial == Material.Air)
				//	return;
				
				chunk.RemoveBlockEntity(chunkPos);
				EntityManager.RemoveBlockEntity(coords);
				
				chunk.AddBlockEntity(chunkPos, blockEntity);
				EntityManager.AddBlockEntity(coords, blockEntity);
			}
		}
		
		public void SetBlockState(int x, int y, int z, BlockState block, BlockUpdatePriority priority = BlockUpdatePriority.High)
		{
			SetBlockState(x, y, z, block, 0, priority);
		}
		
		public void SetBlockState(int x, int y, int z, BlockState block, int storage, BlockUpdatePriority priority = BlockUpdatePriority.High | BlockUpdatePriority.Neighbors)
		{
			var chunkCoords = new ChunkCoordinates(x >> 4, z >> 4);

			ChunkColumn chunk;
			if (ChunkManager.TryGetChunk(chunkCoords, out chunk))
			{
				var cx = x & 0xf;
				var cy = y & 0xff;
				var cz = z & 0xf;

				chunk.SetBlockState(cx, cy, cz, block, storage);

				EntityManager.RemoveBlockEntity(new BlockCoordinates(x, y, z));
				
				var type = ScheduleType.Full;
				
				if ((priority & BlockUpdatePriority.Neighbors) != 0)
				{
					UpdateNeighbors(x, y, z);
					CheckForUpdate(chunkCoords, cx, cz);
				}

				if ((priority & BlockUpdatePriority.NoGraphic) != 0)
				{
					type |= ScheduleType.LowPriority;
				}

				//chunk.SetDirty();
				//chunk.IsDirty = true;
				ChunkManager.ScheduleChunkUpdate(chunkCoords, type, (priority & BlockUpdatePriority.Priority) != 0);
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

			if (Options.VideoOptions.ClientSideLighting && Dimension == Dimension.Overworld)
			{
				new SkyLightCalculations().Calculate(this, source);
			}

			ScheduleBlockUpdate(source, new BlockCoordinates(x + 1, y, z));
			ScheduleBlockUpdate(source, new BlockCoordinates(x - 1, y, z));

			ScheduleBlockUpdate(source, new BlockCoordinates(x, y, z + 1));
			ScheduleBlockUpdate(source, new BlockCoordinates(x, y, z - 1));

			ScheduleBlockUpdate(source, new BlockCoordinates(x, y + 1, z));
			ScheduleBlockUpdate(source, new BlockCoordinates(x, y - 1, z));
		}

		public void ScheduleBlockUpdate(BlockCoordinates coordinates)
		{
			var chunkCoords = new ChunkCoordinates(coordinates.X >> 4, coordinates.Z >> 4);
			
			ChunkColumn chunk;
			if (ChunkManager.TryGetChunk(chunkCoords, out chunk))
			{
				var cx = coordinates.X & 0xf;
				var cy = coordinates.Y & 0xff;
				var cz = coordinates.Z & 0xf;

				chunk.ScheduleBlockUpdate(cx, cy, cz);
			}
		}
		
		private void ScheduleBlockUpdate(BlockCoordinates updatedBlock, BlockCoordinates block)
		{
			ScheduleBlockUpdate(block);
			Ticker.ScheduleTick(() =>
			{
				GetBlockState(block).Block.BlockUpdate(this, block, updatedBlock);
				//GetBlock(block).BlockUpdate(this, block, updatedBlock);
			}, 1);
		}

		public IEnumerable<ChunkSection.BlockEntry> GetBlockStates(int x, int y, int z)
		{
			ChunkColumn chunk;
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
			{
				foreach (var bs in chunk.GetBlockStates(x & 0xf, y & 0xff, z & 0xf))
				{
					yield return bs;
				}
			}
			
			yield break;
		}

		private static BlockState Airstate = BlockFactory.GetBlockState("minecraft:air");
		public BlockState GetBlockState(int x, int y, int z, int storage)
		{
			ChunkColumn chunk;
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
			{
				return chunk.GetBlockState(x & 0xf, y & 0xff, z & 0xf, storage);
			}

			return Airstate;
		}
		
		public BlockState GetBlockState(int x, int y, int z)
		{
			return GetBlockState(x, y, z, 0);
		}

		public BlockState GetBlockState(BlockCoordinates coords)
		{
			return GetBlockState(coords.X, coords.Y, coords.Z);
		}

		public int GetHeight(BlockCoordinates coords)
		{
			ChunkColumn chunk;
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(coords.X >> 4, coords.Z >> 4), out chunk))
			{
				//Worlds.ChunkColumn realColumn = (Worlds.ChunkColumn)chunk;
				return chunk.GetHeight(coords.X & 0xf, coords.Z & 0xf);
			}

			return 255;
		}

		/// <inheritdoc />
		public Biome GetBiome(BlockCoordinates coordinates)
		{
			return BiomeUtils.GetBiomeById(GetBiome(coordinates.X, coordinates.Y, coordinates.Z));
		}
		
		public int GetBiome(int x, int y, int z)
		{
			ChunkColumn chunk;
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
			{
				ChunkColumn realColumn = (ChunkColumn) chunk;
				return	realColumn.GetBiome(x & 0xf, y & 0xff, z & 0xf);
			}

			//Log.Debug($"Failed getting biome: {x} | {y} | {z}");
			return -1;
		}

		public void SetGameRule(MiNET.GameRule gameRule)
        {
	        if (Enum.TryParse(gameRule.Name, out GameRulesEnum val))
	        {
		        if (gameRule is GameRule<bool> grb)
		        {
			        SetGameRule(val, grb.Value);
		        }
		        else if (gameRule is GameRule<int> gri)
		        {
			        SetGameRule(val, gri.Value);
		        }
	        }
        }
        
		public void SetGameRule(GameRulesEnum rule, bool value)
		{
			switch (rule)
			{
				case GameRulesEnum.DrowningDamage:
					DrowningDamage = value;
					break;
				case GameRulesEnum.CommandblockOutput:
					CommandblockOutput = value;
					break;
				case GameRulesEnum.DoTiledrops:
					DoTiledrops = value;
					break;
				case GameRulesEnum.DoMobloot:
					DoMobloot = value;
					break;
				case GameRulesEnum.KeepInventory:
					KeepInventory = value;
					break;
				case GameRulesEnum.DoDaylightcycle:
					DoDaylightcycle = value;
					break;
				case GameRulesEnum.DoMobspawning:
					DoMobspawning = value;
					break;
				case GameRulesEnum.DoEntitydrops:
					DoEntitydrops = value;
					break;
				case GameRulesEnum.DoFiretick:
					DoFiretick = value;
					break;
				case GameRulesEnum.DoWeathercycle:
					DoWeathercycle = value;
					break;
				case GameRulesEnum.Pvp:
					Pvp = value;
					break;
				case GameRulesEnum.Falldamage:
					Falldamage = value;
					break;
				case GameRulesEnum.Firedamage:
					Firedamage = value;
					break;
				case GameRulesEnum.Mobgriefing:
					Mobgriefing = value;
					break;
				case GameRulesEnum.ShowCoordinates:
					ShowCoordinates = value;
					break;
				case GameRulesEnum.NaturalRegeneration:
					NaturalRegeneration = value;
					break;
				case GameRulesEnum.TntExplodes:
					TntExplodes = value;
					break;
				case GameRulesEnum.SendCommandfeedback:
					SendCommandfeedback = value;
					break;
				case GameRulesEnum.DoImmediateRespawn:
					InstantRespawn = value;
					break;
			}
		}

		public void SetGameRule(GameRulesEnum rule, int value)
		{
			switch (rule)
			{
				case GameRulesEnum.DrowningDamage:
					RandomTickSpeed = value;
					break;
			}
		}
		
		public bool GetGameRule(GameRulesEnum rule)
		{
			switch (rule)
			{
				case GameRulesEnum.DoImmediateRespawn:
					return InstantRespawn;
				case GameRulesEnum.DrowningDamage:
					return DrowningDamage;
				case GameRulesEnum.CommandblockOutput:
					return CommandblockOutput;
				case GameRulesEnum.DoTiledrops:
					return DoTiledrops;
				case GameRulesEnum.DoMobloot:
					return DoMobloot;
				case GameRulesEnum.KeepInventory:
					return KeepInventory;
				case GameRulesEnum.DoDaylightcycle:
					return DoDaylightcycle;
				case GameRulesEnum.DoMobspawning:
					return DoMobspawning;
				case GameRulesEnum.DoEntitydrops:
					return DoEntitydrops;
				case GameRulesEnum.DoFiretick:
					return DoFiretick;
				case GameRulesEnum.DoWeathercycle:
					return DoWeathercycle;
				case GameRulesEnum.Pvp:
					return Pvp;
				case GameRulesEnum.Falldamage:
					return Falldamage;
				case GameRulesEnum.Firedamage:
					return Firedamage;
				case GameRulesEnum.Mobgriefing:
					return Mobgriefing;
				case GameRulesEnum.ShowCoordinates:
					return ShowCoordinates;
				case GameRulesEnum.NaturalRegeneration:
					return NaturalRegeneration;
				case GameRulesEnum.TntExplodes:
					return TntExplodes;
				case GameRulesEnum.SendCommandfeedback:
					return SendCommandfeedback;
			}

			return false;
		}
        
        private bool _destroyed = false;
		public void Destroy()
		{
			if (_destroyed) return;
			_destroyed = true;
			
			Ticker.UnregisterTicked(this);
			Ticker.UnregisterTicked(EntityManager);
			Ticker.UnregisterTicked(PhysicsEngine);
			Ticker.UnregisterTicked(ChunkManager);

			BackgroundWorker?.Dispose();

			EntityManager.Dispose();
			EntityManager = null;
			
			ChunkManager.Dispose();
			ChunkManager = null;

			Player.Dispose();
			Ticker.Dispose();
		}

		#region IWorldReceiver (Handle WorldProvider callbacks)

		public ChunkColumn GetChunkColumn(int x, int z)
		{
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x, z), out ChunkColumn val))
			{
				return val;
			}
			else
			{
				return null;
			}
		}

		public void ClearChunksAndEntities()
		{
			EntityManager.ClearEntities();
			ChunkManager.ClearChunks();
		}
	
		public void UnloadChunk(ChunkCoordinates coordinates)
		{
			ChunkManager.RemoveChunk(coordinates);
			EntityManager.UnloadEntities(coordinates);
		}

		public bool SpawnEntity(long entityId, Entity entity)
		{
			if (EntityManager.AddEntity(entityId, entity))
			{
				//entity.RenderLocation = entity.KnownPosition;
				if (entity.HasPhysics)
				{
					PhysicsEngine.AddTickable(entity);
				}

				entity.OnSpawn();
				return true;
			}

			return false;
			//Log.Info($"Spawned entity {entityId} : {entity} at {entity.KnownPosition} with renderer {entity.GetModelRenderer()}");
		}

		public void DespawnEntity(long entityId)
		{
			BackgroundWorker.Enqueue(
				() =>
				{
					if (EntityManager.TryGet(entityId, out Entity entity))
					{
						if (entity.HasPhysics)
						{
							PhysicsEngine.Remove(entity);
						}

						EntityManager.Remove(entityId);

						entity.OnDespawn();
						//entity.Dispose();
					}
				});
			//	Log.Info($"Despawned entity {entityId}");
		}

		public void UpdatePlayerPosition(PlayerLocation location)
		{
		//	var oldPosition = Player.KnownPosition;
			
			if (!ChunkManager.TryGetChunk(new ChunkCoordinates(location), out _))
			{
				Player.WaitingOnChunk = true;
			}

			Player.Movement.MoveTo(location);
			//Player.KnownPosition = location;
			
			//Player.DistanceMoved += MathF.Abs(Vector3.Distance(oldPosition, location));
		}

		public void UpdateEntityPosition(long entityId, PlayerLocation position, bool relative = false, bool updateLook = false, bool updatePitch = false, bool teleport = false)
		{
			if (EntityManager.TryGet(entityId, out Entity entity))
			{
				entity.KnownPosition.OnGround = position.OnGround;
				
				if (updateLook)
				{
					if (updatePitch)
					{
						entity.KnownPosition.Pitch = position.Pitch;
					}
						
					entity.KnownPosition.Yaw = position.Yaw;
					entity.KnownPosition.HeadYaw = position.HeadYaw;
					//	entity.UpdateHeadYaw(position.HeadYaw);
				}
				
				if (relative)
				{
					//var adjusted = entity 
					entity.Movement.Move(position.ToVector3());
				}
				else
				{
					entity.Movement.MoveTo(position, false);
				}
				
				entity.Velocity = Vector3.Zero;
			}
		}

		public void UpdateEntityLook(long entityId, float yaw, float pitch, bool onGround)
		{
			if (EntityManager.TryGet(entityId, out Entity entity))
			{
				entity.KnownPosition.OnGround = onGround;
				entity.KnownPosition.Pitch = pitch;
				entity.KnownPosition.HeadYaw = yaw;
			}
		}

		public bool TryGetEntity(long entityId, out Entity entity)
		{
			return EntityManager.TryGet(entityId, out entity);
		}

		public void SetTime(long worldTime, long timeOfDay)
		{
			Time = worldTime;//timeOfDay;
			TimeOfDay = Math.Abs(timeOfDay);
		}

		public void SetRain(bool raining)
		{
			Raining = raining;
		}

		public void SetBlockState(BlockCoordinates coordinates, BlockState blockState, BlockUpdatePriority priority = BlockUpdatePriority.High)
		{
			SetBlockState(coordinates.X, coordinates.Y, coordinates.Z, blockState, priority);
		}

		public void SetBlockState(BlockCoordinates coordinates, BlockState blockState, int storage, BlockUpdatePriority priority = BlockUpdatePriority.High)
		{
			SetBlockState(coordinates.X, coordinates.Y, coordinates.Z, blockState, storage, priority);
		}

		public void AddPlayerListItem(PlayerListItem item)
		{
			PlayerList.Entries.TryAdd(item.UUID, item);
		}

		public void RemovePlayerListItem(MiNET.Utils.UUID item)
		{
			PlayerList.Entries.Remove(item);
		}

		public void UpdatePlayerLatency(MiNET.Utils.UUID uuid, int latency)
		{
			if (Player.UUID.Equals(uuid))
			{
				Player.Latency = latency;
			}
			else if (EntityManager.TryGet(uuid, out var player))
			{
				if (player is RemotePlayer p)
				{
					p.Latency = latency;
				}
			}
			
			if (PlayerList.Entries.TryGetValue(uuid, out var item))
			{
				item.Ping = latency;
			}
		}
		
		public void UpdatePlayerListDisplayName(MiNET.Utils.UUID uuid, string displayName)
		{
			if (PlayerList.Entries.TryGetValue(uuid, out var item))
			{
				item.Username = displayName;
			}
		}

		#endregion
	}

	[Flags]
	public enum BlockUpdatePriority
	{
		Neighbors = 0x01,
		Network = 0x02,
		NoGraphic = 0x04,
		Priority = 0x08,
		
		High = Neighbors | Priority,
		Normal = Neighbors,
		Low = Neighbors | NoGraphic
	}
}
