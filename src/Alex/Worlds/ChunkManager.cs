using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alex.API;
using Alex.API.Data.Options;
using Alex.API.Graphics;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.Minecraft;
using Alex.Entities.BlockEntities;
using Alex.Gamestates;
using Alex.Utils.Queue;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Chunks;
using Alex.Worlds.Lighting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MathF = System.MathF;

namespace Alex.Worlds
{
	public class ChunkManager : IChunkManager, IDisposable, ITicked
	{
		private readonly static RenderStage[]   RenderStages = ((RenderStage[]) Enum.GetValues(typeof(RenderStage)));
		
		private                 GraphicsDevice  Graphics  { get; }
		private                 ResourceManager Resources { get; }
		private                 World           World     { get; }
		
		private AlexOptions                                         Options        { get; }
		private ConcurrentDictionary<ChunkCoordinates, ChunkColumn> Chunks         { get; }
		
		public  RenderingShaders        Shaders                { get; set; }
		private CancellationTokenSource CancellationToken      { get; }
		public BlockLightCalculations  BlockLightCalculations { get; set; }
		public  SkyLightCalculations    SkyLightCalculator     { get; private set; }

		private FancyQueue<ChunkCoordinates>     FastUpdateQueue   { get; }
		private FancyQueue<ChunkCoordinates>     UpdateQueue       { get; }
		private FancyQueue<ChunkCoordinates>     UpdateBorderQueue { get; }
		private ThreadSafeList<ChunkCoordinates> Scheduled         { get; } = new ThreadSafeList<ChunkCoordinates>();
		public ChunkManager(IServiceProvider serviceProvider, GraphicsDevice graphics, World world)
		{
			Graphics = graphics;
			World = world;
			
			Options = serviceProvider.GetRequiredService<IOptionsProvider>().AlexOptions;
			Resources = serviceProvider.GetRequiredService<ResourceManager>();
			
			var stillAtlas = Resources.Atlas.GetStillAtlas();
	        
			var fogStart = 0;
			Shaders = new RenderingShaders(Graphics);
			Shaders.SetTextures(stillAtlas);
			Shaders.SetAnimatedTextures(Resources.Atlas.GetAtlas(0));
			
			_renderSampler.MaxMipLevel = stillAtlas.LevelCount;
			
			RenderDistance = Options.VideoOptions.RenderDistance;

			Options.VideoOptions.RenderDistance.Bind(
				(value, newValue) =>
				{
					RenderDistance = newValue;
				});
			
			Chunks = new ConcurrentDictionary<ChunkCoordinates, ChunkColumn>();
			CancellationToken = new CancellationTokenSource();
			
			BlockLightCalculations = new BlockLightCalculations(world, CancellationToken.Token);
			SkyLightCalculator = new SkyLightCalculations(CancellationToken.Token);
			
			UpdateQueue = new FancyQueue<ChunkCoordinates>();
			UpdateBorderQueue = new FancyQueue<ChunkCoordinates>();
			FastUpdateQueue = new FancyQueue<ChunkCoordinates>();
		}

		private long _threadsRunning = 0;

		
		public int RenderDistance { get; set; } = 0;
		
		public int ConcurrentChunkUpdates => (int) _threadsRunning;
		public int EnqueuedChunkUpdates   => Scheduled.Count;
		
		/// <inheritdoc />
		public int ChunkCount => Chunks.Count;

		/// <inheritdoc />
		public int RenderedChunks => _renderedChunks.Length;

		public bool FogEnabled
		{
			get { return Shaders.FogEnabled; }
			set
			{
				Shaders.FogEnabled = value;
			}
		}

		public Vector3 FogColor
		{
			get { return Shaders.FogColor; }
			set
			{
				Shaders.FogColor = value;
				//  LightShaders.FogColor = value;
			}
		}

		public float FogDistance
		{
			get { return Shaders.FogDistance; }
			set
			{
				Shaders.FogDistance = value;
				//  LightShaders.FogDistance = value;
			}
		}

