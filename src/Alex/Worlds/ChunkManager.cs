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
using Alex.API.Utils.Collections;
using Alex.API.Utils.Vectors;
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
using RocketUI;
using MathF = System.MathF;

namespace Alex.Worlds
{
	public class ChunkManager : IChunkManager, IDisposable, ITicked
	{
		private readonly static RenderStage[]   RenderStages = ((RenderStage[]) Enum.GetValues(typeof(RenderStage)));
		
		private                 GraphicsDevice  Graphics  { get; }
		private                 World           World     { get; }
		
		private AlexOptions                                         Options        { get; }
		private ConcurrentDictionary<ChunkCoordinates, ChunkColumn> Chunks         { get; }
		
		public  RenderingShaders        Shaders                { get; }
		private CancellationTokenSource CancellationToken      { get; }
		private BlockLightCalculations  BlockLightCalculations { get; set; }
		private SkyLightCalculations    SkyLightCalculator     { get; set; }

		private FancyQueue<ChunkCoordinates>     FastUpdateQueue   { get; }
		private FancyQueue<ChunkCoordinates>     UpdateQueue       { get; }
		private FancyQueue<ChunkCoordinates>     UpdateBorderQueue { get; }
		//private ThreadSafeList<ChunkCoordinates> Scheduled         { get; } = new ThreadSafeList<ChunkCoordinates>();
		
		public ChunkManager(IServiceProvider serviceProvider, GraphicsDevice graphics, World world, CancellationToken cancellationToken)
		{
			Graphics = graphics;
			World = world;
			
			Options = serviceProvider.GetRequiredService<IOptionsProvider>().AlexOptions;

			var stillAtlas =  serviceProvider.GetRequiredService<ResourceManager>().BlockAtlas.GetAtlas();
	        
			var fogStart = 0;
			Shaders = new RenderingShaders(Graphics);
			Shaders.SetTextures(stillAtlas);
			Shaders.FogEnabled = Options.VideoOptions.Fog.Value;
			Options.VideoOptions.Fog.Bind(
				(old, newValue) =>
				{
					Shaders.FogEnabled = newValue;
				});
			//Shaders.SetAnimatedTextures(Resources.Atlas.GetAtlas(0));
			
			_renderSampler.MaxMipLevel = stillAtlas.LevelCount;
			
			RenderDistance = Options.VideoOptions.RenderDistance.Value;

			Options.VideoOptions.RenderDistance.Bind(
				(value, newValue) =>
				{
					RenderDistance = newValue;
				});
			
			Chunks = new ConcurrentDictionary<ChunkCoordinates, ChunkColumn>();
			CancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			
			BlockLightCalculations = new BlockLightCalculations(world, CancellationToken.Token);
			SkyLightCalculator = new SkyLightCalculations(world, CancellationToken.Token);
			
			UpdateQueue = new FancyQueue<ChunkCoordinates>();
			UpdateBorderQueue = new FancyQueue<ChunkCoordinates>();
			FastUpdateQueue = new FancyQueue<ChunkCoordinates>();
		}

		private long _threadsRunning = 0;
		private ChunkData[] _renderedChunks = new ChunkData[0];

		public int RenderDistance
		{
			get => _renderDistance;
			set
			{
				var currentlyRenderedChunks = _renderedChunks;
				ChunkData[] renderedArray = new ChunkData[value * value * 3];

				for (int i = 0; i < renderedArray.Length; i++)
				{
					if (currentlyRenderedChunks != null 
					    && i < currentlyRenderedChunks.Length)
					{
						renderedArray[i] = currentlyRenderedChunks[i];
					}
					else
					{
						renderedArray[i] = null;
					}
				}

				_renderedChunks = renderedArray;
				_renderDistance = value;
			}
		}

		public int ConcurrentChunkUpdates => (int) _threadsRunning;
		public int EnqueuedChunkUpdates => (UpdateQueue?.Count ?? 0) + (FastUpdateQueue?.Count ?? 0) + (UpdateBorderQueue?.Count ?? 0);
		
		/// <inheritdoc />
		public int ChunkCount => Chunks.Count;

		/// <inheritdoc />
		public int RenderedChunks { get; private set; } = 0;

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
		
