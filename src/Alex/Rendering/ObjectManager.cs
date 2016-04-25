using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Rendering
{
    public class ObjectManager : Singleton<ObjectManager>
    {
        public ObjectManager()
        {
            Chunks = new Dictionary<Vector3, Chunk>();

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

                while (chunks.Length == 0)
                {
                    lock (UpdateLock)
                    {
                        chunks = ChunksToUpdate.ToArray();
                    }
                    Thread.Sleep(50);
                }

                foreach (var i in chunks)
                {
                    if (!Chunks.ContainsKey(i))
                    {
                        lock (UpdateLock)
                        {
                            ChunksToUpdate.Remove(i);
                        }
                        continue;
                    }

                    Chunks[i].Mesh = Chunks[i].GenerateMesh();
                    Chunks[i].VertexBuffer = new VertexBuffer(Alex.Instance.GraphicsDevice, VertexPositionNormalTextureColor.VertexDeclaration,
                        Chunks[i].Mesh.Vertices.Length,
                        BufferUsage.WriteOnly);

                    if (Chunks[i].Mesh.Vertices.Length > 0)
                    {
                        Chunks[i].VertexBuffer.SetData(Chunks[i].Mesh.Vertices);
                    }

                    Chunks[i].IsDirty = false;

                    lock (UpdateLock)
                    {
                        ChunksToUpdate.Remove(i);
                    }
                }
            }
        }
        private object UpdateLock { get; set; } = new object();

        private List<Vector3> ChunksToUpdate { get; set; }

        public Dictionary<Vector3, Chunk> Chunks { get; }
		 
        private AlphaTestEffect Effect { get; }

        public int Vertices { get; private set; }
        private object Lock { get; set; } = new object();

        public void Draw(GraphicsDevice device)
        {
            Effect.View = Game.MainCamera.ViewMatrix;
            Effect.Projection = Game.MainCamera.ProjectionMatrix;

            var tempVertices = 0;
            lock (Lock)
            {
                foreach (var c in Chunks.ToArray())
                {
                    var chunk = c.Value;

                    if (chunk.IsDirty || chunk.VertexBuffer == null)
                    {
                        lock (UpdateLock)
                        {
                            var key = new Vector3((int)chunk.Position.X >> 4, 0, (int)chunk.Position.Z >> 4);

                            if (!ChunksToUpdate.Contains(key))
                            {
                                ChunksToUpdate.Add(key);
                            }
                        }
                        continue;
                    }

                    if (chunk.Mesh.Vertices.Length == 0) continue;

                    device.SetVertexBuffer(chunk.VertexBuffer);
                    foreach (var pass in Effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        device.DrawPrimitives(PrimitiveType.TriangleList, 0, chunk.Mesh.Vertices.Length/3);
                    }
                    tempVertices += chunk.Mesh.Vertices.Length;
                }
            }
            Vertices = tempVertices;
        }

        public void AddChunk(Chunk chunk, Vector3 position)
        {
            if (Chunks.ContainsKey(position)) return;

            lock (Lock)
            {
                Chunks.Add(position, chunk);
            }

            lock (UpdateLock)
            {
                if (!ChunksToUpdate.Contains(position)) ChunksToUpdate.Add(position);
            }
        }

        public void RemoveChunk(Vector3 position)
        {
            if (!Chunks.ContainsKey(position)) return;

            Chunks.Remove(position);
        }
    }
}