		public Vector3 AmbientLightColor
		{
			get { return Shaders.AmbientLightColor; }
			set
			{
				Shaders.AmbientLightColor = value;
			}
		}

		/// <inheritdoc />
		public void Start()
		{
			ThreadPool.QueueUserWorkItem(
				o =>
				{
					Thread.CurrentThread.Name = "Chunk Management";

					SpinWait sw = new SpinWait();
					while (!CancellationToken.IsCancellationRequested)
					{
						if (World?.Camera == null)
						{
							sw.SpinOnce();
							continue;
						}

						bool processedLighting = ProcessLighting();
						bool processedQueue    = ProcessQueue();

						if (!processedQueue && !processedLighting)
							sw.SpinOnce();
					}
				});
		}
		
		private ChunkData[] _renderedChunks = new ChunkData[0];
		private ChunkCoordinates[] UpdateRenderedChunks()
		{
			//List<ChunkCoordinates> chunks   = new List<ChunkCoordinates>();
			List<ChunkData>        rendered = new List<ChunkData>();
			foreach (var chunk in Chunks)
			{
				if (chunk.Value.ChunkData == null)
					continue;
				
				//if (!chunk.Value.ChunkData.Ready)
				//	continue;
				
				if (IsWithinView(chunk.Key, World.Camera))
				{
					rendered.Add(chunk.Value.ChunkData);
					//chunks.Add(chunk.Key);
				}
			}

			_renderedChunks = rendered.ToArray();

			return rendered.Select(x => x.Coordinates).ToArray();
		}
		
		private bool ProcessQueue()
		{
			var maxThreads = Options.VideoOptions.ChunkThreads.Value;
			if (Interlocked.Read(ref _threadsRunning) >= maxThreads)
			{
				return false;
			}

			FancyQueue<ChunkCoordinates> queue = FastUpdateQueue;

			if (queue.IsEmpty)
				queue = UpdateQueue;

			if (queue.IsEmpty)
				queue = UpdateBorderQueue;
			
			//maxThreads = Math.Max(1, Math.Min(Options.VideoOptions.ChunkThreads, EnqueuedChunkUpdates / 4));
			
			if (queue.IsEmpty || Interlocked.Read(ref _threadsRunning) >= maxThreads)
			{
				return false;
			}
			
			//if (Interlocked.Read(ref _threadsRunning) )

			Interlocked.Increment(ref _threadsRunning);
						
			ThreadPool.QueueUserWorkItem(
				oo =>
				{
					try
					{
						while (!CancellationToken.IsCancellationRequested && queue.TryDequeue(out var chunkCoordinates, cc => IsWithinView(cc, World.Camera)))
						{
							if (TryGetChunk(chunkCoordinates, out var chunk))
							{
								if (!Monitor.TryEnter(chunk.UpdateLock, 0))
									continue;

								try
								{
									bool newChunk = chunk.IsNew;
									
									if (BlockLightCalculations != null)
									{
										if (newChunk)
										{
											//BlockLightCalculations.Recalculate(chunk);
										}
										if (BlockLightCalculations.HasEnqueued(chunkCoordinates))
										{
											BlockLightCalculations.Process(chunkCoordinates);
										}
									}

									//SkyLightCalculator.TryProcess(chunkCoordinates);

									if (SkyLightCalculator != null)
									{
										if (newChunk)
										{
											SkyLightCalculator.RecalcSkyLight(chunk, World);
										}
									}

									chunk.UpdateBuffer(Graphics, World);

								/*	if (newChunk)
									{
										ScheduleChunkUpdate(new ChunkCoordinates(chunk.X + 1, chunk.Z), ScheduleType.Border);
										ScheduleChunkUpdate(new ChunkCoordinates(chunk.X - 1, chunk.Z), ScheduleType.Border);
										ScheduleChunkUpdate(new ChunkCoordinates(chunk.X, chunk.Z + 1), ScheduleType.Border);
										ScheduleChunkUpdate(new ChunkCoordinates(chunk.X, chunk.Z - 1), ScheduleType.Border);
									}*/
								}
								finally
								{
									Scheduled.Remove(chunkCoordinates);
									Monitor.Exit(chunk.UpdateLock);
								}
							}

							if (queue != FastUpdateQueue && !FastUpdateQueue.IsEmpty)
							{
								queue = FastUpdateQueue;
							}
							else if (queue == FastUpdateQueue && FastUpdateQueue.IsEmpty)
							{
								queue = UpdateQueue;
							}
							else if (queue == UpdateBorderQueue && !UpdateQueue.IsEmpty)
							{
								queue = UpdateQueue;
							}
							else if (queue == UpdateQueue && UpdateQueue.IsEmpty)
							{
								queue = UpdateBorderQueue;
							}
						}
					}
					finally
					{
						Interlocked.Decrement(ref _threadsRunning);
					}
				});

			return true;
		}

