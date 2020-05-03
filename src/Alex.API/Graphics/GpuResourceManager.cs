using System;
using System.Collections;
using System.Collections.Concurrent;
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

        public static GpuResourceManager Instance => _instance;
        
        private ConcurrentDictionary<long, PooledTexture2D> Textures { get; }
        private ConcurrentDictionary<long, PooledVertexBuffer> Buffers { get; }
        private ConcurrentDictionary<long, PooledIndexBuffer> IndexBuffers { get; }

        private long _bufferId = 0;
      //  private long _estMemoryUsage = 0;
        private long _textureId = 0;
        private long _indexBufferId = 0;

        public long EstMemoryUsage => _totalMemoryUsage;// Buffers.Values.ToArray().Sum(x => x.MemoryUsage) + Textures.Values.ToArray().Sum(x => x.MemoryUsage) + IndexBuffers.Values.ToArray().Sum(x => x.MemoryUsage);

        private long _totalMemoryUsage = 0;
        
     //   private long _textureMemoryUsage = 0;
      //  private long _indexMemoryUsage = 0;
        private Timer DisposalTimer { get; }
        private bool ShuttingDown { get; set; } = false;
        private object _disposalLock = new object();
        public GpuResourceManager()
        {
            Textures = new ConcurrentDictionary<long, PooledTexture2D>();
            Buffers = new ConcurrentDictionary<long, PooledVertexBuffer>();
            IndexBuffers = new ConcurrentDictionary<long, PooledIndexBuffer>();
            
            DisposalTimer = new Timer(state =>
            {
               HandleDisposeQueue();
            }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5));

            AttachCtrlcSigtermShutdown();
        }
        
        private void AttachCtrlcSigtermShutdown()
        {
            void Shutdown()
            {
                ShuttingDown = true;
            };

            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => Shutdown();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Shutdown();
                // Don't terminate the process immediately, wait for the Main thread to exit gracefully.
                eventArgs.Cancel = true;
            };
        }

        private object _resourceLock = new object();
        private List<IGpuResource> _resources = new List<IGpuResource>();
        private ConcurrentQueue<(bool add, IGpuResource resource)> _buffer = new ConcurrentQueue<(bool add, IGpuResource resource)>();
        private List<IGpuResource> _disposalQueue = new List<IGpuResource>();
       // private SortedList<long, IGpuResource> _disposalQueue = new SortedList<long, IGpuResource>();
        
        public IEnumerable<IGpuResource> GetResources()
        {
            lock (_resourceLock)
            {
                foreach (var r in _resources.ToArray())
                {
                    yield return r;
                }
            }

            while (_buffer.TryDequeue(out var resource))
            {
                if (resource.add)
                {
                    _resources.Add(resource.resource);
                    yield return resource.resource;
                }
                else
                {
                    _resources.Remove(resource.resource);
                }
            }
        }

        private void HandleDisposeQueue()
        {
            lock (_disposalLock)
            {
                var disposed = _disposalQueue.ToArray();
                _disposalQueue.Clear();
                foreach (var dispose in disposed)
                {
                    dispose.Dispose();
                }
               // while (_disposalQueue.TryDequeue(out IGpuResource resource))
               // {
               //     resource.Dispose();
               // }
            }
        }

        public PooledVertexBuffer CreateBuffer(object caller, GraphicsDevice device, VertexDeclaration vertexDeclaration,
            int vertexCount, BufferUsage bufferUsage)
        {
          /*  if (Monitor.TryEnter(_disposalLock))
            {
                try
                {
                    var items = _resources.Where(x => x is PooledVertexBuffer).Cast<PooledVertexBuffer>()
                        .Where(x => x.VertexCount >= vertexCount && x.VertexDeclaration == vertexDeclaration).ToArray();

                    var closest = items.OrderBy(x => Math.Abs(x.VertexCount - vertexCount)).FirstOrDefault();
                    if (closest != default)
                    {
                        if (Buffers.TryAdd(closest.PoolId, closest))
                        {
                            closest.UnMark();
                            _resources.Remove(closest);
                            
                      //      Interlocked.Add(ref _totalMemoryUsage, closest.MemoryUsage);
                            return closest;
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(_disposalLock);
                }
            }*/
            
            long id = Interlocked.Increment(ref _bufferId);
            PooledVertexBuffer buffer = new PooledVertexBuffer(this, id, caller, device, vertexDeclaration, vertexCount, bufferUsage);
            buffer.Name = $"{caller.ToString()} - {id}";
            
            Buffers.TryAdd(id, buffer);
            
            _buffer.Enqueue((true, buffer));
            
            var size = Interlocked.Add(ref _totalMemoryUsage, buffer.MemoryUsage);
            return buffer;
        }
        
        public PooledTexture2D CreateTexture2D(object caller, GraphicsDevice graphicsDevice, int width, int height)
        {
            var id = Interlocked.Increment(ref _textureId);
            var texture = new PooledTexture2D(_instance, id, caller, graphicsDevice, width, height); 
            texture.Name = $"{caller.ToString()} - {id}";
            
            Textures.TryAdd(id, texture);
            _buffer.Enqueue((true, texture));
            
          //  Interlocked.Add(ref _textureMemoryUsage, texture.Height * texture.Width * 4);
          Interlocked.Add(ref _totalMemoryUsage, texture.MemoryUsage);
            return texture;
        }
        
        public PooledTexture2D CreateTexture2D(object caller, GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format)
        {
            var id = Interlocked.Increment(ref _textureId);
            var texture = new PooledTexture2D(_instance, id, caller, graphicsDevice, width, height, mipmap, format); 
            texture.Name = $"{caller.ToString()} - {id}";
            
            _buffer.Enqueue((true,texture));
            
            Textures.TryAdd(id, texture);
            
        //    Interlocked.Add(ref _textureMemoryUsage, texture.Height * texture.Width * 4);
        Interlocked.Add(ref _totalMemoryUsage, texture.MemoryUsage);
            return texture;
        }
        
        public PooledTexture2D CreateTexture2D(object caller, GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, int arraySize)
        {
            var id = Interlocked.Increment(ref _textureId);
            var texture = new PooledTexture2D(_instance, id, caller, graphicsDevice, width, height, mipmap, format, arraySize); 
            texture.Name = $"{caller.ToString()} - {id}";
            
            _buffer.Enqueue((true,texture));
            
            Textures.TryAdd(id, texture);
            
        //    Interlocked.Add(ref _textureMemoryUsage, texture.Height * texture.Width * 4);
        Interlocked.Add(ref _totalMemoryUsage, texture.MemoryUsage);
            return texture;
        }

        public PooledIndexBuffer CreateIndexBuffer(object caller, GraphicsDevice graphicsDevice, IndexElementSize indexElementSize,
            int indexCount, BufferUsage bufferUsage)
        {
           /* if (Monitor.TryEnter(_disposalLock))
            {
                try
                {
                    var items = _disposalQueue.Where(x => x is PooledIndexBuffer).Cast<PooledIndexBuffer>()
                        .Where(x => x.IndexCount >= indexCount && x.IndexElementSize == indexElementSize).ToArray();

                    var closest = items.OrderBy(x => Math.Abs(x.IndexCount - indexCount)).FirstOrDefault();
                    if (closest != default)
                    {
                        if (IndexBuffers.TryAdd(closest.PoolId, closest))
                        {
                            closest.UnMark();
                            _disposalQueue.Remove(closest);
                            
                          //  Interlocked.Add(ref _totalMemoryUsage, closest.MemoryUsage);
                            return closest;
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(_disposalLock);
                }
            }*/
            
            var id = Interlocked.Increment(ref _indexBufferId);
            var buffer = new PooledIndexBuffer(this, id, caller, graphicsDevice, indexElementSize, indexCount, bufferUsage);
            buffer.Name = $"{caller.ToString()} - {id}";
            
            _buffer.Enqueue((true,buffer));
            
            IndexBuffers.TryAdd(id, buffer);

            //   Interlocked.Add(ref _indexMemoryUsage, size);
         Interlocked.Add(ref _totalMemoryUsage, buffer.MemoryUsage);
            return buffer;
        }

        public void Disposed(PooledVertexBuffer buffer)
        {
            if (!buffer.MarkedForDisposal)
                Log.Debug($"Incorrectly disposing of buffer {buffer.PoolId}, lifetime: {DateTime.UtcNow - buffer.CreatedTime} Creator: {buffer.Owner ?? "N/A"} Memory usage: {Extensions.GetBytesReadable(buffer.MemoryUsage)}");

            //Interlocked.Add(ref _estMemoryUsage, -size);
            Buffers.Remove(buffer.PoolId, out _);
            _buffer.Enqueue((false,buffer));
            
            Interlocked.Add(ref _totalMemoryUsage, -buffer.MemoryUsage);
        }
        
        public void Disposed(PooledTexture2D buffer)
        {
            if (!buffer.MarkedForDisposal)
                Log.Debug($"Incorrectly disposing of texture {buffer.PoolId}, lifetime: {DateTime.UtcNow - buffer.CreatedTime} Creator: {buffer.Owner ?? "N/A"} Memory usage: {Extensions.GetBytesReadable(buffer.MemoryUsage)}");

            //Interlocked.Add(ref _estMemoryUsage, -size);
            Textures.Remove(buffer.PoolId, out _);
            _buffer.Enqueue((false,buffer));
            Interlocked.Add(ref _totalMemoryUsage, -buffer.MemoryUsage);
           // Interlocked.Add(ref _textureMemoryUsage, -(buffer.Height * buffer.Width * 4));
        }

        public void Disposed(PooledIndexBuffer buffer)
        {
            if (!buffer.MarkedForDisposal)
                Log.Debug($"Incorrectly disposing of indexbuffer {buffer.PoolId}, lifetime: {DateTime.UtcNow - buffer.CreatedTime} Creator: {buffer.Owner ?? "N/A"} Memory usage: {Extensions.GetBytesReadable(buffer.MemoryUsage)}");

            //Interlocked.Add(ref _estMemoryUsage, -size);
            IndexBuffers.Remove(buffer.PoolId, out _);
            _buffer.Enqueue((false,buffer));
            Interlocked.Add(ref _totalMemoryUsage, -buffer.MemoryUsage);
           // Interlocked.Add(ref _indexMemoryUsage, -size);
        }

        internal void QueueForDisposal(IGpuResource resource)
        {
            lock (_disposalLock)
            {
                if (!_disposalQueue.Contains(resource))
                {
                    _disposalQueue.Add(resource);
                }
            }
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

        public bool MarkedForDisposal { get; private set; }
        public void MarkForDisposal()
        {
            if (!MarkedForDisposal)
            {
                MarkedForDisposal = true;
                Parent?.QueueForDisposal(this);
            }
        }

        public void UnMark()
        {
            MarkedForDisposal = false;
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
        public bool IsFullyTransparent { get; set; } = false;
        
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
        
        public bool MarkedForDisposal { get; private set; }
        public void MarkForDisposal()
        {
            if (!MarkedForDisposal)
            {
                MarkedForDisposal = true;
                Parent?.QueueForDisposal(this);
            }
        }

        public void UnMark()
        {
            MarkedForDisposal = false;
        }
        
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

        public bool MarkedForDisposal { get; private set; }
        public void MarkForDisposal()
        {
            if (!MarkedForDisposal)
            {
                MarkedForDisposal = true;
                Parent?.QueueForDisposal(this);
            }
        }
        
        public void UnMark()
        {
            MarkedForDisposal = false;
        }
        
        protected override void Dispose(bool disposing)
        {
            Parent?.Disposed(this);
            
            base.Dispose(disposing);
        }
    }

    public interface IGpuResource : IDisposable
    {
        GpuResourceManager Parent { get; }
        DateTime CreatedTime { get; }
        long PoolId { get; }
        
        object Owner { get; }
        
        long MemoryUsage { get; }

        bool MarkedForDisposal { get; }
        void MarkForDisposal();
        void UnMark();
    }
}