		private Thread _processingThread = null;
		/// <inheritdoc />
		public void EnsureStarted()
		{
			if (_processingThread != null)
				return;

			ThreadPool.QueueUserWorkItem(
				o =>
				{
					_processingThread = Thread.CurrentThread;
					Thread.CurrentThread.Name = "Chunk Management";

					SpinWait sw = new SpinWait();
					while (!CancellationToken.IsCancellationRequested)
					{
						if (World?.Camera == null)
						{
							sw.SpinOnce();
							continue;
						}

						if (CancellationToken.IsCancellationRequested)
							break;
						
						bool processedLighting = ProcessLighting();
						bool processedQueue    = ProcessQueue();

						if (!processedQueue && !processedLighting)
							sw.SpinOnce();
					}

					_processingThread = null;
				});
		}

		private AutoResetEvent _processingSync = new AutoResetEvent(false);
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

		//	maxThreads = Math.Max(1, Math.Min(maxThreads, EnqueuedChunkUpdates / 4));

			var activeThreads = Interlocked.Read(ref _threadsRunning);

			if (queue.IsEmpty || activeThreads >= maxThreads)
			{
				return false;
			}

			if (Interlocked.CompareExchange(ref _threadsRunning, activeThreads + 1, activeThreads) == activeThreads)
			{
				ThreadPool.QueueUserWorkItem(
					oo =>
					{
						Thread.CurrentThread.Name = $"ChunkManager Processing Thread {activeThreads}";
						try
						{
							while (!CancellationToken.IsCancellationRequested)
							{
								if (EnqueuedChunkUpdates <= 0 || queue.IsEmpty)
								{
									if (!_processingSync.WaitOne(50))
										break;

									queue = GetQueue(queue);

									if (queue.IsEmpty)
										break;
								}


								if (queue.TryDequeue(out var chunkCoordinates))
								{
									if (TryGetChunk(chunkCoordinates, out var chunk))
									{
										if (!Monitor.TryEnter(chunk.UpdateLock, 0))
											continue;
										

										try
										{
											bool newChunk = chunk.IsNew;

											bool c1 = false;
											bool c2 = false;
											bool c3 = false;
											bool c4 = false;
											
											if (newChunk)
											{
												c1 = TryGetChunk(new ChunkCoordinates(chunk.X + 1, chunk.Z), out _);
												c2 = TryGetChunk(new ChunkCoordinates(chunk.X - 1, chunk.Z), out _);
												
												c3 = TryGetChunk(new ChunkCoordinates(chunk.X, chunk.Z + 1), out _);
												c4 = TryGetChunk(new ChunkCoordinates(chunk.X, chunk.Z - 1), out _);
											}
											
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
													SkyLightCalculator.Recalculate(chunk);
												}
											}

											chunk.UpdateBuffer(Graphics, World);

											if (newChunk)
											{
												if (c1)
													ScheduleChunkUpdate(
														new ChunkCoordinates(chunk.X + 1, chunk.Z), ScheduleType.Border);

												if (c2)
													ScheduleChunkUpdate(
														new ChunkCoordinates(chunk.X - 1, chunk.Z), ScheduleType.Border);

												if (c3)
													ScheduleChunkUpdate(
														new ChunkCoordinates(chunk.X, chunk.Z + 1), ScheduleType.Border);

												if (c4)
													ScheduleChunkUpdate(
														new ChunkCoordinates(chunk.X, chunk.Z - 1), ScheduleType.Border);
											}
										}
										finally
										{
											chunk.ScheduledForUpdate = false;
											//Scheduled.Remove(chunkCoordinates);
											Monitor.Exit(chunk.UpdateLock);
										}
									}
								}

								queue = GetQueue(queue);
							}
						}
						finally
						{
							Interlocked.Decrement(ref _threadsRunning);
						}
					});

