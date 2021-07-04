using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Threading;
using Alex.Common.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Alex.Common.Graphics.GpuResources
{
    public class GpuResourceManager
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(GpuResourceManager));
        public static void Setup(GraphicsDevice device)
        {
           // device.ResourceCreated += DeviceOnResourceCreated;
        }

        private static double TargetElapsedTime => 1d / 30d;
        private static double _elapsedTime = 0d;
        public static void Update(GameTime gameTime, GraphicsDevice device)
        {
            _elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;

            if (_elapsedTime < TargetElapsedTime)
                return;

            _elapsedTime = 0;
            
            var locker = ReflectionHelper.GetPrivateFieldValue<object>(typeof(GraphicsDevice), device, "_resourcesLock");

            WeakReference[] references;
            lock (locker)
            {
                var refs = ReflectionHelper.GetPrivateFieldValue<List<WeakReference>>(
                    typeof(GraphicsDevice), device, "_resources");

                references = refs.ToArray();
            }

            long memUsage = 0;
            int count = 0;
            foreach (var reference in references)
            {
                if (!reference.IsAlive)
                    continue;
                
                GraphicsResource resource = reference.Target as GraphicsResource;
                if (resource == null)
                    continue;

                memUsage += resource.MemoryUsage();
                count++;
            }

            _totalMemoryUsage = memUsage;
            _resourceCount = count;
        }
        
        private static long _totalMemoryUsage = 0;

        private static long _resourceCount = 0;
        public static long ResourceCount => _resourceCount;
        public static long MemoryUsage => _totalMemoryUsage;

        private static void DeviceOnResourceCreated(object? sender, ResourceCreatedEventArgs e)
        {
            var resource = e.Resource;
            string name = Environment.TickCount.ToString();

            if (resource is VertexBuffer vb)
            {
                Interlocked.Increment(ref _resourceCount);
                Interlocked.Add(ref _totalMemoryUsage, vb.MemoryUsage());
                vb.Disposing += (o, args) =>
                {
                    Interlocked.Decrement(ref _resourceCount);
                    Interlocked.Add(ref _totalMemoryUsage, -vb.MemoryUsage());
                };
            }
            else if (resource is IndexBuffer ib)
            {
                Interlocked.Increment(ref _resourceCount);
                Interlocked.Add(ref _totalMemoryUsage, ib.MemoryUsage());
                ib.Disposing += (o, args) =>
                {
                    Interlocked.Decrement(ref _resourceCount);
                    Interlocked.Add(ref _totalMemoryUsage, -ib.MemoryUsage());
                };
            }
            else if (resource is Texture2D texture)
            {
                Interlocked.Increment(ref _resourceCount);
                Interlocked.Add(ref _totalMemoryUsage, texture.MemoryUsage());
                texture.Disposing += (o, args) =>
                {
                    Interlocked.Decrement(ref _resourceCount);
                    Interlocked.Add(ref _totalMemoryUsage, -texture.MemoryUsage());
                };
            }
            else
            {
                Log.Warn($"Unknown resourcetype: {resource.GetType()}");
            }
        }
    }
}