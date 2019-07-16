using System;
using Alex.API.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Worlds
{
    internal class ChunkData : IDisposable
    {
        public PooledIndexBuffer SolidIndexBuffer { get; set; }
        public PooledIndexBuffer TransparentIndexBuffer { get; set; }
        public PooledIndexBuffer AnimatedIndexBuffer { get; set; }
        public PooledVertexBuffer Buffer { get; set; }
        public ChunkCoordinates Coordinates { get; set; }

        public void Dispose()
        {
            SolidIndexBuffer?.MarkForDisposal();
            TransparentIndexBuffer?.MarkForDisposal();
            AnimatedIndexBuffer?.MarkForDisposal();
            Buffer?.MarkForDisposal();
        }
    }
}