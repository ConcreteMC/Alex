using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Blocks.Storage;
using Alex.Common;
using Alex.Common.Blocks;
using Alex.Common.Data.Options;
using Alex.Common.Graphics;
using Alex.Common.Graphics.GpuResources;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Common.Utils.Collections;
using Alex.Common.World;
using Alex.Entities;
using Alex.Entities.BlockEntities;
using Alex.Gamestates;
using Alex.Graphics.Camera;
using Alex.Graphics.Models;
using Alex.Graphics.Models.Entity;
using Alex.Graphics.Models.Items;
using Alex.Gui;
using Alex.Gui.Elements.Map;
using Alex.Net;
using Alex.Net.Bedrock;
using Alex.Utils;
using Alex.Utils.Threading;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Chunks;
using Alex.Worlds.Lighting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET;
using MiNET.Utils;
using NLog;
using RocketUI;
using BlockCoordinates = Alex.Common.Utils.Vectors.BlockCoordinates;
using BlockFace = Alex.Common.Blocks.BlockFace;
using ChunkCoordinates = Alex.Common.Utils.Vectors.ChunkCoordinates;
using Color = Microsoft.Xna.Framework.Color;
using Player = Alex.Entities.Player;
using PlayerLocation = Alex.Common.Utils.Vectors.PlayerLocation;
using Skin = Alex.Common.Utils.Skin;

//using System.Reflection.Metadata.Ecma335;

namespace Alex.Worlds
{
	public enum Dimension
	{
		Overworld,
		Nether,
		TheEnd,
	}
	
	public class World : IBlockAccess, ITicked, IDisposable
	{
		private static readonly Logger       Log = LogManager.GetCurrentClassLogger(typeof(World));
		public                  EntityCamera Camera { get; }

		public Player Player { get; set; }
		private AlexOptions Options { get; }
		
		public InventoryManager InventoryManager { get; }
		private SkyBox SkyBox { get; }

		public long Time      { get; private set; } = 1;
		public long TimeOfDay { get; private set; } = 1;
		public bool Raining   { get; private set; } = false;
		public float RainLevel { get; private set; } = 0f;
		public bool Thundering { get; private set; } = false;
		public float ThunderLevel { get; private set; } = 0f;
		
		public bool DrowningDamage      { get; private set; } = true;
		public bool CommandblockOutput  { get; private set; } = true;
		public bool DoTiledrops         { get; private set; } = true;
		public bool DoMobloot           { get; private set; } = true;
		public bool KeepInventory       { get; private set; } = true;
		public bool DoDaylightcycle     { get; private set; } = true;
		public bool DoMobspawning       { get; private set; } = true;
		public bool DoEntitydrops       { get; private set; } = true;
		public bool DoFiretick          { get; private set; } = true;
		public bool DoWeathercycle      { get; private set; } = true;
		public bool Pvp                 { get; private set; } = true;
		public bool Falldamage          { get; private set; } = true;
		public bool Firedamage          { get; private set; } = true;
		public bool Mobgriefing         { get; private set; } = true;
		public bool ShowCoordinates     { get; private set; } = true;
		public bool NaturalRegeneration { get; private set; } = true;
		public bool TntExplodes         { get; private set; } = true;
		public bool SendCommandfeedback { get; private set; } = true;
		public bool InstantRespawn      { get; private set; } = false;
		public int  RandomTickSpeed     { get; private set; } = 3;

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

		public  BackgroundWorker  BackgroundWorker { get; }
		private List<IDisposable> _disposables = new List<IDisposable>();
		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		private static Texture2D[] _destroyStages = null;//new Texture2D[10];
		
