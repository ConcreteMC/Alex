using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Threading;
using Alex.Graphics;
using Alex.Utils;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Utils;

//using OpenTK.Graphics;

namespace Alex.Rendering
{
    public class RenderingManager
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
            Chunks = new ConcurrentDictionary<Vector3, Chunk>();

			Effect = new AlphaTestEffect(Graphics)
            {
                Texture = alex.Resources.Atlas.GetAtlas(),
                VertexColorEnabled = true,
				World = Matrix.Identity
            };

            Updater = new Thread(ChunkUpdateThread)
            {IsBackground = true};
           // ChunksToUpdate = new List<Vector3>();
		   ChunksToUpdate = new ConcurrentQueue<Vector3>();
            Updater.Start();
        }

        private Thread Updater { get; }
		private List<Vector3> RemovedChunks = new List<Vector3>();
		private ReaderWriterLockSlim RemovedLock = new ReaderWriterLockSlim();
		private AutoResetEvent UpdateResetEvent = new AutoResetEvent(false);
        private void ChunkUpdateThread()
        {
	        while (true)
	        {
		        Vector3 i;
		        if (ChunksToUpdate.TryDequeue(out i))
		        {
				    Chunk chunk;
			        if (!Chunks.TryGetValue(i, out chunk))
			        {
						RemovedLock.EnterUpgradeableReadLock();
				        try
				        {
					        if (!RemovedChunks.Contains(i))
					        {
						        ChunksToUpdate.Enqueue(i);
					        }
					        else
					        {
								RemovedLock.EnterWriteLock();
						        try
						        {
							        RemovedChunks.Remove(i);
							        Interlocked.Decrement(ref _chunkUpdates);
						        }
						        finally
						        {
									RemovedLock.ExitWriteLock();
						        }
					        }
				        }
				        finally
				        {
							RemovedLock.ExitUpgradeableReadLock();
				        }

				        continue;
			        }

			        ThreadPool.QueueUserWorkItem(state =>
			        {
				        if (!Monitor.TryEnter(chunk.UpdateLock)) return; //Another thread is already updating this chunk, return.

				        try
				        {
					        Mesh mesh;
					        Mesh transparentMesh;

							chunk.GenerateMeshes(World, out mesh, out transparentMesh);

					        try
					        {
						        VertexBuffer vertexBuffer = chunk.VertexBuffer;

						        if (vertexBuffer == null ||
						            mesh.Vertices.Length != vertexBuffer.VertexCount)
						        {
							        vertexBuffer = new VertexBuffer(Graphics,
								        VertexPositionNormalTextureColor.VertexDeclaration,
								        mesh.Vertices.Length,
								        BufferUsage.WriteOnly);

							        if (mesh.Vertices.Length > 0)
							        {
								        vertexBuffer.SetData(mesh.Vertices);
							        }

							        VertexBuffer oldBuffer;
							        lock (chunk.VertexLock)
							        {
								        oldBuffer = chunk.VertexBuffer;
								        chunk.VertexBuffer = vertexBuffer;
							        }

							        oldBuffer?.Dispose();
						        }
						        else if (mesh.Vertices.Length > 0)
						        {
							        lock (chunk.VertexLock)
							        {
								        chunk.VertexBuffer.SetData(mesh.Vertices);
							        }
						        }

						        VertexBuffer transVertexBuffer = chunk.TransparentVertexBuffer;

						        if (transVertexBuffer == null ||
						            transparentMesh.Vertices.Length != transVertexBuffer.VertexCount)
						        {
							        transVertexBuffer = new VertexBuffer(Graphics,
								        VertexPositionNormalTextureColor.VertexDeclaration,
								        transparentMesh.Vertices.Length,
								        BufferUsage.WriteOnly);

							        if (transparentMesh.Vertices.Length > 0)
							        {
								        transVertexBuffer.SetData(transparentMesh.Vertices);
							        }

							        VertexBuffer oldBuffer;
							        lock (chunk.VertexLock)
							        {
								        oldBuffer = chunk.TransparentVertexBuffer;
								        chunk.TransparentVertexBuffer = transVertexBuffer;
							        }

							        oldBuffer?.Dispose();
						        }
						        else if (transparentMesh.Vertices.Length > 0)
						        {
							        lock (chunk.VertexLock)
							        {
								        chunk.TransparentVertexBuffer.SetData(transparentMesh.Vertices);
							        }
						        }

						        chunk.IsDirty = false;
						        chunk.Scheduled = false;

						        Interlocked.Decrement(ref _chunkUpdates);
					        }
					        catch (Exception ex)
					        {
						        Log.Warn("Exception while updating chunk!", ex);
					        }
				        }
				        finally
				        {
					        Monitor.Exit(chunk.UpdateLock);
				        }
			        });
				}
		        else
		        {
			        UpdateResetEvent.WaitOne();
		        }
	        }
        }

	    private int _chunkUpdates = 0;
	    public int ChunkUpdates => _chunkUpdates;

		private ConcurrentQueue<Vector3> ChunksToUpdate { get; set; }

        public ConcurrentDictionary<Vector3, Chunk> Chunks { get; }
		 
        private AlphaTestEffect Effect { get; }

        public int Vertices { get; private set; }
	    public int RenderedChunks { get; private set; } = 0;


		public void Draw(GraphicsDevice device)
		{
			Stopwatch sw = Stopwatch.StartNew();

			Effect.View = Camera.ViewMatrix;
			Effect.Projection = Camera.ProjectionMatrix;

			var chunks = Chunks.ToArray().Where(x => Camera.BoundingFrustum.Intersects(new Microsoft.Xna.Framework.BoundingBox(x.Value.Position, x.Value.Position + x.Value.Size))).ToArray();

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
					ScheduleChunkUpdate(chunk.Position);
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
					device.DrawPrimitives(PrimitiveType.TriangleList, 0, b.VertexCount / 3);
				}

				tempVertices += b.VertexCount;
			}

			//Render Transparent
			device.DepthStencilState = DepthStencilState.Default;
			device.BlendState = BlendState.AlphaBlend;

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
			        device.DrawPrimitives(PrimitiveType.TriangleList, 0, b.VertexCount / 3);
		        }
				
		        tempVertices += b.VertexCount;
	        }

			Vertices = tempVertices;
	        RenderedChunks = tempChunks;

			sw.Stop();
		/*	if (sw.Elapsed > TimeSpan.FromMilliseconds(3) || tempChunks < chunks.Length)
			{
				Log.Debug(
					$"Frame time: {sw.Elapsed.TotalMilliseconds}ms\n\t\tTransparent: {transparentFramesFailed} / {transparentBuffers.Length} chunks\n\t\tOpaque: {opaqueFramesFailed} / {opaqueBuffers.Length} chunks\n\t\t" +
					$"Full chunks: {tempChunks} / {chunks.Length}\n\t\t" +
					$"Missed frames: {tempFailed}");
			}*/
		}

        public void AddChunk(Chunk chunk, Vector3 position, bool doUpdates = false)
        {
            Chunks.AddOrUpdate(position, chunk, (vector3, chunk1) =>
            {
	            chunk1.Dispose();
				Log.WarnFormat("Replaced chunk at {0}", position);
                return chunk;
            });

			RemovedLock.EnterUpgradeableReadLock();
			try
			{
				if (RemovedChunks.Contains(position))
				{
					RemovedLock.EnterWriteLock();
					try
					{
						RemovedChunks.Remove(position);
					}
					finally
					{
						RemovedLock.ExitWriteLock();
					}
				}
			}
			finally
			{
				RemovedLock.ExitUpgradeableReadLock();
			}

			ScheduleChunkUpdate(position);

            if (doUpdates)
            {
                ScheduleChunkUpdate(new Vector3(position.X + 1, position.Y, position.Z));
                ScheduleChunkUpdate(new Vector3(position.X - 1, position.Y, position.Z));
                ScheduleChunkUpdate(new Vector3(position.X, position.Y, position.Z + 1));
                ScheduleChunkUpdate(new Vector3(position.X, position.Y, position.Z - 1));
            }
        }

        public void ScheduleChunkUpdate(Vector3 position)
        {
	        if (Chunks.TryGetValue(position, out Chunk chunk))
	        {
		        if (chunk.Scheduled)
			        return;

		        chunk.Scheduled = true;

		        ChunksToUpdate.Enqueue(position);
		        UpdateResetEvent.Set();

				Interlocked.Increment(ref _chunkUpdates);
			}
        }

        public void RemoveChunk(Vector3 position)
        {
            Chunk chunk;
	        if (Chunks.TryRemove(position, out chunk))
	        {
				RemovedLock.EnterWriteLock();
		        try
		        {
					RemovedChunks.Add(position);
		        }
		        finally
		        {
					RemovedLock.ExitWriteLock();
		        }
				
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
    }
}