		private bool ProcessLighting()
		{
			return BlockLightCalculations.Process(
				Chunks.FirstOrDefault(x => BlockLightCalculations.HasEnqueued(x.Key)).Key);
		}


		/// <inheritdoc />
		public void AddChunk(ChunkColumn chunk, ChunkCoordinates position, bool doUpdates = false)
		{
			if (CancellationToken.IsCancellationRequested)
				return;
			//chunk.CalculateHeight();
			if (chunk.IsNew)
			{
				SkyLightCalculator.RecalcSkyLight(chunk, World);
				BlockLightCalculations.Recalculate(chunk);
			}

			foreach (var blockEntity in chunk.BlockEntities)
			{
				//var coordinates = new BlockCoordinates(blockEntity.X, blockEntity.Y, blockEntity.Z);
				//World.SetBlockEntity(coordinates.X, coordinates.Y, coordinates.Z, blockEntity);
				var entity = BlockEntityFactory.ReadFrom(blockEntity.Value, World, 
					chunk.GetBlockState(blockEntity.Key.X & 0xf, blockEntity.Key.Y & 0xff, blockEntity.Key.Z & 0xf).Block);
				if (entity != null)
					World.EntityManager.AddBlockEntity(blockEntity.Key, entity);
			}
			
			Chunks.AddOrUpdate(
				position, coordinates => chunk, (coordinates, column) =>
				{
					if (!ReferenceEquals(column, chunk))
					{
						column.Dispose();
					}

					return chunk;
				});

			ScheduleChunkUpdate(position, ScheduleType.Full);
			//UpdateQueue.Enqueue(position);
		}

		/// <inheritdoc />
		public void RemoveChunk(ChunkCoordinates position, bool dispose = true)
		{
			BlockLightCalculations.Remove(position);
			UpdateQueue.Remove(position);
			UpdateBorderQueue.Remove(position);
			FastUpdateQueue.Remove(position);
			Scheduled.Remove(position);

			if (Chunks.TryRemove(position, out var column))
			{
				//foreach (var blockEntity in column.GetBlockEntities)
				{
				//	World.EntityManager.RemoveBlockEntity(
				//		new BlockCoordinates((column.X << 4) + blockEntity.X, blockEntity.Y, (column.Z << 4) + blockEntity.Z));
				}
				
				if (dispose)
					column.Dispose();
			}
		}

		/// <inheritdoc />
		public bool TryGetChunk(ChunkCoordinates coordinates, out ChunkColumn chunk)
		{
			return Chunks.TryGetValue(coordinates, out chunk);
		}

		/// <inheritdoc />
		public KeyValuePair<ChunkCoordinates, ChunkColumn>[] GetAllChunks()
		{
			return Chunks.ToArray();
		}

		/// <inheritdoc />
		public void ClearChunks()
		{
			UpdateQueue.Clear();
			FastUpdateQueue.Clear();
			UpdateBorderQueue.Clear();
			var chunks = Chunks.ToArray();
			Chunks.Clear();
			
			Scheduled.Clear();

			_renderedChunks = new ChunkData[0];
			foreach (var chunk in chunks)
			{
				chunk.Value.Dispose();
			}
		}