		public WorldMap Map { get; }
		public World(IServiceProvider serviceProvider, GraphicsDevice graphics, AlexOptions options,
			NetworkProvider networkProvider)
		{
			Options = options;

			ChunkManager = new ChunkManager(serviceProvider, graphics, this, _cancellationTokenSource.Token);
			EntityManager = new EntityManager(serviceProvider, graphics, this, networkProvider);
			TickManager = new TickManager();
			PlayerList = new PlayerList();

			Map = new WorldMap(this, options);
			
			TickManager.RegisterTicked(this);
			TickManager.RegisterTicked(EntityManager);
			TickManager.RegisterTicked(ChunkManager);
			TickManager.RegisterTicked(Map);
			
			var resources = serviceProvider.GetRequiredService<ResourceManager>();

			Player = new Player(graphics, serviceProvider.GetRequiredService<Alex>().InputManager, this, networkProvider, PlayerIndex.One);
			
			Camera = new EntityCamera(Player);

			Player.KnownPosition = new PlayerLocation(SpawnPoint);
			_disposables.Add(options.FieldOfVision.Bind(FieldOfVisionOnValueChanged));
			//Options.FieldOfVision.ValueChanged += FieldOfVisionOnValueChanged;
			Camera.FieldOfView = Options.FieldOfVision.Value;

			//PhysicsEngine.AddTickable(Player);

			var guiManager = serviceProvider.GetRequiredService<GuiManager>();
			InventoryManager = new InventoryManager(guiManager);
				
			SkyBox = new SkyBox(serviceProvider, graphics, this);

			_disposables.Add(
				options.VideoOptions.RenderDistance.Bind(
					(old, newValue) =>
					{
						networkProvider.RequestRenderDistance(old, newValue);
						Camera.SetRenderDistance(newValue);
						//ChunkManager.RenderDistance = newValue;
					}));
			
			Camera.SetRenderDistance(options.VideoOptions.RenderDistance.Value);
			ChunkManager.RenderDistance = options.VideoOptions.RenderDistance.Value;

			BackgroundWorker = new BackgroundWorker(_cancellationTokenSource.Token);
			
			//Player?.OnSpawn();
			
			_disposables.Add(TickManager);
			_disposables.Add(EntityManager);
			_disposables.Add(ChunkManager);
			_disposables.Add(BackgroundWorker);

			_breakingEffect = new BasicEffect(graphics);
			_breakingEffect.TextureEnabled = true;
			_breakingEffect.LightingEnabled = false;
			_breakingEffect.FogEnabled = false;
			_breakingEffect.VertexColorEnabled = true;

			if (_destroyStages == null)
			{
				_destroyStages = new Texture2D[10];
				for (int i = 0; i < _destroyStages.Length; i++)
				{
					if (resources.TryGetBitmap($"block/destroy_stage_{i}", out var bmp))
					{
						_destroyStages[i] = TextureUtils.BitmapToTexture2D(graphics, bmp);
					}
				}
			}

			Player.MapIcon.AlwaysShown = true;
			Player.MapIcon.Marker = MapMarker.GreenPointer;
			Player.MapIcon.DrawOrder = int.MaxValue;
			Map.Add(Player.MapIcon);
		}

		private const int MORTON3D_BIT_SIZE = 21;
		private const int BLOCKHASH_Y_BITS = 9;
		private const int BLOCKHASH_Y_PADDING = 128; //size (in blocks) of padding after both boundaries of the Y axis
		private const int BLOCKHASH_Y_OFFSET = BLOCKHASH_Y_PADDING - 0;
		private const int BLOCKHASH_Y_MASK = (1 << BLOCKHASH_Y_BITS) - 1;
		private const int BLOCKHASH_XZ_MASK = (1 << MORTON3D_BIT_SIZE) - 1;
		private const int BLOCKHASH_XZ_EXTRA_BITS = 6;
		private const int BLOCKHASH_XZ_EXTRA_MASK = (1 << BLOCKHASH_XZ_EXTRA_BITS) - 1;
		private const int BLOCKHASH_XZ_SIGN_SHIFT = 64 - MORTON3D_BIT_SIZE - BLOCKHASH_XZ_EXTRA_BITS;
		private const int BLOCKHASH_X_SHIFT = BLOCKHASH_Y_BITS;
		private const int BLOCKHASH_Z_SHIFT = BLOCKHASH_X_SHIFT + BLOCKHASH_XZ_EXTRA_BITS;

		public static int BlockHash(int x, int y, int z)
		{
			var shiftedY = y + BLOCKHASH_Y_OFFSET;

			if ((shiftedY & (~0 << BLOCKHASH_Y_BITS)) != 0)
			{
				throw new ArgumentException("Y coordinate $y is out of range!");
			}

			return Morton3dEncode(
				x & BLOCKHASH_XZ_MASK,
				(shiftedY /* & BLOCKHASH_Y_MASK */)
				| (((x >> MORTON3D_BIT_SIZE) & BLOCKHASH_XZ_EXTRA_MASK) << BLOCKHASH_X_SHIFT)
				| (((z >> MORTON3D_BIT_SIZE) & BLOCKHASH_XZ_EXTRA_MASK) << BLOCKHASH_Z_SHIFT), z & BLOCKHASH_XZ_MASK);
		}

