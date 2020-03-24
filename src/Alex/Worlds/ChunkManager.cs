using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alex.API;
using Alex.API.Data.Options;
using Alex.API.Graphics;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Blocks.Storage;
using Alex.Graphics.Models.Blocks;
using Alex.Services;
using Alex.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using StackExchange.Profiling;
using StackExchange.Profiling.Internal;
using UniversalThreadManagement;

//using OpenTK.Graphics;

namespace Alex.Worlds
{
	public class ChunkManager : IDisposable
    {
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ChunkManager));

		private GraphicsDevice Graphics { get; }
		private ResourceManager Resources { get; }
        private IWorld World { get; }

        private int _chunkUpdates = 0;
        public int ConcurrentChunkUpdates => (int) _threadsRunning;
        public int EnqueuedChunkUpdates => Enqueued.Count;//;LowPriority.Count;
	    public int ChunkCount => Chunks.Count;

	    public AlphaTestEffect AnimatedEffect { get; }
	    public AlphaTestEffect AnimatedTranslucentEffect { get; }
	    public AlphaTestEffect TransparentEffect { get; }
	    public AlphaTestEffect TranslucentEffect { get; }
		public BasicEffect OpaqueEffect { get; }

	    public long Vertices { get; private set; }
	    public int RenderedChunks { get; private set; } = 0;
	    public int IndexBufferSize { get; private set; } = 0;

	    private int FrameCount { get; set; } = 1;
        private ConcurrentDictionary<ChunkCoordinates, ChunkData> _chunkData = new ConcurrentDictionary<ChunkCoordinates, ChunkData>();

        private AlexOptions Options { get; }
        private ProfilerService ProfilerService { get; }
        public ChunkManager(IServiceProvider serviceProvider, GraphicsDevice graphics, IWorld world)
        {
	        Graphics = graphics;
	        World = world;
	        //Options = option;

	        Options = serviceProvider.GetRequiredService<IOptionsProvider>().AlexOptions;
	        ProfilerService = serviceProvider.GetRequiredService<ProfilerService>();
	        Resources = serviceProvider.GetRequiredService<ResourceManager>();
	        
	        Chunks = new ConcurrentDictionary<ChunkCoordinates, IChunkColumn>();

	        var stillAtlas = Resources.Atlas.GetStillAtlas();
	        
	        var fogStart = 0;
	        TransparentEffect = new AlphaTestEffect(Graphics)
	        {
		        Texture = stillAtlas,
		        VertexColorEnabled = true,
		        World = Matrix.Identity,
		        AlphaFunction = CompareFunction.Greater,
		        ReferenceAlpha = 32,
		        FogStart = fogStart,
		        FogEnabled = false,
		       // TextureEnabled = true
	        };
	        
	        TranslucentEffect = new AlphaTestEffect(Graphics)
	        {
		        Texture = stillAtlas,
		        VertexColorEnabled = true,
		        World = Matrix.Identity,
		        AlphaFunction = CompareFunction.Greater,
		        ReferenceAlpha = 32,
		        FogStart = fogStart,
		        FogEnabled = false,
		        Alpha = 0.5f
	        };
	        
	        AnimatedEffect = new AlphaTestEffect(Graphics)
	        {
		        Texture = Resources.Atlas.GetAtlas(0),
		        VertexColorEnabled = true,
		        World = Matrix.Identity,
		        AlphaFunction = CompareFunction.Greater,
		        ReferenceAlpha = 32,
		        FogStart = fogStart,
		        FogEnabled = false,
		       // TextureEnabled = true
	        };
	        
	        AnimatedTranslucentEffect = new AlphaTestEffect(Graphics)
	        {
		        Texture = Resources.Atlas.GetAtlas(0),
		        VertexColorEnabled = true,
		        World = Matrix.Identity,
		        AlphaFunction = CompareFunction.Greater,
		        ReferenceAlpha = 127,
		        FogStart = fogStart,
		        FogEnabled = false,
		        Alpha = 0.5f
	        };

	        OpaqueEffect = new BasicEffect(Graphics)
	        {
		        TextureEnabled = true,
		        Texture = stillAtlas,
		        FogStart = fogStart,
		        VertexColorEnabled = true,
		        LightingEnabled = true,
		        FogEnabled = false,
		    //    AlphaFunction = CompareFunction.Greater,
		    //    ReferenceAlpha = 127
		        
		        PreferPerPixelLighting = false
	        };
	        
	        //if (alex.)

	        FrameCount = Resources.Atlas.GetFrameCount();

	        ChunkManagementThread = new Thread(ChunkUpdateThread)
	        {
		        IsBackground = true,
		        Name = "Chunk Management"
	        };
	        
	        HighestPriority = new ConcurrentQueue<ChunkCoordinates>();
        }

        private ConcurrentQueue<ChunkCoordinates> HighestPriority { get; set; }
        private ThreadSafeList<ChunkCoordinates> Enqueued { get; } = new ThreadSafeList<ChunkCoordinates>();
        private ConcurrentDictionary<ChunkCoordinates, IChunkColumn> Chunks { get; }

        private ChunkData[] _renderedChunks = new ChunkData[0];

        private Thread ChunkManagementThread { get; }
        private CancellationTokenSource CancelationToken { get; set; } = new CancellationTokenSource();

        private Vector3 _cameraPosition = Vector3.Zero;
        private BoundingFrustum _cameraBoundingFrustum = new BoundingFrustum(Matrix.Identity);

        private ConcurrentDictionary<ChunkCoordinates, CancellationTokenSource> _workItems = new ConcurrentDictionary<ChunkCoordinates, CancellationTokenSource>();
        private long _threadsRunning = 0;

        private ReprioritizableTaskScheduler _priorityTaskScheduler = new ReprioritizableTaskScheduler();

        public void Start()
	    {
		    ChunkManagementThread.Start();
	    }

        private bool IsWithinView(ChunkCoordinates chunk, BoundingFrustum frustum)
	    {
		    var chunkPos = new Vector3(chunk.X * ChunkColumn.ChunkWidth, 0, chunk.Z * ChunkColumn.ChunkDepth);
		    return frustum.Intersects(new Microsoft.Xna.Framework.BoundingBox(chunkPos,
			    chunkPos + new Vector3(ChunkColumn.ChunkWidth, _cameraPosition.Y + 10,
				    ChunkColumn.ChunkDepth)));

	    }

        int _currentFrame = 0;
	    int _framerate = 12;     // Animate at 12 frames per second
	    float _timer = 0.0f;
	    
	    private static RasterizerState Copy(RasterizerState state)
	    {
		    return new RasterizerState()
		    {
			    CullMode = state.CullMode,
			    DepthBias = state.DepthBias,
			    FillMode = state.FillMode,
			    MultiSampleAntiAlias = state.MultiSampleAntiAlias,
			    Name = state.Name,
			    ScissorTestEnable = state.ScissorTestEnable,
			    SlopeScaleDepthBias = state.SlopeScaleDepthBias,
			    Tag = state.Tag,
		    };
	    }
	    
	    public bool UseWireFrames { get; set; } = false;
	    public void Draw(IRenderArgs args)
	    {
		    var device = args.GraphicsDevice;
		    var camera = args.Camera;

		    var originalSamplerState = device.SamplerStates[0];
		    
		    device.SamplerStates[0] = new SamplerState()
		    {
			    Filter = TextureFilter.Point,
			    AddressU = TextureAddressMode.Wrap,
			    AddressV = TextureAddressMode.Wrap,
			    MipMapLevelOfDetailBias = -2f,
			    MaxMipLevel = 0
		    };
		    
		    RasterizerState originalState = null;
		    
		    bool usingWireFrames = UseWireFrames;
		    if (usingWireFrames)
		    {
			    originalState = device.RasterizerState;
			    RasterizerState rasterizerState = Copy(originalState);
			    rasterizerState.FillMode = FillMode.WireFrame;
			    device.RasterizerState = rasterizerState;
		    }

		    device.DepthStencilState = DepthStencilState.Default;
		    device.BlendState = BlendState.AlphaBlend;

		    var view = camera.ViewMatrix;
		    var projection = camera.ProjectionMatrix;
		    
		    TransparentEffect.View = view;
		    TransparentEffect.Projection = projection;
		    
		    AnimatedEffect.View = view;
		    AnimatedEffect.Projection = projection;

		    OpaqueEffect.View = view;
		    OpaqueEffect.Projection = projection;

		    TranslucentEffect.View = view;
		    TranslucentEffect.Projection = projection;

		    AnimatedTranslucentEffect.View = view;
		    AnimatedTranslucentEffect.Projection = projection;

		    var tempVertices = 0;
		    int tempChunks = 0;
		    var indexBufferSize = 0;

		    ChunkData[] chunks = _renderedChunks;

		    RenderStage[] renderStages = (RenderStage[]) Enum.GetValues(typeof(RenderStage));
		    foreach (var stage in renderStages)
		    {
			    Effect effect = null;
			    switch (stage)
			    {
				    case RenderStage.OpaqueFullCube:
					    effect = OpaqueEffect;
					    break;
				    case RenderStage.Opaque:
					    effect = OpaqueEffect;
					    break;
				    case RenderStage.Transparent:
					    effect = TransparentEffect;
					    break;
				    case RenderStage.Translucent:
					    effect = TranslucentEffect;
					    break;
				    case RenderStage.Animated:
					    effect = AnimatedEffect;
					    break;
				    case RenderStage.AnimatedTranslucent:
					    effect = AnimatedEffect;
					    break;
				    default:
					    throw new ArgumentOutOfRangeException();
			    }

			    tempVertices += DrawChunks(device, chunks, effect, stage);
		    }

		    tempChunks = chunks.Count(x => x != null && x.RenderStages != null && x.RenderStages.Count > 0);

		    if (usingWireFrames)
				device.RasterizerState = originalState;

		    Vertices = tempVertices;
		    RenderedChunks = tempChunks;
		    IndexBufferSize = indexBufferSize;

		    device.SamplerStates[0] = originalSamplerState;
	    }

	    public bool FogEnabled
	    {
		    get { return TransparentEffect.FogEnabled; }
		    set
		    {
			    TransparentEffect.FogEnabled = value;
			    TranslucentEffect.FogEnabled = value;
			    AnimatedEffect.FogEnabled = value;
			    AnimatedTranslucentEffect.FogEnabled = value;
			    OpaqueEffect.FogEnabled = value;
		    }
	    }

	    public Vector3 FogColor
	    {
		    get { return TransparentEffect.FogColor; }
		    set
		    {
				TransparentEffect.FogColor = value;
			    OpaqueEffect.FogColor = value;
			    AnimatedEffect.FogColor = value;
			    TranslucentEffect.FogColor = value;
			    AnimatedTranslucentEffect.FogColor = value;
		    }
	    }

	    public float FogDistance
	    {
		    get { return TransparentEffect.FogEnd; }
		    set
		    {
			    TransparentEffect.FogEnd = value;
			    OpaqueEffect.FogEnd = value;
			    AnimatedEffect.FogEnd = value;
			    TranslucentEffect.FogEnd = value;
			    AnimatedTranslucentEffect.FogEnd = value;
		    }
	    }

	    public Vector3 AmbientLightColor
		{
		    get { return TransparentEffect.DiffuseColor; }
		    set
		    {
			    TransparentEffect.DiffuseColor = value;
			    TranslucentEffect.DiffuseColor = value;
			    
			    OpaqueEffect.AmbientLightColor = value;
			    // OpaqueEffect.DiffuseColor = value;
			    AnimatedEffect.DiffuseColor = value;
			    AnimatedTranslucentEffect.DiffuseColor = value;
		    }
	    }

	    private Texture2D _currentFrameTexture = null;
		public void Update(IUpdateArgs args)
	    {
		    _timer += (float)args.GameTime.ElapsedGameTime.TotalSeconds;
		    if (_timer >= (1.0f / _framerate ))
		    {
			    _timer -= 1.0f / _framerate ;
			    _currentFrame = (_currentFrame + 1) % FrameCount;

			    _currentFrameTexture = Resources.Atlas.GetAtlas(_currentFrame);
			    AnimatedEffect.Texture = _currentFrameTexture;
			    AnimatedTranslucentEffect.Texture = _currentFrameTexture;
			    // OpaqueEffect.Texture = frame;
			    // TransparentEffect.Texture = frame;
		    }
		    
			var camera = args.Camera;
		    _cameraBoundingFrustum = camera.BoundingFrustum;
		    _cameraPosition = camera.Position;
	    }
		
		#region Add, Remove, Get
		
        public void AddChunk(IChunkColumn chunk, ChunkCoordinates position, bool doUpdates = false)
        {
            var c = Chunks.AddOrUpdate(position, coordinates =>
            {
	           
                return chunk;
            }, (vector3, chunk1) =>
            {
	            if (!ReferenceEquals(chunk1, chunk))
	            {
		            chunk1.Dispose();
	            }

	            Log.Warn($"Replaced/Updated chunk at {position}");
                return chunk;
            });

            if (doUpdates)
			{
				ScheduleChunkUpdate(position, ScheduleType.Full);

				ScheduleChunkUpdate(new ChunkCoordinates(position.X + 1, position.Z), ScheduleType.Border);
				ScheduleChunkUpdate(new ChunkCoordinates(position.X - 1, position.Z), ScheduleType.Border);
				ScheduleChunkUpdate(new ChunkCoordinates(position.X, position.Z + 1), ScheduleType.Border);
				ScheduleChunkUpdate(new ChunkCoordinates(position.X, position.Z - 1), ScheduleType.Border);
            }

            //InitiateChunk(c, position);
        }

        public void RemoveChunk(ChunkCoordinates position, bool dispose = true)
        {
	        if (_workItems.TryGetValue(position, out var r))
	        {
		        if (!r.IsCancellationRequested) 
			        r.Cancel();
	        }

            IChunkColumn chunk;
	        if (Chunks.TryRemove(position, out chunk))
	        {
		        if (dispose)
		        {
			        chunk?.Dispose();
		        }
	        }

	        if (_chunkData.TryRemove(position, out var data))
	        {
		        data?.Dispose();
	        }

	        if (Enqueued.Remove(position))
	        {
		        Interlocked.Decrement(ref _chunkUpdates);
            }

			//SkylightCalculator.Remove(position);
			r?.Dispose();
        }
        
        public bool TryGetChunk(ChunkCoordinates coordinates, out IChunkColumn chunk)
        {
	        return Chunks.TryGetValue(coordinates, out chunk);
        }
        #endregion

        public KeyValuePair<ChunkCoordinates, IChunkColumn>[]GetAllChunks()
        {
	        return Chunks.ToArray();
        }
        
        public void RebuildAll()
        {
            foreach (var i in Chunks)
            {
                ScheduleChunkUpdate(i.Key, ScheduleType.Full | ScheduleType.Lighting);
            }
        }

        public void ClearChunks()
        {
	        var chunks = Chunks.ToArray();
	        Chunks.Clear();

	        foreach (var chunk in chunks)
	        {
		        chunk.Value.Dispose();
	        }

	        var data = _chunkData.ToArray();
	        _chunkData.Clear();
	        foreach (var entry in data)
	        {
		        entry.Value?.Dispose();
	        }
		    
	        Enqueued.Clear();
        }
        
	    public void Dispose()
	    {
		    CancelationToken.Cancel();
			
			foreach (var chunk in Chunks.ToArray())
		    {
			    Chunks.TryRemove(chunk.Key, out IChunkColumn _);
			    Enqueued.Remove(chunk.Key);
                chunk.Value.Dispose();
		    }

			foreach (var data in _chunkData.ToArray())
			{
				_chunkData.TryRemove(data.Key, out _);
				data.Value?.Dispose();
			}

			_chunkData = null;
	    }
	    
	    #region Chunk Rendering
	    
	    
	    private int DrawChunks(GraphicsDevice device, ChunkData[] chunks, Effect effect, RenderStage stage)
	    {
		    int verticeCount = 0;
		    for (var index = 0; index < chunks.Length; index++)
		    {
			    var chunk = chunks[index];
			    if (chunk == null) continue;

			    if (chunk.RenderStages.TryGetValue(stage, out var renderStage))
			    { 
				    verticeCount += renderStage.Render(device, effect);
			    }
		    }

		    return verticeCount;
	    }
	    
	    #endregion
	    
	    #region Chunk Updates

	    private void ChunkUpdateThread()
		{
			 //Environment.ProcessorCount / 2;
			Stopwatch sw = new Stopwatch();

            while (!CancelationToken.IsCancellationRequested)
            {
	            int maxThreads = Options.VideoOptions.ChunkThreads;
	            
	            var cameraChunkPos = new ChunkCoordinates(new PlayerLocation(_cameraPosition.X, _cameraPosition.Y,
		            _cameraPosition.Z));
                //SpinWait.SpinUntil(() => Interlocked.Read(ref _threadsRunning) < maxThreads);

                foreach (var data in _chunkData.ToArray().Where(x =>
	                QuickMath.Abs(cameraChunkPos.DistanceTo(x.Key)) > Options.VideoOptions.RenderDistance))
                {
	                data.Value?.Dispose();
	                _chunkData.TryRemove(data.Key, out _);
                }
                
                var renderedChunks = Chunks.ToArray().Select(x => (KeyValuePair: x, distance: Math.Abs(x.Key.DistanceTo(cameraChunkPos)))).Where(x =>
                {

	                if (x.distance > Options.VideoOptions.RenderDistance)
		                return false;
			    
	                var chunkPos = new Vector3(x.KeyValuePair.Key.X * ChunkColumn.ChunkWidth, 0, x.KeyValuePair.Key.Z * ChunkColumn.ChunkDepth);
	                return _cameraBoundingFrustum.Intersects(new Microsoft.Xna.Framework.BoundingBox(chunkPos,
		                chunkPos + new Vector3(ChunkColumn.ChunkWidth, 256/*16 * ((x.Value.GetHeighest() >> 4) + 1)*/,
			                ChunkColumn.ChunkDepth)));
                }).OrderByDescending(x => x.distance).Select(x => x.KeyValuePair).ToArray();
			
                List<ChunkData> orderedList = new List<ChunkData>();
                foreach (var c in renderedChunks)
                {
	                if (_chunkData.TryGetValue(c.Key, out var data))
	                {
		                orderedList.Add(data);
		              //  if (_renderedChunks.TryAdd(data))
		                {
			                
		                }
	                }
	                else
	                {
		                if (c.Value.Scheduled == ScheduleType.Unscheduled)
		                {
			                ScheduleChunkUpdate(c.Key, ScheduleType.Full);
		                }
	                }
                }

                _renderedChunks = orderedList.ToArray();

                if (Interlocked.Read(ref _threadsRunning) >= maxThreads) continue;
                

                bool nonInView = false;
                try
                {
	                if (HighestPriority.TryDequeue(out var coords))
	                {
                        if (Math.Abs(cameraChunkPos.DistanceTo(coords)) > Options.VideoOptions.RenderDistance)
                        {
                            if (!Enqueued.Contains(coords))
                                Enqueued.Add(coords);
                        }
                        else
                        {
	                        Enqueued.Remove(coords);
	                        Schedule(coords, WorkItemPriority.Highest);
                        }

                        //Enqueued.Remove(coords);
	                }
	                else if (Enqueued.Count > 0)
	                {
		                //var cc = new ChunkCoordinates(CameraPosition);

		                var where = Enqueued.Where(x => IsWithinView(x, _cameraBoundingFrustum)).ToArray();
		                if (where.Length > 0)
		                {
			                coords = where.MinBy(x => Math.Abs(x.DistanceTo(new ChunkCoordinates(_cameraPosition))));
		                }
		                else
		                {
			                coords = Enqueued.MinBy(x => Math.Abs(x.DistanceTo(new ChunkCoordinates(_cameraPosition))));
		                }

                        if (Math.Abs(cameraChunkPos.DistanceTo(coords)) <= Options.VideoOptions.RenderDistance)
                        {
                            if (!_workItems.ContainsKey(coords))
                            {
                                Schedule(coords, WorkItemPriority.AboveNormal);
                            }
                        }
                    }
                }
				catch (OperationCanceledException)
				{
					break;
				}
			}

			if (!CancelationToken.IsCancellationRequested)
				Log.Warn($"Chunk update loop has unexpectedly ended!");

			//TaskScheduler.Dispose();
		}

	    private void Schedule(ChunkCoordinates coords, WorkItemPriority priority)
	    {
		    CancellationTokenSource taskCancelationToken =
			    CancellationTokenSource.CreateLinkedTokenSource(CancelationToken.Token);

		    Interlocked.Increment(ref _threadsRunning);

		    //_tasksQueue.
		    if (_workItems.TryAdd(coords, taskCancelationToken))
		    {
			    Enqueued.Remove(coords);
		    }

		    var task = Task.Factory.StartNew(() =>
		    {
			    if (Chunks.TryGetValue(coords, out var val))
			    {
				    UpdateChunk(coords, val);
			    }

			    Interlocked.Decrement(ref _threadsRunning);
			    _workItems.TryRemove(coords, out _);

		    }, taskCancelationToken.Token, TaskCreationOptions.None, _priorityTaskScheduler);

		    if (priority == WorkItemPriority.Highest)
		    {
			    _priorityTaskScheduler.Prioritize(task);
		    }
	    }

	    public void ScheduleChunkUpdate(ChunkCoordinates position, ScheduleType type, bool prioritize = false)
	    {

		    if (Chunks.TryGetValue(position, out IChunkColumn chunk))
		    {
			    var currentSchedule = chunk.Scheduled;
			    if (prioritize)
			    {
				    chunk.Scheduled = type;

				    if (!Enqueued.Contains(position) && Enqueued.TryAdd(position))
				    {
					    HighestPriority.Enqueue(position);
				    }

				    return;
			    }


			    if (currentSchedule != ScheduleType.Unscheduled)
			    {
				    return;
			    }

			    if (!_workItems.ContainsKey(position) &&
			        !Enqueued.Contains(position) && Enqueued.TryAdd(position))
			    {
				    chunk.Scheduled = type;

				    Interlocked.Increment(ref _chunkUpdates);
			    }
		    }
	    }
	    
        internal bool UpdateChunk(ChunkCoordinates coordinates, IChunkColumn c)
        {
	        var chunk = c as ChunkColumn;
            if (!Monitor.TryEnter(chunk.UpdateLock))
            {
                Interlocked.Decrement(ref _chunkUpdates);
                return false; //Another thread is already updating this chunk, return.
            }

            var scheduleType = chunk.Scheduled;
            var profiler = MiniProfiler.StartNew("chunk.update");
            ChunkData data = null;
            bool force = !_chunkData.TryGetValue(coordinates, out data);

            try
            {
                //chunk.UpdateChunk(Graphics, World);

                var currentChunkY = Math.Min(((int)Math.Round(_cameraPosition.Y)) >> 4, (chunk.GetHeighest() >> 4) - 2);
                if (currentChunkY < 0) currentChunkY = 0;

                List<ChunkMesh> meshes = new List<ChunkMesh>();
                using (profiler.Step("chunk.sections"))
                {
	                for (var i = 0; i < chunk.Sections.Length - 1; i++)
                    {
                        if (i < 0) break;
                        var section = chunk.Sections[i] as ChunkSection;
                        if (section == null || section.IsEmpty())
                        {
                            continue;
                        }

                        if (i != currentChunkY && i != 0)
                        {
                            using (profiler.Step("chunk.neighboring"))
                            {
                                if (i > 0 && i < chunk.Sections.Length - 1)
                                {
                                    var neighbors = chunk.CheckNeighbors(section, i, World).ToArray();

                                    if (!section.HasAirPockets && neighbors.Length == 6) //All surrounded by solid.
                                    {
                                        // Log.Info($"Found section with solid neigbors, skipping.");
                                        continue;
                                    }

                                    if (i < currentChunkY && neighbors.Length >= 6) continue;
                                }
                                else if (i < currentChunkY) continue;
                            }
                        }

                        //if (i == 0) force = true;

                        if (force || section.ScheduledUpdates.Cast<bool>().Contains(true) || section.IsDirty)
                        {
	                        using (profiler.Step("chunk.meshing"))
                            {
                                var sectionMesh = GenerateSectionMesh(World, scheduleType,
                                    new Vector3(chunk.X << 4, 0, chunk.Z << 4), ref section, i);
                                
                                meshes.Add(sectionMesh);
                            }
                        }
                    }
                }

                Dictionary<RenderStage, List<int>> newStageIndexes = new Dictionary<RenderStage, List<int>>(); 
	                
                List<VertexPositionNormalTextureColor> vertices = new List<VertexPositionNormalTextureColor>();

                foreach (var mesh in meshes)
                {
                    var startVerticeIndex = vertices.Count;
                    vertices.AddRange(mesh.Vertices);

                    foreach (var stage in mesh.Indexes)
                    {
	                    if (!newStageIndexes.ContainsKey(stage.Key))
	                    {
		                    newStageIndexes.Add(stage.Key, new List<int>());
	                    }
	                    
	                    newStageIndexes[stage.Key].AddRange(stage.Value.Select(a => startVerticeIndex + a));
                    }
                }

                if (vertices.Count > 0)
                {
	                ProfilerService.ReportCount("chunk.vertexCount", vertices.Count);
	                
                    using (profiler.Step("chunk.buffer"))
                    {
                        var vertexArray = vertices.ToArray();
                        Dictionary<RenderStage, ChunkRenderStage> oldStages = null;
                        if (data == null)
                        {
                            data = new ChunkData()
                            {
                                Buffer = GpuResourceManager.GetBuffer(this, Graphics,
                                    VertexPositionNormalTextureColor.VertexDeclaration, vertexArray.Length,
                                    BufferUsage.WriteOnly),
                                RenderStages = new Dictionary<RenderStage, ChunkRenderStage>()
                            };
                        }
                        else
                        {
	                        oldStages = data.RenderStages;
                        }
                        
                        var newStages = new Dictionary<RenderStage, ChunkRenderStage>();
                        
                        PooledVertexBuffer oldBuffer = data.Buffer;

                        PooledVertexBuffer newVertexBuffer = null;

                        using(profiler.Step("chunk.buffer.check"))
                        if (vertexArray.Length >= data.Buffer.VertexCount)
                        {
	                        PooledVertexBuffer newBuffer = GpuResourceManager.GetBuffer(this, Graphics,
                                VertexPositionNormalTextureColor.VertexDeclaration, vertexArray.Length,
                                BufferUsage.WriteOnly);

                            newBuffer.SetData(vertexArray);
                            newVertexBuffer = newBuffer;

                            ProfilerService.ReportCount("chunk.bufferSize", data.Buffer.MemoryUsage);
                            
                            ProfilerService.TriggerCounter("chunk.bufferResize");
                        }
                        else
                        {
                            data.Buffer.SetData(vertexArray);
                            ProfilerService.ReportCount("chunk.bufferSize", data.Buffer.MemoryUsage);
                        }
                       
                       foreach (var stage in newStageIndexes)
                       {
	                       ChunkRenderStage renderStage;
	                       PooledIndexBuffer newIndexBuffer;
	                       if (oldStages == null || !oldStages.TryGetValue(stage.Key, out renderStage))
	                       {
		                       renderStage = new ChunkRenderStage(data);
	                       }
	                       
	                       if (renderStage.IndexBuffer == null || stage.Value.Count > renderStage.IndexBuffer.IndexCount)
	                       {
		                       newIndexBuffer = GpuResourceManager.GetIndexBuffer(this, Graphics,
			                       IndexElementSize.ThirtyTwoBits, stage.Value.Count, BufferUsage.WriteOnly);
	                       }
	                       else
	                       {
		                       newIndexBuffer = renderStage.IndexBuffer;
	                       }
	                       
	                       newIndexBuffer.SetData(stage.Value.ToArray());

	                       renderStage.IndexBuffer = newIndexBuffer;
	                       
	                       newStages.Add(stage.Key, renderStage);
                       }

                       data.RenderStages = newStages;
                       using (profiler.Step("chunk.buffer.dispose"))
                        {
	                        if (newVertexBuffer != null)
	                        {
		                        data.Buffer = newVertexBuffer;
		                        oldBuffer?.MarkForDisposal();
	                        }

	                        RenderStage[] renderStages = (RenderStage[]) Enum.GetValues(typeof(RenderStage));

	                        foreach (var stage in renderStages)
	                        {
		                        if (newStages.TryGetValue(stage, out var renderStage))
		                        {
			                        if (oldStages != null && oldStages.TryGetValue(stage, out var oldStage))
			                        {
				                        if (oldStage != renderStage)
					                        oldStage.Dispose();
			                        }
		                        }
	                        }
                        }
                    }
                }
                else
                {
	                if (data != null)
	                {
		                data.Dispose();
		                data = null;
	                }
                }

                chunk.IsDirty = chunk.HasDirtySubChunks; //false;
                chunk.Scheduled = ScheduleType.Unscheduled;

                if (data != null)
                {
	                data.Coordinates = coordinates;
                }

                _chunkData.AddOrUpdate(coordinates, data, (chunkCoordinates, chunkData) => data);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception while updating chunk: {ex.ToString()}");
            }
            finally
            {
                //Enqueued.Remove(new ChunkCoordinates(chunk.X, chunk.Z));
                Interlocked.Decrement(ref _chunkUpdates);
                Monitor.Exit(chunk.UpdateLock);

                profiler.Stop();
                
              //   Log.Info(MiniProfiler.Current.RenderPlainText());
            }

            return false;
        }

        private bool HasScheduledNeighbors(IWorld world, BlockCoordinates coordinates)
        {
            var x = coordinates.X;
            var y = coordinates.Y;
            var z = coordinates.Z;

            for (int xOffset = -1; xOffset < 1; xOffset++)
            {
                if (xOffset == 0) continue;

                if (world.IsScheduled(x + xOffset, y, z) || world.IsTransparent(x + xOffset, y, z))
                    return true;

                if (world.IsScheduled(x, y, z + xOffset) || world.IsTransparent(x, y, z + xOffset))
                    return true;

                if (world.IsScheduled(x, y + xOffset, z) || world.IsTransparent(x, y + xOffset, z))
                    return true;
            }

            return false;
        }

        private ChunkMesh GenerateSectionMesh(IWorld world, ScheduleType scheduled, Vector3 chunkPosition,
	        ref ChunkSection section, int yIndex)
        {
	        var force = section.New || section.MeshCache == null || section.MeshPositions == null;

	        var cached = section.MeshCache;
	        var positionCache = section.MeshPositions;

	        Dictionary<BlockCoordinates, IList<ChunkMesh.EntryPosition>> positions = new Dictionary<BlockCoordinates, IList<ChunkMesh.EntryPosition>>();
			
	        List<VertexPositionNormalTextureColor> solidVertices = new List<VertexPositionNormalTextureColor>();

	        Dictionary<RenderStage, List<int>> stages = new Dictionary<RenderStage, List<int>>();
			RenderStage[] stageEnumValues = (RenderStage[]) Enum.GetValues(typeof(RenderStage));
			foreach (var stage in stageEnumValues)
			{
				stages.Add(stage, new List<int>());
			}
			
	        Dictionary<int, int> processedIndices = new Dictionary<int, int>();
	        for (var y = 0; y < 16; y++)
	        for (var x = 0; x < ChunkColumn.ChunkWidth; x++)
	        for (var z = 0; z < ChunkColumn.ChunkDepth; z++)
	        {
		        var blockPosition = new BlockCoordinates((int) (chunkPosition.X + x), y + (yIndex << 4),
			        (int) (chunkPosition.Z + z));
		        var blockCoords = new BlockCoordinates(x, y, z);

		        bool isScheduled = section.IsScheduled(x, y, z);
		        var neighborsScheduled = HasScheduledNeighbors(world, blockPosition);
		        var isBorderBlock = (scheduled == ScheduleType.Border && ((x == 0 || x == 15) || (z == 0 || z == 15)));


		        var isRebuild = ((force || isScheduled || neighborsScheduled || isBorderBlock));
		        // bool isLightingScheduled = section.IsLightingScheduled(x, y, z);

		        bool gotCachedPositions = false;

		        IList<ChunkMesh.EntryPosition> positionCached = default;
		        if (positionCache != null)
		        {
			        gotCachedPositions = positionCache.TryGetValue(blockCoords, out positionCached);
		        }

		        var posCache = new List<ChunkMesh.EntryPosition>();

		        int renderedBlocks = 0;

		        var blockStates = section.GetAll(x, y, z);
		        foreach (var currentBlockState in blockStates)
		        {
			        var blockState = currentBlockState.state;

			        if (((blockState == null || !blockState.Block.Renderable) && !force && !isBorderBlock))
			        {
				        continue;
			        }

			        var shouldRebuildVertices = isRebuild;

			        var model = blockState.Model;

			        if (blockState != null && shouldRebuildVertices && blockState.Block.RequiresUpdate)
			        {
				        blockState = blockState.Block.BlockPlaced(world, blockState, blockPosition);
				        section.Set(currentBlockState.storage, x, y, z, blockState);

				        model = blockState.Model;
			        }

			        if (blockState is BlockState state && state.IsMultiPart && shouldRebuildVertices)
			        {
				        model = new CachedResourcePackModel(Resources,
					        MultiPartModels.GetBlockStateModels(world, blockPosition, state,
						        state.MultiPartHelper));
				        // blockState.Block.Update(world, blockPosition);
			        }

			        ChunkMesh.EntryPosition cachedBlock = null;
			        if (gotCachedPositions && !shouldRebuildVertices)
			        {
				        cachedBlock = positionCached.FirstOrDefault(ps => ps.Storage == currentBlockState.storage);
			        }

			        ChunkMesh.EntryPosition entryPosition;
			        if (!shouldRebuildVertices && cachedBlock != null)
			        {
				        int[] indices = cached.Indexes[cachedBlock.Stage];

					    var indiceIndex = stages[cachedBlock.Stage].Count;


				        for (int index = cachedBlock.Index;
					        index < cachedBlock.Index + cachedBlock.Length;
					        index++)
				        {
					        var vertexIndex = indices[index];
					        if (!processedIndices.TryGetValue(vertexIndex, out var newIndex))
					        {
						        var vertice = cached.Vertices[vertexIndex];
						        solidVertices.Add(vertice);

						        newIndex = solidVertices.Count - 1;
						        
						        processedIndices.Add(vertexIndex, newIndex);
					        }

					        stages[cachedBlock.Stage].Add(newIndex);
				        }

				        entryPosition = new ChunkMesh.EntryPosition(cachedBlock.Stage, indiceIndex,
					        cachedBlock.Length, currentBlockState.storage);

				        posCache.Add(entryPosition);

				        renderedBlocks++;
			        }
			        else if (shouldRebuildVertices || cachedBlock == null)
			        {
				        var data = model.GetVertices(world, blockPosition, blockState.Block);

				        if (data.vertices == null || data.indexes == null || data.vertices.Length == 0 ||
				            data.indexes.Length == 0)
				        {
					        //section.SetRendered(x, y, z, false);
					        continue;
				        }

				        RenderStage targetState = RenderStage.OpaqueFullCube;
				        if (blockState.Block.Animated)
				        {
					        if (blockState.Block.BlockMaterial.IsOpaque())
					        {
						        targetState = RenderStage.Animated;
					        }
					        else
					        {
						        targetState = RenderStage.AnimatedTranslucent;
					        }
				        }
				        else if (blockState.Block.Transparent)
				        {
					        if (blockState.Block.BlockMaterial.IsOpaque())
					        {
						        targetState = RenderStage.Transparent;
					        }
					        else
					        {
						        targetState = RenderStage.Translucent;
					        }
				        }
				        else if (!blockState.Block.IsFullCube)
				        {
					        targetState = RenderStage.Opaque;
				        }

				        if (data.vertices.Length > 0 && data.indexes.Length > 0)
				        {
					        //if (currentBlockState.storage == 0)
					        //	section.SetRendered(x, y, z, true);
					        renderedBlocks++;

					        int startVerticeIndex = solidVertices.Count;
					        foreach (var vert in data.vertices)
					        {
						        solidVertices.Add(vert);
					        }

					        int startIndex = stages[targetState].Count;
					        
					        for (int i = 0; i < data.indexes.Length; i++)
					        {
						        var originalIndex = data.indexes[i];

						        var verticeIndex = startVerticeIndex + originalIndex;

						        stages[targetState].Add(verticeIndex);
					        }

					        entryPosition = new ChunkMesh.EntryPosition(targetState, startIndex,
						        data.indexes.Length, currentBlockState.storage);

					        posCache.Add(entryPosition);
				        }
				        
				        if (isScheduled)
					        section.SetScheduled(x, y, z, false);
			        }
		        }

		        if (renderedBlocks > 0)
		        {
			        if (!positions.TryAdd(blockCoords, posCache))
			        {
				        Log.Warn($"Could not to indice indexer");
			        }
		        }

		        section.SetRendered(x, y, z, renderedBlocks > 0);

		        //if (isLightingScheduled)
		        //    section.SetLightingScheduled(x, y, z, false);
	        }

	        section.New = false;

	        var oldMesh = section.MeshCache;

	        var mesh = new ChunkMesh(solidVertices.ToArray());

	        foreach (var stage in stages)
	        {
		        if (stage.Value.Count > 0)
		        {
			        mesh.Indexes.Add(stage.Key, stage.Value.ToArray());
		        }
	        }
	        
	        section.MeshCache = mesh;
	        section.MeshPositions = positions;
	        
	        if (oldMesh != null)
		        oldMesh.Dispose();
	        
	        return mesh;
        }
        
        #endregion
    }
}