		public void ScheduleChunkUpdate(ChunkCoordinates position, ScheduleType type, bool prioritize = false)
		{
			var queue = UpdateQueue;
			if (Chunks.TryGetValue(position, out var cc))
			{
				if ((type & ScheduleType.Lighting) != 0)
				{
					//cc.SkyLightDirty = true;
				}
				
				if ((type & ScheduleType.Border) != 0)
				{
					queue = UpdateBorderQueue;
				}

				if (prioritize)
					queue = FastUpdateQueue;

				if (queue.Contains(position) && !prioritize)
				{
					return;
				}

				if (Scheduled.TryAdd(position))
				{
					if ((type & ScheduleType.Border) != 0)
						cc.ScheduleBorder();
					
					queue.Enqueue(position);
				}

				//Scheduled.TryAdd(position);
				//queue.Enqueue(position);
			}
		}

		#region  Drawing

		public bool UseWireFrames
		{
			get
			{
				return _rasterizerState.FillMode == FillMode.WireFrame;
			}
			set
			{
				_rasterizerState.FillMode = value ? FillMode.WireFrame : FillMode.Solid;
			}
		}

		private readonly SamplerState _renderSampler = new SamplerState()
		{
			Filter = TextureFilter.PointMipLinear	,
			AddressU = TextureAddressMode.Wrap,
			AddressV = TextureAddressMode.Wrap,
			MipMapLevelOfDetailBias = -1f,
			MaxMipLevel = Alex.MipMapLevel,
			//ComparisonFunction = 
			// MaxMipLevel = 0,
			FilterMode = TextureFilterMode.Default,
			AddressW = TextureAddressMode.Wrap,
			MaxAnisotropy = 16,
			//ComparisonFunction = CompareFunction.Greater
			// ComparisonFunction = 
		};

		private DepthStencilState DepthStencilState { get; } = new DepthStencilState()
		{
			DepthBufferEnable = true,
			DepthBufferFunction = CompareFunction.Less,
			DepthBufferWriteEnable = true
		};

		private readonly RasterizerState _rasterizerState = new RasterizerState()
		{
			//DepthBias = 0.0001f,
			CullMode = CullMode.CullClockwiseFace,
			FillMode = FillMode.Solid,
			//DepthClipEnable = true,
			//ScissorTestEnable = true
		};

		private BlendState TranslucentBlendState { get; } = new BlendState()
		{
			AlphaSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.SourceAlpha,
			AlphaDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.InverseSourceAlpha,
			ColorDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.InverseSourceAlpha,
			ColorSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.SourceAlpha,
			//ColorBlendFunction = BlendFunction.Add,
			IndependentBlendEnable = true,
			//AlphaBlendFunction = BlendFunction.Add,
			// ColorBlendFunction = BlendFunction.Add
		};
		
		public void Draw(IRenderArgs args, params RenderStage[] stages)
		{
			using (GraphicsContext gc = GraphicsContext.CreateContext(
				args.GraphicsDevice, Block.FancyGraphics ? BlendState.AlphaBlend : BlendState.Opaque, DepthStencilState, _rasterizerState,
				_renderSampler))
			{
				DrawStaged(args, null, stages.Length > 0 ? stages : RenderStages);
			}
		}
		
		private bool IsWithinView(ChunkCoordinates chunk, ICamera camera)
		{
			var frustum  = camera.BoundingFrustum;
			var chunkPos = new Vector3(chunk.X << 4, 0, chunk.Z << 4);

			if (chunk.DistanceTo(new ChunkCoordinates(camera.Position)) > RenderDistance)
				return false;
			
			return frustum.Intersects(new Microsoft.Xna.Framework.BoundingBox(chunkPos,
				chunkPos + new Vector3(ChunkColumn.ChunkWidth, MathF.Max(camera.Position.Y + 10f, 256f),
					ChunkColumn.ChunkDepth)));

		}
		