		public static BlockCoordinates BlockHashDecode(int hash)
		{
			Morton3dDecode(hash, out var x, out var y, out var z);

			return new BlockCoordinates(x, y, z);
		}
		public static void Morton3dDecode(
			int morton, out int x, out int y, out int z)
		{
#if INTRINSIC
            if (X86.Bmi2.IsSupported)
            {
                x = X86.Bmi2.ParallelBitExtract(morton, 0x09249249);
                y = X86.Bmi2.ParallelBitExtract(morton, 0x12492492);
                z = X86.Bmi2.ParallelBitExtract(morton, 0x24924924);
            }
            else
#endif
			{
				x = morton & 0x9249249;
				x = (x ^ (x >> 2)) & 0x30c30c3;
				x = (x ^ (x >> 4)) & 0x0300f00f;
				x = (x ^ (x >> 8)) & 0x30000ff;
				x = (x ^ (x >> 16)) & 0x000003ff;

				y = (morton >> 1) & 0x9249249;
				y = (y ^ (y >> 2)) & 0x30c30c3;
				y = (y ^ (y >> 4)) & 0x0300f00f;
				y = (y ^ (y >> 8)) & 0x30000ff;
				y = (y ^ (y >> 16)) & 0x000003ff;

				z = (morton >> 2) & 0x9249249;
				z = (z ^ (z >> 2)) & 0x30c30c3;
				z = (z ^ (z >> 4)) & 0x0300f00f;
				z = (z ^ (z >> 8)) & 0x30000ff;
				z = (z ^ (z >> 16)) & 0x000003ff;

			}
		}
		
		private static int Morton3dEncode(int x, int y, int z)
		{
#if INTRINSIC
            if (X86.Bmi2.IsSupported)
                return X86.Bmi2.ParallelBitDeposit(z, 0x24924924)
                     | X86.Bmi2.ParallelBitDeposit(y, 0x12492492)
                     | X86.Bmi2.ParallelBitDeposit(x, 0x09249249);
            else
#endif
			{
				x = (x | (x << 16)) & 0x030000FF;
				x = (x | (x << 8)) & 0x0300F00F;
				x = (x | (x << 4)) & 0x030C30C3;
				x = (x | (x << 2)) & 0x09249249;

				y = (y | (y << 16)) & 0x030000FF;
				y = (y | (y << 8)) & 0x0300F00F;
				y = (y | (y << 4)) & 0x030C30C3;
				y = (y | (y << 2)) & 0x09249249;

				z = (z | (z << 16)) & 0x030000FF;
				z = (z | (z << 8)) & 0x0300F00F;
				z = (z | (z << 4)) & 0x030C30C3;
				z = (z | (z << 2)) & 0x09249249;

				return x | (y << 1) | (z << 2);
			}
		}
		
		private void FieldOfVisionOnValueChanged(int oldvalue, int newvalue)
		{
			Camera.FieldOfView = newvalue;
		}

		//public long WorldTime { get; private set; } = 6000;

		public PlayerList     PlayerList    { get; }
		public TickManager    TickManager   { get; private set; }
		public EntityManager  EntityManager { get; private set; }
		public ChunkManager   ChunkManager  { get; private set; }
		
		public BlockCoordinates CompassPosition { get; set; } = BlockCoordinates.Zero;
		public Vector3 SpawnPoint { get; set; } = Vector3.Zero;

		public void ToggleWireFrame()
        {
	        ChunkManager.UseWireFrames = !ChunkManager.UseWireFrames;
        }
        
		/// <summary>
		///		The amount of calls made to DrawPrimitives in the last render call
		/// </summary>
		public int ChunkDrawCount { get; set; } = 0;
		
		public bool RenderBoundingBoxes { get; set; } = false;

