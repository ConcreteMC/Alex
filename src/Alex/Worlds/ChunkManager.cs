using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Alex.Blocks.Minecraft;
using Alex.Common;
using Alex.Common.Data.Options;
using Alex.Common.Graphics;
using Alex.Common.Services;
using Alex.Common.Utils.Vectors;
using Alex.Common.World;
using Alex.Entities.BlockEntities;
using Alex.Gamestates;
using Alex.Graphics.Effect;
using Alex.Utils;
using Alex.Utils.Collections.Queue;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Chunks;
using Alex.Worlds.Lighting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using RocketUI;
using MathF = System.MathF;

namespace Alex.Worlds
{
	public class ChunkEventArgs : EventArgs
	{
		public ChunkCoordinates Position { get; }
		protected ChunkEventArgs(ChunkCoordinates coordinates)
		{
			Position = coordinates;
		}
	}

	public class ChunkAddedEventArgs : ChunkEventArgs
	{
		public ChunkColumn Chunk { get; }
		/// <inheritdoc />
		internal ChunkAddedEventArgs(ChunkColumn column) : base(new ChunkCoordinates(column.X, column.Z))
		{
			Chunk = column;
		}
	}
	
	public class ChunkRemovedEventArgs : ChunkEventArgs
	{
		/// <inheritdoc />
		internal ChunkRemovedEventArgs(ChunkCoordinates coordinates) : base(coordinates)
		{
			
		}
	}

	public class ChunkUpdatedEventArgs : ChunkEventArgs
	{
		public ChunkColumn Chunk { get; }
		public TimeSpan ExecutionTime { get; }
		/// <inheritdoc />
		internal ChunkUpdatedEventArgs(ChunkColumn column, TimeSpan executionTime) : base(new ChunkCoordinates(column.X, column.Z))
		{
			Chunk = column;
			ExecutionTime = executionTime;
		}
	}
	
