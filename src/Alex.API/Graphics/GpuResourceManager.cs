using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.API.Graphics
{
    public class GpuResourceManager
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(GpuResourceManager));
        
        public static long GetMemoryUsage => _instance.EstMemoryUsage;
        private static GpuResourceManager _instance;
        static GpuResourceManager()
        {
            _instance = new GpuResourceManager();
        }
        
        private Dictionary<long, PooledTexture2D> Textures { get; }
        private Dictionary<long, PooledVertexBuffer> Buffers { get; }
        private Dictionary<long, PooledIndexBuffer> IndexBuffers { get; }
        
        private long _bufferId = 0;
        private long _estMemoryUsage = 0;
        private long _textureId = 0;
        private long _indexBufferId = 0;
        
        public long EstMemoryUsage => Buffers.Values.Sum(x => x.VertexDeclaration.VertexStride * x.VertexCount) + _textureMemoryUsage;

        private long _textureMemoryUsage = 0;
        private long _indexMemoryUsage = 0;
        
        public GpuResourceManager()
        {
            Textures = new Dictionary<long, PooledTexture2D>();
            Buffers = new Dictionary<long, PooledVertexBuffer>();
            IndexBuffers = new Dictionary<long, PooledIndexBuffer>();
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
        
        public PooledTexture2D CreateTexture2D(GraphicsDevice graphicsDevice, int width, int height)
        {
            var id = Interlocked.Increment(ref _textureId);
            var texture = new PooledTexture2D(_instance, id, graphicsDevice, width, height); 
            
            Textures.Add(id, texture);

            Interlocked.Add(ref _textureMemoryUsage, texture.Height * texture.Width * 4);
            
            return texture;
        }
        
        public PooledTexture2D CreateTexture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format)
        {
            var id = Interlocked.Increment(ref _textureId);
            var texture = new PooledTexture2D(_instance, id, graphicsDevice, width, height, mipmap, format); 
            
            Textures.Add(id, texture);
            
            Interlocked.Add(ref _textureMemoryUsage, texture.Height * texture.Width * 4);
            
            return texture;
        }
        
        public PooledTexture2D CreateTexture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, int arraySize)
        {
            var id = Interlocked.Increment(ref _textureId);
            var texture = new PooledTexture2D(_instance, id, graphicsDevice, width, height, mipmap, format, arraySize); 
            
            Textures.Add(id, texture);
            
            Interlocked.Add(ref _textureMemoryUsage, texture.Height * texture.Width * 4);
            
            return texture;
        }

        public PooledIndexBuffer CreateIndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize,
            int indexCount, BufferUsage bufferUsage)
        {
            var id = Interlocked.Increment(ref _indexBufferId);
            var buffer = new PooledIndexBuffer(this, id, graphicsDevice, indexElementSize, indexCount, bufferUsage);
            
            IndexBuffers.Add(id, buffer);

            var size = 0;
            if (indexElementSize == IndexElementSize.SixteenBits)
            {
                size = indexCount * 2;
            }
            else if (indexElementSize == IndexElementSize.ThirtyTwoBits)
            {
                size = indexCount * 4;
            }
            
            Interlocked.Add(ref _indexMemoryUsage, size);
            
            return buffer;
        }

        public void Disposed(PooledVertexBuffer buffer)
        {
            var size = buffer.VertexDeclaration.VertexStride * buffer.VertexCount;
            Log.Debug($"Disposing of buffer {buffer.PoolId}, lifetime: {DateTime.UtcNow - buffer.CreatedTime} Memory usage: {Extensions.GetBytesReadable(size)}");

            //Interlocked.Add(ref _estMemoryUsage, -size);
            Buffers.Remove(buffer.PoolId);
        }
        
        public void Disposed(PooledTexture2D buffer)
        {
            var size = buffer.Height * buffer.Width * 4;
            Log.Debug($"Disposing of texture {buffer.PoolId}, lifetime: {DateTime.UtcNow - buffer.CreatedTime} Memory usage: {Extensions.GetBytesReadable(size)}");

            //Interlocked.Add(ref _estMemoryUsage, -size);
            Textures.Remove(buffer.PoolId);
            
            Interlocked.Add(ref _textureMemoryUsage, -(buffer.Height * buffer.Width * 4));
        }

        public void Disposed(PooledIndexBuffer buffer)
        {
            var size = 0;
            if (buffer.IndexElementSize == IndexElementSize.SixteenBits)
            {
                size = buffer.IndexCount * 2;
            }
            else if (buffer.IndexElementSize == IndexElementSize.ThirtyTwoBits)
            {
                size = buffer.IndexCount * 4;
            }

            Log.Debug($"Disposing of indexbuffer {buffer.PoolId}, lifetime: {DateTime.UtcNow - buffer.CreatedTime} Memory usage: {Extensions.GetBytesReadable(size)}");

            //Interlocked.Add(ref _estMemoryUsage, -size);
            IndexBuffers.Remove(buffer.PoolId);
            
            Interlocked.Add(ref _indexMemoryUsage, -size);
        }
        
        public static PooledVertexBuffer GetBuffer(GraphicsDevice device, VertexDeclaration vertexDeclaration,
            int vertexCount, BufferUsage bufferUsage)
        {
            return _instance.CreateBuffer(device, vertexDeclaration, vertexCount, bufferUsage);
        }
        
        public static PooledTexture2D GetTexture2D(GraphicsDevice graphicsDevice, int width, int height)
        {
            return _instance.CreateTexture2D(graphicsDevice, width, height);
        }
        
        public static PooledTexture2D GetTexture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format)
        {
            return _instance.CreateTexture2D(graphicsDevice, width, height, mipmap, format);
        }
        
        public static PooledTexture2D GetTexture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, int arraySize)
        {
            return _instance.CreateTexture2D(graphicsDevice, width, height, mipmap, format, arraySize);
        }

        public static PooledTexture2D GetTexture2D(GraphicsDevice graphicsDevice, Stream stream)
        {
             var texture = Texture2D.FromStream(graphicsDevice, stream);
             var pooled = GetTexture2D(texture.GraphicsDevice, texture.Width, texture.Height, false, texture.Format);
             
             uint[] imgData = new uint[texture.Height * texture.Width];
             texture.GetData(imgData);
             pooled.SetData(imgData);
             texture.Dispose();

             return pooled;
        }

        public static PooledIndexBuffer GetIndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize,
            int indexCount, BufferUsage bufferUsage)
        {
            return _instance.CreateIndexBuffer(graphicsDevice, indexElementSize, indexCount, bufferUsage);
        }
    }
    
    public class PooledVertexBuffer : DynamicVertexBuffer
    {
        public GpuResourceManager Parent { get; }
        public long PoolId { get; }
        internal DateTime CreatedTime { get; }
        internal object Creator { get; set; }

        public PooledVertexBuffer(GpuResourceManager parent, long id, GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage) : base(graphicsDevice, vertexDeclaration, vertexCount, bufferUsage)
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

    public class PooledTexture2D : Texture2D
    {
        public GpuResourceManager Parent { get; }
        public long PoolId { get; }
        internal DateTime CreatedTime { get; }

        public PooledTexture2D(GpuResourceManager parent, long id, GraphicsDevice graphicsDevice, int width, int height) : base(graphicsDevice, width, height)
        {
            Parent = parent;
            PoolId = id;
            CreatedTime = DateTime.UtcNow;
        }

        public PooledTexture2D(GpuResourceManager parent, long id, GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format) : base(graphicsDevice, width, height, mipmap, format)
        {
            Parent = parent;
            PoolId = id;
            CreatedTime = DateTime.UtcNow;
        }

        public PooledTexture2D(GpuResourceManager parent, long id, GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, int arraySize) : base(graphicsDevice, width, height, mipmap, format, arraySize)
        {
            Parent = parent;
            PoolId = id;
            CreatedTime = DateTime.UtcNow;
        }

        /*public PooledTexture2D(GpuResourceManager parent, long id, GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared, int arraySize) : base(graphicsDevice, width, height, mipmap, format, type, shared, arraySize)
        {
            Parent = parent;
            PoolId = id;
            CreatedTime = DateTime.UtcNow;
        }*/

        protected override void Dispose(bool disposing)
        {
            Parent?.Disposed(this);
            base.Dispose(disposing);
        }
    }

    public class PooledIndexBuffer : IndexBuffer
    { 
        public GpuResourceManager Parent { get; }
        public long PoolId { get; }
        internal DateTime CreatedTime { get; }
        
        public PooledIndexBuffer(GpuResourceManager parent, long id, GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage bufferUsage) : base(graphicsDevice, indexElementSize, indexCount, bufferUsage)
        {
            Parent = parent;
            PoolId = id;
            CreatedTime = DateTime.UtcNow;
        }

        protected override void Dispose(bool disposing)
        {
            Parent?.Disposed(this);
            
            base.Dispose(disposing);
        }
    }
}