		private ConcurrentDictionary<BlockCoordinates, BlockBreakProgress> _blockBreakProgresses = new ConcurrentDictionary<BlockCoordinates, BlockBreakProgress>();
		private BasicEffect _breakingEffect;
        public void Render(IRenderArgs args)
        {
	        if (_destroyed)
		        return;

	        SkyBox.Draw(args);
            
	        args.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
	        args.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
	        
            int chunkDrawCount = ChunkManager.Draw(args, null,
	            RenderStage.Opaque);
            
            EntityManager.Render(args);
            
            chunkDrawCount += ChunkManager.Draw(args, null,
	            RenderStage.Transparent,
	            RenderStage.Animated,
	            RenderStage.Liquid,
	            RenderStage.Translucent
	            //RenderStage.Animated,
	            );

            ChunkDrawCount = chunkDrawCount;

	        Player.Render(args, Options.VideoOptions.EntityCulling.Value);

	        foreach (var block in _blockBreakProgresses)
	        {
		        var index = block.Value.Stage;
		        var texture = _destroyStages[index];
						        
		        if (texture != null)
		        {
			        _breakingEffect.Texture = texture;
			        _breakingEffect.TextureEnabled = true;
			        _breakingEffect.VertexColorEnabled = false;
		        }
		        else
		        {
			        _breakingEffect.TextureEnabled = false;
			        _breakingEffect.VertexColorEnabled = true;
		        }

		        args.GraphicsDevice.RenderBoundingBox(
			        block.Value.BoundingBox, Camera.ViewMatrix, Camera.ProjectionMatrix, Color.White, true, _breakingEffect);
	        }
	        
	        if (Player != null && Player.Raytracer.HasValue)
	        {
		        var player = Player;
		        var block = player.Raytracer.TracedBlock;
		        //var               blockPos = player.RaytracedBlock;
		        var boxes = player.Raytracer.RaytraceBoundingBoxes;

		        if (boxes != null && boxes.Length > 0)
		        {
			        foreach (var boundingBox in boxes)
			        {
				        if (block.CanInteract || !Player.IsWorldImmutable)
				        {
					        Color color = Color.LightGray;

					        {
						        args.GraphicsDevice.RenderBoundingBox(
							        boundingBox, Camera.ViewMatrix, Camera.ProjectionMatrix, color);
					        }
				        }
			        }
		        }
	        }

	        if (RenderBoundingBoxes)
	        {
		        var hitEntity = Player?.HitEntity;

		        var entities = Player?.EntitiesInRange;

		        if (entities != null)
		        {
			        foreach (var entity in entities)
			        {
				        args.GraphicsDevice.RenderBoundingBox(
					        entity.GetBoundingBox(), Camera.ViewMatrix, Camera.ProjectionMatrix,
					        entity == hitEntity ? Color.Red : Color.Yellow);
			        }
		        }

		        if (Player != null)
		        {
			        args.GraphicsDevice.RenderBoundingBox(
				        Player.GetBoundingBox(), Camera.ViewMatrix, Camera.ProjectionMatrix,
				        Color.Red);

			        var hit = Player.Movement.LastCollision;

			        foreach (var bb in hit)
			        {
				        args.GraphicsDevice.RenderBoundingBox(
					        bb.Box, Camera.ViewMatrix, Camera.ProjectionMatrix, bb.Color, true);
			        }
		        }
	        }
        }

        public void RenderSprites(IRenderArgs args)
        {
	        if (_destroyed)
		        return;
	        
	        EntityManager.Render2D(args);
        }
        
	//	private float _fovModifier  = -1;
		private float _brightnessMod = 0f;
		private bool _wasInWater = false;
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
			
			SkyBox.Update(args);
			ChunkManager.Update(args);
			
			EntityManager.Update(args);

			bool inWater = Player.HeadInWater;
			
			if (Math.Abs(_brightnessMod - SkyBox.BrightnessModifier) > 0f)
			{
				_brightnessMod = SkyBox.BrightnessModifier;
				
				var diffuseColor = Color.White.ToVector3() * SkyBox.BrightnessModifier;
				ChunkManager.AmbientLightColor = diffuseColor;

				if (!inWater)
				{
					if (Options.VideoOptions.Fog.Value)
					{
						ChunkManager.FogColor = SkyBox.WorldFogColor.ToVector3();
						ChunkManager.FogDistance = args.Camera.FarDistance;
						ChunkManager.FogEnabled = Options.VideoOptions.Fog.Value;
					}
				}

				if (Math.Abs(ChunkManager.Shaders.BrightnessModifier - SkyBox.BrightnessModifier) > 0f)
				{
					ChunkManager.Shaders.BrightnessModifier = SkyBox.BrightnessModifier;
				}
				
				var modelRenderer = Player?.ModelRenderer;

				if (modelRenderer != null)
				{
					modelRenderer.DiffuseColor = diffuseColor;
				}
			}
			
