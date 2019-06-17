using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.Storage;
using Alex.Utils;
using Alex.Worlds.Lighting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;
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
//_workItems.Count;// Enqueued.Count;
        public int EnqueuedChunkUpdates => Enqueued.Count;//;LowPriority.Count;
	    public int ChunkCount => Chunks.Count;

	    public AlphaTestEffect TransparentEffect { get; }
		public BasicEffect OpaqueEffect { get; }

	    public long Vertices { get; private set; }
	    public int RenderedChunks { get; private set; } = 0;
	    public int IndexBufferSize { get; private set; } = 0;

	    public SkylightCalculations SkylightCalculator { get; private set; }

	    private int FrameCount { get; set; } = 1;
        private ConcurrentDictionary<ChunkCoordinates, ChunkData> _chunkData = new ConcurrentDictionary<ChunkCoordinates, ChunkData>();

        public ChunkManager(Alex alex, GraphicsDevice graphics, IWorld world)
        {
	        Game = alex;
	        Graphics = graphics;
	        World = world;
	        Chunks = new ConcurrentDictionary<ChunkCoordinates, IChunkColumn>();

	        SkylightCalculator = new SkylightCalculations(world, coordinates =>
	        {
		        if (Chunks.TryGetValue(coordinates, out IChunkColumn column))
		        {
			        if (IsWithinView(coordinates, CameraBoundingFrustum))
			        {
				        var distance = new ChunkCoordinates(CameraPosition).DistanceTo(coordinates);
				        if (Math.Abs(distance) < Game.GameSettings.RenderDistance / 2d)
					        //if (column.SkyLightDirty) //Initial
				        {
					        return SkylightCalculations.CheckResult.HighPriority;
				        }
				        else
				        {
					        return SkylightCalculations.CheckResult.MediumPriority;
				        }
			        }
			        else
			        {
				        return SkylightCalculations.CheckResult.LowPriority;
			        }
		        }

		        return SkylightCalculations.CheckResult.Cancel;
	        });

	        //	var distance = (float)Math.Pow(alex.GameSettings.RenderDistance, 2) * 0.8f;
	        var fogStart = 0; //distance - (distance * 0.35f);
	        TransparentEffect = new AlphaTestEffect(Graphics)
	        {
		        Texture = alex.Resources.Atlas.GetAtlas(0),
		        VertexColorEnabled = true,
		        World = Matrix.Identity,
		        AlphaFunction = CompareFunction.Greater,
		        ReferenceAlpha = 127,
		        //FogEnd = distance,
		        FogStart = fogStart,
		        FogEnabled = false
	        };
	        //TransparentEffect.FogColor = new Vector3(0.5f, 0.5f, 0.5f);

	        //TransparentEffect.FogEnd = distance;
	        //	TransparentEffect.FogStart = distance - (distance * 0.55f);
	        //	TransparentEffect.FogEnabled = true;

	        OpaqueEffect = new BasicEffect(Graphics)
	        {
		        TextureEnabled = true,
		        Texture = alex.Resources.Atlas.GetAtlas(0),
		        //	FogEnd = distance,
		        FogStart = fogStart,
		        VertexColorEnabled = true,
		        LightingEnabled = true,
		        FogEnabled = false
	        };

	        FrameCount = alex.Resources.Atlas.GetFrameCount();

	        Updater = new Thread(ChunkUpdateThread)
		        {IsBackground = true};

	        //HighPriority = new ConcurrentQueue<ChunkCoordinates>();
	        //LowPriority = new ConcurrentQueue<ChunkCoordinates>();
	        LowPriority = new ThreadSafeList<ChunkCoordinates>();
	        HighestPriority = new ConcurrentQueue<ChunkCoordinates>();


	        SkylightThread = new Thread(SkyLightUpdater)
	        {
		        IsBackground = true
	        };
	        //TaskSchedular.MaxThreads = alex.GameSettings.ChunkThreads;
        }

        public void GetPendingLightingUpdates(out int low, out int mid, out int high)
	    {
		    low = SkylightCalculator.LowPriorityPending;
		    mid = SkylightCalculator.MidPriorityPending;
		    high = SkylightCalculator.HighPriorityPending;
	    }

	    public void Start()
	    {
		    Updater.Start();
		    if (Game.GameSettings.ClientSideLighting)
		    {
			    SkylightThread.Start();
		    }
	    }

		//private ThreadSafeList<Entity> Entities { get; private set; } 

		private ConcurrentQueue<ChunkCoordinates> HighestPriority { get; set; }
	   // private ConcurrentQueue<ChunkCoordinates> HighPriority { get; set; }
	    private ThreadSafeList<ChunkCoordinates> LowPriority { get; set; }
		private ThreadSafeList<ChunkCoordinates> Enqueued { get; } = new ThreadSafeList<ChunkCoordinates>();
		private ConcurrentDictionary<ChunkCoordinates, IChunkColumn> Chunks { get; }

	    private readonly ThreadSafeList<ChunkCoordinates> _renderedChunks = new ThreadSafeList<ChunkCoordinates>();

	    private Thread Updater { get; }
		private Thread SkylightThread { get; }

		// private readonly AutoResetEvent _updateResetEvent = new AutoResetEvent(false);
		private CancellationTokenSource CancelationToken { get; set; } = new CancellationTokenSource();

		private Vector3 CameraPosition = Vector3.Zero;
	    private BoundingFrustum CameraBoundingFrustum = new BoundingFrustum(Matrix.Identity);

        //private DynamicVertexBuffer VertexBuffer { get; set; }

        private void SkyLightUpdater()
	    {
            while (!CancelationToken.IsCancellationRequested)
		    {
			    var cameraPos = new ChunkCoordinates(CameraPosition);

			    if (SkylightCalculator.HasPending())
			    {
				    if (SkylightCalculator.TryLightNext(cameraPos, out var coords))
				    {
					    if (Chunks.TryGetValue(coords, out IChunkColumn column))
					    {
						    //column.SkyLightDirty = false;
						    ScheduleChunkUpdate(coords, ScheduleType.Lighting);
					    }
				    }
			    }
		    }
	    }

		private ConcurrentDictionary<ChunkCoordinates, CancellationTokenSource> _workItems = new ConcurrentDictionary<ChunkCoordinates, CancellationTokenSource>();
        private long _threadsRunning = 0;
      //  private SmartThreadPool TaskSchedular = new SmartThreadPool();

		private ReprioritizableTaskScheduler _priorityTaskScheduler = new ReprioritizableTaskScheduler();
        private void Schedule(ChunkCoordinates coords, WorkItemPriority priority)
        {
	        CancellationTokenSource taskCancelationToken = CancellationTokenSource.CreateLinkedTokenSource(CancelationToken.Token);
	        
	       // Interlocked.Increment(ref _threadsRunning);
	        var task = Task.Factory.StartNew(() =>
	        {
		        Interlocked.Increment(ref _threadsRunning);
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

	        if (_workItems.TryAdd(coords, taskCancelationToken))
	        {
		        Enqueued.Remove(coords);
	        }
        }
        
        private void ChunkUpdateThread()
		{
			int maxThreads = Game.GameSettings.ChunkThreads; //Environment.ProcessorCount / 2;
			Stopwatch sw = new Stopwatch();

            while (!CancelationToken.IsCancellationRequested)
            {
	            var cameraChunkPos = new ChunkCoordinates(new PlayerLocation(CameraPosition.X, CameraPosition.Y,
		            CameraPosition.Z));
                //SpinWait.SpinUntil(() => Interlocked.Read(ref _threadsRunning) < maxThreads);

                foreach (var data in _chunkData.ToArray().Where(x =>
	                QuickMath.Abs(cameraChunkPos.DistanceTo(x.Key)) > Game.GameSettings.RenderDistance))
                {
	                data.Value?.Dispose();
	                _chunkData.TryRemove(data.Key, out _);
                }

                if (Interlocked.Read(ref _threadsRunning) >= maxThreads) continue;
                

                bool nonInView = false;
                try
                {
	                if (HighestPriority.TryDequeue(out var coords))
	                {
                        if (Math.Abs(cameraChunkPos.DistanceTo(coords)) > Game.GameSettings.RenderDistance)
                        {
                            if (!Enqueued.Contains(coords))
                                Enqueued.Add(coords);
                        }
                        else
                        {

                            Schedule(coords, WorkItemPriority.Highest);
                        }

                        //Enqueued.Remove(coords);
	                }
	                else if (Enqueued.Count > 0)
	                {
		                //var cc = new ChunkCoordinates(CameraPosition);

		                var where = Enqueued.Where(x => IsWithinView(x, CameraBoundingFrustum)).ToArray();
		                if (where.Length > 0)
		                {
			                coords = where.MinBy(x => Math.Abs(x.DistanceTo(new ChunkCoordinates(CameraPosition))));
		                }
		                else
		                {
			                coords = Enqueued.MinBy(x => Math.Abs(x.DistanceTo(new ChunkCoordinates(CameraPosition))));
		                }

                        if (Math.Abs(cameraChunkPos.DistanceTo(coords)) <= Game.GameSettings.RenderDistance)
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

        private bool IsWithinView(ChunkCoordinates chunk, BoundingFrustum frustum)
	    {
		    var chunkPos = new Vector3(chunk.X * ChunkColumn.ChunkWidth, 0, chunk.Z * ChunkColumn.ChunkDepth);
		    return frustum.Intersects(new Microsoft.Xna.Framework.BoundingBox(chunkPos,
			    chunkPos + new Vector3(ChunkColumn.ChunkWidth, CameraPosition.Y + 10,
				    ChunkColumn.ChunkDepth)));

	    }

        int currentFrame = 0;
	    int framerate = 12;     // Animate at 12 frames per second
	    float timer = 0.0f;
	    
	    public void Draw(IRenderArgs args)
	    {
		    timer += (float)args.GameTime.ElapsedGameTime.TotalSeconds;
		    if (timer >= (1.0f / framerate ))
		    {
			    timer -= 1.0f / framerate ;
			    currentFrame = (currentFrame + 1) % FrameCount;

			    var frame = Game.Resources.Atlas.GetAtlas(currentFrame);
			    OpaqueEffect.Texture = frame;
			    TransparentEffect.Texture = frame;
		    }

		    var device = args.GraphicsDevice;
		    var camera = args.Camera;

		    Stopwatch sw = Stopwatch.StartNew();
			
		    TransparentEffect.View = camera.ViewMatrix;
		    TransparentEffect.Projection = camera.ProjectionMatrix;

		    OpaqueEffect.View = camera.ViewMatrix;
		    OpaqueEffect.Projection = camera.ProjectionMatrix;

		    var tempVertices = 0;
		    int tempChunks = 0;
		    int tempFailed = 0;
		    var indexBufferSize = 0;
		    
			var r = _renderedChunks.ToArray();
			//var transparentChunksRendered = _renderedTransparentChunks.ToArray();
			
		    var chunks = new KeyValuePair<ChunkCoordinates, ChunkData>[r.Length];
		    for (var index = 0; index < r.Length; index++)
		    {
			    var c = r[index];
			    if (_chunkData.TryGetValue(c, out ChunkData chunk))
			    {
				    chunks[index] = new KeyValuePair<ChunkCoordinates, ChunkData>(c, chunk);
			    }
			    else
			    {
				    chunks[index] = new KeyValuePair<ChunkCoordinates, ChunkData>(c, null);
			    }
		    }

		    device.DepthStencilState = DepthStencilState.Default;
		    device.BlendState = BlendState.AlphaBlend;

            for (var index = 0; index < chunks.Length; index++)
            {
	            var chunk = chunks[index].Value;
	            if (chunk == null) continue;

                if (chunk.SolidIndexBuffer.IndexCount == 0)
                    continue;

                device.SetVertexBuffer(chunk.Buffer);
                device.Indices = chunk.SolidIndexBuffer;

                foreach (var pass in OpaqueEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    //device.DrawPrimitives(PrimitiveType.TriangleList, 0, b.VertexCount /3);
                }

                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, chunk.SolidIndexBuffer.IndexCount / 3);
                indexBufferSize += chunk.SolidIndexBuffer.IndexCount / 3;
                tempVertices += chunk.Buffer.VertexCount;
                //chunk.DrawOpaque(device, OpaqueEffect, out int drawn, out int idxSize);
                // tempVertices += drawn;
                // indexBufferSize += idxSize;

                //  if (drawn > 0) tempChunks++;
            }

		    for (var index = 0; index < chunks.Length; index++)
		    {
			    var chunk = chunks[index].Value;
			    if (chunk == null) continue;

                if (chunk.TransparentIndexBuffer.IndexCount == 0)
                    continue;

                device.SetVertexBuffer(chunk.Buffer);
                device.Indices = chunk.TransparentIndexBuffer;

                foreach (var pass in TransparentEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    //device.DrawPrimitives(PrimitiveType.TriangleList, 0, b.VertexCount /3);
                }

                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, chunk.TransparentIndexBuffer.IndexCount / 3);
                indexBufferSize += chunk.TransparentIndexBuffer.IndexCount;
                //tempVertices += chunk.Buffer.VertexCount;
                //chunk.DrawTransparent(device, TransparentEffect, out int draw, out int idxSize);
                // tempVertices += draw;
                // indexBufferSize += idxSize;
		    }

		    tempChunks = chunks.Count(x => x.Value != null && (
			    x.Value.SolidIndexBuffer.IndexCount > 0 || x.Value.TransparentIndexBuffer.IndexCount > 0));
		    
		    Vertices = tempVertices;
		    RenderedChunks = tempChunks;
		    IndexBufferSize = indexBufferSize;

            sw.Stop();
		    if (tempFailed > 0)
		    {
			    /*	Log.Debug(
					    $"Frame time: {sw.Elapsed.TotalMilliseconds}ms\n\t\tTransparent: {transparentFramesFailed} / {transparentBuffers.Length} chunks\n\t\tOpaque: {opaqueFramesFailed} / {opaqueBuffers.Length} chunks\n\t\t" +
					    $"Full chunks: {tempChunks} / {chunks.Length}\n\t\t" +
					    $"Missed frames: {tempFailed}");*/
		    }
	    }

	    public Vector3 FogColor
	    {
		    get { return TransparentEffect.FogColor; }
		    set
		    {
				TransparentEffect.FogColor = value;
			    OpaqueEffect.FogColor = value;
		    }
	    }

	    public float FogDistance
	    {
		    get { return TransparentEffect.FogEnd; }
		    set
		    {
			    TransparentEffect.FogEnd = value;
			    OpaqueEffect.FogEnd = value;
		    }
	    }

	    public Vector3 AmbientLightColor
		{
		    get { return TransparentEffect.DiffuseColor; }
		    set
		    {
			    TransparentEffect.DiffuseColor = value;
			    OpaqueEffect.AmbientLightColor = value;
		    }
	    }

	    private Vector3 CamDir = Vector3.Zero;
		public void Update(IUpdateArgs args)
	    {
		  //  TransparentEffect.FogColor = skyRenderer.WorldFogColor.ToVector3();
		 //   OpaqueEffect.FogColor = skyRenderer.WorldFogColor.ToVector3();

		    //   OpaqueEffect.AmbientLightColor = TransparentEffect.DiffuseColor =
			//    Color.White.ToVector3() * new Vector3((skyRenderer.BrightnessModifier));
			var camera = args.Camera;
		    CameraBoundingFrustum = camera.BoundingFrustum;
		    CameraPosition = camera.Position;
		    CamDir = camera.Target;

		    var cameraChunkPos = new ChunkCoordinates(new PlayerLocation(camera.Position.X, camera.Position.Y,
			    camera.Position.Z));

            var renderedChunks = Chunks.ToArray().Where(x =>
            {
	           if (Math.Abs(x.Key.DistanceTo(cameraChunkPos)) > Game.GameSettings.RenderDistance)
		           return false;
			    
			    var chunkPos = new Vector3(x.Key.X * ChunkColumn.ChunkWidth, 0, x.Key.Z * ChunkColumn.ChunkDepth);
			    return camera.BoundingFrustum.Intersects(new Microsoft.Xna.Framework.BoundingBox(chunkPos,
				    chunkPos + new Vector3(ChunkColumn.ChunkWidth, 256/*16 * ((x.Value.GetHeighest() >> 4) + 1)*/,
					    ChunkColumn.ChunkDepth)));
		    }).ToArray();
			
			foreach (var c in renderedChunks)
			{
				if (_renderedChunks.TryAdd(c.Key))
				{
					if (c.Value.SkyLightDirty)
						SkylightCalculator.CalculateLighting(c.Value, true, true);
                }
			}
			
		    foreach (var c in _renderedChunks.ToArray())
		    {
			    if (!renderedChunks.Any(x => x.Key.Equals(c)))
			    {
				    _renderedChunks.Remove(c);
			    }
		    }
	    }

        public void AddChunk(IChunkColumn chunk, ChunkCoordinates position, bool doUpdates = false)
        {
            Chunks.AddOrUpdate(position, coordinates =>
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

	        //SkylightCalculator.CalculateLighting(chunk, true, true, false);

            if (doUpdates)
			{
				/*if (new ChunkCoordinates(CameraPosition).DistanceTo(position) < 6)
				{
					ScheduleChunkUpdate(position, ScheduleType.Full | ScheduleType.Skylight);
					/*ScheduleChunkUpdate(new ChunkCoordinates(position.X + 1, position.Z), ScheduleType.Border | ScheduleType.Skylight);
					ScheduleChunkUpdate(new ChunkCoordinates(position.X - 1, position.Z), ScheduleType.Border | ScheduleType.Skylight);
					ScheduleChunkUpdate(new ChunkCoordinates(position.X, position.Z + 1), ScheduleType.Border | ScheduleType.Skylight);
					ScheduleChunkUpdate(new ChunkCoordinates(position.X, position.Z - 1), ScheduleType.Border | ScheduleType.Skylight);
                }
				else
				{*/
			    //SkylightCalculator.CalculateLighting(chunk, true, false);
					ScheduleChunkUpdate(position, ScheduleType.Full);
				//}

				ScheduleChunkUpdate(new ChunkCoordinates(position.X + 1, position.Z), ScheduleType.Border);
				ScheduleChunkUpdate(new ChunkCoordinates(position.X - 1, position.Z), ScheduleType.Border);
				ScheduleChunkUpdate(new ChunkCoordinates(position.X, position.Z + 1), ScheduleType.Border);
				ScheduleChunkUpdate(new ChunkCoordinates(position.X, position.Z - 1), ScheduleType.Border);
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

				    //Interlocked.Increment(ref _chunkUpdates);
				//    _updateResetEvent.Set();

                    if (type.HasFlag(ScheduleType.Lighting) && Game.GameSettings.ClientSideLighting)
				    {
					    SkylightCalculator.CalculateLighting(chunk, true, true);
				    }

                    return;
			    }

                if (Game.GameSettings.ClientSideLighting && type.HasFlag(ScheduleType.Lighting) && !currentSchedule.HasFlag(ScheduleType.Lighting))
                {
	                chunk.Scheduled = type;
                    SkylightCalculator.CalculateLighting(chunk, true, true);
                }
			    else
			    {
				    if (currentSchedule != ScheduleType.Unscheduled)
				    {
					    /*if (currentSchedule != ScheduleType.Full)
					    {
						    chunk.Scheduled = type;
					    }*/

					    return;
				    }

				    if (!_workItems.ContainsKey(position) &&
				        !Enqueued.Contains(position) && Enqueued.TryAdd(position))
				    {
					    chunk.Scheduled = type;

					    Interlocked.Increment(ref _chunkUpdates);
					//    _updateResetEvent.Set();
                    }    
			    }
		    }
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

	        LowPriority.Remove(position);

	        if (Enqueued.Remove(position))
	        {
		        Interlocked.Decrement(ref _chunkUpdates);
            }

			SkylightCalculator.Remove(position);
			r?.Dispose();
        }

        public void RebuildAll()
        {
            foreach (var i in Chunks)
            {
                ScheduleChunkUpdate(i.Key, ScheduleType.Full | ScheduleType.Lighting);
            }
        }

	    public KeyValuePair<ChunkCoordinates, IChunkColumn>[]GetAllChunks()
	    {
		    return Chunks.ToArray();
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

	    public bool TryGetChunk(ChunkCoordinates coordinates, out IChunkColumn chunk)
	    {
		    return Chunks.TryGetValue(coordinates, out chunk);
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


        internal bool UpdateChunk(ChunkCoordinates coordinates, IChunkColumn c)
        {
            var chunk = c as ChunkColumn;
            if (!Monitor.TryEnter(chunk.UpdateLock))
            {
                Interlocked.Decrement(ref _chunkUpdates);
                return false; //Another thread is already updating this chunk, return.
            }

            ChunkData data = null;
            bool force = !_chunkData.TryGetValue(coordinates, out data);

            try
            {
                //chunk.UpdateChunk(Graphics, World);

                var currentChunkY = Math.Min(((int)Math.Round(CameraPosition.Y)) >> 4, (chunk.GetHeighest() >> 4) - 2);
                if (currentChunkY < 0) currentChunkY = 0;

                List<ChunkMesh> meshes = new List<ChunkMesh>();
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

                    if (i == 0) force = true;

                    if (force || section.ScheduledUpdates.Any(x => x == true) || section.IsDirty)
                    {
                        var sectionMesh = GenerateSectionMesh(World, chunk.Scheduled,
                            new Vector3(chunk.X * 16f, 0, chunk.Z * 16f), ref section, i, out _);

                        meshes.Add(sectionMesh);
                    }
                }

                List<VertexPositionNormalTextureColor> vertices = new List<VertexPositionNormalTextureColor>();
                List<int> transparentIndexes = new List<int>();
                List<int> solidIndexes = new List<int>();

                foreach (var mesh in meshes)
                {
                    var startVerticeIndex = vertices.Count;
                    vertices.AddRange(mesh.Vertices);

                    solidIndexes.AddRange(mesh.SolidIndexes.Select(a => startVerticeIndex + a));

                    transparentIndexes.AddRange(mesh.TransparentIndexes.Select(a => startVerticeIndex + a));
                }

                if (vertices.Count > 0)
                {

	                var vertexArray = vertices.ToArray();
	                var solidArray = solidIndexes.ToArray();
	                var transparentArray = transparentIndexes.ToArray();

	                if (data == null)
	                {
		                data = new ChunkData()
		                {
			                Buffer = GpuResourceManager.GetBuffer(Graphics,
				                VertexPositionNormalTextureColor.VertexDeclaration, vertexArray.Length,
				                BufferUsage.WriteOnly),
			                SolidIndexBuffer = GpuResourceManager.GetIndexBuffer(Graphics, IndexElementSize.ThirtyTwoBits,
				                solidArray.Length, BufferUsage.WriteOnly),
			                TransparentIndexBuffer = GpuResourceManager.GetIndexBuffer(Graphics, IndexElementSize.ThirtyTwoBits,
				                transparentArray.Length, BufferUsage.WriteOnly)
		                };
	                }

	                if (vertexArray.Length >= data.Buffer.VertexCount)
	                {
		                var oldBuffer = data.Buffer;
		                VertexBuffer newBuffer = GpuResourceManager.GetBuffer(Graphics,
			                VertexPositionNormalTextureColor.VertexDeclaration, vertexArray.Length,
			                BufferUsage.WriteOnly);

		                newBuffer.SetData(vertexArray);

		                data.Buffer = newBuffer;
		                oldBuffer?.Dispose();
	                }
	                else
	                {
		                data.Buffer.SetData(vertexArray);
	                }

	                if (solidArray.Length > data.SolidIndexBuffer.IndexCount)
	                {
		                var old = data.SolidIndexBuffer;
		                var newSolidBuffer = GpuResourceManager.GetIndexBuffer(Graphics, IndexElementSize.ThirtyTwoBits,
			                solidArray.Length,
			                BufferUsage.WriteOnly);

		                newSolidBuffer.SetData(solidArray);
		                data.SolidIndexBuffer = newSolidBuffer;
		                old?.Dispose();
	                }
	                else
	                {
		                data.SolidIndexBuffer.SetData(solidIndexes.ToArray());
	                }

	                if (transparentArray.Length > data.TransparentIndexBuffer.IndexCount)
	                {
		                var old = data.TransparentIndexBuffer;
		                var newSolidBuffer = GpuResourceManager.GetIndexBuffer(Graphics, IndexElementSize.ThirtyTwoBits,
			                transparentArray.Length,
			                BufferUsage.WriteOnly);

		                newSolidBuffer.SetData(transparentArray);
		                data.TransparentIndexBuffer = newSolidBuffer;
		                old?.Dispose();
	                }
	                else
	                {
		                data.TransparentIndexBuffer.SetData(transparentIndexes.ToArray());
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
            }

            return false;
        }

        private ChunkMesh GenerateSectionMesh(IWorld world, ScheduleType scheduled, Vector3 chunkPosition,
                ref ChunkSection section, int yIndex,
               // ref Dictionary<uint, (VertexPositionNormalTextureColor[] vertices, int[] indexes)> blocks,
                out IDictionary<Vector3, ChunkMesh.EntryPosition> outputPositions/*, ref long reUseCounter*/)
        {
            List<VertexPositionNormalTextureColor> solidVertices = new List<VertexPositionNormalTextureColor>();
            /*List<VertexPositionNormalTextureColor> transparentVertices =
                new List<VertexPositionNormalTextureColor>();*/
            var positions = new ConcurrentDictionary<Vector3, ChunkMesh.EntryPosition>();

            List<int> transparentIndexes = new List<int>();
            List<int> solidIndexes = new List<int>();

            //Dictionary<uint, (VertexPositionNormalTextureColor[] vertices, int[] indexes)> blocks =
            //     new Dictionary<uint, (VertexPositionNormalTextureColor[] vertices, int[] indexes)>();

            for (var y = 0; y < 16; y++)
                for (var x = 0; x < ChunkColumn.ChunkWidth; x++)
                    for (var z = 0; z < ChunkColumn.ChunkDepth; z++)
                    {
                        var vector = new Vector3(x, y, z);

                        //if (scheduled.HasFlag(ScheduleType.Scheduled) || scheduled.HasFlag(ScheduleType.Border) || scheduled.HasFlag(ScheduleType.Full) || section.IsDirty)
                        {

                            bool wasScheduled = section.IsScheduled(x, y, z);
                            bool wasLightingScheduled = section.IsLightingScheduled(x, y, z);

                            /*if ((scheduled.HasFlag(ScheduleType.Border) &&
                                 ((x == 0 && z == 0) || (x == ChunkWidth && z == 0) || (x == 0 && z == ChunkDepth)))
                                || (wasScheduled || wasLightingScheduled || scheduled.HasFlag(ScheduleType.Full)))*/
                            //if (true)

                            bool found = false;

                            if (true || (wasScheduled || wasLightingScheduled || scheduled.HasFlag(ScheduleType.Full)
                                 || (scheduled.HasFlag(ScheduleType.Lighting) /*&& cachedMesh == null*/) ||
                                 (scheduled.HasFlag(ScheduleType.Border) && (x == 0 || x == 15) && (z == 0 || z == 15))))
                            {
                                var blockState = section.Get(x, y, z);
                                if (blockState == null || !blockState.Block.Renderable) continue;

                                /*	if (!blocks.TryGetValue(blockState.ID, out var data))
                                    {
                                        data = blockState.Model.GetVertices(world, Vector3.Zero, blockState.Block);
                                        blocks.Add(blockState.ID, data);
                                    }
                                    else
                                    {
                                        reUseCounter++;

                                    }*/
                                var blockPosition = new Vector3(x, y + (yIndex * 16), z) + chunkPosition;
                                var data = blockState.Model.GetVertices(world, blockPosition, blockState.Block);


                                if (data.vertices == null || data.indexes == null || data.vertices.Length == 0 || data.indexes.Length == 0)
                                    continue;

                                bool transparent = blockState.Block.Transparent;



                                //var data = CalculateBlockVertices(world, section,
                                //	yIndex,
                                //	x, y, z, out bool transparent);

                                if (data.vertices.Length > 0 && data.indexes.Length > 0)
                                {
                                    int startVerticeIndex = solidVertices.Count; //transparent ? transparentVertices.Count : solidVertices.Count;
                                    foreach (var vert in data.vertices)
                                    {
                                        //var vertex = vert;
                                        //var vertex = new VertexPositionNormalTextureColor(vert.Position + blockPosition, Vector3.Zero, vert.TexCoords, vert.Color);
                                        //vertex.Position += blockPosition;

                                        //if (transparent)
                                        //{
                                        //transparentVertices.Add(vert);
                                        //}
                                        //else
                                        {
                                            solidVertices.Add(vert);
                                        }
                                    }

                                    int startIndex = transparent ? transparentIndexes.Count : solidIndexes.Count;
                                    for (int i = 0; i < data.indexes.Length; i++)
                                    {
                                        //	var vert = data.vertices[data.indexes[i]];
                                        var a = data.indexes[i];

                                        if (transparent)
                                        {
                                            //transparentVertices.Add(vert);
                                            transparentIndexes.Add(startVerticeIndex + a);

                                            if (a > solidVertices.Count)
                                            {
                                                Log.Warn($"INDEX > AVAILABLE VERTICES {a} > {solidVertices.Count}");
                                            }
                                        }
                                        else
                                        {
                                            //	solidVertices.Add(vert);
                                            solidIndexes.Add(startVerticeIndex + a);

                                            if (a > solidVertices.Count)
                                            {
                                                Log.Warn($"INDEX > AVAILABLE VERTICES {a} > {solidVertices.Count}");
                                            }
                                        }
                                    }


                                    positions.TryAdd(vector,
                                        new ChunkMesh.EntryPosition(transparent, startIndex, data.indexes.Length));
                                }


                                if (wasScheduled)
                                    section.SetScheduled(x, y, z, false);

                                if (wasLightingScheduled)
                                    section.SetLightingScheduled(x, y, z, false);
                            }
                            //else if (cachedMesh != null &&
                            //         cachedMesh.Positions.TryGetValue(vector, out ChunkMesh.EntryPosition position))
                            {
                                /*var cachedVertices = position.Transparent
                                    ? cachedMesh.Mesh.TransparentVertices
                                    : cachedMesh.Mesh.Vertices;

                                var cachedIndices = position.Transparent
                                    ? cachedMesh.Mesh.TransparentIndexes
                                    : cachedMesh.Mesh.SolidIndexes;

                                if (cachedIndices == null) continue;

                                List<int> done = new List<int>();
                                Dictionary<int, int> indiceIndexMap = new Dictionary<int, int>();
                                for (var index = 0; index < position.Length; index++)
                                {
                                    var u = cachedIndices[position.Index + index];
                                    if (!done.Contains(u))
                                    {
                                        done.Add(u);
                                        if (position.Transparent)
                                        {
                                            if (!indiceIndexMap.ContainsKey(u))
                                            {
                                                indiceIndexMap.Add(u, transparentVertices.Count);
                                            }

                                            transparentVertices.Add(
                                                cachedVertices[u]);
                                        }
                                        else
                                        {
                                            if (!indiceIndexMap.ContainsKey(u))
                                            {
                                                indiceIndexMap.Add(u, solidVertices.Count);
                                            }

                                            solidVertices.Add(
                                                cachedVertices[u]);
                                        }
                                    }

                                }


                                int startIndex = position.Transparent ? transparentIndexes.Count : solidIndexes.Count;
                                for (int i = 0; i < position.Length; i++)
                                {
                                    var o = indiceIndexMap[cachedIndices[position.Index + i]];
                                    if (position.Transparent)
                                    {
                                        transparentIndexes.Add(o);
                                    }
                                    else
                                    {
                                        solidIndexes.Add(o);
                                    }
                                }

                                //TODO: Find a way to update just the color of the faces, without having to recalculate vertices
                                //We could save what vertices belong to what faces i suppose?
                                //We could also use a custom shader to light the blocks...

                                positions.TryAdd(vector,
                                    new ChunkMesh.EntryPosition(position.Transparent, startIndex, position.Length));
                                found = true;*/
                            }
                        }
                    }

            outputPositions = positions;
            return new ChunkMesh(solidVertices.ToArray(), null/*transparentVertices.ToArray()*/, solidIndexes.ToArray(),
                transparentIndexes.ToArray());
        }
    }

    internal class ChunkData : IDisposable
    {
        public IndexBuffer SolidIndexBuffer { get; set; }
        public IndexBuffer TransparentIndexBuffer { get; set; }
        public VertexBuffer Buffer { get; set; }


        public void Dispose()
        {
	        SolidIndexBuffer?.Dispose();
	        TransparentIndexBuffer?.Dispose();
	        Buffer?.Dispose();
        }
    }
}