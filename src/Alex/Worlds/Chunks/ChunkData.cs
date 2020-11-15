using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alex.API;
using Alex.API.Blocks;
using Alex.API.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Worlds.Chunks
{
    public class ChunkData : IDisposable
    {
        public        ConcurrentDictionary<RenderStage, ChunkRenderStage> RenderStages { get; set; }

        private static long _instances = 0;
        public ChunkData()
        {
            RenderStages = new ConcurrentDictionary<RenderStage, ChunkRenderStage>();

            Interlocked.Increment(ref _instances);
        }

        public BlockShaderVertex[] Vertices
        {
            get
            {
                List<BlockShaderVertex> vertices = new List<BlockShaderVertex>();

                foreach (var stage in RenderStages)
                {
                    vertices.AddRange(stage.Value.Vertices);
                }

                return vertices.ToArray();
            }
        }
        
        public void AddVertex(BlockCoordinates blockCoordinates, BlockShaderVertex vertex, RenderStage stage)
        {
            var rStage = RenderStages.GetOrAdd(stage, CreateRenderStage);
            rStage.AddVertex(blockCoordinates, vertex);
        }

        private ChunkRenderStage CreateRenderStage(RenderStage arg)
        {
            return new ChunkRenderStage();
        }

        public void Remove(GraphicsDevice device, BlockCoordinates blockCoordinates)
        {
            foreach (var stage in RenderStages.Values.ToArray())
            {
                stage.Remove(blockCoordinates);
            }
        }

        public void ApplyChanges(GraphicsDevice device)
        {
            foreach(var stage in RenderStages)
                stage.Value.Apply(device);
        }

        public bool                Disposed { get; private set; } = false;
        public void Dispose()
        {
           // lock (WriteLock)
           {
             //  int size = Vertices != null ? Vertices.Length : 0;
               // Buffer?.MarkForDisposal();

                foreach (var stage in RenderStages)
                {
                    stage.Value.Dispose();
                }

                RenderStages.Clear();
                // Vertices = null;
                
                Disposed = true;
                Interlocked.Decrement(ref _instances);
               // Interlocked.Add(ref _totalSize, -size);
            }
        }
    }
}