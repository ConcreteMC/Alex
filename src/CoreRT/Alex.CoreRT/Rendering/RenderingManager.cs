using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alex.CoreRT.API.Graphics;
using Alex.CoreRT.API.World;
using Alex.CoreRT.Utils;
using Alex.CoreRT.Worlds;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Utils;

//using OpenTK.Graphics;

namespace Alex.CoreRT.Rendering
{
    public class RenderingManager : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RenderingManager));
        
        private GraphicsDevice Graphics { get; }
        private Camera.Camera Camera { get; }
        private World World { get; }
	    private Alex Game { get; }
		public RenderingManager(Alex alex, GraphicsDevice graphics, Camera.Camera camera, World world)
		{
			Game = alex;
            Graphics = graphics;
            Camera = camera;
            World = world;
            Chunks = new ConcurrentDictionary<ChunkCoordinates, IChunkColumn>();

			Effect = new AlphaTestEffect(Graphics)
            {
                Texture = alex.Resources.Atlas.GetAtlas(),
                VertexColorEnabled = true,
				World = Matrix.Identity,
				FogEnabled = false,
				FogColor = Color.LightGray.ToVector3(),
				FogStart = (alex.GameSettings.RenderDistance - 3) * 16,
				FogEnd = (alex.GameSettings.RenderDistance - 1) * 16,
				DiffuseColor = Color.White.ToVector3(),
            };

            Updater = new Thread(ChunkUpdateThread)
            {IsBackground = true};

			ChunksToUpdate = new ConcurrentQueue<ChunkCoordinates>();
            Updater.Start();
        }

	    private ConcurrentQueue<ChunkCoordinates> ChunksToUpdate { get; set; }
	    private ConcurrentDictionary<ChunkCoordinates, IChunkColumn> Chunks { get; }
	    private ThreadSafeList<ChunkCoordinates> _renderedChunks = new ThreadSafeList<ChunkCoordinates>();

		private Thread Updater { get; }

		private AutoResetEvent UpdateResetEvent = new AutoResetEvent(false);
		private CancellationTokenSource CancelationToken { get; set; } = new CancellationTokenSource();
        private void ChunkUpdateThread()
        {
			ManualResetEventSlim resetEvent = new ManualResetEventSlim(true);
	        int maxThreads = Environment.ProcessorCount / 2;
	        int runningThreads = 0;
	        while (!CancelationToken.IsCancellationRequested)
	        {
		        ChunkCoordinates i;
		        if (ChunksToUpdate.TryDequeue(out i))
		        {
			        resetEvent.Wait(CancelationToken.Token);

					IChunkColumn chunk;
			        if (!Chunks.TryGetValue(i, out chunk))
			        {
				        Interlocked.Decrement(ref _chunkUpdates);
						continue;
			        }

			        try
			        {
				        
				        int newThreads = Interlocked.Increment(ref runningThreads);
				        if (newThreads == maxThreads)
				        {
							resetEvent.Reset();
				        }

						Task.Run(() =>
				        {
					        UpdateChunk(chunk);

					        Interlocked.Decrement(ref runningThreads);
							resetEvent.Set();
				        });
			        }
			        catch(TaskCanceledException)
			        {
				        break;
			        }
		        }
		        else
		        {
			        UpdateResetEvent.WaitOne();
		        }
	        }
        }

	    private bool UpdateChunk(IChunkColumn chunk)
	    {
			if (!Monitor.TryEnter(chunk.UpdateLock))
			{
				return false; //Another thread is already updating this chunk, return.
			}

			try
			{
				chunk.GenerateMeshes(World, out var mesh, out var transparentMesh);

				VertexBuffer opaqueVertexBuffer = chunk.VertexBuffer;
				if (opaqueVertexBuffer == null ||
				    mesh.Vertices.Length != opaqueVertexBuffer.VertexCount)
				{
					opaqueVertexBuffer = RenewVertexBuffer(mesh);

					VertexBuffer oldBuffer;
					lock (chunk.VertexLock)
					{
						oldBuffer = chunk.VertexBuffer;
						chunk.VertexBuffer = opaqueVertexBuffer;
					}

					oldBuffer?.Dispose();
				}
				else if (mesh.Vertices.Length > 0)
				{
					chunk.VertexBuffer.SetData(mesh.Vertices);
				}

				VertexBuffer transparentVertexBuffer = chunk.TransparentVertexBuffer;
				if (transparentVertexBuffer == null ||
				    transparentMesh.Vertices.Length != transparentVertexBuffer.VertexCount)
				{
					transparentVertexBuffer = RenewVertexBuffer(transparentMesh);

					VertexBuffer oldBuffer;
					lock (chunk.VertexLock)
					{
						oldBuffer = chunk.TransparentVertexBuffer;
						chunk.TransparentVertexBuffer = transparentVertexBuffer;
					}

					oldBuffer?.Dispose();
				}
				else if (transparentMesh.Vertices.Length > 0)
				{
					chunk.TransparentVertexBuffer.SetData(transparentMesh.Vertices);
				}

				chunk.IsDirty = false;
				chunk.Scheduled = false;

				Interlocked.Decrement(ref _chunkUpdates);
				return true;
			}
			catch (Exception ex)
			{
				Log.Warn("Exception while updating chunk!", ex);
			}
			finally
			{
				Monitor.Exit(chunk.UpdateLock);
			}

		    return false;
	    }

	    private VertexBuffer RenewVertexBuffer(Mesh mesh)
	    {
		    VertexBuffer buffer = new VertexBuffer(Graphics,
			    VertexPositionNormalTextureColor.VertexDeclaration,
			    mesh.Vertices.Length,
			    BufferUsage.WriteOnly);

		    if (mesh.Vertices.Length > 0)
		    {
			    buffer.SetData(mesh.Vertices);
		    }

		    return buffer;
	    }

	    private int _chunkUpdates = 0;
	    public int ChunkUpdates => _chunkUpdates;
	    public int ChunkCount => Chunks.Count;

		private AlphaTestEffect Effect { get; }

        public int Vertices { get; private set; }
	    public int RenderedChunks { get; private set; } = 0;

		public void Draw(GraphicsDevice device)
		{
			Stopwatch sw = Stopwatch.StartNew();

			Effect.View = Camera.ViewMatrix;
			Effect.Projection = Camera.ProjectionMatrix;

			var r = _renderedChunks.ToArray();
			var chunks = new KeyValuePair<ChunkCoordinates, IChunkColumn>[r.Length];
			for (var index = 0; index < r.Length; index++)
			{
				var c = r[index];
				if (Chunks.TryGetValue(c, out IChunkColumn chunk))
				{
					chunks[index] = new KeyValuePair<ChunkCoordinates, IChunkColumn>(c, chunk);
				}
				else
				{
					chunks[index] = new KeyValuePair<ChunkCoordinates, IChunkColumn>(c, null);
				}
			}

			VertexBuffer[] opaqueBuffers = new VertexBuffer[chunks.Length];
	        VertexBuffer[] transparentBuffers = new VertexBuffer[chunks.Length];

			var tempVertices = 0;
	        int tempChunks = 0;
			int tempFailed = 0;
			int opaqueFramesFailed = 0;
			int transparentFramesFailed = 0;

			for (var index = 0; index < chunks.Length; index++)
			{
				var c = chunks[index];
				var chunk = c.Value;
				if (chunk == null) continue;

				VertexBuffer buffer = null;
				VertexBuffer transparentBuffer = null;

				if (Monitor.TryEnter(chunk.VertexLock, 0))
				{
					buffer = chunk.VertexBuffer;
					transparentBuffer = chunk.TransparentVertexBuffer;
					Monitor.Exit(chunk.VertexLock);
				}
				else
				{
					tempFailed++;
					continue;
				}

				if (buffer != null)
				{
					opaqueBuffers[index] = buffer;
				}

				if (transparentBuffer != null)
				{
					transparentBuffers[index] = transparentBuffer;
				}

				if ((chunk.IsDirty || (buffer == null || transparentBuffer == null)) &&
				    !chunk.Scheduled)
				{
					ScheduleChunkUpdate(c.Key);
					if (buffer == null && transparentBuffer == null)
					{
						tempFailed++;
					}

					continue;
				}

				if (transparentBuffer != null && buffer != null)
					tempChunks++;
			}

			//Render Solid
			device.DepthStencilState = DepthStencilState.Default;
			device.BlendState = BlendState.AlphaBlend;

			foreach (var b in opaqueBuffers)
			{
				if (b == null)
				{
					opaqueFramesFailed++;
					continue;
				}

				if (b.VertexCount == 0) continue;

				device.SetVertexBuffer(b);

				foreach (var pass in Effect.CurrentTechnique.Passes)
				{
					pass.Apply();
					device.DrawPrimitives(PrimitiveType.TriangleList, 0, b.VertexCount /3);
				}

				tempVertices += b.VertexCount;
			}

			//Render Transparent
		//	device.DepthStencilState = DepthStencilState.Default;
		//	device.BlendState = BlendState.AlphaBlend;

	        foreach (var b in transparentBuffers)
	        {
		        if (b == null)
		        {
			        transparentFramesFailed++;
			        continue;
		        }

		        if (b.VertexCount == 0) continue;

		        device.SetVertexBuffer(b);
				
				foreach (var pass in Effect.CurrentTechnique.Passes)
		        {
			        pass.Apply();
			        device.DrawPrimitives(PrimitiveType.TriangleList, 0, b.VertexCount /3);
		        }
				
		        tempVertices += b.VertexCount;
	        }

			Vertices = tempVertices;
	        RenderedChunks = tempChunks;

			sw.Stop();
			if (tempFailed > 0)
			{
				Log.Debug(
					$"Frame time: {sw.Elapsed.TotalMilliseconds}ms\n\t\tTransparent: {transparentFramesFailed} / {transparentBuffers.Length} chunks\n\t\tOpaque: {opaqueFramesFailed} / {opaqueBuffers.Length} chunks\n\t\t" +
					$"Full chunks: {tempChunks} / {chunks.Length}\n\t\t" +
					$"Missed frames: {tempFailed}");
			}
		}

	    public void Update()
	    {
		    var renderedChunks = Chunks.ToArray().Where(x =>
		    {
			    var chunkPos = new Vector3(x.Key.X * ChunkColumn.ChunkWidth, 0, x.Key.Z * ChunkColumn.ChunkDepth);
			    return Camera.BoundingFrustum.Intersects(new Microsoft.Xna.Framework.BoundingBox(chunkPos,
				    chunkPos + new Vector3(ChunkColumn.ChunkWidth, 16 * ((x.Value.GetHeighest() >> 4) + 1),
					    ChunkColumn.ChunkDepth)));
		    }).Select(x => x.Key).ToArray();

			foreach (var c in renderedChunks)
			{
				_renderedChunks.TryAdd(c);
			}

		    foreach (var c in _renderedChunks.ToArray())
		    {
			    if (!renderedChunks.Contains(c))
			    {
				    _renderedChunks.Remove(c);
			    }
		    }
		}

        public void AddChunk(IChunkColumn chunk, ChunkCoordinates position, bool doUpdates = false)
        {
            Chunks.AddOrUpdate(position, chunk, (vector3, chunk1) =>
            {
	            chunk1.Dispose();
				Log.WarnFormat("Replaced chunk at {0}", position);
                return chunk;
            });

	        ScheduleChunkUpdate(position);

			if (doUpdates)
            {
                ScheduleChunkUpdate(new ChunkCoordinates(position.X + 1, position.Z));
                ScheduleChunkUpdate(new ChunkCoordinates(position.X - 1, position.Z));
                ScheduleChunkUpdate(new ChunkCoordinates(position.X, position.Z + 1));
                ScheduleChunkUpdate(new ChunkCoordinates(position.X, position.Z - 1));
            }
		}

		public void ScheduleChunkUpdate(ChunkCoordinates position)
        {
	        if (Chunks.TryGetValue(position, out IChunkColumn chunk))
	        {
		        if (chunk.Scheduled)
			        return;

		        chunk.Scheduled = true;

		        ChunksToUpdate.Enqueue(position);
		        UpdateResetEvent.Set();

				Interlocked.Increment(ref _chunkUpdates);
			}
        }

        public void RemoveChunk(ChunkCoordinates position)
        {
	        IChunkColumn chunk;
	        if (Chunks.TryRemove(position, out chunk))
	        {				
				chunk?.Dispose();
	        }
        }

        public void RebuildAll()
        {
            foreach (var i in Chunks)
            {
                ScheduleChunkUpdate(i.Key);
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
			    Chunks.TryRemove(chunk.Key, out IChunkColumn column);
				chunk.Value.Dispose();
		    }
	    }
    }
}