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
using Alex.Worlds.Lighting;
using Collections.Pooled;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Utils;
using NLog;
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;
using ChunkCoordinates = Alex.API.Utils.ChunkCoordinates;
using MathF = System.MathF;
using PlayerLocation = Alex.API.Utils.PlayerLocation;

//using OpenTK.Graphics;

namespace Alex.Worlds
{
	public class ChunkManager : IDisposable
    {
	    private readonly static RenderStage[] RenderStages = ((RenderStage[]) Enum.GetValues(typeof(RenderStage)));

	    private readonly static RenderStage[] DepthRenderStages = new[]
	    {
		    RenderStage.OpaqueFullCube,
		    RenderStage.Opaque
	    };
	    

		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ChunkManager));

		private GraphicsDevice Graphics { get; }
		private ResourceManager Resources { get; }
        private World World { get; }

        private int _chunkUpdates = 0;
        public int ConcurrentChunkUpdates => (int) _threadsRunning;
        public int EnqueuedChunkUpdates => Enqueued.Count;//;LowPriority.Count;
	    public int ChunkCount => Chunks.Count;

	    public RenderingShaders DefaultShaders { get; set; }

	    private BasicEffect DepthEffect { get; }

	    public long Vertices { get; private set; }
	    public int RenderedChunks { get; private set; } = 0;
	    public int IndexBufferSize { get; private set; } = 0;

	    private int FrameCount { get; set; } = 1;
        private ConcurrentDictionary<ChunkCoordinates, ChunkData> _chunkData = new ConcurrentDictionary<ChunkCoordinates, ChunkData>();

        private AlexOptions Options { get; }
        private RenderTarget2D _depthMap;
        public RenderTarget2D DepthMap => _depthMap;
        private DedicatedThreadPool _threadPool;
       // private PrioritizedActionQueue ActionQueue { get; set; }
       private Utils.Queue.ConcurrentPriorityQueue<ChunkCoordinates, double> PriorityQueue { get; } =
	       new Utils.Queue.ConcurrentPriorityQueue<ChunkCoordinates, double>();
       
		private BlockLightCalculations BlockLightCalculations { get; }
		private long _highPriorityUpdates = 0;
        public ChunkManager(IServiceProvider serviceProvider, GraphicsDevice graphics, World world)
        {
	        _depthMap = new RenderTarget2D(graphics, 512, 512, false, SurfaceFormat.Color, DepthFormat.None);
	        
	        Graphics = graphics;
	        World = world;
	        //Options = option;

	        Options = serviceProvider.GetRequiredService<IOptionsProvider>().AlexOptions;
	        Resources = serviceProvider.GetRequiredService<ResourceManager>();
	        _threadPool = serviceProvider.GetService<Alex>().ThreadPool;
	        
	        Chunks = new ConcurrentDictionary<ChunkCoordinates, ChunkColumn>();

	        var stillAtlas = Resources.Atlas.GetStillAtlas();
	        
	        var fogStart = 0;
	        
	        DepthEffect = new BasicEffect(graphics)
	        {
		        TextureEnabled = false,
		        VertexColorEnabled = false,
		        FogEnabled = false
	        };
	        
	        DefaultShaders = new RenderingShaders(Graphics);
	        DefaultShaders.SetTextures(stillAtlas);
	        DefaultShaders.SetAnimatedTextures(Resources.Atlas.GetAtlas(0));
	      //  DefaultShaders.LightSource1Strength = 15;
	        
	        //if (alex.)

	        FrameCount = Resources.Atlas.GetFrameCount();

	        ChunkManagementThread = new Thread(ChunkUpdateThread)
	        {
		        IsBackground = true,
		        Name = "Chunk Management"
	        };
	        
	        HighestPriority = new ConcurrentQueue<ChunkCoordinates>();
	        BlockLightCalculations = new BlockLightCalculations((World) world);
	       // ActionQueue = new PrioritizedActionQueue(_threadPool, Options.VideoOptions.ChunkThreads);

	       Options.VideoOptions.ClientSideLighting.Bind(ClientSideLightingChanged);
	       
	     
	       //_blockAccessPool = new ObjectPool<ChunkBuilderBlockAccess>(36, () => new ChunkBuilderBlockAccess(World));
        }

        private void ClientSideLightingChanged(bool oldvalue, bool newvalue)
        {
	        RebuildAll();
        }

        private ConcurrentQueue<ChunkCoordinates> HighestPriority { get; set; }
        private ThreadSafeList<ChunkCoordinates> Enqueued { get; } = new ThreadSafeList<ChunkCoordinates>();
        private ConcurrentDictionary<ChunkCoordinates, ChunkColumn> Chunks { get; }

        private ChunkData[] _renderedChunks = new ChunkData[0];

        private Thread ChunkManagementThread { get; }
        private CancellationTokenSource CancelationToken { get; set; } = new CancellationTokenSource();

        private Vector3 _cameraPosition = Vector3.Zero;
        private BoundingFrustum _cameraBoundingFrustum = new BoundingFrustum(Matrix.Identity);

        private ConcurrentDictionary<ChunkCoordinates, CancellationTokenSource> _workItems = new ConcurrentDictionary<ChunkCoordinates, CancellationTokenSource>();
        private long _threadsRunning = 0;

        
        public bool FogEnabled
        {
	        get { return DefaultShaders.FogEnabled; }
	        set
	        {
		        DefaultShaders.FogEnabled = value;
	        }
        }

        public Vector3 FogColor
        {
	        get { return DefaultShaders.FogColor; }
	        set
	        {
		        DefaultShaders.FogColor = value;
		      //  LightShaders.FogColor = value;
	        }
        }

        public float FogDistance
        {
	        get { return DefaultShaders.FogDistance; }
	        set
	        {
		        DefaultShaders.FogDistance = value;
		      //  LightShaders.FogDistance = value;
	        }
        }

        public Vector3 AmbientLightColor
        {
	        get { return DefaultShaders.AmbientLightColor; }
	        set
	        {
		        DefaultShaders.AmbientLightColor = value;
	        }
        }

        public float BrightnessModifier
        {
	        get
	        {
		        return DefaultShaders.BrightnessModifier;
	        }
	        set
	        {
		        DefaultShaders.BrightnessModifier = value;
	        }
        }

        // private ReprioritizableTaskScheduler _priorityTaskScheduler = new ReprioritizableTaskScheduler();

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

	    public bool UseWireFrames { get; set; } = false;
	    
	    private static readonly SamplerState RenderSampler = new SamplerState()
	    {
		    Filter = TextureFilter.Point,
		    AddressU = TextureAddressMode.Wrap,
		    AddressV = TextureAddressMode.Wrap,
		    MipMapLevelOfDetailBias = -2f,
		    MaxMipLevel = 0,
		    FilterMode = TextureFilterMode.Default
	    };
	    
	    private static readonly BlendState LightMapBS = new BlendState()
	    {
		    ColorSourceBlend = Blend.One,
		    ColorDestinationBlend = Blend.One,
		    ColorBlendFunction = BlendFunction.Add,
		    AlphaSourceBlend = Blend.One,
		    AlphaDestinationBlend = Blend.One,
		    AlphaBlendFunction = BlendFunction.Add
	    };
	    
	    private static DepthStencilState DepthStencilState { get; } = new DepthStencilState()
	    {
		    DepthBufferEnable = true,
		    DepthBufferFunction = CompareFunction.Less,
		    DepthBufferWriteEnable = true
	    };

	    private static RasterizerState RasterizerState = new RasterizerState()
	    {
		    //DepthBias = 0.0001f,
		    CullMode = CullMode.CullClockwiseFace,
		    FillMode = FillMode.Solid,
		    //DepthClipEnable = true,
		    //ScissorTestEnable = true
	    };

	    private static BlendState TranslucentBlendState { get; } = new BlendState()
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

	    #region  Drawing

	    public void Draw(IRenderArgs args, bool depthMapOnly)
	    {
		    var view = args.Camera.ViewMatrix;
		    var projection = args.Camera.ProjectionMatrix;

		    DefaultShaders.UpdateMatrix(view, projection);
		//	LightShaders.UpdateMatrix(view, projection);
		    
		    DepthEffect.View = view;
		    DepthEffect.Projection = projection;
		    
		    var device = args.GraphicsDevice;

		    var originalSamplerState = device.SamplerStates[0];

		    device.SamplerStates[0] = RenderSampler;
		    
		    RasterizerState originalState = device.RasterizerState;
		    args.GraphicsDevice.RasterizerState = RasterizerState;

		    bool usingWireFrames = UseWireFrames;
		    if (usingWireFrames)
		    {
			    originalState = device.RasterizerState;
			    RasterizerState rasterizerState = originalState.Copy();
			    rasterizerState.FillMode = FillMode.WireFrame;
			    device.RasterizerState = rasterizerState;
		    }

		    device.DepthStencilState = DepthStencilState;

		    if (depthMapOnly)
		    {
			    args.GraphicsDevice.SetRenderTarget(_depthMap);
			    device.BlendState = LightMapBS;
			    
			    args.GraphicsDevice.Clear(Color.Black);
			    DrawStaged(args, out _, out _, DefaultShaders.LightingEffect, RenderStages);

			    args.GraphicsDevice.SetRenderTarget(null);
		    }
		    else
		    {
			   // device.DepthStencilState = DepthStencilState.DepthRead;
			    device.BlendState = BlendState.AlphaBlend;
			    
			    DrawStaged(args, out int chunksRendered, out int verticesRendered, null, RenderStages);

			    Vertices = verticesRendered;
			    RenderedChunks = chunksRendered;
			    IndexBufferSize = 0;
		    }

		    //if (usingWireFrames)
			    device.RasterizerState = originalState;
		    
		    device.SamplerStates[0] = originalSamplerState;
	    }

	    private void DrawStaged(IRenderArgs args, out int chunksRendered, out int drawnVertices,
		    Effect forceEffect = null, params RenderStage[] stages)
	    {
		    var originalBlendState = args.GraphicsDevice.BlendState;

		    if (stages == null || stages.Length == 0)
			    stages = RenderStages;

		    var tempVertices = 0;
		    int tempChunks = 0;
		    var indexBufferSize = 0;

		    ChunkData[] chunks = _renderedChunks;

		    foreach (var stage in stages)
		    {
			    args.GraphicsDevice.BlendState = originalBlendState;
			    
			    RenderingShaders shaders = DefaultShaders;
			    
			    bool setDepth = false;
			    Effect effect = forceEffect;
			    if (forceEffect == null)
			    {
				    switch (stage)
				    {
					    case RenderStage.OpaqueFullCube:
						    effect = shaders.TransparentEffect;
						    break;
					    case RenderStage.Opaque:
						    effect = shaders.TransparentEffect;
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
					    case RenderStage.AnimatedTranslucent:
						    effect = shaders.AnimatedEffect;
						    break;
					    default:
						    throw new ArgumentOutOfRangeException();
				    }
			    }


			    tempVertices += DrawChunks(args.GraphicsDevice, chunks, effect, stage);
		    }

		    tempChunks = chunks.Count(x => x != null && x.RenderStages != null && x.RenderStages.Count > 0);

		    chunksRendered = tempChunks;
		    drawnVertices = tempVertices;

		    args.GraphicsDevice.BlendState = originalBlendState;
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
	    
	    #endregion

	    private Texture2D _currentFrameTexture = null;
		public void Update(IUpdateArgs args)
	    {
		    _timer += (float)args.GameTime.ElapsedGameTime.TotalSeconds;
		    if (_timer >= (1.0f / _framerate ))
		    {
			    _timer -= 1.0f / _framerate ;
			    _currentFrame = (_currentFrame + 1) % FrameCount;

			    _currentFrameTexture = Resources.Atlas.GetAtlas(_currentFrame);
			    DefaultShaders.SetAnimatedTextures(_currentFrameTexture);
			//    LightShaders.SetAnimatedTextures(_currentFrameTexture);
			   // AnimatedEffect.Texture = _currentFrameTexture;
			    //AnimatedTranslucentEffect.Texture = _currentFrameTexture;
			    // OpaqueEffect.Texture = frame;
			    // TransparentEffect.Texture = frame;
		    }
		    
			var camera = args.Camera;
		    _cameraBoundingFrustum = camera.BoundingFrustum;
		    _cameraPosition = camera.Position;

			    //	    DefaultShaders.LightSource1Position = _cameraPosition;
	    }
		
		#region Add, Remove, Get
		
        public void AddChunk(ChunkColumn chunk, ChunkCoordinates position, bool doUpdates = false)
        {
	        if (Options.VideoOptions.ClientSideLighting)
	        {
		        chunk.CalculateHeight();
	        }
	        
            var c = Chunks.AddOrUpdate(position, coordinates =>
            {
	           
                return chunk;
            }, (vector3, chunk1) =>
            {
	            if (!ReferenceEquals(chunk1, chunk))
	            {
		            chunk1.Dispose();
	            }

	           // Log.Warn($"Replaced/Updated chunk at {position}");
                return chunk;
            });
            
            if (doUpdates)
			{
				ScheduleChunkUpdate(position, ScheduleType.Full, true);

				ScheduleChunkUpdate(new ChunkCoordinates(position.X + 1, position.Z), ScheduleType.Border);
				ScheduleChunkUpdate(new ChunkCoordinates(position.X - 1, position.Z), ScheduleType.Border);
				ScheduleChunkUpdate(new ChunkCoordinates(position.X, position.Z + 1), ScheduleType.Border);
				ScheduleChunkUpdate(new ChunkCoordinates(position.X, position.Z - 1), ScheduleType.Border);
            }

            //InitiateChunk(c, position);
        }

        public void RemoveChunk(ChunkCoordinates position, bool dispose = true)
        {
	        BlockLightCalculations.Remove(position);
	        
	        if (_workItems.TryGetValue(position, out var r))
	        {
		        if (!r.IsCancellationRequested) 
			        r.Cancel();
	        }

            ChunkColumn chunk;
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
        
        public bool TryGetChunk(ChunkCoordinates coordinates, out ChunkColumn chunk)
        {
	        if (Chunks.TryGetValue(coordinates, out var c))
	        {
		        chunk = c;
		        return true;
	        }

	        chunk = null;
	        return false;
        }
        #endregion

        public KeyValuePair<ChunkCoordinates, ChunkColumn>[]GetAllChunks()
        {
	        return Chunks.ToArray();
        }
        
        public void RebuildAll()
        {
	        return;
	        ThreadPool.QueueUserWorkItem(o =>
	        {
		        foreach (var chunk in Chunks)
		        {
			        if (chunk.Value is ChunkColumn column)
			        {
				        column.CalculateHeight(Options.VideoOptions.ClientSideLighting);
			        }
		        }
		        
		        if (Options.VideoOptions.ClientSideLighting)
		        {
			        SkyLightCalculations.Calculate(World as World);
		        }

		        foreach (var i in Chunks)
		        {
			        ScheduleChunkUpdate(i.Key, ScheduleType.Full | ScheduleType.Lighting);
		        }

	        });
        }

        public void ClearChunks()
        {
	        BlockLightCalculations.Clear();
	        
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
			    Chunks.TryRemove(chunk.Key, out ChunkColumn _);
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

	    #region Chunk Updates

	    private bool NeedPrioritization { get; set; } = false;
	    private void ChunkUpdateThread()
		{
			 //Environment.ProcessorCount / 2;
			Stopwatch sw = new Stopwatch();

			SpinWait spinWait = new SpinWait();
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
	                if (Options.VideoOptions.ClientSideLighting)
	                {
		                if (c.Value.Scheduled == ScheduleType.Unscheduled && BlockLightCalculations.HasEnqueued(c.Key)
		                                                                  && !Enqueued.Contains(c.Key)
		                                                                  && !_workItems.ContainsKey(c.Key))
		                {
			                ScheduleChunkUpdate(c.Key, ScheduleType.Lighting);
		                }
	                }

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

                if (NeedPrioritization)
                {
	                NeedPrioritization = false;
	                var enqueued = Enqueued.ToArray();

	                //   int highPriority = 0;
	                foreach (var cc in enqueued)
	                {
		                try
		                {
			                //  if (PriorityQueue.Contains(cc))
			                {
				                if (Chunks.TryGetValue(cc, out var chunk))
				                {
					                if (chunk.Scheduled != ScheduleType.Unscheduled)
					                {
						                var prio = GetUpdatePriority(chunk, chunk.Scheduled);

						                /*if (chunk.Scheduled == ScheduleType.Lighting)
						                {
							                PriorityQueue.UpdatePriority(cc,
								                double.MaxValue - Math.Abs(cameraChunkPos.DistanceTo(cc)));
							                continue;
						                }*/
						                PriorityQueue.UpdatePriority(cc, prio);

						                // if (IsWithinView(cc, _cameraBoundingFrustum))
						                // {
						                //    highPriority++;
						                // }
					                }
				                }
			                }
		                }
		                catch (ArgumentException)
		                {
			                Log.Warn($"Tried updating non-queued chunk: {cc}");
		                }
	                }
                }

                if (Options.VideoOptions.ClientSideLighting)
                {
	                if (BlockLightCalculations.TryProcess(
		                blockCoordinates => { return Chunks.ContainsKey((ChunkCoordinates) blockCoordinates); },
		                out BlockCoordinates coordinates))
	                {
		                ChunkCoordinates cc = (ChunkCoordinates) coordinates;
		                ScheduleChunkUpdate(cc, ScheduleType.Lighting);
	                }
                }

               // _highPriorityUpdates = highPriority;
                var threadsActive = Interlocked.Read(ref _threadsRunning);
                var maxThreadsActive = maxThreads;
                
                if (threadsActive >= maxThreadsActive)
                {
	                spinWait.SpinOnce();
	                continue;
                }

                if (threadsActive < PriorityQueue.Count && threadsActive < maxThreadsActive)
                {
	                ScheduleWorker();
                }
            }

			if (!CancelationToken.IsCancellationRequested)
				Log.Warn($"Chunk update loop has unexpectedly ended!");

			//TaskScheduler.Dispose();
		}

	    private void ScheduleWorker()
	    {
		    Interlocked.Increment(ref _threadsRunning);

		    _threadPool.QueueUserWorkItem(
			    () =>
			    {
				    try
				    {
					    while (PriorityQueue.TryDequeue(out ChunkCoordinates coordinates))
					    {
						    CancellationTokenSource taskCancelationToken =
							    CancellationTokenSource.CreateLinkedTokenSource(CancelationToken.Token);

						    if (_workItems.TryAdd(coordinates, taskCancelationToken))
						    {
							    Enqueued.Remove(coordinates);
						    }

						    if (Chunks.TryGetValue(coordinates, out var val))
						    {
							    if (val.Sections.Any(x => x != null && !x.IsEmpty()))
							    {
								    UpdateChunk(coordinates, val);
							    }
						    }

						    _workItems.TryRemove(coordinates, out _);
					    } //while (TryDequeue(new ChunkCoordinates(_cameraPosition), out coords, out priority));
				    }
				    catch (Exception ex)
				    {
					    Log.Warn(ex, $"Chunk processing error: {ex.ToString()}");
				    }

				    Interlocked.Decrement(ref _threadsRunning);

			    });
	    }

	    private double GetUpdatePriority(ChunkColumn column, ScheduleType type)
	    {
		    var cameraChunk = new ChunkCoordinates(_cameraPosition);
		    var cc = new ChunkCoordinates(column.X, column.Z);
		    bool isInView = IsWithinView(cc, _cameraBoundingFrustum);
		    
		    double priorityOffset = isInView ? 1 : 256;

		    if (!column.IsNew)
		    {
			    if (type == ScheduleType.Border)
			    {
				    priorityOffset *= 4;
			    }
			    if (type == ScheduleType.Lighting)
			    {
				    priorityOffset *= 2;
			    }

			    //else if (type == ScheduleType.Border)
				//    priorityOffset = double.MaxValue / 4d;
		    }

		    if ((column.HighPriority && isInView) || cameraChunk == cc)
		    {
			    priorityOffset = 0;
		    }

		    return priorityOffset + Math.Abs(cameraChunk.DistanceTo(cc));
	    }

	    public void ScheduleChunkUpdate(ChunkCoordinates position, ScheduleType type, bool prioritize = false)
	    {
		    if (Chunks.TryGetValue(position, out ChunkColumn chunk))
		    {
			    var currentSchedule = chunk.Scheduled;

			    if (currentSchedule != ScheduleType.Unscheduled)
			    {
				    return;
			    }

			    if (!_workItems.ContainsKey(position) &&
			        !Enqueued.Contains(position) && Enqueued.TryAdd(position))
			    {
				    if (Options.VideoOptions.ClientSideLighting)
				    {
					    if ((chunk.BlockLightDirty || chunk.IsNew))
					    {
						    ScheduleLightUpdate(position);
					    }
				    }

				    chunk.Scheduled = type;

				    chunk.HighPriority = prioritize;

				    var prio = GetUpdatePriority(chunk, type);
				    PriorityQueue.Enqueue(position,prio);

				    Interlocked.Increment(ref _chunkUpdates);
			    }
		    }
	    }

	    public void ScheduleLightUpdate(ChunkCoordinates coordinates)
	    {
		    if (Chunks.TryGetValue(coordinates, out ChunkColumn chunk))
		    {
			    var chunkpos = new BlockCoordinates(chunk.X * 16, 0, chunk.Z * 16);

			    foreach (var ls in chunk.GetLightSources())
			    {
				    BlockLightCalculations.Enqueue(chunkpos + ls);
			    }

			    chunk.BlockLightDirty = false;
		    }
	    }

	    internal void FlagPrioritization()
	    {
		    NeedPrioritization = true;
	    }
	    
	    public TimeSpan MaxUpdateTime { get; set; } = TimeSpan.Zero;
	    public TimeSpan MinUpdateTIme { get; set; } = TimeSpan.MaxValue;
	    public TimeSpan ChunkUpdateTime { get; set; } = TimeSpan.Zero;
	    public long TotalChunkUpdates { get; set; } = 0;

	    private void UpdateChunk(ChunkCoordinates coordinates, ChunkColumn chunk)
	    {
		    if (!Monitor.TryEnter(chunk.UpdateLock))
		    {
			    Interlocked.Decrement(ref _chunkUpdates);

			    return; //Another thread is already updating this chunk, return.
		    }

		   // using (ChunkBuilderBlockAccess blockAccess = new ChunkBuilderBlockAccess(World))
		   var blockAccess = World;
		    {
			    var scheduleType = chunk.Scheduled;

			    ChunkData data  = null;
			    bool      force = !_chunkData.TryGetValue(coordinates, out data);

			    Stopwatch sw = Stopwatch.StartNew();

			    //  if ((chunk.Scheduled & ScheduleType.Lighting) == ScheduleType.Lighting)
			    if (Options.VideoOptions.ClientSideLighting)
			    {
				    if (chunk.SkyLightDirty || chunk.IsNew || force)
				    {
					    new SkyLightCalculations().RecalcSkyLight(chunk, new SkyLightBlockAccess(this));

					    chunk.SkyLightDirty = false;
				    }

				    //if (chunk.BlockLightDirty || chunk.IsNew)
				    {
					    BlockLightCalculations.Process(coordinates);
				    }
			    }

			    int meshed = 0;

			    try
			    {
				    //chunk.UpdateChunk(Graphics, World);

				    var currentChunkY = Math.Min(
					    ((int) Math.Round(_cameraPosition.Y)) >> 4, (chunk.GetHeighest() >> 4) - 2);

				    if (currentChunkY < 0) currentChunkY = 0;

				    List<ChunkMesh> meshes = new List<ChunkMesh>();

				    foreach (var s in chunk.Sections.Where(x => x != null && !x.IsEmpty())
					   .OrderByDescending(sec => MathF.Abs(currentChunkY - sec.GetYLocation())))
				    {
					    ChunkSection section = s;

					    var i = section.GetYLocation();

					    bool shouldRebuildMesh = section.ScheduledUpdatesCount > 0
					                             || section.ScheduledSkyUpdatesCount > 0
					                             || section.ScheduledBlockUpdatesCount > 0;

					    if (force || section.MeshCache == null || shouldRebuildMesh || (scheduleType & ScheduleType.Border) == ScheduleType.Border)
					    {
						    var oldMesh = section.MeshCache;

						    var sectionMesh = GenerateSectionMesh(
							    blockAccess, new Vector3(chunk.X << 4, 0, chunk.Z << 4), ref section, i);

						    meshes.Add(sectionMesh);

						    if (Options.MiscelaneousOptions.MeshInRam)
						    {
							    section.MeshCache = sectionMesh;
						    }

						    section.IsDirty = false;

						    meshed++;

						    oldMesh?.Dispose();
					    }
					    else
					    {
						    var mesh = section.MeshCache;
						    if (mesh != null && !mesh.Disposed)
							    meshes.Add(mesh);
					    }
				    }

				    if (meshed > 0) //We did not re-mesh ANY chunks. Not worth re-building.
				    {

					    IDictionary<RenderStage, List<int>> newStageIndexes = new Dictionary<RenderStage, List<int>>();

					    IList<BlockShaderVertex> vertices = new List<BlockShaderVertex>();
					    
					    try
					    {
						    foreach (var mesh in meshes)
							    {
								    if (mesh.Disposed)
									    continue;
								    
								    var startVerticeIndex = vertices.Count;

								    foreach (var vertice in mesh.Vertices)
								    {
									    vertices.Add(vertice);
								    }

								    foreach (var stage in mesh.Indexes)
								    {
									    if (!newStageIndexes.ContainsKey(stage.Key))
									    {
										    newStageIndexes.Add(stage.Key, new List<int>());
									    }

									    newStageIndexes[stage.Key]
										   .AddRange(stage.Value.Select(a => startVerticeIndex + a));
								    }
							    }

							    if (vertices.Count > 0)
							    {
								    var                                       vertexArray = vertices.ToArray();
								    Dictionary<RenderStage, ChunkRenderStage> oldStages   = null;

								    if (data == null)
								    {
									    data = new ChunkData()
									    {
										    Buffer = GpuResourceManager.GetBuffer(
											    this, Graphics, BlockShaderVertex.VertexDeclaration,
											    vertexArray.Length, BufferUsage.WriteOnly),
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

								    if (vertexArray.Length >= data.Buffer.VertexCount)
								    {
									    PooledVertexBuffer newBuffer = GpuResourceManager.GetBuffer(
										    this, Graphics, BlockShaderVertex.VertexDeclaration, vertexArray.Length,
										    BufferUsage.WriteOnly);

									    newBuffer.SetData(vertexArray);
									    newVertexBuffer = newBuffer;
								    }
								    else
								    {
									    data.Buffer.SetData(vertexArray);
								    }

								    foreach (var stage in newStageIndexes)
								    {
									    ChunkRenderStage  renderStage;
									    PooledIndexBuffer newIndexBuffer;

									    if (oldStages == null || !oldStages.TryGetValue(stage.Key, out renderStage))
									    {
										    renderStage = new ChunkRenderStage(data);
									    }

									    if (renderStage.IndexBuffer == null
									        || stage.Value.Count > renderStage.IndexBuffer.IndexCount)
									    {
										    newIndexBuffer = GpuResourceManager.GetIndexBuffer(
											    this, Graphics, IndexElementSize.ThirtyTwoBits, stage.Value.Count,
											    BufferUsage.WriteOnly);
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

								    if (newVertexBuffer != null)
								    {
									    data.Buffer = newVertexBuffer;
									    oldBuffer?.MarkForDisposal();
								    }


								    RenderStage[] renderStages = RenderStages;

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
							    else
							    {
								    if (data != null)
								    {
									    data.Dispose();
									    data = null;
								    }
							    }
						    
					    }
					    finally
					    {
						    if (newStageIndexes is IDisposable disposable)
						    {
							    disposable.Dispose();
						    }

						    if (vertices is IDisposable disposableV)
						    {
							    disposableV.Dispose();
						    }
					    }
				    }

				    chunk.IsDirty = chunk.HasDirtySubChunks; //false;
				    chunk.Scheduled = ScheduleType.Unscheduled;
				    chunk.HighPriority = false;
				    chunk.IsNew = false;

				    if (meshed > 0)
				    {
					    if (data != null)
					    {
						    data.Coordinates = coordinates;
					    }

					    _chunkData.AddOrUpdate(coordinates, data, (chunkCoordinates, chunkData) => data);
				    }

				    return;
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

				    sw.Stop();

				    if (force) //Chunk was new.
				    {
					    ChunkUpdateTime += sw.Elapsed;
					    TotalChunkUpdates++;

					    if (sw.Elapsed > MaxUpdateTime)
					    {
						    MaxUpdateTime = sw.Elapsed;
					    }
					    else if (sw.Elapsed < MinUpdateTIme)
					    {
						    MinUpdateTIme = sw.Elapsed;
					    }
				    }

				    //   Log.Info(MiniProfiler.Current.RenderPlainText());
			    }
		    }
	    }

	    public static bool DoMultiPartCalculations { get; set; } = true;

        private ChunkMesh GenerateSectionMesh(IBlockAccess world,
	        Vector3 chunkPosition,
	        ref ChunkSection section,
	        int yIndex)
        {
	        using (PooledList<BlockShaderVertex> vertices =
		        new PooledList<BlockShaderVertex>(ClearMode.Auto))
		  //  List<BlockShaderVertex> vertices = new List<BlockShaderVertex>();

		    {
			    Dictionary<RenderStage, List<int>> stages =
				    new Dictionary<RenderStage, List<int>>(RenderStages.Length);
		        {
			        foreach (var stage in RenderStages)
			        {
				        stages.Add(stage, new List<int>());
			        }

			        for (int x = 0; x < 16; x++)
			        {
				        for (int z = 0; z < 16; z++)
				        {
					        for (int y = 0; y < 16; y++)
					        {
						        var blockPosition = new BlockCoordinates(
							        (int) (chunkPosition.X + x), y + (yIndex << 4), (int) (chunkPosition.Z + z));

						        //var blockStates = section.GetAll(x, y, z);

						        bool isScheduled           = section.IsScheduled(x, y, z);
						        bool isLightScheduled      = section.IsLightingScheduled(x, y, z);
						        bool isBlockLightScheduled = section.IsBlockLightScheduled(x, y, z);

						        foreach (var state in section.GetAll(x, y, z))
						        {
							        var blockState = state.State;

							        if (blockState == null || !blockState.Block.Renderable)
							        {
								        continue;
							        }

							        var model = blockState.Model;

							        if (blockState != null && blockState.Block.RequiresUpdate)
							        {
								        var newblockState = blockState.Block.BlockPlaced(
									        world, blockState, blockPosition);

								        if (newblockState != blockState)
								        {
									        blockState = newblockState;

									        section.Set(state.Storage, x, y, z, blockState);
									        model = blockState.Model;
								        }
							        }

							        if (DoMultiPartCalculations && blockState.IsMultiPart)
							        {
								        var newBlockState = MultiPartModels.GetBlockState(
									        world, blockPosition, blockState, blockState.MultiPartHelper);

								        if (newBlockState != blockState)
								        {
									        blockState = newBlockState;

									        section.Set(state.Storage, x, y, z, blockState);
									        model = blockState.Model;
								        }

								        // blockState.Block.Update(world, blockPosition);
							        }
							        
							    //    bool isLightSource = section.GetBlocklight(x,y,z) > 0;
							        var data = model.GetVertices(world, blockPosition, blockState.Block);

							        if (!(data.vertices == null || data.indexes == null || data.vertices.Length == 0
							              || data.indexes.Length == 0))
							        {
								        RenderStage targetState = RenderStage.OpaqueFullCube;

								        if (blockState.Block.BlockMaterial.IsLiquid())
								        {
									        targetState = RenderStage.Liquid;
								        }
								        else if (blockState.Block.Animated)
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
									        // if (currentBlockState.storage == 0)
									        // 	section.SetRendered(x, y, z, true);

									        int startVerticeIndex = vertices.Count;

									        foreach (var vert in data.vertices)
									        {
										        vertices.Add(vert);
									        }

									        // int startIndex = stages[targetState].Count;

									        for (int i = 0; i < data.indexes.Length; i++)
									        {
										        var originalIndex = data.indexes[i];

										        var verticeIndex = startVerticeIndex + originalIndex;

										        stages[targetState].Add(verticeIndex);
									        }

									        if (state.Storage == 0)
										        section.SetRendered(x, y, z, true);
								        }
							        }
							        else if (state.Storage == 0)
							        {
								        section.SetRendered(x, y, z, false);
							        }

							        if (state.Storage == 0)
							        {
								        if (isScheduled)
									        section.SetScheduled(x, y, z, false);

								        if (isLightScheduled)
									        section.SetLightingScheduled(x, y, z, false);

								        if (isBlockLightScheduled)
									        section.SetBlockLightScheduled(x, y, z, false);
							        }
						        }
					        }
				        }
			        }


			        var mesh = new ChunkMesh(vertices.ToArray());

			        foreach (var stage in stages)
			        {
				        if (stage.Value.Count > 0)
				        {
					        mesh.Indexes.Add(stage.Key, stage.Value.ToArray());
				        }
			        }

			        return mesh;
		        }
	        }

	        // section.MeshCache = mesh
        }

        #region old
        
        /*private ChunkMesh GenerateSectionMesh(IWorld world, ScheduleType scheduled, Vector3 chunkPosition,
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
		        bool isLightScheduled = section.IsLightingScheduled(x, y, z);
		        bool isBlockLightScheduled = section.IsBlockLightScheduled(x, y, z);
				
		        var neighborsScheduled = HasScheduledNeighbors(world, blockPosition);
		        var isBorderBlock = (scheduled == ScheduleType.Border && ((x == 0 || x == 15) || (z == 0 || z == 15)));

		        //var isBlockScheduled = isScheduled || isLightScheduled || isBlockLightScheduled;
		        var isRebuild = ((force || isScheduled || neighborsScheduled || isBorderBlock || isLightScheduled || isBlockLightScheduled));
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

			        if (DoMultiPartCalculations && blockState is BlockState state && state.IsMultiPart &&
			            shouldRebuildVertices && (isScheduled || neighborsScheduled))
			        {
				        blockState = MultiPartModels.GetBlockState(world, blockPosition, state, state.MultiPartHelper);
				        section.Set(currentBlockState.storage, x, y, z, blockState);

				        model = blockState.Model;
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

				        if (!(data.vertices == null || data.indexes == null || data.vertices.Length == 0 ||
				              data.indexes.Length == 0))
				        {
					        RenderStage targetState = RenderStage.OpaqueFullCube;
					        if (blockState.Block.BlockMaterial.IsLiquid())
					        {
						        targetState = RenderStage.Liquid;
					        }
					        else if (blockState.Block.Animated)
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
						        // if (currentBlockState.storage == 0)
						        // 	section.SetRendered(x, y, z, true);

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
				        }

				        if (currentBlockState.storage == 0)
				        {
					        if (isScheduled)
						        section.SetScheduled(x, y, z, false);

					        if (isLightScheduled)
						        section.SetLightingScheduled(x, y, z, false);

					        if (isBlockLightScheduled)
						        section.SetBlockLightScheduled(x, y, z, false);
				        }
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
        }*/
        
        #endregion
        
        #endregion
    }
}