				return true;
			}

			return false;
		}

		private FancyQueue<ChunkCoordinates> GetQueue(FancyQueue<ChunkCoordinates> queue)
		{
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

			return queue;
		}
		
		private bool ProcessLighting()
		{
			var blockLightCalc = BlockLightCalculations;

			if (blockLightCalc == null)
				return false;
			
			var target         = Chunks?.FirstOrDefault(x => blockLightCalc.HasEnqueued(x.Value.Coordinates));

			if (!target.HasValue || target.Value.Value?.Coordinates == null)
				return false;
			
			return blockLightCalc.Process(target.Value.Value.Coordinates);
		}


		/// <inheritdoc />
		public void AddChunk(ChunkColumn chunk, ChunkCoordinates position, bool doUpdates = false)
		{
			if (CancellationToken.IsCancellationRequested)
				return;
			
			chunk.CalculateHeight();
			
			if (chunk.IsNew)
			{
				//	SkyLightCalculator.Recalculate(chunk);
				BlockLightCalculations.Recalculate(chunk);
			}

			foreach (var blockEntity in chunk.BlockEntities)
			{
				//var coordinates = new BlockCoordinates(blockEntity.X, blockEntity.Y, blockEntity.Z);
				//World.SetBlockEntity(coordinates.X, coordinates.Y, coordinates.Z, blockEntity);
				var entity = BlockEntityFactory.ReadFrom(blockEntity.Value, World, 
					chunk.GetBlockState(blockEntity.Key.X & 0xf, blockEntity.Key.Y & 0xff, blockEntity.Key.Z & 0xf).Block);
				if (entity != null)
					World?.EntityManager?.AddBlockEntity(blockEntity.Key, entity);
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

			EnsureStarted();
			//UpdateQueue.Enqueue(position);
		}

		/// <inheritdoc />
		public void RemoveChunk(ChunkCoordinates position, bool dispose = true)
		{
			BlockLightCalculations.Remove(position);
			UpdateQueue.Remove(position);
			UpdateBorderQueue.Remove(position);
			FastUpdateQueue.Remove(position);
			//Scheduled.Remove(position);

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
			UpdateQueue?.Clear();
			FastUpdateQueue?.Clear();
			UpdateBorderQueue?.Clear();
			
			var chunks = Chunks.ToArray();
			Chunks.Clear();

			//_renderedChunks = new ChunkData[0];
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
				if (cc.ScheduledForUpdate && !prioritize)
					return;

				if ((type & ScheduleType.Lighting) != 0)
				{
					//cc.SkyLightDirty = true;
				}
				
				if ((type & ScheduleType.Border) != 0)
				{
					
						cc.ScheduleBorder();
					
					queue = UpdateBorderQueue;
				}

				if (prioritize)
					queue = FastUpdateQueue;

				if (queue.Contains(position) && !prioritize)
				{
					return;
				}

				if (Monitor.TryEnter(cc.UpdateLock, 0))
				{
					try
					{
						if (!cc.ScheduledForUpdate)
						{
							cc.ScheduledForUpdate = true;

							queue.Enqueue(position);

							_processingSync.Set();
						}
					}
					finally
					{
						Monitor.Exit(cc.UpdateLock);
					}
				}
				//Scheduled.TryAdd(position);
				//queue.Enqueue(position);
			}
		}

		#region  Drawing

		private bool _useWireFrame = false;
		public bool UseWireFrames
		{
			get
			{
				return _rasterizerState.FillMode == FillMode.WireFrame;
			}
			set
			{
				if (value && _rasterizerState.FillMode != FillMode.WireFrame)
				{
					_rasterizerState = _rasterizerState.Copy();
					_rasterizerState.FillMode = FillMode.WireFrame;
				}
				else if (!value && _rasterizerState.FillMode == FillMode.WireFrame)
				{
					_rasterizerState = _rasterizerState.Copy();
					_rasterizerState.FillMode = FillMode.Solid;
				}
			}
		}

		private readonly SamplerState _renderSampler = new SamplerState()
		{
			Filter = TextureFilter.PointMipLinear	,
			AddressU = TextureAddressMode.Wrap,
			AddressV = TextureAddressMode.Wrap,
			//MipMapLevelOfDetailBias = -1f,
			MipMapLevelOfDetailBias = -3f,
			MaxMipLevel = Alex.MipMapLevel,
			//ComparisonFunction = 
			// MaxMipLevel = 0,
			FilterMode = TextureFilterMode.Default,
			AddressW = TextureAddressMode.Wrap,
			ComparisonFunction = CompareFunction.Never,
			MaxAnisotropy = 16,
			BorderColor = Color.Black,
			//ComparisonFunction = CompareFunction.Greater
			// ComparisonFunction = 
		};

		private DepthStencilState DepthStencilState { get; } = new DepthStencilState()
		{
			DepthBufferEnable = true,
			DepthBufferFunction = CompareFunction.Less,
			DepthBufferWriteEnable = true
		};

		private RasterizerState _rasterizerState = new RasterizerState()
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
		
		public void Draw(IRenderArgs args, Effect forceEffect = null, params RenderStage[] stages)
		{
			using (GraphicsContext gc = GraphicsContext.CreateContext(
				args.GraphicsDevice, Block.FancyGraphics ? BlendState.AlphaBlend : BlendState.Opaque, DepthStencilState, _rasterizerState,
				_renderSampler))
			{
				DrawCount = DrawStaged(args, forceEffect, stages.Length > 0 ? stages : RenderStages);
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
		
		private int DrawStaged(IRenderArgs args,
			Effect forceEffect = null, params RenderStage[] stages)
		{
			int drawCount = 0;
			var originalBlendState = args.GraphicsDevice.BlendState;

			if (stages == null || stages.Length == 0)
				stages = RenderStages;

			ChunkData[]      chunks  = _renderedChunks;

			if (CancellationToken.IsCancellationRequested || chunks == null)
			{
				return drawCount;
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
						case RenderStage.Opaque:
							effect = Block.FancyGraphics ? shaders.TransparentEffect : shaders.OpaqueEffect;
							break;
						case RenderStage.Transparent:
							effect = shaders.TransparentEffect;
							break;
						case RenderStage.Translucent:
							args.GraphicsDevice.BlendState = TranslucentBlendState;
							effect = shaders.TransparentEffect;
							break;
						case RenderStage.Liquid:
						case RenderStage.Animated:
							effect = shaders.AnimatedEffect;
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}

				drawCount += DrawChunks(args.GraphicsDevice, chunks, effect, stage);
			}

			args.GraphicsDevice.BlendState = originalBlendState;

			return drawCount;
		}

		/// <summary>
		///		The amount of calls made to DrawPrimitives in the last render call
		/// </summary>
		public int DrawCount { get; set; } = 0;
		private int DrawChunks(GraphicsDevice device, ChunkData[] chunks, Effect effect, RenderStage stage)
		{
			int drawn = 0;
			for (var index = 0; index < chunks.Length; index++)
			{
				var chunk = chunks[index];
				if (chunk == null) continue;
				
				drawn += chunk.Draw(device, stage, effect);
			}

			return drawn;
		}
		
		#endregion
		
		public void Update(IUpdateArgs args)
		{
			Shaders.Update((float)args.GameTime.ElapsedGameTime.TotalSeconds, args.Camera);
		}

		/// <inheritdoc />
		public void OnTick()
		{
			//	List<ChunkData> renderList = new List<ChunkData>();
			var array = _renderedChunks;
			int index = 0;
			int max = array.Length;

			foreach (var chunk in Chunks)
			{
				bool inView = IsWithinView(chunk.Key, World.Camera);

				if (inView && index < max)
				{
					index++;
					array[index] = chunk.Value?.ChunkData;
					//renderList.Add(chunk.Value.ChunkData);
				}

				//if ((chunk.Value.BlockLightDirty || chunk.Value.SkyLightDirty))
				//{
				//	ScheduleChunkUpdate(chunk.Key, ScheduleType.Lighting);
				//}
			}

			RenderedChunks = index;

			//_renderedChunks = renderList.ToArray();
		}

		private bool _disposed = false;
		private int _renderDistance = 0;

		/// <inheritdoc />
		public void Dispose()
		{
			if (_disposed)
				return;

			try
			{
				//Graphics?.Dispose();
				CancellationToken?.Cancel();
				
				BlockLightCalculations?.Dispose();

				SkyLightCalculator?.Dispose();

				_renderSampler?.Dispose();
				UpdateQueue?.Clear();
				FastUpdateQueue?.Clear();
				UpdateBorderQueue?.Clear();

				ClearChunks();

				if (_renderedChunks != null)
					foreach (var rendered in _renderedChunks)
						rendered?.Dispose();

				_renderedChunks = null;
				BlockLightCalculations = null;
				SkyLightCalculator = null;
			}
			finally
			{
				_disposed = true;
			}
		}
	}
}