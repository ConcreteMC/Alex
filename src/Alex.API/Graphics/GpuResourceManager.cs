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
      //  private long _estMemoryUsage = 0;
        private long _textureId = 0;
        private long _indexBufferId = 0;

        public long EstMemoryUsage => _totalMemoryUsage;// Buffers.Values.ToArray().Sum(x => x.MemoryUsage) + Textures.Values.ToArray().Sum(x => x.MemoryUsage) + IndexBuffers.Values.ToArray().Sum(x => x.MemoryUsage);

        private long _totalMemoryUsage = 0;
        
     //   private long _textureMemoryUsage = 0;
      //  private long _indexMemoryUsage = 0;
        
        public GpuResourceManager()
        {
            Textures = new Dictionary<long, PooledTexture2D>();
            Buffers = new Dictionary<long, PooledVertexBuffer>();
            IndexBuffers = new Dictionary<long, PooledIndexBuffer>();
        }
        
        public PooledVertexBuffer CreateBuffer(object caller, GraphicsDevice device, VertexDeclaration vertexDeclaration,
            int vertexCount, BufferUsage bufferUsage)
        {
            long id = Interlocked.Increment(ref _bufferId);
            PooledVertexBuffer buffer = new PooledVertexBuffer(this, id, caller, device, vertexDeclaration, vertexCount, bufferUsage);
            Buffers.Add(id, buffer);

            var size = Interlocked.Add(ref _totalMemoryUsage, buffer.MemoryUsage);
            return buffer;
        }
        
        public PooledTexture2D CreateTexture2D(object caller, GraphicsDevice graphicsDevice, int width, int height)
        {
            var id = Interlocked.Increment(ref _textureId);
            var texture = new PooledTexture2D(_instance, id, caller, graphicsDevice, width, height); 
            
            Textures.Add(id, texture);

          //  Interlocked.Add(ref _textureMemoryUsage, texture.Height * texture.Width * 4);
          Interlocked.Add(ref _totalMemoryUsage, texture.MemoryUsage);
            return texture;
        }
        
        public PooledTexture2D CreateTexture2D(object caller, GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format)
        {
            var id = Interlocked.Increment(ref _textureId);
            var texture = new PooledTexture2D(_instance, id, caller, graphicsDevice, width, height, mipmap, format); 
            
            Textures.Add(id, texture);
            
        //    Interlocked.Add(ref _textureMemoryUsage, texture.Height * texture.Width * 4);
        Interlocked.Add(ref _totalMemoryUsage, texture.MemoryUsage);
            return texture;
        }
        
        public PooledTexture2D CreateTexture2D(object caller, GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, int arraySize)
        {
            var id = Interlocked.Increment(ref _textureId);
            var texture = new PooledTexture2D(_instance, id, caller, graphicsDevice, width, height, mipmap, format, arraySize); 
            
            Textures.Add(id, texture);
            
        //    Interlocked.Add(ref _textureMemoryUsage, texture.Height * texture.Width * 4);
        Interlocked.Add(ref _totalMemoryUsage, texture.MemoryUsage);
            return texture;
        }

        public PooledIndexBuffer CreateIndexBuffer(object caller, GraphicsDevice graphicsDevice, IndexElementSize indexElementSize,
            int indexCount, BufferUsage bufferUsage)
        {
            var id = Interlocked.Increment(ref _indexBufferId);
            var buffer = new PooledIndexBuffer(this, id, caller, graphicsDevice, indexElementSize, indexCount, bufferUsage);
            
            IndexBuffers.Add(id, buffer);

            //   Interlocked.Add(ref _indexMemoryUsage, size);
         Interlocked.Add(ref _totalMemoryUsage, buffer.MemoryUsage);
            return buffer;
        }

        public void Disposed(PooledVertexBuffer buffer)
        {
            Log.Debug($"Disposing of buffer {buffer.PoolId}, lifetime: {DateTime.UtcNow - buffer.CreatedTime} Creator: {buffer.Owner ?? "N/A"} Memory usage: {Extensions.GetBytesReadable(buffer.MemoryUsage)}");

            //Interlocked.Add(ref _estMemoryUsage, -size);
            Buffers.Remove(buffer.PoolId);
            
            Interlocked.Add(ref _totalMemoryUsage, -buffer.MemoryUsage);
        }
        
        public void Disposed(PooledTexture2D buffer)
        {
            Log.Debug($"Disposing of texture {buffer.PoolId}, lifetime: {DateTime.UtcNow - buffer.CreatedTime} Creator: {buffer.Owner ?? "N/A"} Memory usage: {Extensions.GetBytesReadable(buffer.MemoryUsage)}");

            //Interlocked.Add(ref _estMemoryUsage, -size);
            Textures.Remove(buffer.PoolId);
            Interlocked.Add(ref _totalMemoryUsage, -buffer.MemoryUsage);
           // Interlocked.Add(ref _textureMemoryUsage, -(buffer.Height * buffer.Width * 4));
        }

        public void Disposed(PooledIndexBuffer buffer)
        {
            Log.Debug($"Disposing of indexbuffer {buffer.PoolId}, lifetime: {DateTime.UtcNow - buffer.CreatedTime} Creator: {buffer.Owner ?? "N/A"} Memory usage: {Extensions.GetBytesReadable(buffer.MemoryUsage)}");

            //Interlocked.Add(ref _estMemoryUsage, -size);
            IndexBuffers.Remove(buffer.PoolId);
            Interlocked.Add(ref _totalMemoryUsage, -buffer.MemoryUsage);
           // Interlocked.Add(ref _indexMemoryUsage, -size);
        }
        
        public static PooledVertexBuffer GetBuffer(object caller, GraphicsDevice device, VertexDeclaration vertexDeclaration,
            int vertexCount, BufferUsage bufferUsage)
        {
            return _instance.CreateBuffer(caller, device, vertexDeclaration, vertexCount, bufferUsage);
        }
        
        public static PooledTexture2D GetTexture2D(object caller, GraphicsDevice graphicsDevice, int width, int height)
        {
            return _instance.CreateTexture2D(caller, graphicsDevice, width, height);
        }
        
        public static PooledTexture2D GetTexture2D(object caller, GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format)
        {
            return _instance.CreateTexture2D(caller, graphicsDevice, width, height, mipmap, format);
        }
        
        public static PooledTexture2D GetTexture2D(object caller, GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, int arraySize)
        {
            return _instance.CreateTexture2D(caller, graphicsDevice, width, height, mipmap, format, arraySize);
        }

        public static PooledTexture2D GetTexture2D(object caller, GraphicsDevice graphicsDevice, Stream stream)
        {
             var texture = Texture2D.FromStream(graphicsDevice, stream);
             var pooled = GetTexture2D(caller, texture.GraphicsDevice, texture.Width, texture.Height, false, texture.Format);
             
             uint[] imgData = new uint[texture.Height * texture.Width];
             texture.GetData(imgData);
             pooled.SetData(imgData);
             texture.Dispose();

             return pooled;
        }

        public static PooledIndexBuffer GetIndexBuffer(object caller, GraphicsDevice graphicsDevice, IndexElementSize indexElementSize,
            int indexCount, BufferUsage bufferUsage)
        {
            return _instance.CreateIndexBuffer(caller, graphicsDevice, indexElementSize, indexCount, bufferUsage);
        }
    }
    
    public class PooledVertexBuffer : DynamicVertexBuffer, IGpuResource
    {
        public GpuResourceManager Parent { get; }
        public long PoolId { get; }
        public object Owner { get; }
        public DateTime CreatedTime { get; }

        public long MemoryUsage
        {
            get { return VertexDeclaration.VertexStride * VertexCount; }
        }
        
        public PooledVertexBuffer(GpuResourceManager parent, long id, object owner, GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage) : base(graphicsDevice, vertexDeclaration, vertexCount, bufferUsage)
        {
            Parent = parent;
            PoolId = id;
            CreatedTime = DateTime.UtcNow;
            Owner = owner;
        }

        protected override void Dispose(bool disposing)
        {
            //  if (!IsDisposed)
            Parent?.Disposed(this);

            base.Dispose(disposing);
        }
    }

    public class PooledTexture2D : Texture2D, IGpuResource
    {
        public GpuResourceManager Parent { get; }
        public long PoolId { get; }
        public object Owner { get; }
        public DateTime CreatedTime { get; }
        
        public long MemoryUsage
        {
            get { return GetFormatSize(Format) * Width * Height * LevelCount; }
        }
        
        public PooledTexture2D(GpuResourceManager parent, long id, object owner, GraphicsDevice graphicsDevice, int width, int height) : base(graphicsDevice, width, height)
        {
            Parent = parent;
            PoolId = id;
            CreatedTime = DateTime.UtcNow;
            Owner = owner;
        }

        public PooledTexture2D(GpuResourceManager parent, long id, object owner, GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format) : base(graphicsDevice, width, height, mipmap, format)
        {
            Parent = parent;
            PoolId = id;
            CreatedTime = DateTime.UtcNow;
            Owner = owner;
        }

        public PooledTexture2D(GpuResourceManager parent, long id, object owner, GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, int arraySize) : base(graphicsDevice, width, height, mipmap, format, arraySize)
        {
            Parent = parent;
            PoolId = id;
            CreatedTime = DateTime.UtcNow;
            Owner = owner;
        }

        private static int GetFormatSize(SurfaceFormat format)
        {
            switch (format)
            {
                case SurfaceFormat.Dxt1:
                    return 8;
                case SurfaceFormat.Dxt3:
                case SurfaceFormat.Dxt5:
                    return 16;
                case SurfaceFormat.Alpha8:
                    return 1;
                case SurfaceFormat.Bgr565:
                case SurfaceFormat.Bgra4444:
                case SurfaceFormat.Bgra5551:
                case SurfaceFormat.HalfSingle:
                case SurfaceFormat.NormalizedByte2:
                    return 2;
                case SurfaceFormat.Color:
                case SurfaceFormat.Single:
                case SurfaceFormat.Rg32:
                case SurfaceFormat.HalfVector2:
                case SurfaceFormat.NormalizedByte4:
                case SurfaceFormat.Rgba1010102:
                case SurfaceFormat.Bgra32:
                    return 4;
                case SurfaceFormat.HalfVector4:
                case SurfaceFormat.Rgba64:
                case SurfaceFormat.Vector2:
                    return 8;
                case SurfaceFormat.Vector4:
                    return 16;
                default:
                    throw new ArgumentException("Should be a value defined in SurfaceFormat", "Format");
            }
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

    public class PooledIndexBuffer : IndexBuffer, IGpuResource
    { 
        public GpuResourceManager Parent { get; }
        public long PoolId { get; }
        public object Owner { get; }
        public DateTime CreatedTime { get; }
        
        public long MemoryUsage
        {
            get { return (IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4) * IndexCount; }
        }
        
        public PooledIndexBuffer(GpuResourceManager parent, long id, object owner, GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage bufferUsage) : base(graphicsDevice, indexElementSize, indexCount, bufferUsage)
        {
            Parent = parent;
            PoolId = id;
            CreatedTime = DateTime.UtcNow;
            Owner = owner;
        }

        protected override void Dispose(bool disposing)
        {
            Parent?.Disposed(this);
            
            base.Dispose(disposing);
        }
    }

    public interface IGpuResource
    {
        GpuResourceManager Parent { get; }
        DateTime CreatedTime { get; }
        long PoolId { get; }
        
        object Owner { get; }
        
        long MemoryUsage { get; }
    }
}