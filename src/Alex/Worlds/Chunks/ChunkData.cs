using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alex.API;
using Alex.API.Blocks;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.Utils.Vectors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Alex.Worlds.Chunks
{
    public class ChunkData : IDisposable
    {
        private        ChunkRenderStage[] _stages;
        private static long               _instances = 0;
        public         ChunkCoordinates   Coordinates { get; }
        public ChunkData(ChunkCoordinates coordinates)
        {
            Coordinates = coordinates;
            var availableStages = Enum.GetValues(typeof(RenderStage));
            _stages = new ChunkRenderStage[availableStages.Length];
            
            
            Interlocked.Increment(ref _instances);
        }

        public bool Draw(GraphicsDevice device, RenderStage stage, Effect effect)
        {
            if (Disposed)
            {
                return false;
            }

            var rStage = _stages[(int) stage];

            if (rStage == null)
            {
                return false;
            }

            return rStage.Render(device, effect);
        }

        public MinifiedBlockShaderVertex[] Vertices
        {
            get
            {
                return _stages.Where(x => x != null).SelectMany(x => x.BuildVertices()).ToArray();
            }
        }

        public void AddVertex(BlockCoordinates blockCoordinates,
            Vector3 position,
            Vector2 textureCoordinates,
            Color color,
            byte blockLight,
            byte skyLight,
            RenderStage stage)
        {
            var rStage = _stages[(int) stage];

            if (rStage == null)
            {
                rStage = CreateRenderStage(stage);
                _stages[(int) stage] = rStage;
            }
           
            rStage.AddVertex(blockCoordinates, position, textureCoordinates, color, blockLight, skyLight);
        }

        private ChunkRenderStage CreateRenderStage(RenderStage arg)
        {
            return new ChunkRenderStage(this);
        }

        public void Remove(GraphicsDevice device, BlockCoordinates blockCoordinates)
        {
            for (var index = 0; index < _stages.Length; index++)
            {
                var stage = _stages[index];

                stage?.Remove(blockCoordinates);
            }
        }

        public bool Contains(BlockCoordinates coordinates)
        {
            return _stages.Where(x => x != null).Any(x => x.Contains(coordinates));
        }

        public void ApplyChanges(GraphicsDevice device, bool keepInMemory)
        {
            for (var index = 0; index < _stages.Length; index++)
            {
                var stage = _stages[index];
                stage?.Apply(device, keepInMemory);
            }
        }

        public bool                Disposed { get; private set; } = false;
        public void Dispose()
        {
            if (Disposed)
                return;
            
           // lock (WriteLock)
           {
               try
               {
                   for (var index = 0; index < _stages.Length; index++)
                   {
                       var stage = _stages[index];
                       stage?.Dispose();
                       _stages[index] = null;
                   }
               }
               finally
               {
                   _stages = null;

                   //  Disposed = true;
                   Interlocked.Decrement(ref _instances);
                   Disposed = true;
               }
           }
        }
    }
}