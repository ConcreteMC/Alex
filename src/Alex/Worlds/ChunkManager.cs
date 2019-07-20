using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Data.Options;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Blocks.Storage;
using Alex.Graphics.Models.Blocks;
using Alex.Utils;
using Alex.Worlds.Lighting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using StackExchange.Profiling;
using UniversalThreadManagement;

//using OpenTK.Graphics;

namespace Alex.Worlds
{
    public class ChunkManager : IDisposable
    {
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ChunkManager));

		private GraphicsDevice Graphics { get; }
        private IWorld World { get; }
	    private Alex Game { get; }

        private int _chunkUpdates = 0;
        public int ConcurrentChunkUpdates => (int) _threadsRunning;
        public int EnqueuedChunkUpdates => Enqueued.Count;//;LowPriority.Count;
	    public int ChunkCount => Chunks.Count;

	    public AlphaTestEffect AnimatedEffect { get; }
	    public AlphaTestEffect TransparentEffect { get; }
		public BasicEffect OpaqueEffect { get; }

	    public long Vertices { get; private set; }
	    public int RenderedChunks { get; private set; } = 0;
	    public int IndexBufferSize { get; private set; } = 0;

	    private int FrameCount { get; set; } = 1;
        private ConcurrentDictionary<ChunkCoordinates, ChunkData> _chunkData = new ConcurrentDictionary<ChunkCoordinates, ChunkData>();

        private AlexOptions Options { get; }
        public ChunkManager(Alex alex, GraphicsDevice graphics, AlexOptions option, IWorld world)
        {
	        Game = alex;
	        Graphics = graphics;
	        World = world;
	        Options = option;

	        Chunks = new ConcurrentDictionary<ChunkCoordinates, IChunkColumn>();
	        
	        var fogStart = 0;
	        TransparentEffect = new AlphaTestEffect(Graphics)
	        {
		        Texture = alex.Resources.Atlas.GetStillAtlas(),
		        VertexColorEnabled = true,
		        World = Matrix.Identity,
		        AlphaFunction = CompareFunction.Greater,
		        ReferenceAlpha = 127,
		        FogStart = fogStart,
		        FogEnabled = false
	        };
	        
	        AnimatedEffect = new AlphaTestEffect(Graphics)
	        {
		        Texture = alex.Resources.Atlas.GetAtlas(0),
		        VertexColorEnabled = true,
		        World = Matrix.Identity,
		        AlphaFunction = CompareFunction.Greater,
		        ReferenceAlpha = 127,
		        FogStart = fogStart,
		        FogEnabled = false
	        };

	        OpaqueEffect = new BasicEffect(Graphics)
	        {
		        TextureEnabled = true,
		        Texture = alex.Resources.Atlas.GetStillAtlas(),
		        FogStart = fogStart,
		        VertexColorEnabled = true,
		        LightingEnabled = true,
		        FogEnabled = false
	        };
	        
	        //if (alex.)

	        FrameCount = alex.Resources.Atlas.GetFrameCount();

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

        private readonly ThreadSafeList<ChunkData> _renderedChunks = new ThreadSafeList<ChunkData>();

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
	    
	    public bool UseWireFrames { get; set; } = false;
	    public void Draw(IRenderArgs args)
	    {
		    var device = args.GraphicsDevice;
		    var camera = args.Camera;
		    
		    RasterizerState originalState = null;
		    bool usingWireFrames = UseWireFrames;
		    if (usingWireFrames)
		    {
			    originalState = device.RasterizerState;
			    RasterizerState rasterizerState = new RasterizerState();
			    rasterizerState.FillMode = FillMode.WireFrame;
			    device.RasterizerState = rasterizerState;
		    }

		    device.DepthStencilState = DepthStencilState.Default;
		    device.BlendState = BlendState.AlphaBlend;
		    
		    TransparentEffect.View = camera.ViewMatrix;
		    TransparentEffect.Projection = camera.ProjectionMatrix;
		    
		    AnimatedEffect.View = camera.ViewMatrix;
		    AnimatedEffect.Projection = camera.ProjectionMatrix;

		    OpaqueEffect.View = camera.ViewMatrix;
		    OpaqueEffect.Projection = camera.ProjectionMatrix;

		    var tempVertices = 0;
		    int tempChunks = 0;
		    var indexBufferSize = 0;

		    ChunkData[] chunks = _renderedChunks.ToArray();

		    tempVertices += DrawChunks(device, chunks, OpaqueEffect, false, false);

		    DrawChunks(device, chunks, AnimatedEffect, false, true);
		    
		    DrawChunks(device, chunks, TransparentEffect, true, false);

		    if (usingWireFrames)
				device.RasterizerState = originalState;

		    tempChunks = chunks.Count(x => x != null && (
			    x.SolidIndexBuffer.IndexCount > 0 || x.TransparentIndexBuffer.IndexCount > 0));
		    
		    Vertices = tempVertices;
		    RenderedChunks = tempChunks;
		    IndexBufferSize = indexBufferSize;
	    }

	    public Vector3 FogColor
	    {
		    get { return TransparentEffect.FogColor; }
		    set
		    {
				TransparentEffect.FogColor = value;
			    OpaqueEffect.FogColor = value;
			    AnimatedEffect.FogColor = value;
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
		    }
	    }

	    public Vector3 AmbientLightColor
		{
		    get { return TransparentEffect.DiffuseColor; }
		    set
		    {
			    TransparentEffect.DiffuseColor = value;
			    OpaqueEffect.AmbientLightColor = value;
			    AnimatedEffect.DiffuseColor = value;
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

			    _currentFrameTexture = Game.Resources.Atlas.GetAtlas(_currentFrame);
			    AnimatedEffect.Texture = _currentFrameTexture;
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
	    
	    
	    private int DrawChunks(GraphicsDevice device, ChunkData[] chunks, Effect effect, bool transparent, bool animated)
	    {
		    int verticeCount = 0;
		    for (var index = 0; index < chunks.Length; index++)
		    {
			    var chunk = chunks[index];
			    if (chunk == null) continue;

			    var buffer = animated ? chunk.AnimatedIndexBuffer : (transparent ? chunk.TransparentIndexBuffer : chunk.SolidIndexBuffer);
			    if (buffer.IsDisposed)
			    {
				    Log.Warn($"Tried to use a disposed buffer: {buffer.Name}");
					continue;    
			    }
			    
			    if (buffer.IndexCount == 0)
				    continue;

			    device.SetVertexBuffer(chunk.Buffer);
			    device.Indices = buffer;

			    foreach (var pass in effect.CurrentTechnique.Passes)
			    {
				    pass.Apply();
				    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, buffer.IndexCount / 3);
			    }

			    
			    verticeCount += chunk.Buffer.VertexCount;
		    }

		    return verticeCount;
	    }
	    
	    #endregion
	    
	    #region Chunk Updates
	    
	    private void InitiateChunk(IChunkColumn chunkColumn, ChunkCoordinates chunkCoordinates)
	    {
		    var chunkCoords = new BlockCoordinates(chunkCoordinates.X * 16, 0, chunkCoordinates.Z * 16);

		    for (int x = 0; x < 16; x++)
		    {
			    for (int z = 0; z < 16; z++)
			    {
				    for (int y = 255; y > 0; y--)
				    {
					    var blockState = chunkColumn.GetBlockState(x, y, z);
					    if (blockState.Block is Block b)
					    {
						    b.BlockPlaced(World, blockState, chunkCoords + new BlockCoordinates(x, y, z));
					    }
				    }
			    }
		    }
	    }
	    
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
                
                //var cameraChunkPos = new ChunkCoordinates(new PlayerLocation(camera.Position.X, camera.Position.Y,
	           //     camera.Position.Z));

                var renderedChunks = Chunks.ToArray().Where(x =>
                {
	                if (Math.Abs(x.Key.DistanceTo(cameraChunkPos)) > Options.VideoOptions.RenderDistance)
		                return false;
			    
	                var chunkPos = new Vector3(x.Key.X * ChunkColumn.ChunkWidth, 0, x.Key.Z * ChunkColumn.ChunkDepth);
	                return _cameraBoundingFrustum.Intersects(new Microsoft.Xna.Framework.BoundingBox(chunkPos,
		                chunkPos + new Vector3(ChunkColumn.ChunkWidth, 256/*16 * ((x.Value.GetHeighest() >> 4) + 1)*/,
			                ChunkColumn.ChunkDepth)));
                }).ToArray();
			
                foreach (var c in renderedChunks)
                {
	                if (_chunkData.TryGetValue(c.Key, out var data))
	                {
		                if (_renderedChunks.TryAdd(data))
		                {
			                
		                }
	                }
                }
			
                foreach (var c in _renderedChunks.ToArray())
                {
	                if (c == null || c.Coordinates == default || !renderedChunks.Any(x => x.Key.Equals(c.Coordinates)))
	                {
		                _renderedChunks.Remove(c);
	                }
                }

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

            var profiler = MiniProfiler.StartNew("Chunk update");
            ChunkData data = null;
            bool force = !_chunkData.TryGetValue(coordinates, out data);

            try
            {
                //chunk.UpdateChunk(Graphics, World);

                var currentChunkY = Math.Min(((int)Math.Round(_cameraPosition.Y)) >> 4, (chunk.GetHeighest() >> 4) - 2);
                if (currentChunkY < 0) currentChunkY = 0;

                List<ChunkMesh> meshes = new List<ChunkMesh>();
                using (var step = profiler.Step("chunk.sections"))
                {
                    for (var i = chunk.Sections.Length - 1; i >= 0; i--)
                    {
                        if (i < 0) break;
                        var section = chunk.Sections[i] as ChunkSection;
                        if (section == null || section.IsEmpty())
                        {
                            continue;
                        }

                        if (i != currentChunkY && i != 0)
                        {
                            using (var neighBorProfiler = profiler.Step("Neighbor checks"))
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

                        if (i == 0) force = true;

                        if (force || section.ScheduledUpdates.Any(x => x == true) || section.IsDirty)
                        {
                            using (var meshProfiler = profiler.Step("chunk.meshing"))
                            {
                                var sectionMesh = GenerateSectionMesh(World, chunk.Scheduled,
                                    new Vector3(chunk.X * 16f, 0, chunk.Z * 16f), ref section, i);
                                
                                meshes.Add(sectionMesh);
                            }
                        }
                    }
                }

                List<VertexPositionNormalTextureColor> vertices = new List<VertexPositionNormalTextureColor>();
                List<int> transparentIndexes = new List<int>();
                List<int> solidIndexes = new List<int>();
                List<int> animatedIndexes = new List<int>();

                foreach (var mesh in meshes)
                {
                    var startVerticeIndex = vertices.Count;
                    vertices.AddRange(mesh.Vertices);

                    solidIndexes.AddRange(mesh.SolidIndexes.Select(a => startVerticeIndex + a));

                    transparentIndexes.AddRange(mesh.TransparentIndexes.Select(a => startVerticeIndex + a));
                    
                    animatedIndexes.AddRange(mesh.AnimatedIndexes.Select(a => startVerticeIndex + a));
                }

                if (vertices.Count > 0)
                {
                    using (var bufferProfiler = profiler.Step("chunk.buffer"))
                    {
                        var vertexArray = vertices.ToArray();
                        var solidArray = solidIndexes.ToArray();
                        var transparentArray = transparentIndexes.ToArray();
                        var animatedArray = animatedIndexes.ToArray();
                        
                        if (data == null)
                        {
                            data = new ChunkData()
                            {
                                Buffer = GpuResourceManager.GetBuffer(this, Graphics,
                                    VertexPositionNormalTextureColor.VertexDeclaration, vertexArray.Length,
                                    BufferUsage.WriteOnly),
                                SolidIndexBuffer = GpuResourceManager.GetIndexBuffer(this, Graphics,
                                    IndexElementSize.ThirtyTwoBits,
                                    solidArray.Length, BufferUsage.WriteOnly),
                                TransparentIndexBuffer = GpuResourceManager.GetIndexBuffer(this, Graphics,
                                    IndexElementSize.ThirtyTwoBits,
                                    transparentArray.Length, BufferUsage.WriteOnly),
                                AnimatedIndexBuffer = GpuResourceManager.GetIndexBuffer(this, Graphics,
	                                IndexElementSize.ThirtyTwoBits,
	                                animatedArray.Length, BufferUsage.WriteOnly)
                            };
                        }

                        PooledVertexBuffer oldBuffer = data.Buffer;

                        PooledVertexBuffer newVertexBuffer = null;
                        PooledIndexBuffer newsolidIndexBuffer = null;
                        PooledIndexBuffer newTransparentIndexBuffer = null;
                        PooledIndexBuffer newAnimatedIndexBuffer = null;
                        
                        PooledIndexBuffer oldAnimatedIndexBuffer = data.AnimatedIndexBuffer;
                        PooledIndexBuffer oldSolidIndexBuffer = data.SolidIndexBuffer;
                        PooledIndexBuffer oldTransparentIndexBuffer = data.TransparentIndexBuffer;

                        if (vertexArray.Length >= data.Buffer.VertexCount)
                        {
                            // var oldBuffer = data.Buffer;
                            PooledVertexBuffer newBuffer = GpuResourceManager.GetBuffer(this, Graphics,
                                VertexPositionNormalTextureColor.VertexDeclaration, vertexArray.Length,
                                BufferUsage.WriteOnly);

                            newBuffer.SetData(vertexArray);
                            newVertexBuffer = newBuffer;
                            //  data.Buffer = newBuffer;
                            //  oldBuffer?.Dispose();
                        }
                        else
                        {
                            data.Buffer.SetData(vertexArray);
                        }

                        if (solidArray.Length > data.SolidIndexBuffer.IndexCount)
                        {
                            //  var old = data.SolidIndexBuffer;
                            var newSolidBuffer = GpuResourceManager.GetIndexBuffer(this, Graphics,
                                IndexElementSize.ThirtyTwoBits,
                                solidArray.Length,
                                BufferUsage.WriteOnly);

                            newSolidBuffer.SetData(solidArray);
                            newsolidIndexBuffer = newSolidBuffer;
                            //  data.SolidIndexBuffer = newSolidBuffer;
                            //   old?.Dispose();
                        }
                        else
                        {
                            data.SolidIndexBuffer.SetData(solidArray);
                        }

                        if (transparentArray.Length > data.TransparentIndexBuffer.IndexCount)
                        {
                            //  var old = data.TransparentIndexBuffer;
                            var newTransparentBuffer = GpuResourceManager.GetIndexBuffer(this, Graphics,
                                IndexElementSize.ThirtyTwoBits,
                                transparentArray.Length,
                                BufferUsage.WriteOnly);

                            newTransparentBuffer.SetData(transparentArray);
                            newTransparentIndexBuffer = newTransparentBuffer;
                        }
                        else
                        {
                            data.TransparentIndexBuffer.SetData(transparentArray);
                        }
                        
                        if (animatedArray.Length > data.AnimatedIndexBuffer.IndexCount)
                        {
	                        //  var old = data.TransparentIndexBuffer;
	                        var newTransparentBuffer = GpuResourceManager.GetIndexBuffer(this, Graphics,
		                        IndexElementSize.ThirtyTwoBits,
		                        animatedArray.Length,
		                        BufferUsage.WriteOnly);

	                        newTransparentBuffer.SetData(animatedArray);
	                        newAnimatedIndexBuffer = newTransparentBuffer;
                        }
                        else
                        {
	                        data.AnimatedIndexBuffer.SetData(animatedArray);
                        }

                        if (newVertexBuffer != null)
                        {
                            data.Buffer = newVertexBuffer;
                            oldBuffer?.MarkForDisposal();
                        }

                        if (newTransparentIndexBuffer != null)
                        {
                            data.TransparentIndexBuffer = newTransparentIndexBuffer;
                            oldTransparentIndexBuffer?.MarkForDisposal();
                        }
                        
                        if (newAnimatedIndexBuffer != null)
                        {
	                        data.AnimatedIndexBuffer = newAnimatedIndexBuffer;
	                        oldAnimatedIndexBuffer?.MarkForDisposal();
                        }

                        if (newsolidIndexBuffer != null)
                        {
                            data.SolidIndexBuffer = newsolidIndexBuffer;
                            oldSolidIndexBuffer?.MarkForDisposal();
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
			Dictionary<Vector3, ChunkMesh.EntryPosition> positions = new Dictionary<Vector3, ChunkMesh.EntryPosition>();
			
	        List<VertexPositionNormalTextureColor> solidVertices = new List<VertexPositionNormalTextureColor>();

	        List<int> animatedIndexes = new List<int>();
	        List<int> transparentIndexes = new List<int>();
	        List<int> solidIndexes = new List<int>();

	        var force = section.New || section.MeshCache == null || section.MeshPositions == null;

	        var cached = section.MeshCache;
	        var positionCache = section.MeshPositions;
	        
	        Dictionary<int, int> processedIndices = new Dictionary<int, int>();
	        for (var y = 0; y < 16; y++)
	        for (var x = 0; x < ChunkColumn.ChunkWidth; x++)
	        for (var z = 0; z < ChunkColumn.ChunkDepth; z++)
	        {
		        bool wasScheduled = section.IsScheduled(x, y, z);
		        bool wasLightingScheduled = section.IsLightingScheduled(x, y, z);
		        
		        var blockPosition = new Vector3(x, y + (yIndex << 4), z) + chunkPosition;

		        var isBorderBlock = (scheduled == ScheduleType.Border && (x == 0 || x == 15) || (z == 0 || z == 15));
		        
		        var neighborsScheduled = HasScheduledNeighbors(world, blockPosition);
			        var blockState = section.Get(x, y, z);

			        var model = blockState.Model;
			        if (blockState is BlockState state && state.IsMultiPart)
			        {
				        model = new CachedResourcePackModel(Game.Resources,
					        MultiPartModels.GetBlockStateModels(world, blockPosition, state, state.MultiPartHelper));
				       // blockState.Block.Update(world, blockPosition);
			        }

			        if (blockState != null && ((force && blockState.Block.RequiresUpdate) || (wasScheduled && blockState.Block.RequiresUpdate)))
			        {
				        blockState = blockState.Block.BlockPlaced(world, blockState, blockPosition);
				        section.Set(x, y, z, blockState);

				        model = blockState.Model;
			        }
			        
			        if ((blockState == null || !blockState.Block.Renderable) ||
			            (!section.New && !section.IsRendered(x, y, z) &&
			             !neighborsScheduled && !isBorderBlock))
			        {
				        continue;
			        }

			        if (force || wasScheduled || neighborsScheduled ||  isBorderBlock)
			        {
				        var data = model.GetVertices(world, blockPosition, blockState.Block);
							
				        if (data.vertices.Length == 0 ||
				            data.indexes.Length == 0)
				        {
					        section.SetRendered(x, y, z, false);
				        }

				        if (data.vertices == null || data.indexes == null || data.vertices.Length == 0 ||
				            data.indexes.Length == 0)
				        {
					        //section.SetRendered(x, y, z, false);
					        continue;
				        }

				        bool transparent = blockState.Block.Transparent;
				        bool animated = blockState.Block.Animated;

				        if (data.vertices.Length > 0 && data.indexes.Length > 0)
				        {
					        section.SetRendered(x, y, z, true);

					        int startVerticeIndex = solidVertices.Count;
					        foreach (var vert in data.vertices)
					        {
						        solidVertices.Add(vert);
					        }

					        int startIndex = animated ? animatedIndexes.Count : (transparent ? transparentIndexes.Count : solidIndexes.Count);
					        for (int i = 0; i < data.indexes.Length; i++)
					        {
						        var a = data.indexes[i];

						        if (animated)
						        {
							        animatedIndexes.Add(startVerticeIndex + a);
						        }
						        else if (transparent)
						        {
							        transparentIndexes.Add(startVerticeIndex + a);
						        }
						        else
						        {
							        solidIndexes.Add(startVerticeIndex + a);
						        }
					        }

					        positions.TryAdd(new Vector3(x, y, z),
						        new ChunkMesh.EntryPosition(transparent, animated, startIndex, data.indexes.Length));
				        }
			        }
			        else
			        {
				        if (positionCache.TryGetValue(new Vector3(x, y, z), out var pos))
				        {
					        var indices = pos.Animated ? cached.AnimatedIndexes : (pos.Transparent ? cached.TransparentIndexes : cached.SolidIndexes);

					        var indiceIndex =  pos.Animated ? animatedIndexes.Count : (pos.Transparent ? transparentIndexes.Count : solidIndexes.Count);
					        for (int index = 0; index < pos.Length; index++)
					        {
						        var indice = indices[pos.Index + index];
						        if (!processedIndices.TryGetValue(indice, out var newIndex))
						        {
							        newIndex = solidVertices.Count;
							        var vertice = cached.Vertices[indice];
							        solidVertices.Add(vertice);
							        
							        processedIndices.Add(indice, newIndex);
						        }
						        
						        if (pos.Animated)
						        {
							        animatedIndexes.Add(newIndex);
						        }
						        else if (pos.Transparent)
						        {
							        transparentIndexes.Add(newIndex);
						        }
						        else
						        {
							        solidIndexes.Add(newIndex);
						        }
					        }
					        
					        positions.TryAdd(new Vector3(x, y, z),
						        new ChunkMesh.EntryPosition(pos.Transparent, pos.Animated, indiceIndex, pos.Length));
				        }
			        }

		        if (wasScheduled)
			        section.SetScheduled(x, y, z, false);

		        if (wasLightingScheduled)
			        section.SetLightingScheduled(x, y, z, false);
	        }

	        section.New = false;

	        var mesh = new ChunkMesh(solidVertices.ToArray(), solidIndexes.ToArray(),
		        transparentIndexes.ToArray(), animatedIndexes.ToArray());

	        section.MeshCache = mesh;
	        section.MeshPositions = positions;
	        
	        return mesh;
        }
        
        #endregion
    }
}