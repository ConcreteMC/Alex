using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Reflection;
using System.Threading;
using Alex.Common.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using SixLabors.ImageSharp;

namespace Alex.Common.Graphics.GpuResources
{
    public static class GpuResourceManager
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(GpuResourceManager));
        
        private static long _totalMemoryUsage = 0;
        private static long _resourceCount = 0;
        public static long ResourceCount => _resourceCount;
        public static long MemoryUsage => _totalMemoryUsage;
        
        private static double TargetElapsedTime => 1d / 30d;
        private static double _elapsedTime = 0d;

        private static FieldInfo _resourceField;
        static GpuResourceManager()
        {
            _resourceField = ReflectionHelper.GetPrivateFieldAccesor(
                typeof(GraphicsDevice), "_resources");
        }
        
        public static void Update(GameTime gameTime, GraphicsDevice device)
        {
            _elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;

            if (_elapsedTime < TargetElapsedTime)
                return;

            _elapsedTime = 0;
            
            var locker = ReflectionHelper.GetPrivateFieldValue<object>(typeof(GraphicsDevice), device, "_resourcesLock");

            List<WeakReference> references;

            if (!Monitor.TryEnter(locker, 0))
                return;

            try
            {
               // var refs = ReflectionHelper.GetPrivateFieldValue<List<WeakReference>>(
               //     typeof(GraphicsDevice), device, "_resources");

                references = (List<WeakReference>)_resourceField.GetValue(device);
            }
            finally
            {
                Monitor.Exit(locker);
            }

            if (references == null)
                return;

            long memUsage = 0;
            int count = 0;

            for (var index = 0; index < references.Count; index++)
            {
                if (index + 1 >= references.Count)
                    break;
                
                var reference = references[index];

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

        public static long TotalResources(this GraphicsDevice device)
        {
            return ResourceCount;
        }
        
        public static long Memory(this GraphicsDevice device)
        {
            return MemoryUsage;
        }
    }
}