using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Alex.Common.Graphics.GpuResources
{
    public class GpuResourceManager
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(GpuResourceManager));
        
        public static long GetMemoryUsage => _instance.EstMemoryUsage;
        public static long GetResourceCount => _instance.ResourceCount;
        private static readonly GpuResourceManager _instance;
        static GpuResourceManager()
        {
            _instance = new GpuResourceManager();
        }

        //public static GpuResourceManager Instance => _instance;

        private long _bufferId = 0;
        private long _textureId = 0;
        private long _indexBufferId = 0;
        private long _totalMemoryUsage = 0;

        private long ResourceCount => _resources.Count;
        private long EstMemoryUsage => _totalMemoryUsage;

        private Timer DisposalTimer { get; }
        private bool ShuttingDown { get; set; } = false;
        private readonly object _disposalLock = new object();
        private readonly List<IGpuResource> _resources = new List<IGpuResource>();
        private readonly List<IGpuResource> _disposalQueue = new List<IGpuResource>();
        private GpuResourceManager()
        {
            DisposalTimer = new Timer(state =>
            {
               HandleDisposeQueue();
            }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5));

            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => Shutdown();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Shutdown();
                // Don't terminate the process immediately, wait for the Main thread to exit gracefully.
                eventArgs.Cancel = true;
            };
        }
        
        void Shutdown()
        {
            ShuttingDown = true;
        }

        private void HandleDisposeQueue()
        {
            IGpuResource[] disposed;

            lock (_disposalLock)
            {
                disposed = _disposalQueue.ToArray();
                _disposalQueue.Clear();
            }

            foreach (var dispose in disposed)
            {
                dispose.Dispose();
            }
        }

        private ManagedVertexBuffer CreateBuffer(object caller, GraphicsDevice device, VertexDeclaration vertexDeclaration,
            int vertexCount, BufferUsage bufferUsage)
        {
            long id = Interlocked.Increment(ref _bufferId);
            ManagedVertexBuffer buffer = new ManagedVertexBuffer(this, id, caller, device, vertexDeclaration, vertexCount, bufferUsage);
            buffer.Name = $"{caller.ToString()} - {id}";

            _resources.Add(buffer);
            
            var size = Interlocked.Add(ref _totalMemoryUsage, buffer.MemoryUsage);
            return buffer;
        }
        
        private ManagedTexture2D CreateTexture2D(object caller, GraphicsDevice graphicsDevice, int width, int height)
        {
            var id = Interlocked.Increment(ref _textureId);
            var texture = new ManagedTexture2D(_instance, id, caller, graphicsDevice, width, height); 
            texture.Name = $"{caller.ToString()} - {id}";
            
            _resources.Add(texture);
 
          Interlocked.Add(ref _totalMemoryUsage, texture.MemoryUsage);
            return texture;
        }
        
        private ManagedTexture2D CreateTexture2D(object caller, GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format)
        {
            var id = Interlocked.Increment(ref _textureId);
            var texture = new ManagedTexture2D(_instance, id, caller, graphicsDevice, width, height, mipmap, format); 
            texture.Name = $"{caller.ToString()} - {id}";
            
            _resources.Add(texture);

            Interlocked.Add(ref _totalMemoryUsage, texture.MemoryUsage);
            return texture;
        }

        private ManagedTexture2D CreateTexture2D(object caller,
            GraphicsDevice graphicsDevice,
            int width,
            int height,
            bool mipmap,
            SurfaceFormat format,
            int arraySize)
        {
            var id = Interlocked.Increment(ref _textureId);

            var texture = new ManagedTexture2D(
                _instance, id, caller, graphicsDevice, width, height, mipmap, format, arraySize);

            texture.Name = $"{caller.ToString()} - {id}";

            _resources.Add(texture);

            Interlocked.Add(ref _totalMemoryUsage, texture.MemoryUsage);

            return texture;
        }

        private ManagedIndexBuffer CreateIndexBuffer(object caller,
            GraphicsDevice graphicsDevice,
            IndexElementSize indexElementSize,
            int indexCount,
            BufferUsage bufferUsage)
        {
            var id = Interlocked.Increment(ref _indexBufferId);

            var buffer = new ManagedIndexBuffer(
                this, id, caller, graphicsDevice, indexElementSize, indexCount, bufferUsage);

            buffer.Name = $"{caller.ToString()} - {id}";

            _resources.Add(buffer);

            Interlocked.Add(ref _totalMemoryUsage, buffer.MemoryUsage);

            return buffer;
        }

        public static bool ReportIncorrectlyDisposedBuffers = true;

        public void Disposed(ManagedVertexBuffer buffer)
        {
            if (!buffer.MarkedForDisposal && ReportIncorrectlyDisposedBuffers)
                Log.Debug(
                    $"Incorrectly disposing of buffer {buffer.PoolId}, lifetime: {DateTime.UtcNow - buffer.CreatedTime} Creator: {buffer.Owner ?? "N/A"} Memory usage: {Extensions.GetBytesReadable(buffer.MemoryUsage)}");

            _resources.Remove(buffer);

            Interlocked.Add(ref _totalMemoryUsage, -buffer.MemoryUsage);
        }

        public void Disposed(ManagedTexture2D buffer)
        {
            if (!buffer.MarkedForDisposal && ReportIncorrectlyDisposedBuffers)
                Log.Debug(
                    $"Incorrectly disposing of texture {buffer.PoolId}, lifetime: {DateTime.UtcNow - buffer.CreatedTime} Creator: {buffer.Owner ?? "N/A"} Memory usage: {Extensions.GetBytesReadable(buffer.MemoryUsage)}");

            _resources.Remove(buffer);
            Interlocked.Add(ref _totalMemoryUsage, -buffer.MemoryUsage);
        }

        public void Disposed(ManagedIndexBuffer buffer)
        {
            if (!buffer.MarkedForDisposal && ReportIncorrectlyDisposedBuffers)
                Log.Debug(
                    $"Incorrectly disposing of indexbuffer {buffer.PoolId}, lifetime: {DateTime.UtcNow - buffer.CreatedTime} Creator: {buffer.Owner ?? "N/A"} Memory usage: {Extensions.GetBytesReadable(buffer.MemoryUsage)}");

            _resources.Remove(buffer);

            Interlocked.Add(ref _totalMemoryUsage, -buffer.MemoryUsage);
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

        public static ManagedVertexBuffer GetBuffer(object caller, GraphicsDevice device, VertexDeclaration vertexDeclaration,
            int vertexCount, BufferUsage bufferUsage)
        {
            return _instance.CreateBuffer(caller, device, vertexDeclaration, vertexCount, bufferUsage);
        }
        
        public static ManagedTexture2D GetTexture2D(object caller, GraphicsDevice graphicsDevice, int width, int height)
        {
            return _instance.CreateTexture2D(caller, graphicsDevice, width, height);
        }
        
        public static ManagedTexture2D GetTexture2D(object caller, GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format)
        {
            return _instance.CreateTexture2D(caller, graphicsDevice, width, height, mipmap, format);
        }
        
        public static ManagedTexture2D GetTexture2D(object caller, GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, int arraySize)
        {
            return _instance.CreateTexture2D(caller, graphicsDevice, width, height, mipmap, format, arraySize);
        }

        public static ManagedTexture2D GetTexture2D(object caller, GraphicsDevice graphicsDevice, Stream stream)
        {
             //var texture = Texture2D.FromStream(graphicsDevice, stream);
             using (var texture = Image.Load<Rgba32>(stream))
             {
                 uint[] colorData;
	        
                 if (texture.TryGetSinglePixelSpan(out var pixelSpan))
                 {
                     colorData = new uint[pixelSpan.Length];

                     for (int i = 0; i < pixelSpan.Length; i++)
                     {
                         colorData[i] = pixelSpan[i].Rgba;
                     }
                 }
                 else
                 {
                     throw new Exception("Could not get image data!");
                 }

                 SurfaceFormat surfaceFormat = SurfaceFormat.Color;

                 var pooled = GetTexture2D(
                     caller, graphicsDevice, texture.Width, texture.Height, false, surfaceFormat);

                 pooled.SetData(colorData);
                 // texture.Dispose();

                 return pooled;
             }
        }

        public static ManagedIndexBuffer GetIndexBuffer(object caller, GraphicsDevice graphicsDevice, IndexElementSize indexElementSize,
            int indexCount, BufferUsage bufferUsage)
        {
            return _instance.CreateIndexBuffer(caller, graphicsDevice, indexElementSize, indexCount, bufferUsage);
        }
    }
}