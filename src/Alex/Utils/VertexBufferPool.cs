using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alex.GameStates.Playing;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.Utils
{
    public class VertexBufferPool
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(VertexBufferPool));

        public static long GetMemoryUsage => _instance.EstMemoryUsage;
        private static VertexBufferPool _instance;
        static VertexBufferPool()
        {
            _instance = new VertexBufferPool();
        }

        private Dictionary<long, PooledVertexBuffer> Buffers { get; }
        private long _bufferId = 0;
        private long _estMemoryUsage = 0;
        public long EstMemoryUsage => Buffers.Values.Sum(x => x.VertexDeclaration.VertexStride * x.VertexCount);

        public VertexBufferPool()
        {
            Buffers = new Dictionary<long, PooledVertexBuffer>();
        }

        public PooledVertexBuffer CreateBuffer(GraphicsDevice device, VertexDeclaration vertexDeclaration,
            int vertexCount, BufferUsage bufferUsage)
        {
            long id = Interlocked.Increment(ref _bufferId);
            PooledVertexBuffer buffer = new PooledVertexBuffer(this, id, device, vertexDeclaration, vertexCount, bufferUsage);
            Buffers.Add(id, buffer);

          //  var size = Interlocked.Add(ref _estMemoryUsage, vertexDeclaration.VertexStride * vertexCount);
            return buffer;
        }

        public static PooledVertexBuffer GetBuffer(GraphicsDevice device, VertexDeclaration vertexDeclaration,
            int vertexCount, BufferUsage bufferUsage)
        {
            return _instance.CreateBuffer(device, vertexDeclaration, vertexCount, bufferUsage);
        }

        public void Disposed(PooledVertexBuffer buffer)
        {
            var size = buffer.VertexDeclaration.VertexStride * buffer.VertexCount;
            Log.Debug($"Disposing of buffer {buffer.PoolId}, lifetime: {DateTime.UtcNow - buffer.CreatedTime} Memory usage: {PlayingState.GetBytesReadable(size)}");

            //Interlocked.Add(ref _estMemoryUsage, -size);
            Buffers.Remove(buffer.PoolId);
        }
    }

    public class PooledVertexBuffer : VertexBuffer
    {
        public VertexBufferPool Parent { get; }
        public long PoolId { get; }
        internal DateTime CreatedTime { get; }
        internal object Creator { get; set; }

        public PooledVertexBuffer(VertexBufferPool parent, long id, GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage) : base(graphicsDevice, vertexDeclaration, vertexCount, bufferUsage)
        {
            Parent = parent;
            PoolId = id;
            CreatedTime = DateTime.UtcNow;
        }

        protected override void Dispose(bool disposing)
        {
          //  if (!IsDisposed)
                Parent?.Disposed(this);

            base.Dispose(disposing);
        }
    } 
}