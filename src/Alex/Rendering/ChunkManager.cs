using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Entities;
using Alex.Gamestates;
using Alex.Graphics.Models;
using Alex.ResourcePackLib.Json.Models;
using Alex.Utils;
using Alex.Worlds;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Utils;
using Color = Microsoft.Xna.Framework.Color;

//using OpenTK.Graphics;

namespace Alex.Rendering
{
    public class ChunkManager : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ChunkManager));
        
        private GraphicsDevice Graphics { get; }
        private Camera.Camera Camera { get; }
        private World World { get; }
	    private Alex Game { get; }

		public ChunkManager(Alex alex, GraphicsDevice graphics, Camera.Camera camera, World world)
		{
			Game = alex;
            Graphics = graphics;
            Camera = camera;
            World = world;
            Chunks = new ConcurrentDictionary<ChunkCoordinates, IChunkColumn>();

			var distance = (float)Math.Pow(alex.GameSettings.RenderDistance, 2);
			Effect = new AlphaTestEffect(Graphics)
            {
                Texture = alex.Resources.Atlas.GetAtlas(),
                VertexColorEnabled = true,
				World = Matrix.Identity
            };
			Effect.FogColor = new Vector3(0.5f, 0.5f, 0.5f);

			Effect.FogEnd = distance;
			Effect.FogStart = distance - 80f;
			Effect.FogEnabled = true;

            Updater = new Thread(ChunkUpdateThread)
            {IsBackground = true};

			ChunksToUpdate = new ConcurrentQueue<ChunkCoordinates>();
            Updater.Start();
        }

		//private ThreadSafeList<Entity> Entities { get; private set; } 

	    private ConcurrentQueue<ChunkCoordinates> ChunksToUpdate { get; set; }
	    private ConcurrentDictionary<ChunkCoordinates, IChunkColumn> Chunks { get; }
	    private readonly ThreadSafeList<ChunkCoordinates> _renderedChunks = new ThreadSafeList<ChunkCoordinates>();

		private Thread Updater { get; }

		private readonly AutoResetEvent _updateResetEvent = new AutoResetEvent(false);
		private CancellationTokenSource CancelationToken { get; set; } = new CancellationTokenSource();

	    private int RunningThreads = 0;
		private ManualResetEventSlim UpdateResetEvent = new ManualResetEventSlim(true);
		private void ChunkUpdateThread()
		{
			int maxThreads = Game.GameSettings.RenderDistance; //Environment.ProcessorCount / 2;
	        //int runningThreads = 0;
			while (!CancelationToken.IsCancellationRequested)
			{
				try
				{
					ChunkCoordinates i;
					if (ChunksToUpdate.TryDequeue(out i))
					{
						UpdateResetEvent.Wait(CancelationToken.Token);

						IChunkColumn chunk;
						if (!Chunks.TryGetValue(i, out chunk))
						{
							Interlocked.Decrement(ref _chunkUpdates);
							continue;
						}

						try
						{

							int newThreads = Interlocked.Increment(ref RunningThreads);
							if (newThreads == maxThreads)
							{
								UpdateResetEvent.Reset();
							}

							Task.Run(() => { UpdateChunk(chunk); }).ContinueWith(ContinuationAction);
						}
						catch (TaskCanceledException)
						{
							break;
						}
					}
					else
					{
						_updateResetEvent.WaitOne();
					}
				}
				catch (TaskCanceledException)
				{
					break;
				}
			}

			if (!CancelationToken.IsCancellationRequested)
				Log.Warn($"Chunk update loop has unexpectedly ended!");
		}

	    private void ContinuationAction(Task task)
	    {
		    Interlocked.Decrement(ref RunningThreads);
		    UpdateResetEvent.Set();
		}

		private bool UpdateChunk(IChunkColumn chunk)
	    {
			if (!Monitor.TryEnter(chunk.UpdateLock))
			{
				Interlocked.Decrement(ref _chunkUpdates);
				return false; //Another thread is already updating this chunk, return.
			}

			try
			{
				chunk.GenerateMeshes(World, out var mesh);

				VertexBuffer opaqueVertexBuffer = chunk.VertexBuffer;
				if (opaqueVertexBuffer == null ||
				    mesh.SolidVertices.Length != opaqueVertexBuffer.VertexCount)
				{
					opaqueVertexBuffer = RenewVertexBuffer(mesh.SolidVertices);

					VertexBuffer oldBuffer;
					lock (chunk.VertexLock)
					{
						oldBuffer = chunk.VertexBuffer;
						chunk.VertexBuffer = opaqueVertexBuffer;
					}

					oldBuffer?.Dispose();
				}
				else if (mesh.SolidVertices.Length > 0)
				{
					chunk.VertexBuffer.SetData(mesh.SolidVertices);
				}

				VertexBuffer transparentVertexBuffer = chunk.TransparentVertexBuffer;
				if (transparentVertexBuffer == null ||
				    mesh.TransparentVertices.Length != transparentVertexBuffer.VertexCount)
				{
					transparentVertexBuffer = RenewVertexBuffer(mesh.TransparentVertices);

					VertexBuffer oldBuffer;
					lock (chunk.VertexLock)
					{
						oldBuffer = chunk.TransparentVertexBuffer;
						chunk.TransparentVertexBuffer = transparentVertexBuffer;
					}

					oldBuffer?.Dispose();
				}
				else if (mesh.TransparentVertices.Length > 0)
				{
					chunk.TransparentVertexBuffer.SetData(mesh.TransparentVertices);
				}

				chunk.IsDirty = false;
				chunk.Scheduled = ScheduleType.Unscheduled;

				return true;
			}
			catch (Exception ex)
			{
				Log.Warn("Exception while updating chunk!", ex);
			}
			finally
			{
				Interlocked.Decrement(ref _chunkUpdates);
				Monitor.Exit(chunk.UpdateLock);
			}

		    return false;
	    }

	    private VertexBuffer RenewVertexBuffer(VertexPositionNormalTextureColor[] vertices)
	    {
		    VertexBuffer buffer = new VertexBuffer(Graphics,
			    VertexPositionNormalTextureColor.VertexDeclaration,
			    vertices.Length,
			    BufferUsage.WriteOnly);

		    if (vertices.Length > 0)
		    {
			    buffer.SetData(vertices);
		    }

		    return buffer;
	    }

	    private int _chunkUpdates = 0;
	    public int ChunkUpdates => _chunkUpdates;
	    public int ChunkCount => Chunks.Count;

		private AlphaTestEffect Effect { get; }

        public int Vertices { get; private set; }
	    public int RenderedChunks { get; private set; } = 0;

		public void Draw(IRenderArgs args)
		{
			var device = args.GraphicsDevice;

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

				//if (Monitor.TryEnter(chunk.VertexLock, 0))
				{
					buffer = chunk.VertexBuffer;
					transparentBuffer = chunk.TransparentVertexBuffer;
				//	Monitor.Exit(chunk.VertexLock);
				}
				//else
				{
			//		tempFailed++;
			//		continue;
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
				    chunk.Scheduled == ScheduleType.Unscheduled)
				{
				//	ScheduleChunkUpdate(c.Key, ScheduleType.Full);
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
					//device.DrawPrimitives(PrimitiveType.TriangleList, 0, b.VertexCount /3);
				}

				device.DrawPrimitives(PrimitiveType.TriangleList, 0, b.VertexCount / 3);

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
				}

		        device.DrawPrimitives(PrimitiveType.TriangleList, 0, b.VertexCount / 3);

				tempVertices += b.VertexCount;
	        }

			Vertices = tempVertices;
	        RenderedChunks = tempChunks;

			sw.Stop();
			if (tempFailed > 0)
			{
			/*	Log.Debug(
					$"Frame time: {sw.Elapsed.TotalMilliseconds}ms\n\t\tTransparent: {transparentFramesFailed} / {transparentBuffers.Length} chunks\n\t\tOpaque: {opaqueFramesFailed} / {opaqueBuffers.Length} chunks\n\t\t" +
					$"Full chunks: {tempChunks} / {chunks.Length}\n\t\t" +
					$"Missed frames: {tempFailed}");*/
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

	        ScheduleChunkUpdate(position, ScheduleType.Full);

			if (doUpdates)
            {
                ScheduleChunkUpdate(new ChunkCoordinates(position.X + 1, position.Z), ScheduleType.Border);
                ScheduleChunkUpdate(new ChunkCoordinates(position.X - 1, position.Z), ScheduleType.Border);
                ScheduleChunkUpdate(new ChunkCoordinates(position.X, position.Z + 1), ScheduleType.Border);
                ScheduleChunkUpdate(new ChunkCoordinates(position.X, position.Z - 1), ScheduleType.Border);
            }
		}

	    public void ScheduleChunkUpdate(ChunkCoordinates position, ScheduleType type)
	    {
		    if (Chunks.TryGetValue(position, out IChunkColumn chunk))
		    {
			    var currentSchedule = chunk.Scheduled;

			    if (currentSchedule != ScheduleType.Unscheduled)
			    {
				    if (currentSchedule != ScheduleType.Full)
				    {
					    chunk.Scheduled = type;
				    }

				    return;
			    }

			    chunk.Scheduled = type;

			    ChunksToUpdate.Enqueue(position);
			    _updateResetEvent.Set();

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
                ScheduleChunkUpdate(i.Key, ScheduleType.Full);
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