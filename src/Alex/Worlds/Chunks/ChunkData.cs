using System;
using System.Collections.Generic;
using Alex.API;
using Alex.API.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Worlds.Chunks
{
    internal class ChunkData : IDisposable
    {
        public PooledVertexBuffer Buffer { get; set; }
        public ChunkCoordinates Coordinates { get; set; }

        public Dictionary<RenderStage, ChunkRenderStage> RenderStages { get; set; }

        public void Dispose()
        {
            Buffer?.MarkForDisposal();

           foreach (var stage in RenderStages)
           {
               stage.Value.Dispose();
           }

           RenderStages.Clear();
        }
    }

    internal class ChunkRenderStage : IDisposable
    {
        private ChunkData Parent { get; }
        public PooledIndexBuffer IndexBuffer { get; set; }

        public ChunkRenderStage(ChunkData parent)
        {
            Parent = parent;
        }
        
        public virtual int Render(GraphicsDevice device, Effect effect)
        {
            device.SetVertexBuffer(Parent.Buffer);
            device.Indices = IndexBuffer;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, IndexBuffer.IndexCount / 3);
            }

            return IndexBuffer.IndexCount;
        }

        public void Dispose()
        {
            IndexBuffer?.MarkForDisposal();
            //Buffer?.MarkForDisposal();
        }
    }
}