			Player?.Update(args);
			var biome = Player.CurrentBiome;
			ChunkManager.WaterSurfaceTransparency = biome.WaterSurfaceTransparency;
			
			if (inWater && !_wasInWater)
			{
				ChunkManager.FogColor = biome.WaterFogColor.ToVector3();
				ChunkManager.FogDistance = biome.WaterFogDistance;
				ChunkManager.FogEnabled = true;
			}
			else if (_wasInWater && !inWater)
			{
				ChunkManager.FogColor = SkyBox.WorldFogColor.ToVector3();
				ChunkManager.FogDistance = args.Camera.FarDistance;
				ChunkManager.FogEnabled = Options.VideoOptions.Fog.Value;
			}

			_wasInWater = inWater;
		}

		public void OnTick()
		{
			Player?.OnTick();
				//Alex.Instance.ParticleManager.OnTick();

			Time++;
			
			if (DoDaylightcycle)
			{
				var tod = TimeOfDay;
				TimeOfDay = ((tod + 1) % 24000);
			}

			foreach (var blockBreak in _blockBreakProgresses)
			{
				blockBreak.Value.Tick();
			}
		}

		public void AddOrUpdateBlockBreak(BlockCoordinates coordinates, double requiredTime, byte destroyStage = 0)
		{
			var block = GetBlockState(coordinates);

			if (block == null)
				return;

			if (_blockBreakProgresses.TryGetValue(coordinates, out var i))
			{
				i.SetStage(destroyStage);
			}
			else
			{
				var boundingBoxes = block.Block.GetBoundingBoxes(coordinates).ToArray();

				if (boundingBoxes.Length > 0)
				{
					var item = new BlockBreakProgress(coordinates, requiredTime);
					item.BoundingBox = boundingBoxes.OrderByDescending(x => (x.Max - x.Min).LengthSquared())
					   .FirstOrDefault();

					_blockBreakProgresses.TryAdd(coordinates, item);
				}
			}
		}

		public void EndBreakBlock(BlockCoordinates coordinates)
		{
			_blockBreakProgresses.TryRemove(coordinates, out _);
		}