	public class ChunkManager : IDisposable, ITicked
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ChunkManager));
		private readonly static RenderStage[]   RenderStages = ((RenderStage[]) Enum.GetValues(typeof(RenderStage)));
		
		private                 GraphicsDevice  Graphics  { get; }
		private                 World           World     { get; }
		
		private AlexOptions                                         Options        { get; }
		private ConcurrentDictionary<ChunkCoordinates, ChunkColumn> Chunks         { get; }
		
		public  RenderingShaders        Shaders                { get; }
		private CancellationTokenSource CancellationToken      { get; }
		internal BlockLightCalculations  BlockLightUpdate { get; set; }
		internal SkyLightCalculations    SkyLightCalculator     { get; set; }

		public EventHandler<ChunkUpdatedEventArgs> OnChunkUpdate;
		public EventHandler<ChunkAddedEventArgs> OnChunkAdded;
		public EventHandler<ChunkRemovedEventArgs> OnChunkRemoved;
		
		private ActionBlock<ChunkCoordinates> _actionBlock;
		private PriorityBufferBlock<ChunkCoordinates> _priorityBuffer;
		public ChunkManager(IServiceProvider serviceProvider, GraphicsDevice graphics, World world, CancellationToken cancellationToken)
		{
			Graphics = graphics;
			World = world;
			
			Options = serviceProvider.GetRequiredService<IOptionsProvider>().AlexOptions;

			var stillAtlas =  serviceProvider.GetRequiredService<ResourceManager>().BlockAtlas.GetAtlas();
	        
			var fogStart = 0;
			Shaders = new RenderingShaders();
			Shaders.SetTextures(stillAtlas);
			Shaders.FogEnabled = Options.VideoOptions.Fog.Value;
			Options.VideoOptions.Fog.Bind(
				(old, newValue) =>
				{
					Shaders.FogEnabled = newValue;
				});
			_renderSampler.MaxMipLevel = stillAtlas.LevelCount;

			Chunks = new ConcurrentDictionary<ChunkCoordinates, ChunkColumn>();
			CancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			
			BlockLightUpdate = new BlockLightCalculations(world, CancellationToken.Token);
			SkyLightCalculator = new SkyLightCalculations(world, CancellationToken.Token);

			_priorityBuffer = new PriorityBufferBlock<ChunkCoordinates>(
				new ExecutionDataflowBlockOptions
				{
					CancellationToken = CancellationToken.Token, 
					EnsureOrdered = false, 
					MaxDegreeOfParallelism = 1
				},
				new ExecutionDataflowBlockOptions
				{
					CancellationToken = CancellationToken.Token, 
					EnsureOrdered = false,
					MaxDegreeOfParallelism = 1
				},
				new ExecutionDataflowBlockOptions
				{
					CancellationToken = CancellationToken.Token,
					EnsureOrdered = false,
					MaxDegreeOfParallelism = 1
				});

			_actionBlock = new ActionBlock<ChunkCoordinates>(DoUpdate, new ExecutionDataflowBlockOptions()
			{
				CancellationToken = cancellationToken,
				MaxDegreeOfParallelism = Options.MiscelaneousOptions.ChunkThreads.Value
			});
			
			_priorityBuffer.LinkTo(_actionBlock);
			EnsureStarted();
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
		public int EnqueuedChunkUpdates => _actionBlock.InputCount;
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
			}
		}

		public float FogDistance
		{
			get { return Shaders.FogDistance; }
			set
			{
				Shaders.FogDistance = value;
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

		public float WaterSurfaceTransparency { get; set; } = 0.65f;
		
		private Thread _processingThread = null;
		/// <inheritdoc />
		public void EnsureStarted()
		{
			
			
			if (_processingThread != null)
				return;

			var task = new Task(
				() =>
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

						int lightUpdatesExecuted = BlockLightUpdate.Execute() + SkyLightCalculator.Execute();
						sw.SpinOnce();
					}

					_processingThread = null;
				}, TaskCreationOptions.LongRunning);
			
			task.Start();
		}

		private void DoUpdate(ChunkCoordinates chunkCoordinates)
		{
			Stopwatch timingWatch = Stopwatch.StartNew();
			if (!TryGetChunk(chunkCoordinates, out var chunk) || !chunk.Scheduled) return;

			if (!Monitor.TryEnter(chunk.UpdateLock, 0))
				return;

			Interlocked.Increment(ref _threadsRunning);
			try
			{
				bool newChunk = chunk.IsNew;

				bool c1 = false;
				bool c2 = false;
				bool c3 = false;
				bool c4 = false;

				if (newChunk)
				{
					if (SkyLightCalculator != null)
						SkyLightCalculator.Recalculate(chunk);
										
					if (BlockLightUpdate != null)
						BlockLightUpdate.RecalculateChunk(chunk);
				}
											
				if (chunk.UpdateBuffer(World, true))
				{
					if (newChunk)
					{
						foreach (var neighbor in chunk.Neighbors)
						{
							ScheduleChunkUpdate(
								neighbor,
								ScheduleType.Border, false, chunkCoordinates);
						}
					}
												
					OnChunkUpdate?.Invoke(this, new ChunkUpdatedEventArgs(chunk, timingWatch.Elapsed));
				}
			}
			finally
			{
				Interlocked.Decrement(ref _threadsRunning);
				
				chunk.Scheduled = false;
				Monitor.Exit(chunk.UpdateLock);
			}
		}

		/// <inheritdoc />
		public void AddChunk(ChunkColumn chunk, ChunkCoordinates position, bool doUpdates = false)
		{
			if (CancellationToken.IsCancellationRequested)
				return;
			
			chunk.CalculateHeight();

			foreach (var blockEntity in chunk.BlockEntities)
			{
				var entity = BlockEntityFactory.ReadFrom(blockEntity.Value, World, 
					chunk.GetBlockState(blockEntity.Key.X & 0xf, blockEntity.Key.Y, blockEntity.Key.Z & 0xf).Block);
				
				if (entity != null)
					World?.EntityManager?.AddBlockEntity(blockEntity.Key, entity);
			}
			
			var column = Chunks.AddOrUpdate(
				position, coordinates => chunk, (coordinates, oldColumn) =>
				{
					if (!ReferenceEquals(oldColumn, chunk))
					{
						oldColumn.Dispose();
					}

					return chunk;
				});

			ScheduleChunkUpdate(position, ScheduleType.Full, false);

			OnChunkAdded?.Invoke(this, new ChunkAddedEventArgs(column));
		}

		/// <inheritdoc />
		public void RemoveChunk(ChunkCoordinates position, bool dispose = true)
		{
			//Scheduled.Remove(position);

			if (Chunks.TryRemove(position, out var column))
			{
				if (dispose)
					column.Destroyed = true;
				
				foreach (var blockEntity in column.BlockEntities)
				{
					var pos = blockEntity.Key;
					World.EntityManager.RemoveBlockEntity(
						new BlockCoordinates((column.X << 4) + pos.X, pos.Y, (column.Z << 4) + pos.Z));
				}
				
				if (dispose)
					column.Dispose();

				if (TryGetChunk(new ChunkCoordinates(position.X + 1, position.Z), out var c1))
					c1.Neighbors.Remove(position);
				
				if (TryGetChunk(new ChunkCoordinates(position.X - 1, position.Z), out var c2))
					c2.Neighbors.Remove(position);
				
				if (TryGetChunk(new ChunkCoordinates(position.X, position.Z + 1), out var c3))
					c3.Neighbors.Remove(position);
				
				if (TryGetChunk(new ChunkCoordinates(position.X, position.Z - 1), out var c4))
					c4.Neighbors.Remove(position);
				
				OnChunkRemoved?.Invoke(this, new ChunkRemovedEventArgs(position));
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
			_priorityBuffer?.TryReceiveAll(out _);
			
			var chunks = Chunks.ToArray();
			Chunks.Clear();

			foreach (var chunk in chunks)
			{
				OnChunkRemoved?.Invoke(this, new ChunkRemovedEventArgs(chunk.Key));
				chunk.Value.Dispose();
			}
		}

		public void ScheduleChunkUpdate(ChunkCoordinates position,
			ScheduleType type,
			bool prioritize = false,
			ChunkCoordinates source = default)
		{
			if (!Chunks.TryGetValue(position, out var cc)) return;

			if (cc.Scheduled && !prioritize)
				return;

			if ((type & ScheduleType.Border) != 0)
			{
				if (cc.Neighbors.Contains(source))
					return;

				cc.Neighbors.Add(source);
			}

			if (prioritize)
			{
				_priorityBuffer.Post(position, Priority.High);
				cc.Scheduled = true;
			}
			else
			{
				_priorityBuffer.Post(position, ((type & ScheduleType.Border) != 0) ? Priority.Low : Priority.Medium);
				cc.Scheduled = true;
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
			IndependentBlendEnable = false,
			//AlphaBlendFunction = BlendFunction.Add,
			// ColorBlendFunction = BlendFunction.Add
		};
		
		public int Draw(IRenderArgs args, Effect forceEffect = null, params RenderStage[] stages)
		{
			using (GraphicsContext gc = GraphicsContext.CreateContext(
				args.GraphicsDevice, Block.FancyGraphics ? BlendState.AlphaBlend : BlendState.Opaque, DepthStencilState, _rasterizerState,
				_renderSampler))
			{
				return DrawStaged(args, forceEffect, stages.Length > 0 ? stages : RenderStages);
			}
		}
		
		private bool IsWithinView(ChunkCoordinates chunk, ICamera camera)
		{
			ChunkCoordinates center = ViewPosition.GetValueOrDefault(new ChunkCoordinates(camera.Position));
			var frustum  = camera.BoundingFrustum;
			var chunkPos = new Vector3(chunk.X << 4, -64, chunk.Z << 4);

			if (chunk.DistanceTo(center) > RenderDistance)
				return false;

			return frustum.Intersects(new Microsoft.Xna.Framework.BoundingBox(chunkPos,
				chunkPos + new Vector3(ChunkColumn.ChunkWidth, MathF.Max(camera.Position.Y + 16f, 256f),
					ChunkColumn.ChunkDepth)));
		}
		
		private int DrawStaged(IRenderArgs args,
			Effect forceEffect = null, params RenderStage[] stages)
		{
			int drawCount = 0;
			var originalBlendState = args.GraphicsDevice.BlendState;
			var originalCullMode = args.GraphicsDevice.RasterizerState;
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
				args.GraphicsDevice.RasterizerState = originalCullMode;
				
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
							if (World.Player.HeadInWater)
							{
								args.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
							}

							effect = shaders.AnimatedEffect;

							break;
						case RenderStage.Animated:
							effect = shaders.AnimatedEffect;
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}

				//if (effect is BlockEffect be)
				//{
				//	be.CurrentTechnique = be.Techniques[technique];
				//}
				
				for (var index = 0; index < chunks.Length; index++)
				{
					var chunk = chunks[index];
					if (chunk == null) continue;
				
					drawCount += chunk.Draw(args.GraphicsDevice, stage, effect);
				}
			}

			args.GraphicsDevice.BlendState = originalBlendState;

			return drawCount;
		}

		public ChunkCoordinates? ViewPosition { get; set; } = null;

		#endregion
		
		public void Update(IUpdateArgs args)
		{
			Shaders.Update((float)args.GameTime.ElapsedGameTime.TotalSeconds, args.Camera);
		}

		/// <inheritdoc />
		public void OnTick()
		{
			var array = _renderedChunks;
			int index = 0;
			int max = array.Length;

			foreach (var chunk in Chunks)
			{
				bool inView = IsWithinView(chunk.Key, World.Camera);

				var data = chunk.Value.ChunkData;
				if (data != null)
				{
					if (inView && index + 1 < max)
					{
						data.Rendered = true;
						
						array[index] = data;
						index++;
					}
					else
					{
						data.Rendered = false;
					}
				}
			}

			RenderedChunks = index;
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
				CancellationToken?.Cancel();
				SkyLightCalculator?.Dispose();

				_renderSampler?.Dispose();
				_priorityBuffer?.Complete();
				ClearChunks();

				if (_renderedChunks != null)
					foreach (var rendered in _renderedChunks)
						rendered?.Dispose();

				_renderedChunks = null;
				SkyLightCalculator = null;
			}
			finally
			{
				_disposed = true;
			}
		}
	}
}