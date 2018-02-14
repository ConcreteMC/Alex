using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using Alex.Utils;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
//using OpenTK.Graphics;

namespace Alex.Rendering
{
    public class ObjectManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ObjectManager));
        
        private GraphicsDevice Graphics { get; }
        private Camera.Camera Camera { get; }
        private World World { get; }
        public ObjectManager(GraphicsDevice graphics, Camera.Camera camera, World world)
        {
            Graphics = graphics;
            Camera = camera;
            World = world;
            Chunks = new ConcurrentDictionary<Vector3, Chunk>();

			Effect = new AlphaTestEffect(Graphics)
            {
                Texture = ResManager.GetAtlas(graphics),
                World = Matrix.Identity,
                VertexColorEnabled = true,
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
				        bool w = false;
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
				        if (!Monitor.TryEnter(chunk.UpdateLock)) return;
				        try
				        {
					        chunk.Mesh = chunk.GenerateSolidMesh(World);
					        chunk.TransparentMesh = chunk.GenerateTransparentMesh(World);

					        try
					        {
						        lock (chunk.VertexLock)
						        {
							        if (chunk.VertexBuffer == null ||
							            chunk.Mesh.Vertices.Length != chunk.VertexBuffer.VertexCount)
							        {
								        chunk.VertexBuffer = new VertexBuffer(Graphics,
									        VertexPositionNormalTextureColor.VertexDeclaration,
									        chunk.Mesh.Vertices.Length,
									        BufferUsage.WriteOnly);
							        }

							        if (chunk.Mesh.Vertices.Length > 0)
							        {
								        chunk.VertexBuffer.SetData(chunk.Mesh.Vertices);
							        }

							        if (chunk.TransparentVertexBuffer == null ||
							            chunk.TransparentMesh.Vertices.Length != chunk.TransparentVertexBuffer.VertexCount)
							        {
								        chunk.TransparentVertexBuffer = new VertexBuffer(Graphics,
									        VertexPositionNormalTextureColor.VertexDeclaration,
									        chunk.TransparentMesh.Vertices.Length,
									        BufferUsage.WriteOnly);
							        }

							        if (chunk.TransparentMesh.Vertices.Length > 0)
							        {
								        chunk.TransparentVertexBuffer.SetData(chunk.TransparentMesh.Vertices);
							        }

									chunk.IsDirty = false;
							        chunk.Scheduled = false;

							        Interlocked.Decrement(ref _chunkUpdates);
						        }
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
					Thread.Sleep(10);
		        }
	        }
        }

	    private int _chunkUpdates = 0;
	    public int ChunkUpdates => _chunkUpdates;

		private ConcurrentQueue<Vector3> ChunksToUpdate { get; set; }

        public ConcurrentDictionary<Vector3, Chunk> Chunks { get; }
		 
        private AlphaTestEffect Effect { get; }

        public int Vertices { get; private set; }

		private DepthStencilState OpaqueState = new DepthStencilState
		{
			StencilEnable = true,
			StencilFunction = CompareFunction.Always,
			StencilPass = StencilOperation.Replace,
			ReferenceStencil = 1,
			DepthBufferEnable = true,
			
		};

		public void Draw(GraphicsDevice device)
        {
            Effect.View = Camera.ViewMatrix;
            Effect.Projection = Camera.ProjectionMatrix;

	        var chunks = Chunks.ToArray().OrderBy(x => Vector3.Distance(x.Key * 16, Camera.Position)).ToArray();

			VertexBuffer[] opaqueBuffers = new VertexBuffer[chunks.Length];
	        VertexBuffer[] transparentBuffers = new VertexBuffer[chunks.Length];

			var tempVertices = 0;
	        for (var index = 0; index < chunks.Length; index++)
	        {
		        var c = chunks[index];
		        var chunk = c.Value;
				if (chunk == null) continue;

		        VertexBuffer buffer;
		        VertexBuffer transparentBuffer;
		        if (Monitor.TryEnter(chunk.VertexLock, 0))
		        {
			        buffer = chunk.VertexBuffer;
			        transparentBuffer = chunk.TransparentVertexBuffer;

			        opaqueBuffers[index] = buffer;
			        transparentBuffers[index] = transparentBuffer;

					Monitor.Exit(chunk.VertexLock);
		        }
		        else
		        {
			        continue;
		        }

		        if ((chunk.IsDirty || buffer == null || transparentBuffer == null) && !chunk.Scheduled && Monitor.TryEnter(chunk.UpdateLock, 0))
		        {
			        try
			        {
				        chunk.Scheduled = true;
				        ScheduleChunkUpdate(chunk.Position);
			        }
			        finally
			        {
				        Monitor.Exit(chunk.UpdateLock);
			        }
		        }
	        }

			//Render Solid
	        device.DepthStencilState = OpaqueState;
			device.BlendState = BlendState.NonPremultiplied;

			foreach (var buffer in opaqueBuffers)
	        {
		        if (buffer == null) continue;

		        if (buffer.VertexCount == 0) continue;

		        device.SetVertexBuffer(buffer);
		        foreach (var pass in Effect.CurrentTechnique.Passes)
		        {
			        pass.Apply();
			        device.DrawPrimitives(PrimitiveType.TriangleList, 0, buffer.VertexCount / 3);
		        }

		        tempVertices += buffer.VertexCount;
			}

	        //Render Transparent
			device.DepthStencilState = DepthStencilState.DepthRead;
			device.BlendState = BlendState.AlphaBlend;

	        foreach (var buffer in transparentBuffers)
	        {
		        if (buffer == null) continue;

		        if (buffer.VertexCount == 0) continue;

		        device.SetVertexBuffer(buffer);
		        foreach (var pass in Effect.CurrentTechnique.Passes)
		        {
			        pass.Apply();
			        device.DrawPrimitives(PrimitiveType.TriangleList, 0, buffer.VertexCount / 3);
		        }

		        tempVertices += buffer.VertexCount;
	        }

			Vertices = tempVertices;
        }

        public void AddChunk(Chunk chunk, Vector3 position, bool doUpdates = false)
        {
            Chunks.AddOrUpdate(position, chunk, (vector3, chunk1) =>
            {
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