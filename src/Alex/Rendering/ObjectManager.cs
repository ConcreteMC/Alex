using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alex.Utils;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenTK.Graphics;

namespace Alex.Rendering
{
    public class ObjectManager : Singleton<ObjectManager>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ObjectManager));
        
        public ObjectManager()
        {
            Chunks = new ConcurrentDictionary<Vector3, Chunk>();

			Effect = new AlphaTestEffect(Game.Instance.GraphicsDevice)
            {
                Texture = ResManager.GetAtlas(),
                World = Matrix.Identity,
                VertexColorEnabled = true,
            };
            
            Updater = new Thread(ChunkUpdateThread);
            ChunksToUpdate = new List<Vector3>();
            Updater.Start();
        }

        private Thread Updater { get; }

        private void ChunkUpdateThread()
        {
            while (true)
            {
                Vector3[] chunks = new Vector3[0];

                SpinWait sw = new SpinWait();
                while (chunks.Length == 0)
                {
                    lock (UpdateLock)
                    {
                        chunks = ChunksToUpdate.ToArray();
                    }
                    sw.SpinOnce();
                }

                foreach (var i in chunks)
                {
                    Chunk chunk;
                    if (!Chunks.TryGetValue(i, out chunk))
                    {
                        lock (UpdateLock)
                        {
                            ChunksToUpdate.Remove(i);
                        }
                        continue;
                    }

                    if (!Monitor.TryEnter(chunk.UpdateLock)) return;
                    try
                    {
                        chunk.Mesh = chunk.GenerateMesh();
                        try
                        {
                            lock (chunk.VertexLock)
                            {
                                if (chunk.VertexBuffer == null ||
                                    chunk.Mesh.Vertices.Length > chunk.VertexBuffer.VertexCount)
                                {
                                    chunk.VertexBuffer = new VertexBuffer(Alex.Instance.GraphicsDevice,
                                        VertexPositionNormalTextureColor.VertexDeclaration,
                                        chunk.Mesh.Vertices.Length,
                                        BufferUsage.WriteOnly);
                                }

                                if (chunk.Mesh.Vertices.Length > 0)
                                {
                                    chunk.VertexBuffer.SetData(chunk.Mesh.Vertices);
                                }

                                chunk.IsDirty = false;
                                lock (UpdateLock)
                                {
                                    ChunksToUpdate.Remove(i);
                                }
                            }
                        }
                        catch (GraphicsContextException)
                        {

                        }
                        catch
                        {

                        }
                    }
                    finally
                    {
                        Monitor.Exit(chunk.UpdateLock);
                    }
                }
            }
        }

        public int ChunkUpdates
        {
            get { return ChunksToUpdate.Count; }
        }

        private object UpdateLock { get; set; } = new object();
        private List<Vector3> ChunksToUpdate { get; set; }
        public ConcurrentDictionary<Vector3, Chunk> Chunks { get; }
		 
        private AlphaTestEffect Effect { get; }

        public int Vertices { get; private set; }

        public void Draw(GraphicsDevice device)
        {
            Effect.View = Game.MainCamera.ViewMatrix;
            Effect.Projection = Game.MainCamera.ProjectionMatrix;

            var tempVertices = 0;
            foreach (var c in Chunks.ToArray())
            {
                var chunk = c.Value;

                VertexBuffer buffer;
                if (Monitor.TryEnter(chunk.VertexLock))
                {
                    buffer = chunk.VertexBuffer;
                    Monitor.Exit(chunk.VertexLock);
                }
                else
                {
                    continue;
                }

                if ((chunk.IsDirty || buffer == null) && Monitor.TryEnter(chunk.UpdateLock, 0))
                {
                    try
                    {
                        var key = new Vector3((int) chunk.Position.X >> 4, 0, (int) chunk.Position.Z >> 4);
                        ScheduleChunkUpdate(key);
                       // lock (UpdateLock)
                        //{
                        //    if (!ChunksToUpdate.Contains(key))
                       //     {
                       //         ChunksToUpdate.Add(key);
                       //     }
                       // }
                    }
                    finally
                    {
                        Monitor.Exit(chunk.UpdateLock);
                    }
                }

                if (buffer == null) continue;

                if (buffer.VertexCount == 0) continue;

                device.SetVertexBuffer(buffer);
                foreach (var pass in Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawPrimitives(PrimitiveType.TriangleList, 0, chunk.VertexBuffer.VertexCount/3);
                }
                tempVertices += buffer.VertexCount;
            }
            Vertices = tempVertices;
        }

        public void AddChunk(Chunk chunk, Vector3 position, bool doUpdates = false)
        {
            if (!Chunks.TryAdd(position, chunk))
            {
                Log.WarnFormat("Tried adding duplicate chunk for position {0}", position);
                return;
            }

            ScheduleChunkUpdate(position);

            if (doUpdates)
            {
                ScheduleChunkUpdate(position + new Vector3(1, 0, 0));
                ScheduleChunkUpdate(position + new Vector3(0, 0, 1));
                ScheduleChunkUpdate(position - new Vector3(1, 0, 0));
                ScheduleChunkUpdate(position - new Vector3(0, 0, 1));
            }
        }

        public void ScheduleChunkUpdate(Vector3 position)
        {
            Chunk chunk;
            if (Chunks.TryGetValue(position, out chunk))
            {
                lock (UpdateLock)
                {
                    if (!ChunksToUpdate.Contains(position))
                    {
                        ChunksToUpdate.Add(position);
                    }
                }
            }
        }

        public void RemoveChunk(Vector3 position)
        {
            Chunk chunk;
            Chunks.TryRemove(position, out chunk);
        }
    }
}