		public float BrightnessModifier
        {
	        get { return (_brightnessMod + ((Options.VideoOptions.Brightness - 50f) * (0.5f / 100f))); }
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
		        if (chunk.SetSkyLight(coordinates.X & 0xf, coordinates.Y, coordinates.Z & 0xf, p1))
		        {
			        var x = coordinates.X;
			        var y = coordinates.Y;
			        var z = coordinates.Z;
			        
			        ScheduleBlockUpdate(new BlockCoordinates(x + 1, y, z));
			        ScheduleBlockUpdate(new BlockCoordinates(x - 1, y, z));
			        
			        ScheduleBlockUpdate(new BlockCoordinates(x, y, z + 1));
			        ScheduleBlockUpdate(new BlockCoordinates(x, y, z - 1));
			        
			        ScheduleBlockUpdate(new BlockCoordinates(x, y + 1, z));
			        ScheduleBlockUpdate(new BlockCoordinates(x, y - 1, z));
		        }
	        }
        }
        
        public void SetBlockLight(BlockCoordinates coordinates, byte value)
        {
	        var         chunkCoords = new ChunkCoordinates(coordinates);
	        ChunkColumn chunk;
	        if (ChunkManager.TryGetChunk(chunkCoords, out chunk))
	        {
		        if (chunk.SetBlocklight(coordinates.X & 0xf, coordinates.Y, coordinates.Z & 0xf, value))
		        {
			        var x = coordinates.X;
			        var y = coordinates.Y;
			        var z = coordinates.Z;

			        ScheduleBlockUpdate(new BlockCoordinates(x + 1, y, z));
			        ScheduleBlockUpdate(new BlockCoordinates(x + -1, y, z));

			        ScheduleBlockUpdate(new BlockCoordinates(x, y, z + 1));
			        ScheduleBlockUpdate(new BlockCoordinates(x, y, z + -1));

			        ScheduleBlockUpdate(new BlockCoordinates(x, y + 1, z));
			        ScheduleBlockUpdate(new BlockCoordinates(x, y + -1, z));
		        }
	        }
        }

        /// <inheritdoc />
        public void GetLight(BlockCoordinates coordinates, out byte blockLight, out byte skyLight)
        {
	        blockLight = 0;
	        skyLight = 15;

	        var x = coordinates.X;
	        var y = coordinates.Y;
	        var z = coordinates.Z;
	        
	        ChunkColumn chunk;
	        if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
	        {
		        chunk.GetLight(x & 0xf, y, z & 0xf, out skyLight, out blockLight);
	        }
        }

        public byte GetSkyLight(BlockCoordinates position)
        {
	        return GetSkyLight(position.X, position.Y, position.Z);
        }

        public bool TryGetBlockLight(BlockCoordinates coordinates, out byte blockLight)
        {
	        blockLight = 0;

	        ChunkColumn chunk;
	        if (ChunkManager.TryGetChunk(new ChunkCoordinates(coordinates), out chunk))
	        {
		        blockLight = chunk.GetBlocklight(coordinates.X & 0xf, coordinates.Y, coordinates.Z & 0xf);
		        return true;
	        }
	        
	        return false;
        }
        
        public byte GetBlockLight(BlockCoordinates coordinates)
        {
	        return GetBlockLight(coordinates.X, coordinates.Y, coordinates.Z);
        }
        
        public byte GetSkyLight(int x, int y, int z)
        {
	        ChunkColumn chunk;
	        if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
	        {
				return chunk.GetSkylight(x & 0xf, y, z & 0xf);
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
	        ChunkColumn chunk;
	        if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
	        {
                return chunk.GetBlocklight(x & 0xf, y, z & 0xf);
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
				EntityManager.RemoveBlockEntity(coords);
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
				var cy = y;
				var cz = z & 0xf;

				var blockcoords = new BlockCoordinates(x, y, z);
				chunk.SetBlockState(cx, cy, cz, block, storage);

				if (storage == 0 && EntityManager.TryGetBlockEntity(blockcoords, out var blockEntity))
				{
					if (!blockEntity.SetBlock(block.Block))
					{
						EntityManager.RemoveBlockEntity(blockcoords);
					}
				}
				
				var type = ScheduleType.Full;
				
				//if ((priority & BlockUpdatePriority.Neighbors) != 0)
				{
					UpdateNeighbors(x, y, z);
				}

				if ((priority & BlockUpdatePriority.NoGraphic) != 0)
				{
					type |= ScheduleType.LowPriority;
				}

				/*if (block.Block.BlockMaterial.BlocksLight)
				{
					SetBlockLight(blockCoords, 0);
				}
				*/

				if ((type & ScheduleType.Lighting) == 0)
				{
					type |= ScheduleType.Lighting;
				}

				ChunkManager.BlockLightUpdate.Enqueue(blockcoords);
				ChunkManager.SkyLightCalculator.Enqueue(blockcoords);
				//ChunkManager.SkyLightCalculator.Calculate(blockcoords);
				
				ChunkManager.ScheduleChunkUpdate(chunkCoords, ScheduleType.Scheduled, true);
			}
		}

		private void UpdateNeighbors(int x, int y, int z)
		{
			var source = new BlockCoordinates(x, y, z);
			ScheduleBlockUpdate(source);
			if (Options.VideoOptions.ClientSideLighting && Dimension == Dimension.Overworld)
			{
				//ChunkManager.SkyLightCalculator.Calculate(this, source);
				//new SkyLightCalculations().Calculate(this, source);
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
				var cy = coordinates.Y;
				var cz = coordinates.Z & 0xf;

				chunk.ScheduleBlockUpdate(cx, cy, cz);
			}
		}
		
		private void ScheduleBlockUpdate(BlockCoordinates updatedBlock, BlockCoordinates block)
		{
			var blockInstance = GetBlockState(block).Block;

			TickManager.ScheduleTick(
				() =>
				{
					blockInstance.BlockUpdate(this, block, updatedBlock);
				}, 0, _cancellationTokenSource.Token);
		}

		public IEnumerable<ChunkSection.BlockEntry> GetBlockStates(int x, int y, int z)
		{
			ChunkColumn chunk;
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
			{
				foreach (var bs in chunk.GetBlockStates(x & 0xf, y, z & 0xf))
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
				return chunk.GetBlockState(x & 0xf, y, z & 0xf, storage);
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

			return coords.Y;
		}

		/// <inheritdoc />
		public Biome GetBiome(BlockCoordinates coordinates)
		{
			return GetBiome(coordinates.X, coordinates.Y, coordinates.Z);
		}
		
		public Biome GetBiome(int x, int y, int z)
		{
			ChunkColumn chunk;
			if (ChunkManager.TryGetChunk(new ChunkCoordinates(x >> 4, z >> 4), out chunk))
			{
				ChunkColumn realColumn = (ChunkColumn) chunk;
				return	realColumn.GetBiome(x & 0xf, y, z & 0xf);
			}

			//Log.Debug($"Failed getting biome: {x} | {y} | {z}");
			return BiomeUtils.Biomes[0];
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

		public bool SpawnEntity(Entity entity)
		{
			if (EntityManager.AddEntity(entity) || (EntityManager.TryGet(entity.EntityId, out var entity2) && entity2 == entity))
			{
				entity.OnSpawn();
				return true;
			}

			return false;
			//Log.Info($"Spawned entity {entityId} : {entity} at {entity.KnownPosition} with renderer {entity.GetModelRenderer()}");
		}

		public void DespawnEntity(long entityId)
		{

			if (EntityManager.TryGet(entityId, out Entity entity))
			{
				entity.OnDespawn();
				EntityManager.Remove(entityId);
			}
		}

		public void UpdatePlayerPosition(PlayerLocation location, bool teleport = false)
		{
			Player.Movement.MoveTo(location);

			if (GetChunk(new ChunkCoordinates(location)) == null)
			{
				Player.WaitingOnChunk = true;
			}
		}

		public void UpdateEntityPosition(long entityId,
			PlayerLocation position,
			bool relative = false,
			bool updateLook = false,
			bool updatePitch = false,
			bool teleport = false,
			bool adjustForEntityHeight = false)
		{
			if (EntityManager == null)
				return;
			
			Entity entity = null;

			if (Player != null && entityId == Player.EntityId)
				entity = Player;
			else
			{
				EntityManager.TryGet(entityId, out entity);
			}
			
			if (entity != null)
			{
				entity.KnownPosition.OnGround = position.OnGround;

				if (relative)
				{
					entity.Movement.Move(position.ToVector3());
				}
				else
				{
					if (adjustForEntityHeight)
					{
						if (entity is RemotePlayer)
						{
							position.Y -= Player.EyeLevel;
						}
					}

					entity.Movement.MoveTo(position, updateLook);
				}

				entity.Velocity = Vector3.Zero;
			}
		}

		public void UpdateEntityLook(long entityId, float yaw, float pitch, bool onGround)
		{
			if (EntityManager != null &&  EntityManager.TryGet(entityId, out Entity entity))
			{
				entity.KnownPosition.OnGround = onGround;
				entity.KnownPosition.Pitch = pitch;
				entity.KnownPosition.HeadYaw = yaw;
			}
		}

		public bool TryGetEntity(long entityId, out Entity entity)
		{
		//	if (entityId == Player.EntityId)
		//	{
		//		entity = Player;
		//		return true;
		//	}

			return EntityManager.TryGet(entityId, out entity);
		}

		public void SetTime(long worldTime, long timeOfDay)
		{
			Time = worldTime;//timeOfDay;
			TimeOfDay = Math.Abs(timeOfDay);
		}

		public void SetRain(bool raining, float rainLevel = 0f)
		{
			Raining = raining;
			RainLevel = rainLevel;
		}
		
		public void SetThunder(bool thundering, float thunderLevel = 0f)
		{
			Thundering = thundering;
			ThunderLevel = thunderLevel;
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
			if (PlayerList.Entries.TryAdd(item.UUID, item))
			{
				UpdatePlayerLatency(item.UUID, item.Ping);
			}
		}

		public void RemovePlayerListItem(MiNET.Utils.UUID item)
		{
			PlayerList.Entries.TryRemove(item, out _);
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
		
		private bool _destroyed = false;
		public void Dispose()
		{
			if (_destroyed) return;
			_destroyed = true;
			
			Log.Info("World disposing...");
			_cancellationTokenSource?.Cancel();

			foreach (var disposable in _disposables)
			{
				disposable.Dispose();
			}
			_disposables?.Clear();
			
			TickManager?.UnregisterTicked(this);
			TickManager?.UnregisterTicked(EntityManager);
			TickManager?.UnregisterTicked(ChunkManager);
			TickManager?.UnregisterTicked(Map);
			
			Map?.Dispose();
			//Map = null;

			EntityManager = null;
			ChunkManager = null;

			Player?.Dispose();
			//Ticker.Dispose();
			TickManager = null;
			Player = null;
			
			_breakingEffect?.Dispose();
			Log.Info($"World disposed.");
		}
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
