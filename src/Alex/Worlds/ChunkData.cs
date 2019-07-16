using System;
using Alex.API.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Worlds
{
    internal class ChunkData : IDisposable
    {
        public IndexBuffer SolidIndexBuffer { get; set; }
        public IndexBuffer TransparentIndexBuffer { get; set; }
        public IndexBuffer AnimatedIndexBuffer { get; set; }
        public VertexBuffer Buffer { get; set; }
        public ChunkCoordinates Coordinates { get; set; }

        public void Dispose()
        {
            SolidIndexBuffer?.Dispose();
            TransparentIndexBuffer?.Dispose();
            AnimatedIndexBuffer?.Dispose();
            Buffer?.Dispose();
        }
    }
}