using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Alex.Blocks;
using Alex.Common;
using Alex.Common.Data.Options;
using Alex.Common.Graphics;
using Alex.Common.Services;
using Alex.Common.Utils.Vectors;
using Alex.Common.World;
using Alex.Graphics;
using Alex.Utils;
using Alex.Worlds.Chunks;
using Alex.Worlds.Lighting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using RocketUI;

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
		public ChunkColumn Chunk { get; }

		/// <inheritdoc />
		internal ChunkRemovedEventArgs(ChunkColumn column) : base(new ChunkCoordinates(column.X, column.Z))
		{
			Chunk = column;
		}
	}

	public class ChunkUpdatedEventArgs : ChunkEventArgs
	{
		public ChunkColumn Chunk { get; }
		public TimeSpan ExecutionTime { get; }

		/// <inheritdoc />
		internal ChunkUpdatedEventArgs(ChunkColumn column, TimeSpan executionTime) : base(
			new ChunkCoordinates(column.X, column.Z))
		{
			Chunk = column;
			ExecutionTime = executionTime;
		}
	}

	public record ChunkUpdateData(ChunkCoordinates Coordinates, ScheduleType Type, ChunkCoordinates? Source);

	public class ChunkManager : IDisposable, ITicked
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ChunkManager));
		private readonly static RenderStage[] RenderStages = ((RenderStage[])Enum.GetValues(typeof(RenderStage)));

		private GraphicsDevice Graphics { get; }
		private World World { get; }

		private AlexOptions Options { get; }
		private ConcurrentDictionary<ChunkCoordinates, ChunkColumn> Chunks { get; }

		public RenderingShaders Shaders { get; }
		private CancellationTokenSource CancellationToken { get; }
		internal BlockLightCalculations BlockLightUpdate { get; set; }
		internal SkyLightCalculations SkyLightCalculator { get; set; }

		public EventHandler<ChunkUpdatedEventArgs> OnChunkUpdate;
		public EventHandler<ChunkAddedEventArgs> OnChunkAdded;
		public EventHandler<ChunkRemovedEventArgs> OnChunkRemoved;

		private ActionBlock<ChunkUpdateData> _actionBlock;
		private PriorityBufferBlock<ChunkUpdateData> _priorityBuffer;
		private List<ChunkCoordinates> _queued = new List<ChunkCoordinates>();

		public bool CalculateSkyLighting { get; set; } = true;
		public bool CalculateBlockLighting { get; set; } = true;

		private readonly ResourceManager _resourceManager;

		public ChunkManager(IServiceProvider serviceProvider,
			GraphicsDevice graphics,
			World world,
			CancellationToken cancellationToken)
		{
			Graphics = graphics;
			World = world;

			Options = serviceProvider.GetRequiredService<IOptionsProvider>().AlexOptions;

			_resourceManager = serviceProvider.GetRequiredService<ResourceManager>();
			//var stillAtlas =  serviceProvider.GetRequiredService<ResourceManager>().BlockAtlas.GetAtlas();

			//var fogStart = 0;
			Shaders = new RenderingShaders();
			//Shaders.SetTextures(stillAtlas);
			/*FogEnabled = Options.VideoOptions.Fog.Value;
			FogDistance = Options.VideoOptions.RenderDistance.Value;
			Options.VideoOptions.Fog.Bind(
				(old, newValue) =>
				{
					FogEnabled = newValue;
					FogDistance = RenderDistance;
				});*/
			//_renderSampler.MaxMipLevel = stillAtlas.LevelCount;

			Chunks = new ConcurrentDictionary<ChunkCoordinates, ChunkColumn>();
			CancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

			BlockLightUpdate = new BlockLightCalculations(world, CancellationToken.Token);
			SkyLightCalculator = new SkyLightCalculations(world, CancellationToken.Token);

			Options.MiscelaneousOptions.ChunkThreads.Bind((value, newValue) => BuildActionBlock(newValue));
			BuildActionBlock(Options.MiscelaneousOptions.ChunkThreads.Value);

			EnsureStarted();

			InitTextures();

			_resourceManager.OnResourcesReloaded += OnResourcesReloaded;
		}

		private void OnResourcesReloaded(object sender, EventArgs e)
		{
			InitTextures();

			foreach (var chunk in Chunks.ToArray())
			{
				chunk.Value.Reset();
				ScheduleChunkUpdate(chunk.Key, ScheduleType.Full, true);
			}
		}

		private void InitTextures()
		{
			var blockAtlas = _resourceManager?.BlockAtlas;

			if (blockAtlas == null)
				return;

			var texture = blockAtlas.GetAtlas(false);
			SetTextures(texture);
			blockAtlas.AtlasGenerated += AtlasGenerated;
		}

		private void AtlasGenerated(object sender, AtlasTexturesGeneratedEventArgs e)
		{
			SetTextures(e.Texture);
		}

		private bool _hadTexture = false;

		private void SetTextures(Texture2D texture)
		{
			var shaders = Shaders;

			if (shaders == null || texture == null || texture.IsDisposed)
				return;

			shaders.SetTextures(texture);

			if (!_hadTexture)
			{
				_hadTexture = true;
				_renderSampler.MaxMipLevel = texture.LevelCount;
			}
		}

		private void BuildActionBlock(int threads)
		{
			var oldActionBlock = _actionBlock;
			var oldPrioBuffer = _priorityBuffer;

			var options = new ExecutionDataflowBlockOptions
			{
				CancellationToken = CancellationToken.Token,
				EnsureOrdered = false,
				MaxDegreeOfParallelism = Math.Max(threads / 3, 1),
				NameFormat = "Chunk Builder: {0}-{1}"
			};

			var priorityBuffer = new PriorityBufferBlock<ChunkUpdateData>(options, options, options);

			var actionBlock = new ActionBlock<ChunkUpdateData>(
				DoUpdate,
				new ExecutionDataflowBlockOptions
				{
					CancellationToken = CancellationToken.Token,
					EnsureOrdered = false,
					MaxDegreeOfParallelism = threads,
					NameFormat = "Chunk ActionBlock: {0}-{1}"
				});

			priorityBuffer.LinkTo(actionBlock);

			_actionBlock = actionBlock;
			_priorityBuffer = priorityBuffer;

			if (oldActionBlock != null)
			{
				oldActionBlock.Complete();
			}

			if (oldPrioBuffer != null)
			{
				oldPrioBuffer.Complete();
				oldPrioBuffer.MoveItems(priorityBuffer);
			}
		}

		private WeakReference<ChunkData>[] _renderedChunks = Array.Empty<WeakReference<ChunkData>>();

		public int RenderDistance
		{
			get => _renderDistance;
			set
			{
				var currentlyRenderedChunks = _renderedChunks;
				WeakReference<ChunkData>[] renderedArray = new WeakReference<ChunkData>[value * value * 3];

				for (int i = 0; i < renderedArray.Length; i++)
				{
					if (currentlyRenderedChunks != null && i < currentlyRenderedChunks.Length)
					{
						renderedArray[i] = currentlyRenderedChunks[i];
					}
					else
					{
						renderedArray[i] = new WeakReference<ChunkData>(null);
					}
				}

				_renderedChunks = renderedArray;
				_renderDistance = value;
			}
		}

		private long _threadsRunning = 0;
		private int _lightingUpdates = 0;
		public int ConcurrentChunkUpdates => (int)_threadsRunning;
		public int MaxConcurrentChunksUpdates => Options.MiscelaneousOptions.ChunkThreads.Value;

		public int LightingUpdates
		{
			get
			{
				return Interlocked.Exchange(ref _lightingUpdates, 0);
			}
		}

		public int EnqueuedChunkUpdates => _actionBlock.InputCount;

		/// <inheritdoc />
		public int ChunkCount => Chunks.Count;

		/// <inheritdoc />
		public int RenderedChunks { get; private set; } = 0;

		public float WaterSurfaceTransparency { get; set; } = 0.65f;

		private Thread _processingThread = null;

		/// <inheritdoc />
		public void EnsureStarted()
		{
			if (_processingThread != null)
				return;

			var task = new Thread(
				() =>
				{
					Thread.CurrentThread.Name = "Chunk Management";

					SpinWait sw = new SpinWait();

					while (!CancellationToken.IsCancellationRequested)
					{
						//Thread.Yield();

						if (World?.Camera == null)
						{
							sw.SpinOnce();

							continue;
						}

						if (CancellationToken.IsCancellationRequested)
							break;

						int lightUpdatesExecuted = 0; //BlockLightUpdate.Execute() + SkyLightCalculator.Execute();

						if (CalculateBlockLighting)
							lightUpdatesExecuted += BlockLightUpdate.Execute();

						if (CalculateSkyLighting)
							lightUpdatesExecuted += SkyLightCalculator.Execute();

						if (lightUpdatesExecuted != 0)
							Interlocked.Add(ref _lightingUpdates, lightUpdatesExecuted);

						if (lightUpdatesExecuted <= 0)
							sw.SpinOnce();
					}

					_processingThread = null;
				});

			task.Start();

			_processingThread = task;
		}

		private void DoUpdate(ChunkUpdateData data)
		{
			Interlocked.Increment(ref _threadsRunning);

			try
			{
				if (!TryGetChunk(data.Coordinates, out var chunk) || !chunk.Scheduled) return;

				if (!Monitor.TryEnter(chunk.UpdateLock, 0))
					return;

				Stopwatch timingWatch = Stopwatch.StartNew();

				try
				{
					if (chunk.CalculateLighting)
					{
						if (SkyLightCalculator != null && CalculateSkyLighting)
							SkyLightCalculator.Recalculate(chunk);

						if (BlockLightUpdate != null && CalculateBlockLighting)
							BlockLightUpdate.RecalculateChunk(chunk);

						chunk.CalculateLighting = false;
					}

					//using (var ba = new CachedBlockAccess(World))
					//{
						if (chunk.UpdateBuffer(World, true))
						{
							chunk.Scheduled = false;
							OnChunkUpdate?.Invoke(this, new ChunkUpdatedEventArgs(chunk, timingWatch.Elapsed));
						}
					//}
				}
				finally
				{
					_queued.Remove(data.Coordinates);
					//chunk.Scheduled = false;
					Monitor.Exit(chunk.UpdateLock);
				}
			}
			finally
			{
				Interlocked.Decrement(ref _threadsRunning);
			}
		}

		/// <inheritdoc />
		public void AddChunk(ChunkColumn chunk, ChunkCoordinates position, bool doUpdates = false)
		{
			if (CancellationToken.IsCancellationRequested)
			{
				Log.Warn($"Cancellation requested?!");

				return;
			}

			chunk.CalculateHeight(doUpdates);

			ChunkColumn toRemove = null;

			var column = Chunks.AddOrUpdate(
				position, chunk, (coordinates, oldColumn) =>
				{
					if (!ReferenceEquals(oldColumn, chunk))
					{
						//Log.Warn($"Replaced: {coordinates}");
						toRemove = oldColumn;
					}

					return chunk;
				});

			OnChunkAdded?.Invoke(this, new ChunkAddedEventArgs(column));

			if (toRemove != null)
			{
				toRemove?.Dispose();
			}
		}

		private void OnRemoveChunk(ChunkColumn column, bool dispose)
		{
			_queued?.Remove(new ChunkCoordinates(column.X, column.Z));
			OnChunkRemoved?.Invoke(this, new ChunkRemovedEventArgs(column));

			if (dispose)
				column.Dispose();
		}

		/// <inheritdoc />
		public void RemoveChunk(ChunkCoordinates position, bool dispose = true)
		{
			if (Chunks.TryRemove(position, out var column))
			{
				OnRemoveChunk(column, dispose);
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

			foreach (var chunk in chunks)
			{
				RemoveChunk(chunk.Key, true);
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

			cc.Scheduled = true;

			//if (!cc.Neighboring.Contains(source))
			//	cc.Neighboring.Add(source);

			Priority priority = Priority.Medium;

			if (prioritize)
			{
				priority = Priority.High;
			}
			else if ((type & ScheduleType.Border) != 0)
			{
				priority = Priority.Low;
			}

			if (!_queued.Contains(position))
			{
				_queued.Add(position);
				_priorityBuffer.Post(new ChunkUpdateData(position, type, source), priority);
			}
		}

		#region Drawing

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
			Filter = TextureFilter.PointMipLinear,
			AddressU = TextureAddressMode.Wrap,
			AddressV = TextureAddressMode.Wrap,
			//MipMapLevelOfDetailBias = -1f,
			MipMapLevelOfDetailBias = -3f,
			MaxMipLevel = Alex.MipMapLevel,
			FilterMode = TextureFilterMode.Default,
			AddressW = TextureAddressMode.Wrap,
			ComparisonFunction = CompareFunction.Never,
			MaxAnisotropy = 16,
			BorderColor = Color.Black
		};

		private DepthStencilState DepthStencilState { get; } = new DepthStencilState()
		{
			DepthBufferEnable = true, DepthBufferFunction = CompareFunction.Less, DepthBufferWriteEnable = true
		};

		private RasterizerState _rasterizerState = new RasterizerState()
		{
			CullMode = CullMode.CullClockwiseFace, FillMode = FillMode.Solid
		};

		private BlendState TranslucentBlendState { get; } = new BlendState()
		{
			AlphaSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.SourceAlpha,
			AlphaDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.InverseSourceAlpha,
			ColorDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.InverseSourceAlpha,
			ColorSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.SourceAlpha,
			IndependentBlendEnable = false
		};

		public int Draw(IRenderArgs args, Effect forceEffect = null, params RenderStage[] stages)
		{
			using (GraphicsContext.CreateContext(
				       args.GraphicsDevice, Block.FancyGraphics ? BlendState.AlphaBlend : BlendState.Opaque,
				       DepthStencilState, _rasterizerState, _renderSampler))
			{
				return DrawStaged(args, forceEffect, stages.Length > 0 ? stages : RenderStages);
			}
		}

		private bool IsWithinView(ChunkColumn chunk, ChunkCoordinates coordinates, ICamera camera)
		{
			ChunkCoordinates center = ViewPosition.GetValueOrDefault(new ChunkCoordinates(camera.Position));

			if (coordinates.DistanceTo(center) > RenderDistance)
				return false;

			var chunkPos = new Vector3(coordinates.X << 4, chunk.WorldSettings.MinY, coordinates.Z << 4);

			return camera.BoundingFrustum.Intersects(
				new Microsoft.Xna.Framework.BoundingBox(
					chunkPos,
					chunkPos + new Vector3(
						ChunkColumn.ChunkWidth, chunk.WorldSettings.TotalHeight, ChunkColumn.ChunkDepth)));
		}

		private int DrawStaged(IRenderArgs args, Effect forceEffect = null, params RenderStage[] stages)
		{
			int drawCount = 0;
			var originalBlendState = args.GraphicsDevice.BlendState;
			var originalCullMode = args.GraphicsDevice.RasterizerState;

			if (stages == null || stages.Length == 0)
				stages = RenderStages;

			WeakReference<ChunkData>[] chunks = _renderedChunks;

			if (CancellationToken.IsCancellationRequested || chunks == null)
			{
				return drawCount;
			}

			RenderingShaders shaders = Shaders;

			foreach (var stage in stages)
			{
				args.GraphicsDevice.BlendState = originalBlendState;
				args.GraphicsDevice.RasterizerState = originalCullMode;

				Effect effect = forceEffect;

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

					if (chunk.TryGetTarget(out var target))
					{
						drawCount += target.Draw(args.GraphicsDevice, stage, effect);
					}
				}
			}

			args.GraphicsDevice.BlendState = originalBlendState;

			return drawCount;
		}

		public ChunkCoordinates? ViewPosition { get; set; } = null;

		#endregion

		public void Update(IUpdateArgs args)
		{
			Shaders.Update(Alex.DeltaTime, args.Camera);
		}

		/// <inheritdoc />
		public void OnTick()
		{
			var array = _renderedChunks;
			int index = 0;
			int max = array.Length;

			foreach (var chunk in Chunks)
			{
				var data = chunk.Value.ChunkData;

				if (data == null)
					continue;

				bool inView = IsWithinView(chunk.Value, chunk.Key, World.Camera);

				if (inView && index + 1 < max)
				{
					if (chunk.Value.IsNew && !chunk.Value.Scheduled)
					{
						ScheduleChunkUpdate(chunk.Key, ScheduleType.Full);
					}
					else
					{
						data.Rendered = true;
						array[index].SetTarget(data); // = data;
						index++;
					}
				}
				else
				{
					data.Rendered = false;
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
				_actionBlock?.Complete();
				_priorityBuffer?.TryReceiveAll(out _);

				ClearChunks();

				if (_renderedChunks != null)
					foreach (var chunk in _renderedChunks)
					{
						if (chunk.TryGetTarget(out var target))
						{
							target?.Dispose();
						}
					}

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