		private void DrawStaged(IRenderArgs args,
			Effect forceEffect = null, params RenderStage[] stages)
		{
			var originalBlendState = args.GraphicsDevice.BlendState;

			if (stages == null || stages.Length == 0)
				stages = RenderStages;

			ChunkData[]      chunks  = _renderedChunks;

			if (CancellationToken.IsCancellationRequested || chunks == null)
			{
				return;
			}
			
			RenderingShaders shaders = Shaders;
			foreach (var stage in stages)
			{
				args.GraphicsDevice.BlendState = originalBlendState;
				
				Effect effect   = forceEffect;
				if (forceEffect == null)
				{
					switch (stage)
					{
						case RenderStage.OpaqueFullCube:
							if (Block.FancyGraphics)
							{
								effect = shaders.TransparentEffect;
							}
							else
							{
								effect = shaders.OpaqueEffect;
							}
							break;
						case RenderStage.Opaque:
							if (Block.FancyGraphics)
							{
								effect = shaders.TransparentEffect;
							}
							else
							{
								effect = shaders.OpaqueEffect;
							}
							//effect = shaders.TransparentEffect;
							break;
						case RenderStage.Transparent:
							effect = shaders.TransparentEffect;
							break;
						case RenderStage.Translucent:
							args.GraphicsDevice.BlendState = TranslucentBlendState;
							effect = shaders.TranslucentEffect;
							break;
						case RenderStage.Animated:
							effect = shaders.AnimatedEffect;
							break;
						case RenderStage.Liquid:
					//	case RenderStage.AnimatedTranslucent:
							effect = shaders.AnimatedEffect;
						    
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
				
				DrawChunks(args.GraphicsDevice, chunks, effect, stage);
			}

			args.GraphicsDevice.BlendState = originalBlendState;
		}
		
		private void DrawChunks(GraphicsDevice device, ChunkData[] chunks, Effect effect, RenderStage stage)
		{
			for (var index = 0; index < chunks.Length; index++)
			{
				var chunk = chunks[index];
				if (chunk == null) continue;

				chunk.Draw(device, stage, effect);
			}
		}
		
		#endregion

		int                      _currentFrame = 0;
		int                      _framerate    = 12;     // Animate at 12 frames per second
		float                    _timer        = 0.0f;
		public void Update(IUpdateArgs args)
		{
			Shaders.Update(args.Camera);

			_timer += (float)args.GameTime.ElapsedGameTime.TotalSeconds;
			if (_timer >= (1.0f / _framerate ))
			{
				_timer -= 1.0f / _framerate ;
				_currentFrame = (_currentFrame + 1) % Resources.Atlas.GetFrameCount();
				
				Shaders.SetAnimatedTextures(Resources.Atlas.GetAtlas(_currentFrame));
			}
		}

		/// <inheritdoc />
		public void OnTick()
		{
			List<ChunkData> renderList = new List<ChunkData>();
			foreach (var chunk in Chunks)
			{
				bool inView = IsWithinView(chunk.Key, World.Camera);
				if (inView)
				{
					renderList.Add(chunk.Value.ChunkData);
				}

				if ((chunk.Value.BlockLightDirty || chunk.Value.SkyLightDirty))
				{
					//if (chunk.Value.ScheduledLightUpdates < 3)
						ScheduleChunkUpdate(chunk.Key, ScheduleType.Lighting);
				}
			}

			_renderedChunks = renderList.ToArray();
		}

		private bool _disposed = false;
		/// <inheritdoc />
		public void Dispose()
		{
			if (_disposed)
				return;

			try
			{
				//Graphics?.Dispose();
				BlockLightCalculations?.Dispose();
				BlockLightCalculations = null;
				
				SkyLightCalculator?.Dispose();
				SkyLightCalculator = null;
				
				_renderSampler.Dispose();
				CancellationToken.Cancel();
				UpdateQueue.Clear();
				FastUpdateQueue.Clear();
				UpdateBorderQueue.Clear();

				foreach (var chunk in Chunks)
				{
					chunk.Value.Dispose();
				}

				Chunks.Clear();

				foreach (var rendered in _renderedChunks)
					rendered.Dispose();

				_renderedChunks = null;
			}
			finally
			{
				_disposed = true;
			}
		}
	}
}